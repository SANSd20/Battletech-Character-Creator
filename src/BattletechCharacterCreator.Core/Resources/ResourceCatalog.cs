namespace BattletechCharacterCreator.Core.Resources;

public sealed class ResourceCatalog
{
    public IReadOnlyList<string> Affiliations { get; private init; } = [];
    public IReadOnlyList<string> Phenotypes { get; private init; } = [];
    public IReadOnlyList<string> EyeColors { get; private init; } = [];
    public IReadOnlyList<string> HairColors { get; private init; } = [];
    public IReadOnlyList<string> Planets { get; private init; } = [];
    public IReadOnlyList<string> SkillNames { get; private init; } = [];
    public IReadOnlyList<string> TraitNames { get; private init; } = [];

    public static ResourceCatalog Load(string directory) => new()
    {
        Affiliations = ReadLines(directory, "affilations.dat"),
        Phenotypes = ReadLines(directory, "phenotype.dat"),
        EyeColors = ReadLines(directory, "eyecolor.dat"),
        HairColors = ReadLines(directory, "haircolor.dat"),
        Planets = ReadLines(directory, "planets.dat"),
        SkillNames = ReadDelimitedNames(directory, "allskills.dat"),
        TraitNames = ReadDelimitedNames(directory, "alltraits.dat")
    };

    private static string[] ReadLines(string directory, string name)
    {
        var path = Path.Combine(directory, name);
        return File.Exists(path)
            ? File.ReadAllLines(path).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray()
            : [];
    }

    private static string[] ReadDelimitedNames(string directory, string name) =>
        ReadLines(directory, name)
            .Select(line => line.Split(';', 2)[0])
            .Where(line => !line.StartsWith('#'))
            .ToArray();
}
