using MountDock.CloudProvider.CloudFiles;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class DriveLetterValidationPlanTests
{
    [Fact]
    public void BuildSubstMapCommand_NormalizesDriveLetter()
    {
        var command = DriveLetterValidationPlan.BuildSubstMapCommand("x:", "C:/Users/me/MountDockCloudSpike");

        Assert.Equal(new[] { "subst", "X:", "C:/Users/me/MountDockCloudSpike" }, command);
    }

    [Fact]
    public void BuildSubstUnmapCommand_BuildsDeleteMappingCommand()
    {
        var command = DriveLetterValidationPlan.BuildSubstUnmapCommand("x");

        Assert.Equal(new[] { "subst", "X:", "/D" }, command);
    }
}
