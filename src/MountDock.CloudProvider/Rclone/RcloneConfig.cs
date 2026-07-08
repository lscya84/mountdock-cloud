namespace MountDock.CloudProvider.Rclone;

public sealed record RcloneRemote(string Name, string Type);

public static class RcloneConfigParser
{
    public static IReadOnlyList<RcloneRemote> Parse(string content)
    {
        var remotes = new List<RcloneRemote>();
        string? currentName = null;
        string currentType = string.Empty;

        foreach (var rawLine in (content ?? string.Empty).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#') || line.StartsWith(';'))
            {
                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                AddCurrentRemote();
                currentName = line[1..^1].Trim();
                currentType = string.Empty;
                if (currentName.Length == 0)
                {
                    currentName = null;
                }
                continue;
            }

            if (currentName is null)
            {
                continue;
            }

            var separator = line.IndexOf('=');
            if (separator < 0)
            {
                continue;
            }

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();
            if (key.Equals("type", StringComparison.OrdinalIgnoreCase))
            {
                currentType = value;
            }
        }

        AddCurrentRemote();
        return remotes;

        void AddCurrentRemote()
        {
            if (!string.IsNullOrWhiteSpace(currentName))
            {
                remotes.Add(new RcloneRemote(currentName, currentType));
            }
        }
    }
}

public static class RcloneConfigPaths
{
    public static string? FindDefaultConfig(string appDir, string? appDataDir, string homeDir)
    {
        foreach (var candidate in GetDefaultConfigCandidates(appDir, appDataDir, homeDir))
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    public static IReadOnlyList<string> GetDefaultConfigCandidates(string appDir, string? appDataDir, string homeDir)
    {
        if (string.IsNullOrWhiteSpace(appDir)) throw new ArgumentException("App directory is required.", nameof(appDir));
        if (string.IsNullOrWhiteSpace(homeDir)) throw new ArgumentException("Home directory is required.", nameof(homeDir));

        var candidates = new List<string>
        {
            Path.GetFullPath(Path.Combine(appDir, ".mountdock", "rclone.conf")),
            Path.GetFullPath(Path.Combine(appDir, "rclone.conf")),
        };

        if (!string.IsNullOrWhiteSpace(appDataDir))
        {
            candidates.Add(Path.GetFullPath(Path.Combine(appDataDir, "rclone", "rclone.conf")));
        }

        candidates.Add(Path.GetFullPath(Path.Combine(homeDir, ".config", "rclone", "rclone.conf")));
        return candidates;
    }
}
