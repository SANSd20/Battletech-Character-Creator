namespace BattletechCharacterCreator.Core.Resources;

public sealed record EraPreset(
    string Name,
    int StartYear,
    int? EndYear,
    string Source)
{
    public int DefaultYear => StartYear;
    public string DisplayName => EndYear is int endYear
        ? $"{Name} ({StartYear}-{endYear})"
        : $"{Name} ({StartYear})";
}

public static class EraPresetCatalog
{
    public static IReadOnlyList<EraPreset> Presets { get; } =
    [
        new("Age of War", 2398, 2571,
            "BattleTech: Era Digest: Age of War"),
        new("Star League", 2750, null,
            "BattleTech: Era Report: 2750"),
        new("Golden Century", 2830, 2930,
            "BattleTech: Era Digest: Golden Century"),
        new("Clan Invasion", 3052, null,
            "BattleTech: Era Report: 3052"),
        new("Civil War", 3062, null,
            "BATTLETECH ERA REPORT 3062"),
        new("Dark Age", 3128, 3134,
            "Battletech: Era Digest: Dark Age"),
        new("Late Dark Age", 3145, null,
            "BattleTech: Era Report 3145")
    ];
}
