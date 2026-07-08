using MountDock.CloudProvider.CloudFiles;
using MountDock.CloudProvider.Rclone;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class RcloneMetadataTests
{
    [Fact]
    public void Parser_ParsesLsJsonFilesAndDirectories()
    {
        const string json = """
        [
          {"Path":"Docs","Name":"Docs","Size":-1,"MimeType":"inode/directory","ModTime":"2026-07-08T01:02:03Z","IsDir":true},
          {"Path":"Docs/report.xlsx","Name":"report.xlsx","Size":42,"MimeType":"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","ModTime":"2026-07-08T02:03:04Z","IsDir":false,"Hashes":{"MD5":"abc"}}
        ]
        """;

        var items = RcloneLsJsonParser.Parse(json);

        Assert.Equal(2, items.Count);
        Assert.True(items[0].IsDirectory);
        Assert.False(items[1].IsDirectory);
        Assert.Equal("Docs/report.xlsx", items[1].Path);
        Assert.Equal(42, items[1].Size);
        Assert.Equal("abc", items[1].Hashes["MD5"]);
    }

    [Fact]
    public void PlaceholderPlanner_ConvertsRemoteItemsToPlaceholders()
    {
        var items = new[]
        {
            new RcloneRemoteItem("Docs", "Docs", true, 0, DateTimeOffset.Parse("2026-07-08T01:02:03Z"), new Dictionary<string, string>()),
            new RcloneRemoteItem("Docs/report.xlsx", "report.xlsx", false, 42, DateTimeOffset.Parse("2026-07-08T02:03:04Z"), new Dictionary<string, string> { ["MD5"] = "abc" }),
        };

        var placeholders = PlaceholderPlanner.FromRemoteItems(items);

        Assert.Equal(2, placeholders.Count);
        Assert.Equal("Docs", placeholders[0].RelativePath);
        Assert.True(placeholders[0].IsDirectory);
        Assert.Equal("Docs/report.xlsx", placeholders[1].RelativePath);
        Assert.False(placeholders[1].IsDirectory);
        Assert.Equal(42, placeholders[1].Size);
        Assert.NotEmpty(placeholders[1].ProviderIdentity);
    }
}
