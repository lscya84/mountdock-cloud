namespace MountDock.CloudProvider.CloudFiles;

/// <summary>
/// Boundary for Windows Cloud Files API interop.
/// The spike should replace these stubs with real CfAPI P/Invoke calls.
/// </summary>
public interface ICloudFilesApi
{
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
    public void RegisterSyncRoot(SyncRootRegistration registration) =>
        throw new NotImplementedException("CfAPI sync root registration spike is not implemented yet.");

    public void UnregisterSyncRoot(string syncRootId) =>
        throw new NotImplementedException("CfAPI sync root unregistration spike is not implemented yet.");

    public void CreatePlaceholder(PlaceholderDefinition placeholder) =>
        throw new NotImplementedException("CfAPI placeholder creation spike is not implemented yet.");
}
