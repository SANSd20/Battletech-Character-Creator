namespace BattletechCharacterCreator.Core.Resources;

public enum RulebookSource
{
    CoreRulebook,
    Companion
}

public static class RulebookSourceExtensions
{
    public static string DisplayName(this RulebookSource source) =>
        source switch
        {
            RulebookSource.Companion => "A Time of War Companion",
            _ => "A Time of War"
        };
}

public sealed record ResourceCatalogOptions(bool IncludeCompanion = false);

public sealed record EquipmentCatalogItem(
    string Category,
    string Name,
    string Cost,
    string Mass,
    string Locations,
    string Armor,
    string Notes,
    RulebookSource Source = RulebookSource.CoreRulebook) : ISourceCatalogItem
{
    public string SourceLabel => Source.DisplayName();
}

public sealed record WeaponCatalogItem(
    string Category,
    string Skill,
    string Name,
    string Damage,
    string Range,
    string Cost,
    string Mass,
    string Shots,
    string AmmoCost,
    string AmmoMass,
    string Notes,
    RulebookSource Source = RulebookSource.CoreRulebook) : ISourceCatalogItem
{
    public string SourceLabel => Source.DisplayName();
}

public sealed record AmmoModifierCatalogItem(
    string Category,
    string Name,
    string ApBdModifier,
    string RangeModifier,
    decimal CostMultiplier,
    string Availability,
    IReadOnlyList<string> CompatibleCategories,
    string Notes,
    RulebookSource Source = RulebookSource.CoreRulebook) : ISourceCatalogItem
{
    public string SourceLabel => Source.DisplayName();
    public string DisplayName => $"{Name} (x{CostMultiplier:0.##})";
}

public sealed record SkillCatalogItem(
    string Name,
    string Rules,
    string Description,
    IReadOnlyList<string> Subskills,
    RulebookSource Source = RulebookSource.CoreRulebook) : ISourceCatalogItem
{
    public string SourceLabel => Source.DisplayName();
}

public sealed record TraitCatalogItem(
    string Name,
    string Reference,
    string Description,
    RulebookSource Source = RulebookSource.CoreRulebook) : ISourceCatalogItem
{
    public string SourceLabel => Source.DisplayName();
}

public sealed class ResourceCatalog
{
    public IReadOnlyList<string> Affiliations { get; private init; } = [];
    public IReadOnlyList<string> Phenotypes { get; private init; } = [];
    public IReadOnlyList<string> EyeColors { get; private init; } = [];
    public IReadOnlyList<string> HairColors { get; private init; } = [];
    public IReadOnlyList<string> Planets { get; private init; } = [];
    public IReadOnlyList<string> SkillNames { get; private init; } = [];
    public IReadOnlyList<string> TraitNames { get; private init; } = [];
    public IReadOnlyList<string> Careers { get; private init; } = [];
    public IReadOnlyList<EquipmentCatalogItem> Equipment { get; private init; } = [];
    public IReadOnlyList<WeaponCatalogItem> Weapons { get; private init; } = [];
    public IReadOnlyList<AmmoModifierCatalogItem> AmmoModifiers { get; private init; } = [];
    public IReadOnlyList<SkillCatalogItem> Skills { get; private init; } = [];
    public IReadOnlyList<TraitCatalogItem> Traits { get; private init; } = [];
    public IReadOnlyDictionary<string, string> SkillDescriptions { get; private init; } =
        new Dictionary<string, string>();
    public IReadOnlyDictionary<string, string> TraitDescriptions { get; private init; } =
        new Dictionary<string, string>();
    public ResourceCatalogOptions Options { get; private init; } = new();

    public static ResourceCatalog Load(string directory) =>
        Load(directory, new ResourceCatalogOptions());

    public static ResourceCatalog Load(
        string directory,
        ResourceCatalogOptions options)
    {
        var skillDescriptions = ReadDescriptions(directory, "skillsdesc.dat");
        var traitDescriptions = ReadTraitDescriptions(directory, options);
        var subskills = ReadSubskills(directory);
        var skills = ReadPairs(directory, "allskills.dat")
            .Select(pair => new SkillCatalogItem(
                pair.Name,
                pair.Value,
                FindDescription(skillDescriptions, pair.Name),
                subskills.GetValueOrDefault(BaseName(pair.Name), [])))
            .ToArray();
        var traits = ReadTraits(directory, options, traitDescriptions);
        var equipment = ReadEquipment(directory, options);
        var weapons = ReadWeapons(directory, options);
        var ammoModifiers = ReadAmmoModifiers(directory, options);
        var filteredSkills = FilterBySource(skills, options).ToArray();
        var filteredTraits = FilterBySource(traits, options).ToArray();

        return new ResourceCatalog
        {
            Options = options,
            Affiliations = ReadLines(directory, "affilations.dat"),
            Phenotypes = ReadLines(directory, "phenotype.dat"),
            EyeColors = ReadLines(directory, "eyecolor.dat"),
            HairColors = ReadLines(directory, "haircolor.dat"),
            Planets = ReadLines(directory, "planets.dat"),
            SkillNames = filteredSkills.Select(item => item.Name).ToArray(),
            TraitNames = filteredTraits.Select(item => item.Name).ToArray(),
            Careers = ReadLines(directory, "career.dat"),
            Equipment = equipment,
            Weapons = weapons,
            AmmoModifiers = ammoModifiers,
            Skills = filteredSkills,
            Traits = filteredTraits,
            SkillDescriptions = skillDescriptions,
            TraitDescriptions = traitDescriptions
        };
    }

    public static bool IsSourceEnabled(
        RulebookSource source,
        ResourceCatalogOptions options) =>
        source == RulebookSource.CoreRulebook || options.IncludeCompanion;

    private static IEnumerable<T> FilterBySource<T>(
        IEnumerable<T> items,
        ResourceCatalogOptions options)
        where T : ISourceCatalogItem =>
        items.Where(item => IsSourceEnabled(item.Source, options));

    private static EquipmentCatalogItem[] ReadEquipment(
        string directory,
        ResourceCatalogOptions options)
    {
        var items = ReadEquipmentFile(
            directory,
            "equiplist.dat",
            RulebookSource.CoreRulebook);
        return options.IncludeCompanion
            ? items.Concat(ReadEquipmentFile(
                    directory,
                    "companion_equiplist.dat",
                    RulebookSource.Companion))
                .ToArray()
            : items.ToArray();
    }

    private static EquipmentCatalogItem[] ReadEquipmentFile(
        string directory,
        string name,
        RulebookSource source) =>
        ReadDataLines(directory, name)
            .Select(line => line.Split(';', 7))
            .Where(fields => fields.Length == 7)
            .Select(fields => new EquipmentCatalogItem(
                fields[0], fields[1], fields[2], fields[3], fields[4],
                fields[5], fields[6], source))
            .ToArray();

    private static WeaponCatalogItem[] ReadWeapons(
        string directory,
        ResourceCatalogOptions options)
    {
        var items = ReadWeaponsFile(
            directory,
            "weaponslist.dat",
            RulebookSource.CoreRulebook);
        return options.IncludeCompanion
            ? items.Concat(ReadWeaponsFile(
                    directory,
                    "companion_weaponslist.dat",
                    RulebookSource.Companion))
                .ToArray()
            : items.ToArray();
    }

    private static WeaponCatalogItem[] ReadWeaponsFile(
        string directory,
        string name,
        RulebookSource source) =>
        ReadDataLines(directory, name)
            .Select(line => line.Split(';', 11))
            .Where(fields => fields.Length == 11)
            .Select(fields => new WeaponCatalogItem(
                fields[0], fields[1], fields[2], fields[3], fields[4],
                fields[5], fields[6], fields[7], fields[8], fields[9],
                fields[10], source))
            .ToArray();

    private static AmmoModifierCatalogItem[] ReadAmmoModifiers(
        string directory,
        ResourceCatalogOptions options) =>
        FilterBySource(
                ReadAmmoModifiersFile(
                    directory,
                    "ammo_modifiers.dat",
                    RulebookSource.CoreRulebook),
                options)
            .ToArray();

    private static AmmoModifierCatalogItem[] ReadAmmoModifiersFile(
        string directory,
        string name,
        RulebookSource source) =>
        ReadDataLines(directory, name)
            .Select(line => line.Split(';', 8))
            .Where(fields => fields.Length == 8)
            .Select(fields => new AmmoModifierCatalogItem(
                fields[0], fields[1], fields[2], fields[3],
                ParseDecimal(fields[4]), fields[5],
                ParseList(fields[6]), fields[7], source))
            .ToArray();

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> ReadSubskills(
        string directory) =>
        ReadPairs(directory, "subskill.dat")
            .GroupBy(pair => pair.Name, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(pair => pair.Value)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);

    private static TraitCatalogItem[] ReadTraits(
        string directory,
        ResourceCatalogOptions options,
        IReadOnlyDictionary<string, string> descriptions)
    {
        var items = ReadPairs(directory, "alltraits.dat")
            .Select(pair => new TraitCatalogItem(
                pair.Name,
                pair.Value,
                FindDescription(descriptions, pair.Name),
                RulebookSource.CoreRulebook));
        return options.IncludeCompanion
            ? items.Concat(ReadPairs(directory, "companion_alltraits.dat")
                    .Select(pair => new TraitCatalogItem(
                        pair.Name,
                        pair.Value,
                        FindDescription(descriptions, pair.Name),
                        RulebookSource.Companion)))
                .ToArray()
            : items.ToArray();
    }

    private static IReadOnlyDictionary<string, string> ReadTraitDescriptions(
        string directory,
        ResourceCatalogOptions options)
    {
        var descriptions = ReadDescriptions(directory, "traitsdesc.dat");
        if (!options.IncludeCompanion)
        {
            return descriptions;
        }

        return descriptions
            .Concat(ReadDescriptions(directory, "companion_traitsdesc.dat"))
            .ToDictionary(
                pair => pair.Key,
                pair => pair.Value,
                StringComparer.Ordinal);
    }

    private static IReadOnlyDictionary<string, string> ReadDescriptions(
        string directory,
        string name)
    {
        var descriptions = new Dictionary<string, string>(StringComparer.Ordinal);
        string? currentName = null;
        foreach (var line in ReadDataLines(directory, name))
        {
            var separator = line.IndexOf(';');
            if (separator > 0)
            {
                currentName = line[..separator];
                descriptions[currentName] = line[(separator + 1)..];
            }
            else if (currentName is not null)
            {
                descriptions[currentName] += Environment.NewLine + line;
            }
        }
        return descriptions;
    }

    private static (string Name, string Value)[] ReadPairs(
        string directory,
        string name) =>
        ReadDataLines(directory, name)
            .Select(line => line.Split(';', 2))
            .Where(fields => fields.Length == 2)
            .Select(fields => (fields[0], fields[1]))
            .ToArray();

    private static string FindDescription(
        IReadOnlyDictionary<string, string> descriptions,
        string name) =>
        descriptions.GetValueOrDefault(name) ??
        descriptions.GetValueOrDefault(BaseName(name)) ??
        "";

    private static string BaseName(string name)
    {
        var separator = name.IndexOf('/');
        return separator < 0 ? name : name[..separator];
    }

    private static string[] ReadLines(string directory, string name) =>
        ReadDataLines(directory, name).ToArray();

    private static decimal ParseDecimal(string value) =>
        decimal.TryParse(
            value,
            System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : 1m;

    private static IReadOnlyList<string> ParseList(string value) =>
        value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static IEnumerable<string> ReadDataLines(
        string directory,
        string name)
    {
        var path = Path.Combine(directory, name);
        return File.Exists(path)
            ? File.ReadLines(path)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0 &&
                    !line.StartsWith('#'))
            : [];
    }
}

public interface ISourceCatalogItem
{
    RulebookSource Source { get; }
}
