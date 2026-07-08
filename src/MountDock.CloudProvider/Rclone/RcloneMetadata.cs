using System.Text.Json;
using System.Text.Json.Serialization;

namespace MountDock.CloudProvider.Rclone;

public sealed record RcloneRemoteItem(
    string Path,
    string Name,
    bool IsDirectory,
    long Size,
    DateTimeOffset? ModTime,
    IReadOnlyDictionary<string, string> Hashes);

public static class RcloneLsJsonParser
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    public static IReadOnlyList<RcloneRemoteItem> Parse(string json)
    {
        var rawItems = JsonSerializer.Deserialize<List<RawRcloneLsJsonItem>>(json, JsonOptions) ?? [];
        return rawItems
            .Where(item => !string.IsNullOrWhiteSpace(item.Path))
            .Select(item => new RcloneRemoteItem(
                Path: NormalizeRemotePath(item.Path),
                Name: item.Name ?? Path.GetFileName(NormalizeRemotePath(item.Path)),
                IsDirectory: item.IsDir,
                Size: item.IsDir ? 0 : Math.Max(0, item.Size),
                ModTime: item.ModTime,
                Hashes: item.Hashes ?? new Dictionary<string, string>()))
            .ToList();
    }

    private static string NormalizeRemotePath(string path) => path.Replace('\\', '/').Trim('/');

    private sealed class RawRcloneLsJsonItem
    {
        public string Path { get; set; } = string.Empty;
        public string? Name { get; set; }
        public long Size { get; set; }
        public DateTimeOffset? ModTime { get; set; }
        public bool IsDir { get; set; }
        public Dictionary<string, string>? Hashes { get; set; }
    }
}
