namespace MountDock.CloudProvider.CloudFiles;

/// <summary>
/// Windows Cloud Files API interop boundary.
/// Real CfAPI calls are intentionally isolated here so Linux CI can still build and test pure logic.
/// </summary>
public interface ICloudFilesApi
{
    bool IsSupported { get; }
    void RegisterSyncRoot(SyncRootRegistration registration);
    void UnregisterSyncRoot(string syncRootId);
    void CreatePlaceholder(PlaceholderDefinition placeholder);
}

public sealed record SyncRootRegistration(
    string SyncRootId,
    string DisplayName,
    string Path);

public sealed record PlaceholderDefinition(
    string RelativePath,
    bool IsDirectory,
    long Size,
    DateTimeOffset? LastWriteTimeUtc,
    byte[] ProviderIdentity);

public sealed class CloudFilesApi : ICloudFilesApi
{
    public bool IsSupported => OperatingSystem.IsWindows();

    public void RegisterSyncRoot(SyncRootRegistration registration)
    {
        ValidateRegistration(registration);
        ThrowIfUnsupported();
        throw new NotImplementedException("Windows CfAPI sync root registration P/Invoke is not implemented yet.");
    }

    public void UnregisterSyncRoot(string syncRootId)
    {
        if (string.IsNullOrWhiteSpace(syncRootId)) throw new ArgumentException("Sync root id is required.", nameof(syncRootId));
        ThrowIfUnsupported();
        throw new NotImplementedException("Windows CfAPI sync root unregistration P/Invoke is not implemented yet.");
    }

    public void CreatePlaceholder(PlaceholderDefinition placeholder)
    {
        ValidatePlaceholder(placeholder);
        ThrowIfUnsupported();
        throw new NotImplementedException("Windows CfAPI placeholder creation P/Invoke is not implemented yet.");
    }

    private void ThrowIfUnsupported()
    {
        if (!IsSupported)
        {
            throw new PlatformNotSupportedException("Windows Cloud Files API is only available on Windows.");
        }
    }

    private static void ValidateRegistration(SyncRootRegistration registration)
    {
        if (string.IsNullOrWhiteSpace(registration.SyncRootId)) throw new ArgumentException("Sync root id is required.", nameof(registration));
        if (string.IsNullOrWhiteSpace(registration.DisplayName)) throw new ArgumentException("Display name is required.", nameof(registration));
        if (string.IsNullOrWhiteSpace(registration.Path)) throw new ArgumentException("Sync root path is required.", nameof(registration));
    }

    private static void ValidatePlaceholder(PlaceholderDefinition placeholder)
    {
        if (string.IsNullOrWhiteSpace(placeholder.RelativePath)) throw new ArgumentException("Placeholder relative path is required.", nameof(placeholder));
        if (Path.IsPathRooted(placeholder.RelativePath)) throw new ArgumentException("Placeholder path must be relative.", nameof(placeholder));
        if (placeholder.Size < 0) throw new ArgumentOutOfRangeException(nameof(placeholder));
    }
}
