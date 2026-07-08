using MountDock.CloudProvider.CloudFiles;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class CloudFilesApiTests
{
    [Fact]
    public void IsSupported_ReturnsFalseOnNonWindowsHosts()
    {
        var api = new CloudFilesApi();

        if (!OperatingSystem.IsWindows())
        {
            Assert.False(api.IsSupported);
        }
    }

    [Fact]
    public void RegisterSyncRoot_ThrowsPlatformNotSupportedOnNonWindows()
    {
        if (OperatingSystem.IsWindows()) return;

        var api = new CloudFilesApi();

        Assert.Throws<PlatformNotSupportedException>(() =>
            api.RegisterSyncRoot(new SyncRootRegistration("id", "표시 이름", Path.Combine(Path.GetTempPath(), "sync-root"))));
    }

    [Fact]
    public void CreatePlaceholder_ValidatesRelativePath()
    {
        var api = new CloudFilesApi();

        Assert.Throws<ArgumentException>(() =>
            api.CreatePlaceholder(new PlaceholderDefinition("", IsDirectory: false, Size: 10, LastWriteTimeUtc: null, ProviderIdentity: Array.Empty<byte>())));
    }
}
