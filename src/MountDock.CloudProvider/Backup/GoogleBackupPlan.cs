namespace MountDock.CloudProvider.Backup;

public sealed record GoogleBackupPlan(
    string FileName,
    string RequiredScope,
    bool RequiresPassphrase,
    bool StoresPlainTextRcloneConfig)
{
    public static GoogleBackupPlan CreateDefault(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Backup file name is required.", nameof(fileName));
        return new GoogleBackupPlan(
            FileName: fileName.Trim(),
            RequiredScope: "https://www.googleapis.com/auth/drive.appdata",
            RequiresPassphrase: true,
            StoresPlainTextRcloneConfig: false);
    }
}

public interface IEncryptedRcloneConfigBackupService
{
    Task BackupAsync(string rcloneConfigPath, string passphrase, CancellationToken cancellationToken);
    Task RestoreAsync(string targetRcloneConfigPath, string passphrase, CancellationToken cancellationToken);
}
