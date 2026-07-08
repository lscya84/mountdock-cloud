using MountDock.CloudProvider.Rclone;
using MountDock.CloudProvider.State;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class OperationWorkerTests
{
    [Fact]
    public async Task RunOnceAsync_ExecutesPendingHydrateAndMarksSucceeded()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"mountdock-cloud-{Guid.NewGuid():N}", "state.db");
        var db = new SyncStateDb(dbPath);
        db.Initialize();

        try
        {
            var item = new SyncItem("Docs/report.xlsx", "Docs", "Docs/report.xlsx", "report.xlsx", false, 42, "online_only", "clean", "unpinned");
            var operation = HydrationPlanner.BuildHydrationOperation(item, "gdrive:", "C:/Temp/report.xlsx");
            var operationId = db.EnqueueOperation(operation);
            var runner = new RecordingRcloneRunner(new RcloneResult(0, "", ""));
            var worker = new OperationWorker(db, runner, "rclone.exe", "rclone.conf", TimeSpan.FromSeconds(30));

            var processed = await worker.RunOnceAsync(CancellationToken.None);

            var saved = db.GetOperation(operationId);
            Assert.Equal(1, processed);
            Assert.NotNull(saved);
            Assert.Equal("succeeded", saved.Status);
            Assert.Equal(new[] { "rclone.exe", "copyto", "gdrive:Docs/report.xlsx", "C:/Temp/report.xlsx", "--config", "rclone.conf" }, runner.LastCommand);
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(dbPath)!, recursive: true);
        }
    }

    [Fact]
    public async Task RunOnceAsync_MarksFailedWhenRcloneFails()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"mountdock-cloud-{Guid.NewGuid():N}", "state.db");
        var db = new SyncStateDb(dbPath);
        db.Initialize();

        try
        {
            var item = new SyncItem("Docs/report.xlsx", "Docs", "Docs/report.xlsx", "report.xlsx", false, 42, "online_only", "clean", "unpinned");
            var operationId = db.EnqueueOperation(HydrationPlanner.BuildHydrationOperation(item, "gdrive:", "C:/Temp/report.xlsx"));
            var runner = new RecordingRcloneRunner(new RcloneResult(1, "", "remote error"));
            var worker = new OperationWorker(db, runner, "rclone.exe", null, TimeSpan.FromSeconds(30));

            var processed = await worker.RunOnceAsync(CancellationToken.None);

            var saved = db.GetOperation(operationId);
            Assert.Equal(1, processed);
            Assert.NotNull(saved);
            Assert.Equal("failed", saved.Status);
            Assert.Equal(1, saved.Attempts);
            Assert.Contains("remote error", saved.LastError);
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(dbPath)!, recursive: true);
        }
    }

    private sealed class RecordingRcloneRunner : IRcloneRunner
    {
        private readonly RcloneResult _result;

        public RecordingRcloneRunner(RcloneResult result)
        {
            _result = result;
        }

        public IReadOnlyList<string> LastCommand { get; private set; } = Array.Empty<string>();

        public Task<RcloneResult> RunRequiredWithTimeoutAsync(IReadOnlyList<string> command, TimeSpan timeout, CancellationToken cancellationToken)
        {
            LastCommand = command;
            if (!_result.Success)
            {
                throw new RcloneCommandException(command, _result);
            }
            return Task.FromResult(_result);
        }
    }
}
