using System.Text.Json;
using MountDock.CloudProvider.Rclone;
using MountDock.CloudProvider.State;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class HydrationPlannerTests
{
    [Fact]
    public void BuildHydrationOperation_CreatesDownloadOperationForFile()
    {
        var item = new SyncItem(
            ItemId: "Docs/report.xlsx",
            ParentId: "Docs",
            Path: "Docs/report.xlsx",
            Name: "report.xlsx",
            IsDirectory: false,
            Size: 42,
            LocalState: "online_only",
            SyncState: "clean",
            PinState: "unpinned");

        var operation = HydrationPlanner.BuildHydrationOperation(item, "gdrive:", "C:/Users/me/MountDock/Docs/report.xlsx");

        Assert.Equal("hydrate", operation.Kind);
        Assert.Equal("Docs/report.xlsx", operation.ItemId);
        Assert.Equal("Docs/report.xlsx", operation.Path);
        Assert.Equal(10, operation.Priority);

        using var payload = JsonDocument.Parse(operation.PayloadJson);
        Assert.Equal("gdrive:Docs/report.xlsx", payload.RootElement.GetProperty("remotePath").GetString());
        Assert.Equal("C:/Users/me/MountDock/Docs/report.xlsx", payload.RootElement.GetProperty("localPath").GetString());
    }

    [Fact]
    public void BuildHydrationCommand_CreatesRcloneCopyToCommand()
    {
        var operation = new SyncOperationDraft(
            Kind: "hydrate",
            ItemId: "Docs/report.xlsx",
            Path: "Docs/report.xlsx",
            Priority: 10,
            PayloadJson: "{\"remotePath\":\"gdrive:Docs/report.xlsx\",\"localPath\":\"C:/Temp/report.xlsx\"}");

        var command = HydrationPlanner.BuildHydrationCommand("rclone.exe", operation, "rclone.conf");

        Assert.Equal(new[] { "rclone.exe", "copyto", "gdrive:Docs/report.xlsx", "C:/Temp/report.xlsx", "--config", "rclone.conf" }, command);
    }

    [Fact]
    public void BuildHydrationOperation_RejectsDirectories()
    {
        var item = new SyncItem("Docs", "", "Docs", "Docs", true, 0, "online_only", "clean", "unpinned");

        Assert.Throws<ArgumentException>(() => HydrationPlanner.BuildHydrationOperation(item, "gdrive:", "C:/Temp/Docs"));
    }
}
