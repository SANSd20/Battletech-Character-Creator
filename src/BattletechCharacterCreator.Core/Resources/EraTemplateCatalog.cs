using BattletechCharacterCreator.Core.Models;

namespace BattletechCharacterCreator.Core.Resources;

public sealed record EraTemplateValue(string Name, int Xp);

public sealed record EraCharacterTemplate(
    string Name,
    int CampaignYear,
    string Affiliation,
    string SubAffiliation,
    string Description,
    IReadOnlyList<EraTemplateValue> Skills,
    IReadOnlyList<EraTemplateValue> Traits)
{
    public string DisplayName => $"{Name} ({CampaignYear})";
}

public static class EraTemplateCatalog
{
    public static IReadOnlyList<EraCharacterTemplate> Templates { get; } =
    [
        new(
            "Star League Technician",
            2750,
            "Terran",
            "Terran Citizen",
            "A Star League-era technical specialist with Terran roots.",
            [
                new("Technician/Electronic", 30),
                new("Communications/HPG", 20),
                new("Protocol/Terran", 15)
            ],
            [
                new("Equipped", 50)
            ]),
        new(
            "Clan Invasion Rasalhague Survivor",
            3052,
            "Free Rasalhague Republic",
            "Clan War Expatriate",
            "A Rasalhague survivor shaped by the Clan Invasion.",
            [
                new("Language/Swedish", 20),
                new("Survival/City", 15),
                new("Perception", 15)
            ],
            [
                new("Connections", 25)
            ]),
        new(
            "Civil War Mercenary",
            3062,
            "Independent",
            "Mercenary",
            "A freelance combatant ready for Civil War-era contracts.",
            [
                new("Small Arms", 25),
                new("Tactics/Land", 20),
                new("Negotiation", 10)
            ],
            [
                new("Equipped", 50),
                new("Reputation", 25)
            ]),
        new(
            "Late Dark Age Scout",
            3145,
            "Independent",
            "Spacer",
            "A mobile scout for late Dark Age travel and reconnaissance.",
            [
                new("Navigation/Space", 20),
                new("Perception", 20),
                new("Piloting/Spacecraft", 15)
            ],
            [
                new("Good Vision", 50)
            ])
    ];

    public static void Apply(Character character, EraCharacterTemplate template)
    {
        character.Name = template.Name;
        character.GameYear = template.CampaignYear;
        character.Affiliation = template.Affiliation;
        character.SubAffiliation = template.SubAffiliation;
        character.Notes = template.Description;

        ApplyValues(character.Skills, template.Skills);
        ApplyValues(character.Traits, template.Traits);
    }

    private static void ApplyValues(
        ICollection<NamedValue> target,
        IEnumerable<EraTemplateValue> values)
    {
        foreach (var value in values)
        {
            var existing = target.FirstOrDefault(item =>
                item.Name.Equals(value.Name, StringComparison.Ordinal));
            if (existing is null)
            {
                target.Add(new NamedValue(value.Name, value.Xp));
            }
            else
            {
                existing.Value = value.Xp;
            }
        }
    }
}
