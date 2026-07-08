namespace MountDock.CloudProvider.CloudFiles;

public static class DriveLetterValidationPlan
{
    public static IReadOnlyList<string> BuildSubstMapCommand(string driveLetter, string syncRootPath)
    {
        var letter = NormalizeDriveLetter(driveLetter);
        if (string.IsNullOrWhiteSpace(syncRootPath)) throw new ArgumentException("Sync root path is required.", nameof(syncRootPath));
        return new[] { "subst", $"{letter}:", syncRootPath };
    }

    public static IReadOnlyList<string> BuildSubstUnmapCommand(string driveLetter)
    {
        var letter = NormalizeDriveLetter(driveLetter);
        return new[] { "subst", $"{letter}:", "/D" };
    }

    private static string NormalizeDriveLetter(string driveLetter)
    {
        var normalized = (driveLetter ?? string.Empty).Replace(":", string.Empty).Trim().ToUpperInvariant();
        if (normalized.Length != 1 || normalized[0] < 'A' || normalized[0] > 'Z')
        {
            throw new ArgumentException("Drive letter must be a single A-Z letter.", nameof(driveLetter));
        }
        return normalized;
    }
}
