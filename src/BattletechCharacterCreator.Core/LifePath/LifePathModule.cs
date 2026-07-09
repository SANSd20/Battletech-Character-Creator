using BattletechCharacterCreator.Core.Models;

namespace BattletechCharacterCreator.Core.LifePath;

public enum EffectTarget
{
    None,
    Attribute,
    Skill,
    Trait,
    Flexible,
    PreAttribute,
    PreSkill,
    PreTrait
}

public sealed record ModuleEffect(EffectTarget Target, string Name, int Xp);

public sealed record ModuleChoice(
    string Id,
    string Label,
    EffectTarget Target,
    int Xp,
    int Count,
    IReadOnlyList<string> Options,
    bool Distinct = true,
    IReadOnlyDictionary<string, IReadOnlyList<ModuleEffect>>? OptionEffects = null,
    int? AttributeMaximumXp = null,
    int? TraitMaximumXp = null,
    int? SkillMaximumXp = null,
    int MinimumAttributeOrTraitXp = 0,
    IReadOnlyList<string>? EducationFieldNames = null,
    int MinimumEducationFieldSkillXp = 0,
    int MaximumEducationFieldSkillTargets = int.MaxValue,
    bool ClanWarriorFieldSkillsOnly = false,
    bool SelectedEducationFieldSkillsOnly = false,
    bool FixedFlexibleSelections = false,
    bool SolarisInternshipFieldSkillsOnly = false);

public sealed record LifePathModule(
    string Id,
    string Name,
    string Description,
    int ModuleCost,
    IReadOnlyList<ModuleEffect> Effects,
    IReadOnlyList<ModuleChoice> Choices,
    IReadOnlyList<string>? Languages = null,
    string? ProtocolSkill = null,
    string? StreetwiseSkill = null,
    IReadOnlyList<LifePathModule>? SubAffiliations = null,
    IReadOnlyList<LifePathModule>? Castes = null,
    IReadOnlyList<LifePathModule>? BasicFields = null,
    IReadOnlyList<LifePathModule>? AdvancedFields = null,
    IReadOnlyList<LifePathModule>? SpecialistFields = null,
    int AffiliationLanguageXp = 0,
    int AffiliationProtocolXp = 0,
    int AffiliationStreetwiseXp = 0,
    int TimeYears = 0,
    bool Repeatable = true,
    bool AwardFlexibleXpOnRepeat = true,
    IReadOnlyList<ModuleEffect>? RepeatEffects = null);

public sealed record ModuleSelection(
    LifePathModule Module,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Choices,
    IReadOnlyDictionary<string, IReadOnlyList<ChoiceAllocation>>? Allocations = null);

public sealed record ChoiceAllocation(string Name, int Xp);

public static class LifePathEngine
{
    public const int StartingXp = 5000;
    public const int UniversalModuleCost = 850;

    private static readonly HashSet<string> AttributeNames =
        ["STR", "BOD", "RFL", "DEX", "INT", "WIL", "CHA", "EDG"];

    private static readonly HashSet<string> TraitNames =
    [
        "Alternate ID", "Ambidextrous", "Animal Empathy", "Attractive", "Combat Sense",
        "Connections", "Equipped", "Exceptional Attribute/STR", "Exceptional Attribute/BOD",
        "Exceptional Attribute/RFL", "Exceptional Attribute/DEX", "Exceptional Attribute/INT",
        "Exceptional Attribute/WIL", "Exceptional Attribute/CHA", "Exceptional Attribute/EDG",
        "Extra Income", "Fast Learner", "Fit", "Good Hearing", "Good Vision",
        "Natural Aptitude/Perception", "Pain Resistance", "Patient", "Property", "Reputation",
        "Sixth Sense", "Thick-Skinned", "Toughness", "Wealth"
    ];

    public static Character CreateBase(string name, string language)
    {
        var character = new Character { Name = name };
        Add(character.Skills, language, 20);
        Add(character.Skills, "Language/English", 20);
        Add(character.Skills, "Perception", 10);
        return character;
    }

    public static int CalculateModuleCost(IEnumerable<LifePathModule> modules) =>
        UniversalModuleCost + modules.Sum(module => module.ModuleCost);

    public static int CalculateModuleCost(
        Character character,
        IEnumerable<LifePathModule> modules)
    {
        var fieldSkillCost = Find(character.Traits, "Fast Learner") >= 300
            ? 24
            : Find(character.Traits, "Slow Learner") <= -300
                ? 36
                : 30;
        return UniversalModuleCost + modules.Sum(module =>
            module.Id.StartsWith("field-", StringComparison.Ordinal)
                ? module.ModuleCost / 30 * fieldSkillCost
                : module.ModuleCost);
    }

    public static void ApplyModuleAccounting(
        Character character,
        IEnumerable<LifePathModule> modules)
    {
        var allocatedXp = character.Attributes.Sum(item => item.Value) +
            character.Skills.Sum(item => item.Value) +
            character.Traits.Sum(item => item.Value);
        character.GmXpModifier = CalculateModuleCost(character, modules) - allocatedXp;
    }

    public static void Apply(Character character, ModuleSelection selection)
    {
        Apply(character, selection, repeated: false);
    }

    public static void ApplyStage4(
        Character character,
        ModuleSelection selection)
    {
        var priorSelections = character.RealLifeHistory.Count(name =>
            name == selection.Module.Name);
        if (priorSelections > 0 && !selection.Module.Repeatable)
        {
            throw new InvalidOperationException(
                $"{selection.Module.Name} may not be repeated.");
        }

        Apply(character, selection, repeated: priorSelections > 0);
        character.RealLife = selection.Module.Name;
        character.RealLifeHistory.Add(selection.Module.Name);
    }

    public static void Apply(
        Character character,
        ModuleSelection selection,
        bool repeated)
    {
        if (repeated && selection.Module.RepeatEffects is not null)
        {
            foreach (var effect in selection.Module.RepeatEffects)
            {
                ApplyEffect(character, effect.Target, effect.Name, effect.Xp);
            }
        }

        foreach (var effect in selection.Module.Effects)
        {
            if (repeated && effect.Target != EffectTarget.Skill)
            {
                continue;
            }
            ApplyEffect(character, effect.Target, effect.Name, effect.Xp);
        }

        foreach (var choice in selection.Module.Choices)
        {
            if (repeated &&
                choice.Target is not (EffectTarget.Skill or EffectTarget.Flexible))
            {
                continue;
            }
            if (repeated &&
                choice.Target == EffectTarget.Flexible &&
                !selection.Module.AwardFlexibleXpOnRepeat)
            {
                continue;
            }
            if (choice.Target == EffectTarget.Flexible &&
                selection.Allocations?.TryGetValue(choice.Id, out var allocations) == true)
            {
                ApplyFlexibleAllocations(character, selection.Module, choice, allocations);
                continue;
            }

            if (!selection.Choices.TryGetValue(choice.Id, out var selected) ||
                selected.Count != choice.Count)
            {
                throw new InvalidOperationException(
                    $"{selection.Module.Name}: '{choice.Label}' requires {choice.Count} selection(s).");
            }
            if (choice.Distinct && selected.Distinct(StringComparer.Ordinal).Count() != selected.Count)
            {
                throw new InvalidOperationException(
                    $"{selection.Module.Name}: '{choice.Label}' requires distinct selections.");
            }

            foreach (var name in selected)
            {
                var validOptions = LifePathCatalog.FilterEraAvailableSkillOptions(
                        character,
                        choice.Options)
                    .Concat(choice.EducationFieldNames is null
                        ? []
                        : LifePathCatalog.ResolveEducationFieldSkills(
                            character, choice.EducationFieldNames))
                    .Concat(choice.ClanWarriorFieldSkillsOnly
                        ? LifePathCatalog.ResolveClanWarriorFieldSkills(character)
                        : [])
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                if (choice.SelectedEducationFieldSkillsOnly)
                {
                    var selectedFieldSkills =
                        LifePathCatalog.ResolveSelectedEducationFieldSkills(character);
                    validOptions = selectedFieldSkills.Count == 0
                        ? LifePathCatalog.FilterEraAvailableSkillOptions(
                            character,
                            choice.Options).ToArray()
                        : selectedFieldSkills.ToArray();
                }
                if (choice.SolarisInternshipFieldSkillsOnly)
                {
                    var internshipSkills =
                        LifePathCatalog.ResolveSolarisInternshipFieldSkills(character);
                    validOptions = internshipSkills.Count == 0
                        ? LifePathCatalog.FilterEraAvailableSkillOptions(
                            character,
                            choice.Options).ToArray()
                        : internshipSkills.ToArray();
                }
                if (!validOptions.Contains(name, StringComparer.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"{selection.Module.Name}: '{name}' is not valid for '{choice.Label}'.");
                }
                var target = choice.Target == EffectTarget.Flexible
                    ? ClassifyFlexibleTarget(name)
                    : choice.Target;
                ApplyEffect(character, target, name, choice.Xp);
                if (choice.OptionEffects?.TryGetValue(name, out var optionEffects) == true)
                {
                    foreach (var optionEffect in optionEffects)
                    {
                        if (repeated &&
                            optionEffect.Target != EffectTarget.Skill)
                        {
                            continue;
                        }
                        ApplyEffect(character, optionEffect.Target, optionEffect.Name, optionEffect.Xp);
                    }
                }
            }
        }
    }

    private static void ApplyFlexibleAllocations(
        Character character,
        LifePathModule module,
        ModuleChoice choice,
        IReadOnlyList<ChoiceAllocation> allocations)
    {
        var active = allocations.Where(allocation => allocation.Xp > 0).ToArray();
        var requiredXp = choice.Xp * choice.Count;
        if (active.Length == 0 || active.Sum(allocation => allocation.Xp) != requiredXp)
        {
            throw new InvalidOperationException(
                $"{module.Name}: '{choice.Label}' must allocate exactly {requiredXp} XP.");
        }
        var attributeOrTraitXp = active
            .Where(allocation =>
                ClassifyFlexibleTarget(allocation.Name) is
                    EffectTarget.Attribute or EffectTarget.Trait)
            .Sum(allocation => allocation.Xp);
        if (attributeOrTraitXp < choice.MinimumAttributeOrTraitXp)
        {
            throw new InvalidOperationException(
                $"{module.Name}: '{choice.Label}' must allocate at least " +
                $"{choice.MinimumAttributeOrTraitXp} XP to Attributes or Traits.");
        }
        var educationFieldOptions = choice.EducationFieldNames is null
            ? []
            : LifePathCatalog.ResolveEducationFieldSkills(
                character, choice.EducationFieldNames);
        var educationAllocations = active
            .Where(allocation => educationFieldOptions.Contains(
                allocation.Name, StringComparer.Ordinal))
            .ToArray();
        if (educationAllocations.Sum(allocation => allocation.Xp) <
            choice.MinimumEducationFieldSkillXp)
        {
            throw new InvalidOperationException(
                $"{module.Name}: '{choice.Label}' must allocate at least " +
                $"{choice.MinimumEducationFieldSkillXp} XP to selected Education Field skills.");
        }
        if (educationAllocations.Select(allocation => allocation.Name)
                .Distinct(StringComparer.Ordinal).Count() >
            choice.MaximumEducationFieldSkillTargets)
        {
            throw new InvalidOperationException(
                $"{module.Name}: '{choice.Label}' allows at most " +
                $"{choice.MaximumEducationFieldSkillTargets} Education Field skill targets.");
        }

        if (choice.AttributeMaximumXp is int maximum &&
            active.Where(allocation =>
                    ClassifyFlexibleTarget(allocation.Name) ==
                    EffectTarget.Attribute)
                .GroupBy(allocation => allocation.Name, StringComparer.Ordinal)
                .Any(group => group.Sum(allocation => allocation.Xp) > maximum))
        {
            throw new InvalidOperationException(
                $"{module.Name}: '{choice.Label}' allows at most {maximum} XP per Attribute.");
        }
        if (choice.TraitMaximumXp is int traitMaximum &&
            active.Where(allocation =>
                    ClassifyFlexibleTarget(allocation.Name) ==
                    EffectTarget.Trait)
                .GroupBy(allocation => allocation.Name, StringComparer.Ordinal)
                .Any(group => group.Sum(allocation => allocation.Xp) > traitMaximum))
        {
            throw new InvalidOperationException(
                $"{module.Name}: '{choice.Label}' allows at most {traitMaximum} XP per Trait.");
        }
        if (choice.SkillMaximumXp is int skillMaximum &&
            active.Where(allocation =>
                    ClassifyFlexibleTarget(allocation.Name) ==
                    EffectTarget.Skill)
                .GroupBy(allocation => allocation.Name, StringComparer.Ordinal)
                .Any(group => group.Sum(allocation => allocation.Xp) > skillMaximum))
        {
            throw new InvalidOperationException(
                $"{module.Name}: '{choice.Label}' allows at most {skillMaximum} XP per Skill.");
        }

        foreach (var allocation in active)
        {
            if (!choice.Options.Contains(allocation.Name, StringComparer.Ordinal) &&
                !educationFieldOptions.Contains(allocation.Name, StringComparer.Ordinal))
            {
                throw new InvalidOperationException(
                    $"{module.Name}: '{allocation.Name}' is not valid for '{choice.Label}'.");
            }
            var target = ClassifyFlexibleTarget(allocation.Name);
            ApplyEffect(
                character,
                target,
                allocation.Name,
                allocation.Xp);
        }
    }

    private static int Find(IEnumerable<NamedValue> values, string name) =>
        values.FirstOrDefault(item => item.Name == name)?.Value ?? 0;

    public static void ApplyAffiliationContext(
        Character character,
        LifePathModule affiliation,
        LifePathModule module,
        string primaryLanguage)
    {
        if (module.AffiliationLanguageXp != 0)
        {
            ApplyEffect(character, EffectTarget.Skill, primaryLanguage, module.AffiliationLanguageXp);
        }
        if (module.AffiliationProtocolXp != 0 && affiliation.ProtocolSkill is not null)
        {
            ApplyEffect(character, EffectTarget.Skill, affiliation.ProtocolSkill, module.AffiliationProtocolXp);
        }
        if (module.AffiliationStreetwiseXp != 0 && affiliation.StreetwiseSkill is not null)
        {
            ApplyEffect(character, EffectTarget.Skill, affiliation.StreetwiseSkill, module.AffiliationStreetwiseXp);
        }
    }

    public static EffectTarget ClassifyFlexibleTarget(string name)
    {
        if (AttributeNames.Contains(name)) return EffectTarget.Attribute;
        if (TraitNames.Contains(name) ||
            name.StartsWith("Compulsion/", StringComparison.Ordinal) ||
            name.StartsWith("Exceptional Attribute/", StringComparison.Ordinal) ||
            name.StartsWith("Natural Aptitude/", StringComparison.Ordinal))
        {
            return EffectTarget.Trait;
        }
        return EffectTarget.Skill;
    }

    private static void ApplyEffect(Character character, EffectTarget target, string name, int xp)
    {
        switch (target)
        {
            case EffectTarget.None: break;
            case EffectTarget.Attribute: Add(character.Attributes, name, xp); break;
            case EffectTarget.Skill: Add(character.Skills, name, xp); break;
            case EffectTarget.Trait: Add(character.Traits, name, xp); break;
            case EffectTarget.PreAttribute: Add(character.PreAttributes, name, xp); break;
            case EffectTarget.PreSkill: Add(character.PreSkills, name, xp); break;
            case EffectTarget.PreTrait: Add(character.PreTraits, name, xp); break;
        }
    }

    private static void Add(ICollection<NamedValue> values, string name, int xp)
    {
        var existing = values.FirstOrDefault(item => item.Name == name);
        if (existing is null)
        {
            values.Add(new NamedValue(name, xp));
        }
        else
        {
            existing.Value += xp;
        }
    }
}
