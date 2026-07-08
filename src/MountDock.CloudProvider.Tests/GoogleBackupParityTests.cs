using MountDock.CloudProvider.Backup;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class GoogleBackupParityTests
{
    [Fact]
    public void BackupPlan_UsesEncryptedAppDataFolderPayload()
    {
        var plan = GoogleBackupPlan.CreateDefault("mountdock_rclone_conf_v1.json");

        Assert.Equal("https://www.googleapis.com/auth/drive.appdata", plan.RequiredScope);
        Assert.Equal("mountdock_rclone_conf_v1.json", plan.FileName);
        Assert.True(plan.RequiresPassphrase);
        Assert.False(plan.StoresPlainTextRcloneConfig);
    }

    [Fact]
    public void BackupPlan_RejectsEmptyFileName()
    {
        Assert.Throws<ArgumentException>(() => GoogleBackupPlan.CreateDefault(""));
    }
}
