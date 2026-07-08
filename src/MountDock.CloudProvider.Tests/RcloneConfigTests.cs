using MountDock.CloudProvider.Rclone;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class RcloneConfigTests
{
    [Fact]
    public void Parser_ReturnsRemoteNamesAndTypesFromRcloneConf()
    {
        const string content = """
        [gdrive]
        type = drive
        token = should-not-matter

        [webdav-team]
        type = webdav
        url = https://example.invalid
        """;

        var remotes = RcloneConfigParser.Parse(content);

        Assert.Equal(2, remotes.Count);
        Assert.Equal(new RcloneRemote("gdrive", "drive"), remotes[0]);
        Assert.Equal(new RcloneRemote("webdav-team", "webdav"), remotes[1]);
    }

    [Fact]
    public void Parser_IgnoresEmptySectionsAndComments()
    {
        const string content = """
        # comment
        ; another comment

        []
        type = ignored

        [dropbox]
        # comment inside section
        type = dropbox
        """;

        var remotes = RcloneConfigParser.Parse(content);

        var remote = Assert.Single(remotes);
        Assert.Equal("dropbox", remote.Name);
        Assert.Equal("dropbox", remote.Type);
    }

    [Fact]
    public void PathResolver_PrefersManagedThenAppThenAppDataThenHomeConfig()
    {
        var root = Path.Combine(Path.GetTempPath(), $"mountdock-cloud-test-{Guid.NewGuid():N}");
        try
        {
            var appDir = Path.Combine(root, "app");
            var appData = Path.Combine(root, "appdata");
            var home = Path.Combine(root, "home");
            Directory.CreateDirectory(appDir);
            Directory.CreateDirectory(appData);
            Directory.CreateDirectory(home);

            var appConf = Path.Combine(appDir, "rclone.conf");
            File.WriteAllText(appConf, "[app]\ntype = drive\n");
            var appDataConf = Path.Combine(appData, "rclone", "rclone.conf");
            Directory.CreateDirectory(Path.GetDirectoryName(appDataConf)!);
            File.WriteAllText(appDataConf, "[appdata]\ntype = drive\n");

            var resolved = RcloneConfigPaths.FindDefaultConfig(appDir, appData, home);

            Assert.Equal(appConf, resolved);

            var managedConf = Path.Combine(appDir, ".mountdock", "rclone.conf");
            Directory.CreateDirectory(Path.GetDirectoryName(managedConf)!);
            File.WriteAllText(managedConf, "[managed]\ntype = drive\n");

            Assert.Equal(managedConf, RcloneConfigPaths.FindDefaultConfig(appDir, appData, home));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
