using MountDock.CloudProvider.Localization;
using Xunit;

namespace MountDock.CloudProvider.Tests;

public sealed class LocalizationTests
{
    [Fact]
    public void Translator_ReturnsKoreanStringWhenAvailable()
    {
        var translator = MountDockTranslator.CreateDefault("ko");

        Assert.Equal("MountDock Cloud", translator.Get("app_title"));
        Assert.Equal("연결", translator.Get("connect"));
    }

    [Fact]
    public void Translator_FallsBackToEnglishThenKey()
    {
        var translator = MountDockTranslator.CreateDefault("ko");

        Assert.Equal("Cloud Files sync root", translator.Get("sync_root"));
        Assert.Equal("missing_key", translator.Get("missing_key"));
    }

    [Fact]
    public void Translator_FormatsValues()
    {
        var translator = MountDockTranslator.CreateDefault("ko");

        Assert.Equal("원격 3개를 불러왔습니다.", translator.Get("loaded_remotes", new Dictionary<string, string> { ["count"] = "3" }));
    }
}
