namespace MountDock.CloudProvider.Rclone;

public static class RcloneCommandBuilder
{
    public static IReadOnlyList<string> BuildLsJson(string rclonePath, string remotePath, bool recursive, string? configPath)
    {
        var args = BaseCommand(rclonePath, "lsjson", remotePath, configPath);
        if (recursive)
        {
            args.Add("--recursive");
        }
        return args;
    }

    public static IReadOnlyList<string> BuildCopyTo(string rclonePath, string source, string destination, string? configPath)
    {
        return BaseCommand(rclonePath, "copyto", source, configPath, destination);
    }

    public static IReadOnlyList<string> BuildDeleteFile(string rclonePath, string remotePath, string? configPath)
    {
        return BaseCommand(rclonePath, "deletefile", remotePath, configPath);
    }

    private static List<string> BaseCommand(string rclonePath, string command, string firstPath, string? configPath, string? secondPath = null)
    {
        if (string.IsNullOrWhiteSpace(rclonePath)) throw new ArgumentException("rclone path is required.", nameof(rclonePath));
        if (string.IsNullOrWhiteSpace(command)) throw new ArgumentException("command is required.", nameof(command));
        if (string.IsNullOrWhiteSpace(firstPath)) throw new ArgumentException("path is required.", nameof(firstPath));

        var args = new List<string> { rclonePath, command, firstPath };
        if (!string.IsNullOrWhiteSpace(secondPath))
        {
            args.Add(secondPath);
        }
        if (!string.IsNullOrWhiteSpace(configPath))
        {
            args.Add("--config");
            args.Add(configPath);
        }
        return args;
    }
}
