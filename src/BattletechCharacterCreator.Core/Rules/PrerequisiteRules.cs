using BattletechCharacterCreator.Core.LifePath;
using BattletechCharacterCreator.Core.Models;

namespace BattletechCharacterCreator.Core.Rules;

public static class PrerequisiteRules
{
    public static IReadOnlyList<PrerequisiteIssue> Evaluate(Character character)
    {
        var issues = new List<PrerequisiteIssue>();

        foreach (var requirement in character.PreAttributes)
        {
            AddIfMissing(issues, "Attribute", requirement,
                FindValue(character.Attributes, requirement.Name));
        }
        foreach (var requirement in character.PreSkills)
        {
            AddIfMissing(issues, "Skill", requirement,
                FindValue(character.Skills, requirement.Name));
        }
        foreach (var requirement in character.PreTraits)
        {
            AddIfMissing(issues, "Trait", requirement,
                FindValue(character.Traits, requirement.Name));
        }

        if (character.EarlyChildhood == "Nobility" &&
            !HasAny(character.Traits, 500, "Wealth", "Title", "Title/Inner Sphere", "Title/Clan", "Property"))
        {
            issues.Add(new("Choice", "Wealth, Title, or Property", 500, 0));
        }
        if (character.EarlyChildhood == "White Collar" &&
            !HasAny(character.Traits, 300, "Wealth", "Property"))
        {
            issues.Add(new("Choice", "Wealth or Property", 300, 0));
        }
        if (character.EarlyChildhood == "Trueborn Creche" &&
            character.Affiliation is not ("Invading Clan" or "Homeworld Clan"))
        {
            issues.Add(new("Affiliation", "Invading Clan or Homeworld Clan", 0, 0));
        }
        if (character.EarlyChildhood == "Trueborn Creche" &&
            !character.Traits.Any(item => item.Name.StartsWith("Phenotype/", StringComparison.Ordinal)))
        {
            issues.Add(new("Trait", "Phenotype", 0, 0));
        }
        var isClan = character.Affiliation is "Invading Clan" or "Homeworld Clan";
        if (character.LateChildhood is "Clan Apprenticeship" or "Freeborn Sibko" or "Trueborn Sibko" &&
            !isClan)
        {
            issues.Add(new("Affiliation", "Clan affiliation", 0, 0));
        }
        if (character.LateChildhood == "High School" && isClan)
        {
            issues.Add(new("Affiliation", "Non-Clan affiliation", 0, 0));
        }
        if (character.LateChildhood is "High School" or "Preparatory School" &&
            character.Traits.Any(item => item.Name == "Illiterate" && item.Value < 0))
        {
            issues.Add(new("Trait", "May not be Illiterate", 0, 0));
        }
        if (character.LateChildhood == "Preparatory School" &&
            character.EarlyChildhood is "Back Woods" or "Fugitives")
        {
            issues.Add(new("Early childhood", "Not Back Woods or Fugitives", 0, 0));
        }
        if (character.LateChildhood == "Adolescent Warfare" &&
            character.EarlyChildhood is "Nobility" or "Trueborn Creche")
        {
            issues.Add(new("Early childhood", "Not Nobility or Trueborn Creche", 0, 0));
        }
        if (character.School == "Family Training" &&
            character.LateChildhood is not ("Preparatory School" or "Military School") &&
            FindValue(character.Traits, "Connections") < 100)
        {
            issues.Add(new("Choice", "Preparatory School, Military School, or Connections", 100,
                FindValue(character.Traits, "Connections")));
        }
        if (character.School == "Solaris Internship" &&
            FindValue(character.Traits, "Connections") < 200)
        {
            issues.Add(new("Trait", "Connections", 200,
                FindValue(character.Traits, "Connections")));
        }
        if (character.School == "Officer Candidate School" &&
            (character.BasicSchool.Length == 0 || character.AdvancedSchool.Length == 0))
        {
            issues.Add(new("Education", "Basic and advanced fields", 0, 0));
        }
        if (character.SpecialSchool.Length > 0 && character.AdvancedSchool.Length == 0)
        {
            issues.Add(new("Education", "Specialist training requires an advanced field", 0, 0));
        }
        if (character.Affiliation is "ComStar" or "Word of Blake")
        {
            if (FindValue(character.Traits, "Extra Income") > 0)
            {
                issues.Add(new("Trait", "May not have Extra Income", 0,
                    FindValue(character.Traits, "Extra Income")));
            }
            if (FindValue(character.Traits, "Property") > 0)
            {
                issues.Add(new("Trait", "May not have Property", 0,
                    FindValue(character.Traits, "Property")));
            }
        }
        if (character.RealLife is "ComStar Service" or "Word of Blake Service")
        {
            var requiredAffiliation =
                character.RealLife[..^" Service".Length];
            if (character.Affiliation != requiredAffiliation)
            {
                issues.Add(new("Affiliation", requiredAffiliation, 0, 0));
            }
            foreach (var trait in new[]
                     {
                         "Lost Limb", "Poor Hearing", "Poor Vision", "TDS",
                         "Transit Disorientation Syndrome"
                     })
            {
                var value = FindValue(character.Traits, trait);
                if (value < -100)
                {
                    issues.Add(new("Trait",
                        $"{trait} may not exceed its lowest level", -100, value));
                }
            }
        }
        if (character.RealLife.StartsWith("Covert Operations - ",
                StringComparison.Ordinal))
        {
            var module = LifePathCatalog.RealLifeModules.Single(item =>
                item.Name == character.RealLife);
            var priorConnections = FindValue(character.Traits, "Connections") -
                module.Effects
                    .Where(effect => effect.Target == EffectTarget.Trait &&
                        effect.Name == "Connections")
                    .Sum(effect => effect.Xp);
            var priorLeadership = FindValue(character.Skills, "Leadership") -
                module.Effects
                    .Where(effect => effect.Target == EffectTarget.Skill &&
                        effect.Name == "Leadership")
                    .Sum(effect => effect.Xp);
            if (LifePathCatalog.ResolveCovertOperationsFieldSkills(character).Count == 0 &&
                (!HasPriorCareer(character, "Tour of Duty - ") ||
                 Math.Max(priorConnections, priorLeadership) < 150))
            {
                issues.Add(new("Education",
                    "Military or Intelligence/Police Field, or prior Tour of Duty with Connections or Leadership",
                    150, Math.Max(priorConnections, priorLeadership)));
            }
            if (character.Affiliation is "Invading Clan" or "Homeworld Clan")
            {
                issues.Add(new("Affiliation", "Inner Sphere or Periphery", 0, 0));
            }
            var variant = character.RealLife["Covert Operations - ".Length..];
            var affiliationMatches = variant switch
            {
                "Periphery" => character.Affiliation is "Minor Periphery" or
                    "Major Periphery State" or "Deep Periphery",
                "Independent" => character.Affiliation is not
                    ("Invading Clan" or "Homeworld Clan"),
                _ => character.Affiliation == variant
            };
            if (!affiliationMatches)
            {
                issues.Add(new("Affiliation", variant, 0, 0));
            }
            if (FindValue(character.Traits, "Combat Paralysis") < 0)
            {
                issues.Add(new("Trait", "May not have Combat Paralysis", 0,
                    FindValue(character.Traits, "Combat Paralysis")));
            }
        }
        if (character.RealLife.StartsWith("Clan Watch Operative - ",
                StringComparison.Ordinal))
        {
            var requiredAffiliation =
                character.RealLife["Clan Watch Operative - ".Length..];
            if (character.Affiliation != requiredAffiliation)
            {
                issues.Add(new("Affiliation", requiredAffiliation, 0, 0));
            }
            if (!IsWarriorCaste(character.ClanCaste) &&
                character.ClanCaste is not ("Scientist Caste" or "Technician Caste"))
            {
                issues.Add(new("Caste",
                    "Warrior, Scientist, or Technician caste", 0, 0));
            }
        }
        if (character.RealLife.StartsWith("Clan Warrior Washout - ",
                StringComparison.Ordinal))
        {
            if (character.Affiliation is not ("Invading Clan" or "Homeworld Clan"))
            {
                issues.Add(new("Affiliation", "Clan affiliation", 0, 0));
            }
            if (character.LateChildhood is not ("Freeborn Sibko" or "Trueborn Sibko"))
            {
                issues.Add(new("Background",
                    "Freeborn Sibko or Trueborn Sibko", 0, 0));
            }
            var newCaste =
                character.RealLife["Clan Warrior Washout - ".Length..];
            if (character.ClanCaste != newCaste)
            {
                issues.Add(new("Caste", newCaste, 0, 0));
            }
            if (character.ClanTrainingField.StartsWith("ProtoMech",
                    StringComparison.Ordinal))
            {
                issues.Add(new("Training",
                    "Use ProtoMech Pilot Training for a ProtoMech washout", 0, 0));
            }
        }
        if (character.RealLife == "Dark Caste")
        {
            if (character.Affiliation is not ("Invading Clan" or "Homeworld Clan"))
            {
                issues.Add(new("Affiliation", "Clan affiliation", 0, 0));
            }
            if (character.ClanCaste != "Dark Caste")
            {
                issues.Add(new("Caste", "Dark Caste after leaving Clan society", 0, 0));
            }
        }
        if (character.RealLife == "Civilian Job" &&
            character.Affiliation is "Invading Clan" or "Homeworld Clan" &&
            character.ClanCaste is not
                ("Technician Caste" or "Merchant Caste" or "Laborer Caste"))
        {
            issues.Add(new("Caste",
                "Technician, Merchant, or Laborer caste", 0, 0));
        }
        if (character.RealLife.StartsWith("Guerilla Insurgent - ",
                StringComparison.Ordinal))
        {
            if (character.Affiliation is "Invading Clan" or "Homeworld Clan")
            {
                issues.Add(new("Affiliation", "Non-Clan affiliation", 0, 0));
            }
            if (character.RealLife == "Guerilla Insurgent - Free Rasalhague" &&
                character.Affiliation != "Free Rasalhague Republic")
            {
                issues.Add(new("Affiliation",
                    "Free Rasalhague Republic", 0, 0));
            }
            if (character.RealLife == "Guerilla Insurgent - General" &&
                character.Affiliation == "Free Rasalhague Republic")
            {
                issues.Add(new("Affiliation",
                    "Use the Free Rasalhague insurgent variant", 0, 0));
            }
        }
        if (character.RealLife.StartsWith("Merchant - ", StringComparison.Ordinal))
        {
            var administrationAward = character.RealLife switch
            {
                "Merchant - Free Trader" => 25,
                "Merchant - Merchant Master" => 15,
                "Merchant - Deep Periphery Trader" => 20,
                "Merchant - Diamond Shark Warrior-Merchant" => 25,
                _ => 0
            };
            var negotiationAward =
                character.RealLife == "Merchant - Diamond Shark Warrior-Merchant"
                    ? 40
                    : 20 + (character.RealLife == "Merchant - Merchant Master" ? 15 : 0);
            if (!HasEducationField(character, "Merchant") &&
                (FindValue(character.Skills, "Administration") -
                    administrationAward < 50 ||
                 FindValue(character.Skills, "Negotiation") -
                    negotiationAward < 50))
            {
                issues.Add(new("Education",
                    "Merchant Field or 50 XP each in Administration and Negotiation",
                    50, Math.Min(
                        FindValue(character.Skills, "Administration") -
                            administrationAward,
                        FindValue(character.Skills, "Negotiation") -
                            negotiationAward)));
            }
            if (character.RealLife == "Merchant - Deep Periphery Trader" &&
                character.Affiliation != "Deep Periphery")
            {
                issues.Add(new("Affiliation", "Deep Periphery", 0, 0));
            }
            if (character.RealLife ==
                "Merchant - Diamond Shark Warrior-Merchant")
            {
                if (character.Affiliation != "Invading Clan" ||
                    character.SubAffiliation != "Diamond Shark")
                {
                    issues.Add(new("Affiliation",
                        "Invading Clan / Diamond Shark", 0, 0));
                }
                if (!IsWarriorCaste(character.ClanCaste) &&
                    character.ClanCaste != "Merchant Caste")
                {
                    issues.Add(new("Caste",
                        "Warrior or Merchant caste", 0, 0));
                }
                if (FindValue(character.Traits, "TDS") < 0 ||
                    FindValue(character.Traits,
                        "Transit Disorientation Syndrome") < 0)
                {
                    issues.Add(new("Trait", "May not have TDS", 0,
                        Math.Min(FindValue(character.Traits, "TDS"),
                            FindValue(character.Traits,
                                "Transit Disorientation Syndrome"))));
                }
            }
            else if (character.Affiliation is "Invading Clan" or "Homeworld Clan")
            {
                issues.Add(new("Affiliation",
                    "Non-Clan affiliation for this Merchant variant", 0, 0));
            }
        }
        if (character.RealLife == "Organized Crime" &&
            character.Affiliation is "Invading Clan" or "Homeworld Clan")
        {
            issues.Add(new("Affiliation",
                "Use the Clan Dark Caste Organized Crime variant", 0, 0));
        }
        if (character.RealLife == "Organized Crime - Clan Dark Caste")
        {
            if (character.Affiliation is not ("Invading Clan" or "Homeworld Clan"))
            {
                issues.Add(new("Affiliation", "Clan affiliation", 0, 0));
            }
            if (character.ClanCaste != "Dark Caste" &&
                !HasPriorCareer(character, "Dark Caste"))
            {
                issues.Add(new("Caste",
                    "Prior Dark Caste module", 0, 0));
            }
        }
        if (character.RealLife == "Solaris Insider" &&
            character.School != "Solaris Internship" &&
            FindValue(character.Traits, "Connections") < 350)
        {
            issues.Add(new("Trait",
                "Solaris Internship or 200 XP in Connections before Solaris Insider",
                200, FindValue(character.Traits, "Connections") - 150));
        }
        if (character.RealLife == "Solaris VII Games")
        {
            var arenaTraining = character.School == "Solaris Internship" ||
                HasAnyEducationField(character,
                    "MechWarrior", "Cavalry", "Pilot - Battle Armor") ||
                HasPriorCareer(character, "Tour of Duty - ") ||
                character.ClanTrainingField is
                    "MechWarrior" or "Cavalry" or "Elemental" or
                    "Elemental (Advanced)";
            if (!arenaTraining)
            {
                issues.Add(new("Training",
                    "Solaris Internship, Tour of Duty, MechWarrior, Cavalry, or Battle Armor training",
                    0, 0));
            }
            if (LifePathCatalog.ResolveSolarisGamesFieldSkills(character).Count == 0)
            {
                issues.Add(new("Education",
                    "Tech or Military Field other than Officer", 0, 0));
            }
        }
        if (character.RealLife == "Combat Correspondent" &&
            character.Affiliation is "Invading Clan" or "Homeworld Clan")
        {
            issues.Add(new("Affiliation", "Non-Clan affiliation", 0, 0));
        }
        if (character.RealLife == "Combat Correspondent" &&
            !HasEducationField(character, "Journalist"))
        {
            issues.Add(new("Education", "Journalist Field", 0, 0));
        }
        if (character.RealLife == "Combat Correspondent" &&
            FindValue(character.Traits, "Combat Paralysis") < 0)
        {
            issues.Add(new("Trait", "May not have Combat Paralysis", 0,
                FindValue(character.Traits, "Combat Paralysis")));
        }
        if (character.RealLife == "Explorer")
        {
            var clan = character.Affiliation is "Invading Clan" or "Homeworld Clan";
            if (clan && character.ClanCaste != "Scientist Caste")
            {
                issues.Add(new("Caste", "Scientist Caste", 0, 0));
            }
            if (!clan && character.Affiliation is not
                ("Minor Periphery" or "Major Periphery State" or "Deep Periphery" or
                    "Independent") &&
                FindValue(character.Traits, "Connections") < 150)
            {
                issues.Add(new("Trait", "Connections", 150,
                    FindValue(character.Traits, "Connections")));
            }
            if (FindValue(character.Traits, "TDS") < 0)
            {
                issues.Add(new("Trait", "May not have TDS", 0,
                    FindValue(character.Traits, "TDS")));
            }
        }
        if (character.RealLife == "Ne'er-Do-Well" &&
            character.Affiliation is "Invading Clan" or "Homeworld Clan")
        {
            issues.Add(new("Affiliation", "Non-Clan affiliation", 0, 0));
        }
        if (character.RealLife == "Goliath Scorpion Seeker")
        {
            if (character.SubAffiliation != "Goliath Scorpion")
            {
                issues.Add(new("Affiliation", "Clan Goliath Scorpion", 0, 0));
            }
            if (!IsWarriorCaste(character.ClanCaste))
            {
                issues.Add(new("Caste", "Warrior caste", 0, 0));
            }
        }
        if (character.RealLife == "ProtoMech Pilot Training")
        {
            var eligibleClan = character.SubAffiliation is
                "Blood Spirit" or "Fire Mandrill" or "Goliath Scorpion" or
                "Hell's Horses" or "Snow Raven";
            if (!eligibleClan)
            {
                issues.Add(new("Affiliation",
                    "Blood Spirit, Fire Mandrill, Goliath Scorpion, Hell's Horses, or Snow Raven",
                    0, 0));
            }
            if (!IsWarriorCaste(character.ClanCaste))
            {
                issues.Add(new("Caste", "Warrior caste", 0, 0));
            }
            if (character.Phenotype != "Phenotype/Aerospace" &&
                FindValue(character.Traits, "Phenotype/Aerospace") <= 0)
            {
                issues.Add(new("Phenotype", "Aerospace", 0, 0));
            }
            // The module itself awards 150 XP, so more than 150 proves it was already present.
            if (FindValue(character.Traits, "Implant/EI Neural Implant") <= 150)
            {
                issues.Add(new("Trait", "Pre-existing Implant/EI Neural Implant", 151,
                    FindValue(character.Traits, "Implant/EI Neural Implant")));
            }
            foreach (var forbidden in new[]
                     {
                         "Combat Paralysis", "Glass Jaw", "Lost Limb", "Poor Hearing",
                         "Poor Vision", "Slow Learner"
                     })
            {
                if (FindValue(character.Traits, forbidden) < 0)
                {
                    issues.Add(new("Trait", $"May not have {forbidden}", 0,
                        FindValue(character.Traits, forbidden)));
                }
            }
        }
        if (character.RealLife == "Scientist Caste Service")
        {
            if (character.Affiliation is not ("Invading Clan" or "Homeworld Clan"))
            {
                issues.Add(new("Affiliation", "Clan affiliation", 0, 0));
            }
            if (character.ClanCaste != "Scientist Caste")
            {
                issues.Add(new("Caste", "Scientist Caste", 0, 0));
            }
        }
        if (character.RealLife == "Think Tank")
        {
            var priorInt = FindValue(character.Attributes, "INT") - 90;
            var priorConnections =
                FindValue(character.Traits, "Connections") - 100;
            if (priorInt < 700)
            {
                issues.Add(new("Attribute", "INT before Think Tank", 700,
                    priorInt));
            }
            if (priorConnections < 300)
            {
                issues.Add(new("Trait", "Connections before Think Tank", 300,
                    priorConnections));
            }
            if (!HasAnyEducationField(character,
                    "Analysis", "Doctor", "Engineer", "Military Scientist"))
            {
                issues.Add(new("Education",
                    "Analysis, Doctor, Engineer, or Military Scientist Field", 0, 0));
            }
        }
        if (character.RealLife == "Travel")
        {
            if (FindValue(character.Traits, "TDS") < 0)
            {
                issues.Add(new("Trait", "May not have TDS", 0,
                    FindValue(character.Traits, "TDS")));
            }
            if (FindValue(character.Traits, "Extra Income") < 200 &&
                FindValue(character.Traits, "Wealth") < 200)
            {
                issues.Add(new("Choice", "Extra Income or Wealth", 200,
                    Math.Max(FindValue(character.Traits, "Extra Income"),
                        FindValue(character.Traits, "Wealth"))));
            }
        }
        if (character.RealLife == "To Serve and Protect" &&
            !HasAnyEducationField(character,
                "Police Officer", "Police Tactical Officer", "Detective"))
        {
            issues.Add(new("Education",
                "Police Officer, Police Tactical Officer, or Detective Field", 0, 0));
        }
        if (character.RealLife == "Postgraduate Studies" &&
            character.School != "University")
        {
            issues.Add(new("Education", "University", 0, 0));
        }
        if (character.RealLife == "Cloister Training")
        {
            if (character.Affiliation is not ("Invading Clan" or "Homeworld Clan"))
            {
                issues.Add(new("Affiliation", "Clan affiliation", 0, 0));
            }
            if (!IsWarriorCaste(character.ClanCaste))
            {
                issues.Add(new("Caste", "Warrior caste", 0, 0));
            }
            if (FindValue(character.Attributes, "WIL") < 575)
            {
                issues.Add(new("Attribute", "WIL before Cloister Training", 500,
                    FindValue(character.Attributes, "WIL") - 75));
            }
            if (character.SubAffiliation != "Cloud Cobra" &&
                FindValue(character.Traits, "Connections") < 250)
            {
                issues.Add(new("Trait", "Connections before Cloister Training", 200,
                    FindValue(character.Traits, "Connections") - 50));
            }
            if (LifePathCatalog.ResolveClanWarriorFieldSkills(character).Count == 0)
            {
                issues.Add(new("Education", "Clan Warrior Field", 0, 0));
            }
        }
        if (character.RealLife.StartsWith("Tour of Duty - ", StringComparison.Ordinal))
        {
            var validAffiliation = character.RealLife switch
            {
                "Tour of Duty - Periphery" =>
                    character.Affiliation is "Minor Periphery" or
                        "Major Periphery State" or "Deep Periphery" or "Independent",
                "Tour of Duty - Clan" =>
                    character.Affiliation is "Invading Clan" or "Homeworld Clan",
                "Tour of Duty - Inner Sphere" =>
                    character.Affiliation is "Federated Suns" or
                        "Capellan Confederation" or "Draconis Combine" or
                        "Free Worlds League" or "Lyran Alliance" or
                        "Free Rasalhague Republic" or "ComStar" or
                        "Word of Blake" or "Terran",
                _ => false
            };
            if (!validAffiliation)
            {
                issues.Add(new("Affiliation",
                    character.RealLife["Tour of Duty - ".Length..], 0, 0));
            }
            if (LifePathCatalog.ResolveMilitaryFieldSkills(character).Count == 0)
            {
                issues.Add(new("Education", "Military Skill Field", 0, 0));
            }
        }

        return issues;
    }

    private static bool HasPriorCareer(Character character, string name)
    {
        var priorCareers = character.RealLifeHistory.Count > 0
            ? character.RealLifeHistory.Take(
                Math.Max(0, character.RealLifeHistory.Count - 1))
            : [];
        return name.EndsWith(" - ", StringComparison.Ordinal)
            ? priorCareers.Any(career =>
                career.StartsWith(name, StringComparison.Ordinal))
            : priorCareers.Contains(name, StringComparer.Ordinal);
    }

    private static bool HasEducationField(Character character, string field) =>
        character.BasicSchool == field ||
        character.AdvancedSchool == field ||
        character.SpecialSchool == field;

    private static bool HasAnyEducationField(Character character, params string[] fields) =>
        fields.Any(field => HasEducationField(character, field));

    private static bool IsWarriorCaste(string caste) =>
        caste is "MechWarrior" or "Elemental" or "Elemental-Advanced" or
            "Aerospace" or "ProtoMech" or "Aerospace-Naval" or
            "Warrior Caste (Other)";

    private static void AddIfMissing(
        ICollection<PrerequisiteIssue> issues,
        string category,
        NamedValue requirement,
        int actual)
    {
        if (actual < requirement.Value)
        {
            issues.Add(new(category, requirement.Name, requirement.Value, actual));
        }
    }

    private static bool HasAny(IEnumerable<NamedValue> values, int minimum, params string[] names) =>
        values.Any(item => names.Contains(item.Name, StringComparer.Ordinal) && item.Value >= minimum);

    private static int FindValue(IEnumerable<NamedValue> values, string name) =>
        values.FirstOrDefault(item => item.Name == name)?.Value ?? 0;
}

public sealed record PrerequisiteIssue(string Category, string Name, int RequiredXp, int ActualXp);
