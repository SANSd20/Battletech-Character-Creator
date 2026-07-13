using System.Globalization;
using System.Text;
using BattletechCharacterCreator.Core.Models;

namespace BattletechCharacterCreator.Core.Persistence;

public static class LegacyCharacterSerializer
{
    public static Character Load(string path)
    {
        using var reader = new StreamReader(path, Encoding.UTF8, true);
        return Read(reader);
    }

    public static Character Read(TextReader reader)
    {
        var character = new Character();
        character.Skills.Clear();
        character.Traits.Clear();
        var notes = new StringBuilder();
        var readingNotes = false;

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (line == "<notes>")
            {
                readingNotes = true;
                continue;
            }

            if (line == "</notes>")
            {
                readingNotes = false;
                continue;
            }

            if (readingNotes)
            {
                if (notes.Length > 0)
                {
                    notes.Append('\n');
                }
                notes.Append(line);
                continue;
            }

            var separator = line.IndexOf(':');
            if (separator <= 0)
            {
                continue;
            }

            Apply(character, line[..separator], line[(separator + 1)..]);
        }

        character.Notes = notes.ToString();
        if (character.RealLifeHistory.Count == 0 &&
            character.RealLife.Length > 0)
        {
            character.RealLifeHistory.Add(character.RealLife);
        }
        return character;
    }

    public static void Save(Character character, string path)
    {
        var tempPath = path + ".tmp";
        using (var writer = new StreamWriter(tempPath, false, new UTF8Encoding(false)))
        {
            Write(character, writer);
        }
        File.Move(tempPath, path, true);
    }

    public static void Write(Character c, TextWriter writer)
    {
        WriteValue(writer, "name", c.Name);
        WriteValue(writer, "aff", c.Affiliation);
        WriteValue(writer, "subaff", c.SubAffiliation);
        WriteValue(writer, "birthaff", c.BirthAffiliation);
        WriteValue(writer, "birthsubaff", c.BirthSubAffiliation);
        WriteValue(writer, "clancaste", c.ClanCaste);
        WriteValue(writer, "clantraining", c.ClanTrainingField);
        WriteValue(writer, "earlychild", c.EarlyChildhood);
        WriteValue(writer, "latechild", c.LateChildhood);
        WriteValue(writer, "schoolname", c.School);
        WriteValue(writer, "basicschool", c.BasicSchool);
        WriteValue(writer, "advschool", c.AdvancedSchool);
        WriteValue(writer, "specschool", c.SpecialSchool);
        WriteValue(writer, "reallife", c.RealLife);
        foreach (var career in c.RealLifeHistory)
        {
            WriteValue(writer, "reallifehistory", career);
        }
        WriteValue(writer, "phenotype", c.Phenotype);
        WriteValue(writer, "nameplanet", c.HomePlanet);
        WriteValue(writer, "sex", c.Sex);
        WriteValue(writer, "birthyear", c.BirthYear);
        WriteValue(writer, "gameyear", c.GameYear);
        WriteValue(writer, "age", c.Age);
        WriteValue(writer, "haircolor", c.HairColor);
        WriteValue(writer, "eyecolor", c.EyeColor);
        WriteValue(writer, "height", c.Height);
        WriteValue(writer, "weight", c.Weight);
        WriteValue(writer, "gmxpmod", c.GmXpModifier);
        WriteValue(writer, "cbillmod", c.CBillModifier);

        WriteNamedValues(writer, "attr", c.Attributes);
        WriteNamedValues(writer, "skill", c.Skills);
        WriteNamedValues(writer, "trait", c.Traits);
        WriteNamedValues(writer, "preattr", c.PreAttributes);
        WriteNamedValues(writer, "preskill", c.PreSkills);
        WriteNamedValues(writer, "pretrait", c.PreTraits);

        foreach (var item in c.Equipment)
        {
            WriteValue(writer, "equip", string.Join(';', item.Name, item.Cost, item.Mass,
                item.Locations, item.Armor, item.Notes, item.PatchCount, item.Count));
        }

        foreach (var pair in c.EquipmentLocations)
        {
            WriteValue(writer, "equiploc", $"{pair.Key}={pair.Value}");
        }

        foreach (var item in c.Weapons)
        {
            WriteValue(writer, "weapon", string.Join(';', item.Skill, item.Name, item.Damage,
                item.Range, item.Cost, item.Mass, item.Shots, item.AmmoCost, item.AmmoMass,
                item.AmmoCount, item.AmmoModifier, item.AmmoCostModifier,
                item.AmmoMassModifier, item.Notes, item.Count));
        }

        foreach (var weapon in c.EquippedWeapons)
        {
            WriteValue(writer, "chrweapon", weapon);
        }

        writer.WriteLine("<notes>");
        writer.WriteLine(c.Notes);
        writer.WriteLine("</notes>");
    }

    private static void Apply(Character c, string key, string value)
    {
        switch (key)
        {
            case "name": c.Name = value; break;
            case "aff": c.Affiliation = value; break;
            case "subaff": c.SubAffiliation = value; break;
            case "birthaff": c.BirthAffiliation = value; break;
            case "birthsubaff": c.BirthSubAffiliation = value; break;
            case "clancaste": c.ClanCaste = value; break;
            case "clantraining": c.ClanTrainingField = value; break;
            case "earlychild": c.EarlyChildhood = value; break;
            case "latechild": c.LateChildhood = value; break;
            case "schoolname": c.School = value; break;
            case "basicschool": c.BasicSchool = value; break;
            case "advschool": c.AdvancedSchool = value; break;
            case "specschool": c.SpecialSchool = value; break;
            case "reallife": c.RealLife = value; break;
            case "reallifehistory": c.RealLifeHistory.Add(value); break;
            case "phenotype": c.Phenotype = value; break;
            case "nameplanet": c.HomePlanet = value; break;
            case "sex": c.Sex = value; break;
            case "birthyear": c.BirthYear = ParseInt(value, c.BirthYear); break;
            case "gameyear": c.GameYear = ParseInt(value, c.GameYear); break;
            case "age": c.Age = ParseInt(value, c.Age); break;
            case "haircolor": c.HairColor = value; break;
            case "eyecolor": c.EyeColor = value; break;
            case "height": c.Height = ParseInt(value, c.Height); break;
            case "weight": c.Weight = ParseInt(value, c.Weight); break;
            case "gmxpmod": c.GmXpModifier = ParseInt(value, 0); break;
            case "cbillmod": c.CBillModifier = ParseInt(value, 0); break;
            case "attr": SetNamedValue(c.Attributes, value); break;
            case "skill": AddNamedValue(c.Skills, value); break;
            case "trait": AddNamedValue(c.Traits, value); break;
            case "preattr": AddNamedValue(c.PreAttributes, value); break;
            case "preskill": AddNamedValue(c.PreSkills, value); break;
            case "pretrait": AddNamedValue(c.PreTraits, value); break;
            case "equip": AddEquipment(c, value); break;
            case "weapon": AddWeapon(c, value); break;
            case "chrweapon": c.EquippedWeapons.Add(value); break;
            case "equiploc": AddLocation(c, value); break;
        }
    }

    private static void AddEquipment(Character c, string value)
    {
        var f = value.Split(';');
        if (f.Length is not (7 or 8)) return;
        c.Equipment.Add(new EquipmentItem
        {
            Name = f[0], Cost = f[1], Mass = f[2], Locations = f[3],
            Armor = f[4],
            Notes = f[5],
            PatchCount = f.Length == 8 ? f[6] : "0",
            Count = f.Length == 8 ? f[7] : f[6]
        });
    }

    private static void AddWeapon(Character c, string value)
    {
        var f = value.Split(';');
        if (f.Length is not (11 or 12 or 15)) return;
        var item = new WeaponItem
        {
            Skill = f[0], Name = f[1], Damage = f[2], Range = f[3], Cost = f[4],
            Mass = f[5], Shots = f[6], AmmoCost = f[7], AmmoMass = f[8],
            AmmoCount = f.Length == 11 ? "0" : f[9]
        };

        if (f.Length == 15)
        {
            item.AmmoModifier = f[10];
            item.AmmoCostModifier = f[11];
            item.AmmoMassModifier = f[12];
            item.Notes = f[13];
            item.Count = f[14];
        }
        else
        {
            item.Notes = f.Length == 12 ? f[10] : f[9];
            item.Count = f.Length == 12 ? f[11] : f[10];
        }

        c.Weapons.Add(item);
    }

    private static void AddLocation(Character c, string value)
    {
        var separator = value.IndexOf('=');
        if (separator > 0)
        {
            c.EquipmentLocations[value[..separator]] = value[(separator + 1)..];
        }
    }

    private static void SetNamedValue(ICollection<NamedValue> values, string value)
    {
        if (!TryParseNamedValue(value, out var name, out var number)) return;
        var existing = values.FirstOrDefault(item => item.Name == name);
        if (existing is null) values.Add(new NamedValue(name, number));
        else existing.Value = number;
    }

    private static void AddNamedValue(ICollection<NamedValue> values, string value)
    {
        if (TryParseNamedValue(value, out var name, out var number))
        {
            values.Add(new NamedValue(name, number));
        }
    }

    private static bool TryParseNamedValue(string value, out string name, out int number)
    {
        var separator = value.LastIndexOf('=');
        name = separator > 0 ? value[..separator] : "";
        number = 0;
        return separator > 0 &&
            int.TryParse(value[(separator + 1)..], NumberStyles.Integer,
                CultureInfo.InvariantCulture, out number);
    }

    private static int ParseInt(string value, int fallback) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            ? number : fallback;

    private static void WriteNamedValues(TextWriter writer, string key, IEnumerable<NamedValue> values)
    {
        foreach (var item in values)
        {
            WriteValue(writer, key, $"{item.Name}={item.Value.ToString(CultureInfo.InvariantCulture)}");
        }
    }

    private static void WriteValue(TextWriter writer, string key, object value) =>
        writer.WriteLine($"{key}:{value}");
}
