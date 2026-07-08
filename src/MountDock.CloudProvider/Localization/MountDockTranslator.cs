namespace MountDock.CloudProvider.Localization;

public sealed class MountDockTranslator
{
    private readonly string _language;
    private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _strings;

    public MountDockTranslator(string language, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> strings)
    {
        _language = string.IsNullOrWhiteSpace(language) ? "en" : language;
        _strings = strings;
    }

    public static MountDockTranslator CreateDefault(string language) => new(language, DefaultStrings.All);

    public string Get(string key, IReadOnlyDictionary<string, string>? values = null)
    {
        var template = FindTemplate(key);
        if (values is null || values.Count == 0)
        {
            return template;
        }

        var result = template;
        foreach (var (name, value) in values)
        {
            result = result.Replace("{" + name + "}", value, StringComparison.Ordinal);
        }
        return result;
    }

    private string FindTemplate(string key)
    {
        if (_strings.TryGetValue(_language, out var lang) && lang.TryGetValue(key, out var localized))
        {
            return localized;
        }
        if (_strings.TryGetValue("en", out var english) && english.TryGetValue(key, out var fallback))
        {
            return fallback;
        }
        return key;
    }
}

public static class DefaultStrings
{
    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> All { get; } =
        new Dictionary<string, IReadOnlyDictionary<string, string>>
        {
            ["en"] = new Dictionary<string, string>
            {
                ["app_title"] = "MountDock Cloud",
                ["connect"] = "Connect",
                ["disconnect"] = "Disconnect",
                ["sync_root"] = "Cloud Files sync root",
                ["loaded_remotes"] = "Loaded {count} remote(s).",
                ["syncing"] = "Syncing",
                ["conflict"] = "Conflict",
                ["error"] = "Error",
            },
            ["ko"] = new Dictionary<string, string>
            {
                ["app_title"] = "MountDock Cloud",
                ["connect"] = "연결",
                ["disconnect"] = "해제",
                ["loaded_remotes"] = "원격 {count}개를 불러왔습니다.",
                ["syncing"] = "동기화 중",
                ["conflict"] = "충돌",
                ["error"] = "오류",
            },
        };
}
