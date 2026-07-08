using MountDock.CloudProvider.Rclone;
using MountDock.CloudProvider.State;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class SyncPlannerTests
{
    [Fact]
    public void BuildUpserts_ConvertsRemoteMetadataToOnlineOnlySyncItems()
    {
        var remoteItems = new[]
        {
            new RcloneRemoteItem("Docs", "Docs", true, 0, DateTimeOffset.Parse("2026-07-08T01:02:03Z"), new Dictionary<string, string>()),
            new RcloneRemoteItem("Docs/report.xlsx", "report.xlsx", false, 42, DateTimeOffset.Parse("2026-07-08T02:03:04Z"), new Dictionary<string, string> { ["MD5"] = "abc" }),
        };

        var items = SyncPlanner.BuildUpserts(remoteItems);

        Assert.Equal(2, items.Count);
        Assert.Equal("Docs", items[0].ItemId);
        Assert.Equal("", items[0].ParentId);
        Assert.True(items[0].IsDirectory);
        Assert.Equal("online_only", items[1].LocalState);
        Assert.Equal("clean", items[1].SyncState);
        Assert.Equal("unpinned", items[1].PinState);
        Assert.Equal("Docs", items[1].ParentId);
        Assert.Equal("Docs/report.xlsx", items[1].Path);
    }

    [Fact]
    public void ApplyRemoteSnapshot_UpsertsRemoteItemsIntoDatabase()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"mountdock-cloud-{Guid.NewGuid():N}", "state.db");
        var db = new SyncStateDb(dbPath);
        db.Initialize();

        try
        {
            var remoteItems = new[]
            {
                new RcloneRemoteItem("Docs/report.xlsx", "report.xlsx", false, 42, null, new Dictionary<string, string>()),
            };

            var count = SyncPlanner.ApplyRemoteSnapshot(db, remoteItems);

            var item = db.GetItemByPath("Docs/report.xlsx");
            Assert.Equal(1, count);
            Assert.NotNull(item);
            Assert.Equal(42, item.Size);
            Assert.Equal("online_only", item.LocalState);
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(dbPath)!, recursive: true);
        }
    }
}
