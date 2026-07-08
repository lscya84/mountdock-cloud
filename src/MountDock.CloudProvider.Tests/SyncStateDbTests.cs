using MountDock.CloudProvider.State;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class SyncStateDbTests
{
    [Fact]
    public void Initialize_CreatesDatabaseAndCoreTables()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"mountdock-cloud-state-{Guid.NewGuid():N}", "state.db");
        var db = new SyncStateDb(dbPath);

        db.Initialize();

        Assert.True(File.Exists(dbPath));
        var tables = db.GetTableNames();
        Assert.Contains("items", tables);
        Assert.Contains("operations", tables);
        Assert.Contains("conflicts", tables);
    }

    [Fact]
    public void UpsertItem_PersistsAcrossReopen()
    {
        var root = Path.Combine(Path.GetTempPath(), $"mountdock-cloud-state-{Guid.NewGuid():N}");
        var dbPath = Path.Combine(root, "state.db");
        try
        {
            var db = new SyncStateDb(dbPath);
            db.Initialize();
            db.UpsertItem(new SyncItem(
                ItemId: "item-1",
                ParentId: "",
                Path: "Docs/hello.txt",
                Name: "hello.txt",
                IsDirectory: false,
                Size: 42,
                LocalState: "online_only",
                SyncState: "clean",
                PinState: "unpinned"));

            var reopened = new SyncStateDb(dbPath);
            reopened.Initialize();
            var item = reopened.GetItemByPath("Docs/hello.txt");

            Assert.NotNull(item);
            Assert.Equal("item-1", item.ItemId);
            Assert.Equal(42, item.Size);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
