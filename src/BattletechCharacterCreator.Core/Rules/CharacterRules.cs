using BattletechCharacterCreator.Core.Models;

namespace BattletechCharacterCreator.Core.Rules;

public static class CharacterRules
{
    private static readonly int[] SkillThresholds = [30, 50, 80, 120, 170, 230, 300, 380, 470, 570];

    private static readonly Dictionary<string, (int Min, int Max)> TraitLimits = new(StringComparer.Ordinal)
    {
        ["Alternate ID"] = (0, 2),
        ["Animal Antipathy"] = (-1, 0),
        ["Animal Empathy"] = (0, 1),
        ["Attractive"] = (0, 2),
        ["Bloodmark"] = (-5, 0),
        ["Combat Paralysis"] = (-4, 0),
        ["Combat Sense"] = (0, 4),
        ["Connections"] = (1, 10),
        ["Custom Vehicle"] = (0, 6),
        ["Dark Secret"] = (-5, 0),
        ["Dependent"] = (-2, 0),
        ["Design Quirk"] = (-5, 5),
        ["Enemy"] = (-10, 0),
        ["Equipped"] = (-1, 8),
        ["Extra Income"] = (-10, 10),
        ["Fast Learner"] = (0, 3),
        ["Fit"] = (0, 2),
        ["G-Tolerance"] = (0, 1),
        ["Glass Jaw"] = (-3, 0),
        ["Good Hearing"] = (0, 1),
        ["Good Vision"] = (0, 1),
        ["Gregarious"] = (0, 1),
        ["Gremlins"] = (-3, 0),
        ["Handicap"] = (-5, -1),
        ["Illiterate"] = (-1, 0),
        ["Impatient"] = (-1, 0),
        ["Implant/Prosthetic"] = (0, 6),
        ["In For Life"] = (-3, 0),
        ["Introvert"] = (-1, 0),
        ["Lost Limb"] = (-5, -1),
        ["Mutation"] = (-5, 3),
        ["Pain Resistance"] = (0, 3),
        ["Patient"] = (0, 1),
        ["Phenotype"] = (0, 0),
        ["Poison Resistance"] = (0, 2),
        ["Poor Hearing"] = (-5, -1),
        ["Poor Vision"] = (-9, -2),
        ["Property"] = (0, 10),
        ["Rank"] = (0, 15),
        ["Reputation"] = (-5, 5),
        ["Sixth Sense"] = (0, 4),
        ["Slow Learner"] = (-3, 0),
        ["Tech Empathy"] = (0, 3),
        ["Thick-Skinned"] = (0, 1),
        ["Thin-Skinned"] = (-1, 0),
        ["TDS"] = (-1, 0),
        ["Unattractive"] = (-1, 0),
        ["Unlucky"] = (-10, -2),
        ["Vehicle"] = (0, 12),
        ["Wealth"] = (-1, 10)
    };

    public static CharacterSummary Calculate(Character character, int startingXp = 5000)
    {
        var attributeXp = character.Attributes.Sum(item => item.Value);
        var skillXp = character.Skills.Sum(item => item.Value);
        var traitXp = character.Traits.Sum(item => item.Value);
        var allocatedXp = attributeXp + skillXp + traitXp;
        var spentXp = allocatedXp + character.GmXpModifier;

        var strength = FindValue(character.Attributes, "STR");
        var reflexes = FindValue(character.Attributes, "RFL");
        var walk = AttributeValue(strength) + AttributeValue(reflexes);
        var run = 10 + walk + SkillLevel(FindValue(character.Skills, "Running"), character.Traits);
        var climb = DivideRoundUp(walk, 2) +
            SkillLevel(FindValue(character.Skills, "Climbing"), character.Traits);
        var crawl = DivideRoundUp(walk, 4);
        var swim = walk + SkillLevel(FindValue(character.Skills, "Swimming"), character.Traits);

        var wealthXp = FindValue(character.Traits, "Wealth");
        var wealthLevel = TraitLevel("Wealth", wealthXp);
        var startingCBills = StartingCBills(wealthLevel) + character.CBillModifier;
        var inventoryCost = character.Equipment.Sum(item =>
                BasePurchaseCost(item.Cost) * ItemCount(item.Count) +
                SecondaryPurchaseCost(item.Cost) * OptionalItemCount(item.PatchCount)) +
            character.Weapons.Sum(item =>
                BasePurchaseCost(item.Cost) * ItemCount(item.Count) +
                (BasePurchaseCost(item.AmmoCost) +
                 BasePurchaseCost(item.AmmoCostModifier)) *
                OptionalItemCount(item.AmmoCount));
        var inventoryMass = character.Equipment.Sum(item =>
                CatalogMass(item.Mass) * ItemCount(item.Count)) +
            character.Weapons.Sum(item =>
                CatalogMass(item.Mass) * ItemCount(item.Count) +
                (CatalogMass(item.AmmoMass) +
                 CatalogMass(item.AmmoMassModifier)) *
                OptionalItemCount(item.AmmoCount));
        var unresolvedInventoryPrices = character.Equipment.Sum(item =>
                (HasUnresolvedPurchaseCost(item.Cost) ? ItemCount(item.Count) : 0) +
                (HasUnresolvedPatchCost(item.Cost)
                    ? OptionalItemCount(item.PatchCount)
                    : 0)) +
            character.Weapons.Sum(item =>
                (HasUnresolvedPurchaseCost(item.Cost) ? ItemCount(item.Count) : 0) +
                (HasUnresolvedPurchaseCost(item.AmmoCost)
                    ? OptionalItemCount(item.AmmoCount)
                    : 0) +
                (HasUnresolvedPurchaseCost(item.AmmoCostModifier)
                    ? OptionalItemCount(item.AmmoCount)
                    : 0));
        var capacity = CarryingCapacity(strength);

        return new CharacterSummary(
            attributeXp,
            skillXp,
            traitXp,
            spentXp,
            startingXp - spentXp,
            wealthLevel,
            inventoryCost,
            startingCBills - inventoryCost,
            unresolvedInventoryPrices,
            capacity,
            inventoryMass,
            capacity - inventoryMass,
            walk,
            run,
            run * 2,
            climb,
            crawl,
            swim);
    }

    public static int AttributeValue(int xp) => xp < 100 ? 1 : xp / 100;

    public static int LinkModifier(int attributeValue) => attributeValue switch
    {
        <= 1 => -2,
        <= 3 => -1,
        <= 6 => 0,
        <= 9 => 1,
        _ => 2
    };

    public static decimal CarryingCapacity(int strengthXp) => strengthXp switch
    {
        < 100 => 0.1m,
        < 200 => 5m,
        < 300 => 10m,
        < 400 => 15m,
        < 500 => 20m,
        < 600 => 30m,
        < 700 => 40m,
        < 800 => 55m,
        < 900 => 70m,
        < 1000 => 85m,
        _ => 100m
    };

    public static int SkillLevel(int xp, IEnumerable<NamedValue> traits)
    {
        var multiplier = 1.0;
        if (FindValue(traits, "Fast Learner") >= 300) multiplier -= 0.2;
        if (FindValue(traits, "Slow Learner") <= -300) multiplier += 0.2;

        var level = 0;
        foreach (var threshold in SkillThresholds)
        {
            if (xp < Math.Floor(threshold * multiplier)) break;
            level++;
        }
        return level;
    }

    public static int TraitLevel(string name, int xp)
    {
        var level = (int)Math.Floor(xp / 100m);
        if (name.StartsWith("Citizenship", StringComparison.Ordinal)) return Math.Clamp(level, 0, 2);
        if (name.StartsWith("Compulsion", StringComparison.Ordinal)) return Math.Clamp(level, -5, 0);
        if (name.StartsWith("Exceptional Attribute", StringComparison.Ordinal)) return Math.Clamp(level, 0, 2);
        if (name.StartsWith("Title", StringComparison.Ordinal)) return Math.Clamp(level, 3, 10);
        if (name.StartsWith("Natural Aptitude", StringComparison.Ordinal))
        {
            return level >= 5 ? 5 : level >= 3 ? 3 : 0;
        }
        if (name == "Design Quirk/Rumble Seat") return 0;
        return TraitLimits.TryGetValue(name, out var limits)
            ? Math.Clamp(level, limits.Min, limits.Max)
            : level;
    }

    public static int StartingCBills(int wealthLevel) => wealthLevel switch
    {
        <= -1 => 100,
        0 => 1_000,
        1 => 2_500,
        2 => 5_000,
        3 => 10_000,
        4 => 25_000,
        5 => 50_000,
        6 => 100_000,
        7 => 250_000,
        8 => 500_000,
        9 => 1_000_000,
        _ => 2_000_000
    };

    public static int BasePurchaseCost(string value)
    {
        var baseValue = CostPart(value, 0)
            .Replace(",", "", StringComparison.Ordinal)
            .Replace("*", "", StringComparison.Ordinal)
            .Trim();
        return int.TryParse(baseValue, System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
    }

    public static int SecondaryPurchaseCost(string value)
    {
        var secondaryValue = CostPart(value, 1)
            .Replace(",", "", StringComparison.Ordinal)
            .Replace("*", "", StringComparison.Ordinal)
            .Trim();
        return int.TryParse(secondaryValue, System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
    }

    public static bool HasUnresolvedPurchaseCost(string value) =>
        BasePurchaseCost(value) == 0 && value.Contains('*', StringComparison.Ordinal);

    public static bool HasUnresolvedPatchCost(string value) =>
        SecondaryPurchaseCost(value) == 0 &&
        CostPart(value, 1).Contains('*', StringComparison.Ordinal);

    public static decimal CatalogMass(string value) =>
        decimal.TryParse(value, System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0m;

    public static int ItemCount(string value) =>
        int.TryParse(value, out var parsed) && parsed > 0 ? parsed : 1;

    public static int OptionalItemCount(string value) =>
        int.TryParse(value, out var parsed) && parsed > 0 ? parsed : 0;

    public static int PatchPurchasesNeedingPrice(Character character) =>
        character.Equipment
            .Where(item => PatchPurchaseNeedsPrice(item.Cost))
            .Sum(item => OptionalItemCount(item.PatchCount));

    public static IReadOnlyList<string> PatchPurchasesNeedingPriceItems(Character character) =>
        character.Equipment
            .Where(item => OptionalItemCount(item.PatchCount) > 0)
            .Where(item => PatchPurchaseNeedsPrice(item.Cost))
            .Select(item => item.Name)
            .ToArray();

    public static int AmmoPurchasesNeedingDetails(Character character) =>
        character.Weapons
            .Where(item => AmmoPurchaseNeedsDetails(item.AmmoCost, item.AmmoMass))
            .Sum(item => OptionalItemCount(item.AmmoCount));

    public static IReadOnlyList<string> AmmoPurchasesNeedingDetailsItems(Character character) =>
        character.Weapons
            .Where(item => OptionalItemCount(item.AmmoCount) > 0)
            .Where(item => AmmoPurchaseNeedsDetails(item.AmmoCost, item.AmmoMass))
            .Select(item => item.Name)
            .ToArray();

    public static int AmmoPurchasesNeedingReloadReview(Character character) =>
        character.Weapons
            .Where(item => AmmoPurchaseNeedsReloadReview(item.Shots))
            .Sum(item => OptionalItemCount(item.AmmoCount));

    public static IReadOnlyList<string> AmmoPurchasesNeedingReloadReviewItems(Character character) =>
        character.Weapons
            .Where(item => OptionalItemCount(item.AmmoCount) > 0)
            .Where(item => AmmoPurchaseNeedsReloadReview(item.Shots))
            .Select(item => item.Name)
            .ToArray();

    public static int UnmountedProstheticEnhancements(Character character)
    {
        var enhancementCount = character.Equipment
            .Where(item => IsProstheticEnhancement(item.Name))
            .Sum(item => ItemCount(item.Count));
        if (enhancementCount == 0) return 0;

        var hostCount = character.Equipment
            .Where(item => !IsProstheticEnhancement(item.Name) && IsProstheticOrImplantHost(item))
            .Sum(item => ItemCount(item.Count));
        return hostCount == 0 ? enhancementCount : 0;
    }

    public static IReadOnlyList<string> UnmountedProstheticEnhancementItems(Character character)
    {
        if (character.Equipment.Any(item =>
                !IsProstheticEnhancement(item.Name) && IsProstheticOrImplantHost(item)))
        {
            return [];
        }

        return character.Equipment
            .Where(item => IsProstheticEnhancement(item.Name))
            .Select(item => item.Name)
            .ToArray();
    }

    public static int UnbackedVehiclePurchases(Character character)
    {
        var vehicleCount = character.Equipment
            .Where(IsVehiclePurchase)
            .Sum(item => ItemCount(item.Count));
        if (vehicleCount == 0) return 0;

        var hasVehicleTrait = character.Traits.Any(item =>
            item.Value > 0 &&
            (item.Name == "Vehicle" ||
             item.Name == "Custom Vehicle" ||
             item.Name.StartsWith("Vehicle/", StringComparison.Ordinal) ||
             item.Name.StartsWith("Custom Vehicle/", StringComparison.Ordinal)));
        return hasVehicleTrait ? 0 : vehicleCount;
    }

    public static IReadOnlyList<string> UnbackedVehiclePurchaseItems(Character character)
    {
        var hasVehicleTrait = character.Traits.Any(item =>
            item.Value > 0 &&
            (item.Name == "Vehicle" ||
             item.Name == "Custom Vehicle" ||
             item.Name.StartsWith("Vehicle/", StringComparison.Ordinal) ||
             item.Name.StartsWith("Custom Vehicle/", StringComparison.Ordinal)));
        if (hasVehicleTrait)
        {
            return [];
        }

        return character.Equipment
            .Where(IsVehiclePurchase)
            .Select(item => item.Name)
            .ToArray();
    }

    private static string CostPart(string value, int index)
    {
        var parts = value.Split('/');
        return index < parts.Length ? parts[index] : "";
    }

    private static bool IsProstheticEnhancement(string name) =>
        name.StartsWith("Prosthetic Enhancement - ", StringComparison.Ordinal);

    private static bool IsProstheticOrImplantHost(EquipmentItem item) =>
        item.Locations.Equals("Implant", StringComparison.OrdinalIgnoreCase) ||
        item.Locations.Equals("Prosthetic", StringComparison.OrdinalIgnoreCase) ||
        item.Name.Contains("Implant", StringComparison.OrdinalIgnoreCase) ||
        item.Name.Contains("Prosthetic", StringComparison.OrdinalIgnoreCase);

    private static bool IsVehiclePurchase(EquipmentItem item) =>
        item.Locations.Equals("Vehicle", StringComparison.OrdinalIgnoreCase);

    private static bool PatchPurchaseNeedsPrice(string value)
    {
        var patchCost = CostPart(value, 1).Trim();
        return patchCost.Length == 0 ||
            (SecondaryPurchaseCost(value) == 0 &&
             !patchCost.Contains('*', StringComparison.Ordinal));
    }

    private static bool AmmoPurchaseNeedsDetails(string cost, string mass)
    {
        var trimmedCost = cost.Trim();
        var trimmedMass = mass.Trim();
        return (trimmedCost.Length == 0 ||
                (BasePurchaseCost(cost) == 0 &&
                 !trimmedCost.Contains('*', StringComparison.Ordinal))) ||
            trimmedMass.Length == 0 ||
            CatalogMass(mass) <= 0m;
    }

    private static bool AmmoPurchaseNeedsReloadReview(string shots)
    {
        var trimmedShots = shots.Trim();
        return trimmedShots.Length == 0 ||
            trimmedShots == "0" ||
            trimmedShots.Contains(',', StringComparison.Ordinal) ||
            trimmedShots.Contains("PPS", StringComparison.OrdinalIgnoreCase);
    }

    private static int FindValue(IEnumerable<NamedValue> values, string name) =>
        values.FirstOrDefault(item => item.Name == name)?.Value ?? 0;

    private static int DivideRoundUp(int value, int divisor) =>
        (int)Math.Ceiling(value / (double)divisor);
}

public sealed record CharacterSummary(
    int AttributeXp,
    int SkillXp,
    int TraitXp,
    int SpentXp,
    int FreeXp,
    int WealthLevel,
    int InventoryBaseCost,
    int RemainingCBills,
    int UnresolvedInventoryPrices,
    decimal CarryingCapacity,
    decimal InventoryMass,
    decimal RemainingCapacity,
    int Walk,
    int Run,
    int Sprint,
    int Climb,
    int Crawl,
    int Swim);
