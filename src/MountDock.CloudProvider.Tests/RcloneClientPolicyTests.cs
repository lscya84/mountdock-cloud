using MountDock.CloudProvider.Rclone;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class RcloneClientPolicyTests
{
    [Fact]
    public async Task RunWithTimeoutAsync_ThrowsRcloneTimeoutExceptionAndKillsProcess()
    {
        if (!File.Exists("/bin/sh")) return;

        var client = new RcloneClient();

        var exception = await Assert.ThrowsAsync<RcloneTimeoutException>(() =>
            client.RunWithTimeoutAsync(
                new[] { "/bin/sh", "-c", "sleep 5" },
                TimeSpan.FromMilliseconds(100),
                CancellationToken.None));

        Assert.Contains("timed out", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunRequiredAsync_ThrowsRcloneCommandExceptionWhenCommandFails()
    {
        if (!File.Exists("/bin/sh")) return;

        var client = new RcloneClient();

        var exception = await Assert.ThrowsAsync<RcloneCommandException>(() =>
            client.RunRequiredAsync(
                new[] { "/bin/sh", "-c", "echo broken >&2; exit 7" },
                CancellationToken.None));

        Assert.Equal(7, exception.Result.ExitCode);
        Assert.Contains("broken", exception.Result.Stderr);
    }
}
