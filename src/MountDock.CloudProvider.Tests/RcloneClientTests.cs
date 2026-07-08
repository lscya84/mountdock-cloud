using MountDock.CloudProvider.Rclone;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class RcloneClientTests
{
    [Fact]
    public async Task RunAsync_CapturesStdoutStderrAndExitCode()
    {
        if (!File.Exists("/bin/sh")) return;

        var client = new RcloneClient();
        var result = await client.RunAsync(
            new[] { "/bin/sh", "-c", "echo out; echo err >&2; exit 3" },
            CancellationToken.None);

        Assert.Equal(3, result.ExitCode);
        Assert.Contains("out", result.Stdout);
        Assert.Contains("err", result.Stderr);
        Assert.False(result.Success);
    }

    [Fact]
    public void CommandBuilder_BuildsMoveToCommandWithConfig()
    {
        var command = RcloneCommandBuilder.BuildMoveTo("rclone.exe", "gdrive:old.txt", "gdrive:new.txt", "rclone.conf");

        Assert.Equal(new[] { "rclone.exe", "moveto", "gdrive:old.txt", "gdrive:new.txt", "--config", "rclone.conf" }, command);
    }
}
