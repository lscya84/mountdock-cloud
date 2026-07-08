using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MountDock.CloudProvider.Rclone;

namespace MountDock.CloudProvider.CloudFiles;

public static class PlaceholderPlanner
{
    public static IReadOnlyList<PlaceholderDefinition> FromRemoteItems(IEnumerable<RcloneRemoteItem> remoteItems)
    {
        return remoteItems
            .OrderBy(item => item.Path, StringComparer.OrdinalIgnoreCase)
            .Select(ToPlaceholder)
            .ToList();
    }

    private static PlaceholderDefinition ToPlaceholder(RcloneRemoteItem item)
    {
        return new PlaceholderDefinition(
            RelativePath: item.Path,
            IsDirectory: item.IsDirectory,
            Size: item.IsDirectory ? 0 : item.Size,
            LastWriteTimeUtc: item.ModTime,
            ProviderIdentity: BuildProviderIdentity(item));
    }

    private static byte[] BuildProviderIdentity(RcloneRemoteItem item)
    {
        var payload = JsonSerializer.Serialize(new
        {
            item.Path,
            item.Name,
            item.IsDirectory,
            item.Size,
            item.ModTime,
            item.Hashes,
        });
        return SHA256.HashData(Encoding.UTF8.GetBytes(payload));
    }
}
