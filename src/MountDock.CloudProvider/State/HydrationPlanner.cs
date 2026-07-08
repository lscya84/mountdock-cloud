using System.Text.Json;
using MountDock.CloudProvider.Rclone;

namespace MountDock.CloudProvider.State;

public static class HydrationPlanner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static SyncOperationDraft BuildHydrationOperation(SyncItem item, string remoteRoot, string localPath)
    {
        if (item.IsDirectory) throw new ArgumentException("Directories cannot be hydrated as file downloads.", nameof(item));
        if (string.IsNullOrWhiteSpace(remoteRoot)) throw new ArgumentException("Remote root is required.", nameof(remoteRoot));
        if (string.IsNullOrWhiteSpace(localPath)) throw new ArgumentException("Local path is required.", nameof(localPath));

        var remotePath = CombineRemotePath(remoteRoot, item.Path);
        var payloadJson = JsonSerializer.Serialize(new HydrationPayload(remotePath, localPath), JsonOptions);
        return new SyncOperationDraft(
            Kind: "hydrate",
            ItemId: item.ItemId,
            Path: item.Path,
            Priority: 10,
            PayloadJson: payloadJson);
    }

    public static IReadOnlyList<string> BuildHydrationCommand(string rclonePath, SyncOperationDraft operation, string? configPath)
    {
        if (operation.Kind != "hydrate") throw new ArgumentException("Operation must be a hydrate operation.", nameof(operation));
        var payload = JsonSerializer.Deserialize<HydrationPayload>(operation.PayloadJson, JsonOptions)
            ?? throw new InvalidOperationException("Hydration payload is invalid.");
        return RcloneCommandBuilder.BuildCopyTo(rclonePath, payload.RemotePath, payload.LocalPath, configPath);
    }

    private static string CombineRemotePath(string remoteRoot, string relativePath)
    {
        var root = remoteRoot.Trim();
        var path = relativePath.Replace('\\', '/').Trim('/');
        return root.EndsWith(":", StringComparison.Ordinal) || root.EndsWith("/", StringComparison.Ordinal)
            ? root + path
            : root + "/" + path;
    }

    private sealed record HydrationPayload(string RemotePath, string LocalPath);
}
