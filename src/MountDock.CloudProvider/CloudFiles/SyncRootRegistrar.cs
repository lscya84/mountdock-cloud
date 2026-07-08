namespace MountDock.CloudProvider.CloudFiles;

public sealed class SyncRootRegistrar
{
    private readonly ICloudFilesApi _cloudFilesApi;

    public SyncRootRegistrar(ICloudFilesApi cloudFilesApi)
    {
        _cloudFilesApi = cloudFilesApi;
    }

    public void Register(string profileId, string displayName, string syncRootPath)
    {
        if (string.IsNullOrWhiteSpace(profileId)) throw new ArgumentException("Profile id is required.", nameof(profileId));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name is required.", nameof(displayName));
        if (string.IsNullOrWhiteSpace(syncRootPath)) throw new ArgumentException("Sync root path is required.", nameof(syncRootPath));

        Directory.CreateDirectory(syncRootPath);
        _cloudFilesApi.RegisterSyncRoot(new SyncRootRegistration(profileId, displayName, syncRootPath));
    }
}
