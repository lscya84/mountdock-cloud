namespace MountDock.CloudProvider.CloudFiles;

public sealed class HydrationManager
{
    public Task HydrateAsync(string itemId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(itemId)) throw new ArgumentException("Item id is required.", nameof(itemId));
        throw new NotImplementedException("Hydration callback wiring belongs to the CfAPI spike.");
    }
}
