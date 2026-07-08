using MountDock.CloudProvider.Rclone;

namespace MountDock.CloudProvider.State;

public sealed class OperationWorker
{
    private readonly SyncStateDb _db;
    private readonly IRcloneRunner _rcloneRunner;
    private readonly string _rclonePath;
    private readonly string? _configPath;
    private readonly TimeSpan _operationTimeout;

    public OperationWorker(
        SyncStateDb db,
        IRcloneRunner rcloneRunner,
        string rclonePath,
        string? configPath,
        TimeSpan operationTimeout)
    {
        _db = db;
        _rcloneRunner = rcloneRunner;
        _rclonePath = rclonePath;
        _configPath = configPath;
        _operationTimeout = operationTimeout;
    }

    public async Task<int> RunOnceAsync(CancellationToken cancellationToken)
    {
        var operation = _db.GetPendingOperations(limit: 1).FirstOrDefault();
        if (operation is null)
        {
            return 0;
        }

        try
        {
            var draft = new SyncOperationDraft(
                Kind: operation.Kind,
                ItemId: operation.ItemId,
                Path: operation.Path,
                Priority: operation.Priority,
                PayloadJson: operation.PayloadJson);
            var command = BuildCommand(draft);
            await _rcloneRunner.RunRequiredWithTimeoutAsync(command, _operationTimeout, cancellationToken);
            _db.MarkOperationSucceeded(operation.Id);
        }
        catch (Exception exc) when (exc is not OperationCanceledException)
        {
            _db.MarkOperationFailed(operation.Id, BuildErrorMessage(exc));
        }

        return 1;
    }

    private IReadOnlyList<string> BuildCommand(SyncOperationDraft operation)
    {
        return operation.Kind switch
        {
            "hydrate" => HydrationPlanner.BuildHydrationCommand(_rclonePath, operation, _configPath),
            _ => throw new NotSupportedException($"Unsupported sync operation kind: {operation.Kind}"),
        };
    }

    private static string BuildErrorMessage(Exception exc)
    {
        if (exc is RcloneCommandException rcloneCommandException)
        {
            return string.IsNullOrWhiteSpace(rcloneCommandException.Result.Stderr)
                ? rcloneCommandException.Message
                : rcloneCommandException.Result.Stderr;
        }
        return exc.Message;
    }
}
