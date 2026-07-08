using MountDock.CloudProvider.State;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class SyncOperationStateTests
{
    [Fact]
    public void MarkOperationSucceeded_StoresSucceededStatus()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"mountdock-cloud-{Guid.NewGuid():N}", "state.db");
        var db = new SyncStateDb(dbPath);
        db.Initialize();

        try
        {
            var id = db.EnqueueOperation(new SyncOperationDraft("hydrate", "item-1", "Docs/a.txt", 10, "{}"));

            db.MarkOperationSucceeded(id);

            var operation = db.GetOperation(id);
            Assert.NotNull(operation);
            Assert.Equal("succeeded", operation.Status);
            Assert.Equal(string.Empty, operation.LastError);
        }
        finally
        {
            Directory.Delete(Path.GetDirectoryName(dbPath)!, recursive: true);
        }
    }
}
