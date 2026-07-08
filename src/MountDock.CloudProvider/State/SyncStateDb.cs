namespace MountDock.CloudProvider.State;

public sealed class SyncStateDb
{
    public string DatabasePath { get; }

    public SyncStateDb(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath)) throw new ArgumentException("Database path is required.", nameof(databasePath));
        DatabasePath = databasePath;
    }

    public void Initialize()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(DatabasePath))!);
        // SQLite schema implementation will be added after the CfAPI spike proves viability.
    }
}
