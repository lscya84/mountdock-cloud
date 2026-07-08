namespace MountDock.CloudProvider.Ipc;

public interface IProviderApi
{
    Task RegisterProfileAsync(CloudProfile profile, CancellationToken cancellationToken);
    Task HydrateAsync(string profileId, string itemId, CancellationToken cancellationToken);
    Task DehydrateAsync(string profileId, string itemId, CancellationToken cancellationToken);
}

public sealed record CloudProfile(
    string ProfileId,
    string DisplayName,
    string SyncRootPath,
    string RemoteName,
    string RootFolder);
