using BattletechCharacterCreator.Core.LifePath;

namespace BattletechCharacterCreator.Core.Resources;

public sealed record EraAvailabilityRule(
    string ModuleId,
    int? FromYear,
    int? ToYear,
    string Note,
    string Source)
{
    public bool IsAvailable(int year) =>
        (FromYear is null || year >= FromYear) &&
        (ToYear is null || year <= ToYear);
}

public static class EraAvailabilityCatalog
{
    public static IReadOnlyList<EraAvailabilityRule> AffiliationRules { get; } =
    [
        new("comstar", 2788, null,
            "ComStar appears after the fall of the Star League.",
            "BattleTech: Era Report: 2750"),
        new("word-of-blake", 3052, null,
            "Word of Blake is treated as available from the Clan Invasion era onward.",
            "BattleTech: Era Report: 3052"),
        new("rasalhague", 3034, null,
            "Free Rasalhague Republic choices are hidden before the state forms.",
            "BattleTech: Era Report: 3052"),
        new("invading-clan", 3050, null,
            "Invading Clan choices are hidden before the Clan Invasion.",
            "BattleTech: Era Report: 3052"),
        new("homeworld-clan", 2830, null,
            "Homeworld Clan choices are hidden before the Golden Century context.",
            "BattleTech: Era Digest: Golden Century")
    ];

    public static bool IsAffiliationAvailable(LifePathModule module, int year) =>
        FindAffiliationRule(module.Id)?.IsAvailable(year) ?? true;

    public static int? EarliestAffiliationYear(string moduleId) =>
        FindAffiliationRule(moduleId)?.FromYear;

    public static IReadOnlyList<LifePathModule> FilterAffiliations(
        IEnumerable<LifePathModule> modules,
        int year) =>
        modules.Where(module => IsAffiliationAvailable(module, year)).ToArray();

    public static IReadOnlyList<LifePathModule> HiddenAffiliations(
        IEnumerable<LifePathModule> modules,
        int year) =>
        modules.Where(module => !IsAffiliationAvailable(module, year)).ToArray();

    public static string BuildAffiliationSummary(
        IEnumerable<LifePathModule> modules,
        int year)
    {
        var hidden = HiddenAffiliations(modules, year);
        if (hidden.Count == 0)
        {
            return $"Era availability: all affiliations are visible for {year}.";
        }

        return $"Era availability: {hidden.Count} affiliation option(s) hidden for {year}: " +
            string.Join(", ", hidden.Select(module => module.Name)) + ".";
    }

    public static string BuildModuleNote(LifePathModule module, int year)
    {
        var rule = FindAffiliationRule(module.Id);
        if (rule is null)
        {
            return $"Era note: no additional era availability limit is tracked for {module.Name}.";
        }

        var status = rule.IsAvailable(year) ? "available" : "not available";
        return $"Era note: {module.Name} is {status} in {year}. {rule.Note} Source: {rule.Source}.";
    }

    private static EraAvailabilityRule? FindAffiliationRule(string moduleId) =>
        AffiliationRules.FirstOrDefault(rule =>
            rule.ModuleId.Equals(moduleId, StringComparison.Ordinal));
}
