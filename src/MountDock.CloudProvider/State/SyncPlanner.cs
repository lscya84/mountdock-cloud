using MountDock.CloudProvider.Rclone;

namespace MountDock.CloudProvider.State;

public static class SyncPlanner
{
    public static IReadOnlyList<SyncItem> BuildUpserts(IEnumerable<RcloneRemoteItem> remoteItems)
    {
        return remoteItems
            .OrderBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
            .Select(item => new SyncItem(
                ItemId: BuildItemId(item.Path),
                ParentId: BuildParentId(item.Path),
                Path: item.Path,
                Name: string.IsNullOrWhiteSpace(item.Name) ? Path.GetFileName(item.Path) : item.Name,
                IsDirectory: item.IsDirectory,
                Size: item.IsDirectory ? 0 : item.Size,
                LocalState: "online_only",
                SyncState: "clean",
                PinState: "unpinned"))
            .ToList();
    }

    public static int ApplyRemoteSnapshot(SyncStateDb db, IEnumerable<RcloneRemoteItem> remoteItems)
    {
        var items = BuildUpserts(remoteItems);
        foreach (var item in items)
        {
            db.UpsertItem(item);
        }
        return items.Count;
    }

    private static string BuildItemId(string remotePath) => Normalize(remotePath);

    private static string BuildParentId(string remotePath)
    {
        var normalized = Normalize(remotePath);
        var index = normalized.LastIndexOf('/');
        return index <= 0 ? string.Empty : normalized[..index];
    }

    private static string Normalize(string remotePath) => remotePath.Replace('\\', '/').Trim('/');
}
