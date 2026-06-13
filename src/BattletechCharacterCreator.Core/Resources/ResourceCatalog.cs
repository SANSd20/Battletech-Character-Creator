namespace BattletechCharacterCreator.Core.Resources;

public sealed record EquipmentCatalogItem(
    string Category,
    string Name,
    string Cost,
    string Mass,
    string Locations,
    string Armor,
    string Notes);

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
    string Notes);

public sealed record SkillCatalogItem(
    string Name,
    string Rules,
    string Description,
    IReadOnlyList<string> Subskills);

public sealed record TraitCatalogItem(
    string Name,
    string Reference,
    string Description);

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
    public IReadOnlyList<SkillCatalogItem> Skills { get; private init; } = [];
    public IReadOnlyList<TraitCatalogItem> Traits { get; private init; } = [];
    public IReadOnlyDictionary<string, string> SkillDescriptions { get; private init; } =
        new Dictionary<string, string>();
    public IReadOnlyDictionary<string, string> TraitDescriptions { get; private init; } =
        new Dictionary<string, string>();

    public static ResourceCatalog Load(string directory)
    {
        var skillDescriptions = ReadDescriptions(directory, "skillsdesc.dat");
        var traitDescriptions = ReadDescriptions(directory, "traitsdesc.dat");
        var subskills = ReadSubskills(directory);
        var skills = ReadPairs(directory, "allskills.dat")
            .Select(pair => new SkillCatalogItem(
                pair.Name,
                pair.Value,
                FindDescription(skillDescriptions, pair.Name),
                subskills.GetValueOrDefault(BaseName(pair.Name), [])))
            .ToArray();
        var traits = ReadPairs(directory, "alltraits.dat")
            .Select(pair => new TraitCatalogItem(
                pair.Name,
                pair.Value,
                FindDescription(traitDescriptions, pair.Name)))
            .ToArray();

        return new ResourceCatalog
        {
            Affiliations = ReadLines(directory, "affilations.dat"),
            Phenotypes = ReadLines(directory, "phenotype.dat"),
            EyeColors = ReadLines(directory, "eyecolor.dat"),
            HairColors = ReadLines(directory, "haircolor.dat"),
            Planets = ReadLines(directory, "planets.dat"),
            SkillNames = skills.Select(item => item.Name).ToArray(),
            TraitNames = traits.Select(item => item.Name).ToArray(),
            Careers = ReadLines(directory, "career.dat"),
            Equipment = ReadEquipment(directory),
            Weapons = ReadWeapons(directory),
            Skills = skills,
            Traits = traits,
            SkillDescriptions = skillDescriptions,
            TraitDescriptions = traitDescriptions
        };
    }

    private static EquipmentCatalogItem[] ReadEquipment(string directory) =>
        ReadDataLines(directory, "equiplist.dat")
            .Select(line => line.Split(';'))
            .Where(fields => fields.Length == 7)
            .Select(fields => new EquipmentCatalogItem(
                fields[0], fields[1], fields[2], fields[3], fields[4],
                fields[5], fields[6]))
            .ToArray();

    private static WeaponCatalogItem[] ReadWeapons(string directory) =>
        ReadDataLines(directory, "weaponslist.dat")
            .Select(line => line.Split(';'))
            .Where(fields => fields.Length == 11)
            .Select(fields => new WeaponCatalogItem(
                fields[0], fields[1], fields[2], fields[3], fields[4],
                fields[5], fields[6], fields[7], fields[8], fields[9], fields[10]))
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
