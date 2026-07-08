using MountDock.CloudProvider.State;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class SyncOperationTests
{
    [Fact]
    public void EnqueueOperation_PersistsPendingOperationInPriorityOrder()
    {
        var root = Path.Combine(Path.GetTempPath(), $"mountdock-cloud-ops-{Guid.NewGuid():N}");
        var db = new SyncStateDb(Path.Combine(root, "state.db"));
        try
        {
            db.Initialize();
            var low = db.EnqueueOperation(new SyncOperationDraft("upload", "item-2", "b.txt", 200, "{}"));
            var high = db.EnqueueOperation(new SyncOperationDraft("hydrate", "item-1", "a.txt", 10, "{\"reason\":\"open\"}"));

            var pending = db.GetPendingOperations(limit: 10);

            Assert.Equal(new[] { high, low }, pending.Select(operation => operation.Id).ToArray());
            Assert.Equal("hydrate", pending[0].Kind);
            Assert.Equal("pending", pending[0].Status);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void MarkOperationFailed_IncrementsAttemptsAndStoresError()
    {
        var root = Path.Combine(Path.GetTempPath(), $"mountdock-cloud-ops-{Guid.NewGuid():N}");
        var db = new SyncStateDb(Path.Combine(root, "state.db"));
        try
        {
            db.Initialize();
            var id = db.EnqueueOperation(new SyncOperationDraft("upload", "item-1", "a.txt", 100, "{}"));

            db.MarkOperationFailed(id, "network failed");
            var operation = db.GetOperation(id);

            Assert.NotNull(operation);
            Assert.Equal("failed", operation.Status);
            Assert.Equal(1, operation.Attempts);
            Assert.Equal("network failed", operation.LastError);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void AddConflict_PersistsConflictRecord()
    {
        var root = Path.Combine(Path.GetTempPath(), $"mountdock-cloud-conflict-{Guid.NewGuid():N}");
        var db = new SyncStateDb(Path.Combine(root, "state.db"));
        try
        {
            db.Initialize();
            var id = db.AddConflict(new SyncConflictDraft(
                ItemId: "item-1",
                Path: "Docs/report.xlsx",
                LocalConflictPath: "Docs/report (conflict).xlsx",
                RemoteConflictPath: "",
                Reason: "remote changed while local dirty"));

            var conflicts = db.GetOpenConflicts();

            var conflict = Assert.Single(conflicts);
            Assert.Equal(id, conflict.Id);
            Assert.Equal("item-1", conflict.ItemId);
            Assert.Equal("remote changed while local dirty", conflict.Reason);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
