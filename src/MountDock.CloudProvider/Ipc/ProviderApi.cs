using System.Text.Json;
using System.Text.Json.Serialization;

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

public sealed record ProviderRequest(
    string RequestId,
    string Method,
    IReadOnlyDictionary<string, string> Parameters)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() },
    };

    public static ProviderRequest Hydrate(string profileId, string itemId, string targetPath) =>
        new(
            Guid.NewGuid().ToString("N"),
            "Hydrate",
            new Dictionary<string, string>
            {
                ["profileId"] = profileId,
                ["itemId"] = itemId,
                ["targetPath"] = targetPath,
            });

    public string Require(string key)
    {
        if (!Parameters.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Provider request parameter is missing: {key}");
        }
        return value;
    }

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    public static ProviderRequest FromJson(string json) =>
        JsonSerializer.Deserialize<ProviderRequest>(json, JsonOptions)
        ?? throw new InvalidOperationException("Provider request JSON is invalid.");
}

public sealed record ProviderResponse(string RequestId, bool Ok, string ErrorMessage)
{
    public static ProviderResponse Success(string requestId) => new(requestId, true, string.Empty);

    public static ProviderResponse Error(string requestId, string message) => new(requestId, false, message);
}
