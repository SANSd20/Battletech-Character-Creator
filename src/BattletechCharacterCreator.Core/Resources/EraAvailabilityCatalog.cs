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

public sealed record EraSubAffiliationAvailabilityRule(
    string ParentModuleId,
    string SubAffiliationName,
    int? FromYear,
    int? ToYear,
    string Note,
    string Source)
{
    public bool IsAvailable(int year) =>
        (FromYear is null || year >= FromYear) &&
        (ToYear is null || year <= ToYear);
}

public sealed record EraCatalogItemAvailabilityRule(
    string ItemName,
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
        new("invading-clan", 2830, null,
            "Invading Clan choices remain available before the Clan Invasion for Clan-origin characters.",
            "BattleTech: Era Digest: Golden Century"),
        new("homeworld-clan", 2830, null,
            "Homeworld Clan choices are hidden before the Golden Century context.",
            "BattleTech: Era Digest: Golden Century")
    ];

    public static IReadOnlyList<EraSubAffiliationAvailabilityRule> SubAffiliationRules { get; } =
    [
        new("rasalhague", "Clan War Expatriate", 3050, null,
            "Clan War expatriate choices are hidden before the Clan Invasion.",
            "BattleTech: Era Report: 3052"),
        new("rasalhague", "Ghost Bear Dominion", 3060, null,
            "Ghost Bear Dominion choices are hidden until the Civil War era context.",
            "BATTLETECH ERA REPORT 3062")
    ];

    public static IReadOnlyList<EraCatalogItemAvailabilityRule> EquipmentRules { get; } =
    [
        new("Cybernetic Eye (IR)", 3062, null,
            "Advanced cybernetics should be checked against the campaign era before purchase.",
            "BATTLETECH ERA REPORT 3062"),
        new("Pain Shunt", 3062, null,
            "Advanced combat implants should be checked against the campaign era before purchase.",
            "BATTLETECH ERA REPORT 3062"),
        new("Flight Wings", 3062, null,
            "Extreme prosthetics should be checked against the campaign era before purchase.",
            "BATTLETECH ERA REPORT 3062"),
        new("Field Simulation Server", 3062, null,
            "Advanced combat practice equipment should be checked against the campaign era before purchase.",
            "BATTLETECH ERA REPORT 3062"),
        new("Hoodling Sensor HoverJeep", 3145, null,
            "Late Dark Age vehicle availability should be checked against the campaign era before purchase.",
            "BattleTech: Era Report 3145")
    ];

    public static IReadOnlyList<EraCatalogItemAvailabilityRule> WeaponRules { get; } =
    [
        new("Shock Staff", 3052, null,
            "Advanced personal weapons should be checked against the campaign era before purchase.",
            "BattleTech: Era Report: 3052"),
        new("Snub-Nose Support PPC", 3052, null,
            "Advanced support weapons should be checked against the campaign era before purchase.",
            "BattleTech: Era Report: 3052")
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

    public static bool IsSubAffiliationAvailable(
        string parentModuleId,
        LifePathModule subAffiliation,
        int year) =>
        FindSubAffiliationRule(parentModuleId, subAffiliation.Name)?.IsAvailable(year) ?? true;

    public static IReadOnlyList<LifePathModule> FilterSubAffiliations(
        string parentModuleId,
        IEnumerable<LifePathModule> modules,
        int year) =>
        modules
            .Where(module => IsSubAffiliationAvailable(parentModuleId, module, year))
            .ToArray();

    public static IReadOnlyList<LifePathModule> HiddenSubAffiliations(
        string parentModuleId,
        IEnumerable<LifePathModule> modules,
        int year) =>
        modules
            .Where(module => !IsSubAffiliationAvailable(parentModuleId, module, year))
            .ToArray();

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

    public static string BuildSubAffiliationSummary(
        LifePathModule parentModule,
        int year)
    {
        var hidden = HiddenSubAffiliations(
            parentModule.Id,
            parentModule.SubAffiliations ?? [],
            year);
        if (hidden.Count == 0)
        {
            return $"All tracked sub-affiliations are visible for {year}.";
        }

        return $"{hidden.Count} sub-affiliation option(s) hidden for {year}: " +
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

    public static string BuildSubAffiliationNote(
        string parentModuleId,
        LifePathModule subAffiliation,
        int year)
    {
        var rule = FindSubAffiliationRule(parentModuleId, subAffiliation.Name);
        if (rule is null)
        {
            return $"Era note: no additional era availability limit is tracked for {subAffiliation.Name}.";
        }

        var status = rule.IsAvailable(year) ? "available" : "not available";
        return $"Era note: {subAffiliation.Name} is {status} in {year}. " +
            $"{rule.Note} Source: {rule.Source}.";
    }

    public static string BuildEquipmentAvailabilityNote(
        EquipmentCatalogItem item,
        int year) =>
        BuildCatalogItemAvailabilityNote(
            item.Name,
            year,
            FindEquipmentRule(item.Name));

    public static string BuildWeaponAvailabilityNote(
        WeaponCatalogItem item,
        int year) =>
        BuildCatalogItemAvailabilityNote(
            item.Name,
            year,
            FindWeaponRule(item.Name));

    private static string BuildCatalogItemAvailabilityNote(
        string itemName,
        int year,
        EraCatalogItemAvailabilityRule? rule)
    {
        if (rule is null)
        {
            return $"Era availability: no additional era limit is tracked for {itemName}.";
        }

        var status = rule.IsAvailable(year) ? "available" : "check availability";
        return $"Era availability: {itemName} is {status} in {year}. " +
            $"{rule.Note} Source: {rule.Source}.";
    }

    private static EraAvailabilityRule? FindAffiliationRule(string moduleId) =>
        AffiliationRules.FirstOrDefault(rule =>
            rule.ModuleId.Equals(moduleId, StringComparison.Ordinal));

    private static EraSubAffiliationAvailabilityRule? FindSubAffiliationRule(
        string parentModuleId,
        string subAffiliationName) =>
        SubAffiliationRules.FirstOrDefault(rule =>
            rule.ParentModuleId.Equals(parentModuleId, StringComparison.Ordinal) &&
            rule.SubAffiliationName.Equals(subAffiliationName, StringComparison.Ordinal));

    private static EraCatalogItemAvailabilityRule? FindEquipmentRule(string itemName) =>
        EquipmentRules.FirstOrDefault(rule =>
            rule.ItemName.Equals(itemName, StringComparison.Ordinal));

    private static EraCatalogItemAvailabilityRule? FindWeaponRule(string itemName) =>
        WeaponRules.FirstOrDefault(rule =>
            rule.ItemName.Equals(itemName, StringComparison.Ordinal));
}
