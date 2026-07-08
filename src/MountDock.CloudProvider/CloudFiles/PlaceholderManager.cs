namespace MountDock.CloudProvider.CloudFiles;

public sealed class PlaceholderManager
{
    private readonly ICloudFilesApi _cloudFilesApi;

    public PlaceholderManager(ICloudFilesApi cloudFilesApi)
    {
        _cloudFilesApi = cloudFilesApi;
    }

    public void CreateFilePlaceholder(string relativePath, long size, byte[] providerIdentity, DateTimeOffset? lastWriteTimeUtc = null)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) throw new ArgumentException("Relative path is required.", nameof(relativePath));
        if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));

        _cloudFilesApi.CreatePlaceholder(new PlaceholderDefinition(
            relativePath,
            IsDirectory: false,
            Size: size,
            LastWriteTimeUtc: lastWriteTimeUtc,
            ProviderIdentity: providerIdentity));
    }
}
