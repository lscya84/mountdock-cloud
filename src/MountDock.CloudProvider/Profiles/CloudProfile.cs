namespace MountDock.CloudProvider.Profiles;

public enum CloudProfileBackend
{
    CloudFiles,
    LocalMirror,
    LiveMount,
    ExplorerSafeLiveMount,
}

public sealed record CloudProfile
{
    public CloudProfile(
        string ProfileId,
        string DisplayName,
        CloudProfileBackend Backend,
        string Remote,
        string RootFolder,
        string SyncRootPath,
        string Icon = "auto",
        bool AutoConnect = false,
        bool OnlineOnlyByDefault = true,
        bool AllowPinOffline = true,
        bool AllowDehydrate = true,
        int RemotePollIntervalSeconds = 300,
        string ConflictPolicy = "keep_both",
        string LegacyDriveLetter = "",
        IReadOnlyList<string>? AdvancedRcloneGlobalArgs = null,
        IReadOnlyList<string>? AdvancedRcloneListArgs = null,
        IReadOnlyList<string>? AdvancedRcloneTransferArgs = null)
    {
        this.ProfileId = Require(ProfileId, nameof(ProfileId));
        this.DisplayName = Require(DisplayName, nameof(DisplayName));
        this.Backend = Backend;
        this.Remote = Require(Remote, nameof(Remote));
        this.RootFolder = NormalizeRootFolder(RootFolder);
        this.SyncRootPath = Require(SyncRootPath, nameof(SyncRootPath));
        this.Icon = string.IsNullOrWhiteSpace(Icon) ? "auto" : Icon;
        this.AutoConnect = AutoConnect;
        this.OnlineOnlyByDefault = OnlineOnlyByDefault;
        this.AllowPinOffline = AllowPinOffline;
        this.AllowDehydrate = AllowDehydrate;
        this.RemotePollIntervalSeconds = RemotePollIntervalSeconds;
        this.ConflictPolicy = string.IsNullOrWhiteSpace(ConflictPolicy) ? "keep_both" : ConflictPolicy;
        this.LegacyDriveLetter = LegacyDriveLetter.Replace(":", string.Empty).Trim().ToUpperInvariant();
        this.AdvancedRcloneGlobalArgs = AdvancedRcloneGlobalArgs ?? Array.Empty<string>();
        this.AdvancedRcloneListArgs = AdvancedRcloneListArgs ?? Array.Empty<string>();
        this.AdvancedRcloneTransferArgs = AdvancedRcloneTransferArgs ?? Array.Empty<string>();
    }

    public string ProfileId { get; }
    public string DisplayName { get; }
    public CloudProfileBackend Backend { get; }
    public string Remote { get; }
    public string RootFolder { get; }
    public string SyncRootPath { get; }
    public string Icon { get; }
    public bool AutoConnect { get; }
    public bool OnlineOnlyByDefault { get; }
    public bool AllowPinOffline { get; }
    public bool AllowDehydrate { get; }
    public int RemotePollIntervalSeconds { get; }
    public string ConflictPolicy { get; }
    public string LegacyDriveLetter { get; }
    public IReadOnlyList<string> AdvancedRcloneGlobalArgs { get; }
    public IReadOnlyList<string> AdvancedRcloneListArgs { get; }
    public IReadOnlyList<string> AdvancedRcloneTransferArgs { get; }

    private static string Require(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{name} is required.", name);
        return value.Trim();
    }

    private static string NormalizeRootFolder(string value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "/" : value.Trim().Replace('\\', '/');
        return normalized.StartsWith('/') ? normalized : "/" + normalized;
    }
}

public sealed class LegacyMountDockProfile
{
    public string Id { get; init; } = "";
    public string Remote { get; init; } = "";
    public string RootFolder { get; init; } = "/";
    public string Letter { get; init; } = "";
    public string VolName { get; init; } = "";
    public string Icon { get; init; } = "auto";
    public bool AutoMount { get; init; }
    public string CacheDir { get; init; } = "";
    public string CustomArgs { get; init; } = "";
    public string ExtraFlags { get; init; } = "";
    public string VfsMode { get; init; } = "";
}

public static class CloudProfileImporter
{
    public static CloudProfile FromLegacy(LegacyMountDockProfile legacy, string syncRootBasePath)
    {
        if (legacy is null) throw new ArgumentNullException(nameof(legacy));
        if (string.IsNullOrWhiteSpace(syncRootBasePath)) throw new ArgumentException("Sync root base path is required.", nameof(syncRootBasePath));

        var profileId = string.IsNullOrWhiteSpace(legacy.Id) ? Guid.NewGuid().ToString("N") : legacy.Id.Trim();
        var displayName = !string.IsNullOrWhiteSpace(legacy.VolName) ? legacy.VolName.Trim() : legacy.Remote.Trim();
        var syncRootPath = Path.Combine(syncRootBasePath, SanitizePathSegment(displayName));

        return new CloudProfile(
            ProfileId: profileId,
            DisplayName: displayName,
            Backend: CloudProfileBackend.CloudFiles,
            Remote: legacy.Remote,
            RootFolder: legacy.RootFolder,
            SyncRootPath: syncRootPath,
            Icon: legacy.Icon,
            AutoConnect: legacy.AutoMount,
            OnlineOnlyByDefault: true,
            AllowPinOffline: true,
            AllowDehydrate: true,
            RemotePollIntervalSeconds: 300,
            ConflictPolicy: "keep_both",
            LegacyDriveLetter: legacy.Letter);
    }

    private static string SanitizePathSegment(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "MountDock Cloud Profile" : cleaned;
    }
}
