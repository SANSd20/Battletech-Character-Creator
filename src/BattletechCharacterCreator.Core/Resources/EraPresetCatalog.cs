namespace BattletechCharacterCreator.Core.Resources;

public sealed record EraPreset(
    string Name,
    string BroadEra,
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
        new("Age of War", "Star League", 2005, 2570,
            "Eras.xlsx"),
        new("Star League", "Star League", 2571, 2780,
            "Eras.xlsx"),
        new("Early Succession War", "Succession Wars", 2781, 2900,
            "Eras.xlsx"),
        new("Late Succession War - LosTech", "Succession Wars", 2901, 3019,
            "Eras.xlsx"),
        new("Late Succession War - Renaissance", "Succession Wars", 3020, 3049,
            "Eras.xlsx"),
        new("Clan Invasion", "Clan Invasion", 3050, 3061,
            "Eras.xlsx"),
        new("Civil War", "Civil War", 3062, 3067,
            "Eras.xlsx"),
        new("Jihad", "Jihad", 3068, 3080,
            "Eras.xlsx"),
        new("Republic Age", "Dark Age", 3081, 3130,
            "Eras.xlsx"),
        new("Dark Age", "Dark Age", 3131, 3150,
            "Eras.xlsx"),
        new("IlClan", "ilClan", 3151, null,
            "Eras.xlsx")
    ];

    public static EraPreset? InferEra(int year) =>
        Presets
            .Where(preset => preset.StartYear <= year)
            .LastOrDefault(preset =>
                preset.EndYear is null || year <= preset.EndYear);

    public static string BuildInferredEraLabel(int year) =>
        InferEra(year) is { } era
            ? $"Inferred era: {era.DisplayName}"
            : $"Inferred era: before tracked sources ({year})";
}
