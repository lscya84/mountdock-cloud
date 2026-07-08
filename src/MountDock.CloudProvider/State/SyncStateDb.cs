using Microsoft.Data.Sqlite;

namespace MountDock.CloudProvider.State;

public sealed record SyncItem(
    string ItemId,
    string ParentId,
    string Path,
    string Name,
    bool IsDirectory,
    long Size,
    string LocalState,
    string SyncState,
    string PinState);

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
        using var connection = OpenConnection();
        ExecuteNonQuery(connection, """
            CREATE TABLE IF NOT EXISTS items (
              item_id TEXT PRIMARY KEY,
              parent_id TEXT NOT NULL DEFAULT '',
              path TEXT NOT NULL UNIQUE,
              name TEXT NOT NULL,
              is_dir INTEGER NOT NULL,
              size INTEGER NOT NULL DEFAULT 0,
              remote_mtime TEXT NOT NULL DEFAULT '',
              remote_hash TEXT NOT NULL DEFAULT '',
              remote_id TEXT NOT NULL DEFAULT '',
              local_mtime_ns INTEGER NOT NULL DEFAULT 0,
              local_state TEXT NOT NULL DEFAULT 'online_only',
              sync_state TEXT NOT NULL DEFAULT 'clean',
              pin_state TEXT NOT NULL DEFAULT 'unpinned',
              provider_blob BLOB,
              last_seen_remote_at TEXT NOT NULL DEFAULT '',
              updated_at TEXT NOT NULL
            );
            """);
        ExecuteNonQuery(connection, """
            CREATE TABLE IF NOT EXISTS operations (
              id INTEGER PRIMARY KEY AUTOINCREMENT,
              kind TEXT NOT NULL,
              item_id TEXT NOT NULL,
              path TEXT NOT NULL,
              status TEXT NOT NULL DEFAULT 'pending',
              priority INTEGER NOT NULL DEFAULT 100,
              attempts INTEGER NOT NULL DEFAULT 0,
              payload_json TEXT NOT NULL DEFAULT '{}',
              last_error TEXT NOT NULL DEFAULT '',
              created_at TEXT NOT NULL,
              updated_at TEXT NOT NULL
            );
            """);
        ExecuteNonQuery(connection, """
            CREATE TABLE IF NOT EXISTS conflicts (
              id INTEGER PRIMARY KEY AUTOINCREMENT,
              item_id TEXT NOT NULL,
              path TEXT NOT NULL,
              local_conflict_path TEXT NOT NULL,
              remote_conflict_path TEXT NOT NULL DEFAULT '',
              reason TEXT NOT NULL,
              created_at TEXT NOT NULL,
              resolved_at TEXT NOT NULL DEFAULT ''
            );
            """);
    }

    public IReadOnlyList<string> GetTableNames()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name";
        using var reader = command.ExecuteReader();
        var tables = new List<string>();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }
        return tables;
    }

    public void UpsertItem(SyncItem item)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO items (
              item_id, parent_id, path, name, is_dir, size, local_state, sync_state, pin_state, updated_at
            ) VALUES (
              $item_id, $parent_id, $path, $name, $is_dir, $size, $local_state, $sync_state, $pin_state, $updated_at
            )
            ON CONFLICT(path) DO UPDATE SET
              item_id = excluded.item_id,
              parent_id = excluded.parent_id,
              name = excluded.name,
              is_dir = excluded.is_dir,
              size = excluded.size,
              local_state = excluded.local_state,
              sync_state = excluded.sync_state,
              pin_state = excluded.pin_state,
              updated_at = excluded.updated_at;
            """;
        command.Parameters.AddWithValue("$item_id", item.ItemId);
        command.Parameters.AddWithValue("$parent_id", item.ParentId);
        command.Parameters.AddWithValue("$path", item.Path);
        command.Parameters.AddWithValue("$name", item.Name);
        command.Parameters.AddWithValue("$is_dir", item.IsDirectory ? 1 : 0);
        command.Parameters.AddWithValue("$size", item.Size);
        command.Parameters.AddWithValue("$local_state", item.LocalState);
        command.Parameters.AddWithValue("$sync_state", item.SyncState);
        command.Parameters.AddWithValue("$pin_state", item.PinState);
        command.Parameters.AddWithValue("$updated_at", DateTimeOffset.UtcNow.ToString("O"));
        command.ExecuteNonQuery();
    }

    public SyncItem? GetItemByPath(string path)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT item_id, parent_id, path, name, is_dir, size, local_state, sync_state, pin_state
            FROM items
            WHERE path = $path
            """;
        command.Parameters.AddWithValue("$path", path);
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;
        return new SyncItem(
            ItemId: reader.GetString(0),
            ParentId: reader.GetString(1),
            Path: reader.GetString(2),
            Name: reader.GetString(3),
            IsDirectory: reader.GetInt32(4) != 0,
            Size: reader.GetInt64(5),
            LocalState: reader.GetString(6),
            SyncState: reader.GetString(7),
            PinState: reader.GetString(8));
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection($"Data Source={DatabasePath}");
        connection.Open();
        return connection;
    }

    private static void ExecuteNonQuery(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
