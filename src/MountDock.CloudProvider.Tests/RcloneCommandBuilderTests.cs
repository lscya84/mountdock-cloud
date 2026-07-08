using MountDock.CloudProvider.Rclone;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class RcloneCommandBuilderTests
{
    [Fact]
    public void LsJson_IncludesRecursiveAndConfigWhenRequested()
    {
        var command = RcloneCommandBuilder.BuildLsJson("rclone.exe", "gdrive:Docs", recursive: true, configPath: "rclone.conf");

        Assert.Equal(new[] { "rclone.exe", "lsjson", "gdrive:Docs", "--recursive", "--config", "rclone.conf" }, command);
    }

    [Fact]
    public void CopyTo_UsesArgumentListShape()
    {
        var command = RcloneCommandBuilder.BuildCopyTo("rclone.exe", "gdrive:Docs/a.txt", "C:/Temp/a.txt", configPath: null);

        Assert.Equal(new[] { "rclone.exe", "copyto", "gdrive:Docs/a.txt", "C:/Temp/a.txt" }, command);
    }

    [Fact]
    public void DeleteFile_BuildsDeleteFileCommand()
    {
        var command = RcloneCommandBuilder.BuildDeleteFile("rclone.exe", "gdrive:Docs/a.txt", "rclone.conf");

        Assert.Equal(new[] { "rclone.exe", "deletefile", "gdrive:Docs/a.txt", "--config", "rclone.conf" }, command);
    }
}
