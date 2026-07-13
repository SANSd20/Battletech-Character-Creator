using System.Collections.ObjectModel;

namespace BattletechCharacterCreator.Core.Models;

public sealed class Character
{
    public string Name { get; set; } = "New Character";
    public string Affiliation { get; set; } = "";
    public string SubAffiliation { get; set; } = "";
    public string BirthAffiliation { get; set; } = "";
    public string BirthSubAffiliation { get; set; } = "";
    public string ClanCaste { get; set; } = "";
    public string ClanTrainingField { get; set; } = "";
    public string EarlyChildhood { get; set; } = "";
    public string LateChildhood { get; set; } = "";
    public string School { get; set; } = "";
    public string BasicSchool { get; set; } = "";
    public string AdvancedSchool { get; set; } = "";
    public string SpecialSchool { get; set; } = "";
    public string RealLife { get; set; } = "";
    public string Phenotype { get; set; } = "Phenotype/Normal Human";
    public string HomePlanet { get; set; } = "";
    public string Sex { get; set; } = "Male";
    public int BirthYear { get; set; } = 3024;
    public int GameYear { get; set; } = 3045;
    public int Age
    {
        get => Math.Max(0, GameYear - BirthYear);
        set
        {
            if (value > 0)
            {
                BirthYear = GameYear - value;
            }
        }
    }
    public string HairColor { get; set; } = "";
    public string EyeColor { get; set; } = "";
    public int Height { get; set; } = 175;
    public int Weight { get; set; } = 80;
    public int GmXpModifier { get; set; }
    public int CBillModifier { get; set; }
    public string Notes { get; set; } = "";

    public ObservableCollection<NamedValue> Attributes { get; } =
    [
        new("STR", 100), new("BOD", 100), new("RFL", 100), new("DEX", 100),
        new("INT", 100), new("WIL", 100), new("CHA", 100), new("EDG", 100)
    ];

    public ObservableCollection<NamedValue> Skills { get; } = [];
    public ObservableCollection<NamedValue> Traits { get; } = [];
    public ObservableCollection<NamedValue> PreAttributes { get; } = [];
    public ObservableCollection<NamedValue> PreSkills { get; } = [];
    public ObservableCollection<NamedValue> PreTraits { get; } = [];
    public ObservableCollection<string> EducationHistory { get; } = [];
    public ObservableCollection<string> EducationFields { get; } = [];
    public ObservableCollection<string> RealLifeHistory { get; } = [];
    public ObservableCollection<EquipmentItem> Equipment { get; } = [];
    public ObservableCollection<WeaponItem> Weapons { get; } = [];
    public ObservableCollection<string> EquippedWeapons { get; } = [];
    public Dictionary<string, string> EquipmentLocations { get; } = [];
}

public sealed class NamedValue(string name, int value)
{
    public string Name { get; set; } = name;
    public int Value { get; set; } = value;
}

public sealed class EquipmentItem
{
    public string Name { get; set; } = "";
    public string Cost { get; set; } = "";
    public string Mass { get; set; } = "";
    public string Locations { get; set; } = "";
    public string Armor { get; set; } = "";
    public string Notes { get; set; } = "";
    public string PatchCount { get; set; } = "0";
    public string Count { get; set; } = "1";
}

public sealed class WeaponItem
{
    public string Category { get; set; } = "";
    public string Skill { get; set; } = "";
    public string Name { get; set; } = "";
    public string Damage { get; set; } = "";
    public string Range { get; set; } = "";
    public string Cost { get; set; } = "";
    public string Mass { get; set; } = "";
    public string Shots { get; set; } = "";
    public string AmmoCost { get; set; } = "";
    public string AmmoMass { get; set; } = "";
    public string AmmoCount { get; set; } = "0";
    public string AmmoModifier { get; set; } = "";
    public string AmmoDamageModifier { get; set; } = "";
    public string AmmoRangeModifier { get; set; } = "";
    public string AmmoCostModifier { get; set; } = "";
    public string AmmoMassModifier { get; set; } = "";
    public string AmmoRequiredAccessories { get; set; } = "";
    public string Notes { get; set; } = "";
    public string Count { get; set; } = "1";
}
