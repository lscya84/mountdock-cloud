using MountDock.CloudProvider.Profiles;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class CloudProfileTests
{
    [Fact]
    public void LegacyImport_PreservesCoreFieldsAndMapsAutoMount()
    {
        var legacy = new LegacyMountDockProfile
        {
            Id = "legacy-id",
            Remote = "gdrive",
            RootFolder = "/CompanyDocs",
            Letter = "X",
            VolName = "Company Docs",
            Icon = "google_drive",
            AutoMount = true,
            CacheDir = "C:/MountDockCache",
            CustomArgs = "--network-mode --dir-cache-time 1h",
            VfsMode = "full",
        };

        var cloud = CloudProfileImporter.FromLegacy(legacy, "%USERPROFILE%/MountDock");

        Assert.Equal("legacy-id", cloud.ProfileId);
        Assert.Equal("Company Docs", cloud.DisplayName);
        Assert.Equal("gdrive", cloud.Remote);
        Assert.Equal("/CompanyDocs", cloud.RootFolder);
        Assert.Equal("X", cloud.LegacyDriveLetter);
        Assert.Equal("google_drive", cloud.Icon);
        Assert.True(cloud.AutoConnect);
        Assert.True(cloud.OnlineOnlyByDefault);
        Assert.True(cloud.AllowPinOffline);
        Assert.True(cloud.AllowDehydrate);
        Assert.Equal("keep_both", cloud.ConflictPolicy);
        Assert.Empty(cloud.AdvancedRcloneGlobalArgs);
        Assert.Empty(cloud.AdvancedRcloneListArgs);
        Assert.Empty(cloud.AdvancedRcloneTransferArgs);
    }

    [Fact]
    public void LegacyImport_UsesRemoteAsDisplayNameWhenVolNameIsEmpty()
    {
        var legacy = new LegacyMountDockProfile
        {
            Id = "legacy-id",
            Remote = "webdav-team",
            RootFolder = "/",
        };

        var cloud = CloudProfileImporter.FromLegacy(legacy, "C:/Users/me/MountDock");

        Assert.Equal("webdav-team", cloud.DisplayName);
        Assert.Contains("webdav-team", cloud.SyncRootPath);
    }

    [Fact]
    public void CloudProfile_NormalizesEmptyRootFolderToSlash()
    {
        var profile = new CloudProfile(
            ProfileId: "id",
            DisplayName: "Docs",
            Backend: CloudProfileBackend.CloudFiles,
            Remote: "gdrive",
            RootFolder: "",
            SyncRootPath: "C:/Users/me/MountDock/Docs");

        Assert.Equal("/", profile.RootFolder);
    }
}
