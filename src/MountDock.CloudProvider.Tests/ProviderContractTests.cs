using MountDock.CloudProvider.Ipc;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class ProviderContractTests
{
    [Fact]
    public void ProviderRequest_RoundTripsHydratePayload()
    {
        var request = ProviderRequest.Hydrate("profile-1", "item-1", "C:/Temp/item.bin");
        var json = request.ToJson();

        var parsed = ProviderRequest.FromJson(json);

        Assert.Equal("Hydrate", parsed.Method);
        Assert.Equal("profile-1", parsed.Require("profileId"));
        Assert.Equal("item-1", parsed.Require("itemId"));
        Assert.Equal("C:/Temp/item.bin", parsed.Require("targetPath"));
    }

    [Fact]
    public void ProviderResponse_CreatesSuccessAndErrorShapes()
    {
        var success = ProviderResponse.Success("req-1");
        var error = ProviderResponse.Error("req-2", "failed");

        Assert.True(success.Ok);
        Assert.False(error.Ok);
        Assert.Equal("failed", error.ErrorMessage);
    }
}
