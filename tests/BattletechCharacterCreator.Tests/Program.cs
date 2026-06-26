using BattletechCharacterCreator.Core.Models;
using BattletechCharacterCreator.Core.Persistence;
using BattletechCharacterCreator.Core.Resources;
using BattletechCharacterCreator.Core.Rules;
using BattletechCharacterCreator.Core.LifePath;

var character = new Character
{
    Name = "Test: Pilot",
    Notes = "Line one\nLine: two"
};
character.Skills.Add(new NamedValue("Piloting/'Mech", 80));
character.Equipment.Add(new EquipmentItem
{
    Name = "Helmet", Cost = "300", Mass = "1", Locations = "H",
    Armor = "5/6/5/2", Notes = "p.292", PatchCount = "2", Count = "1"
});
character.Weapons.Add(new WeaponItem
{
    Skill = "Small Arms", Name = "Laser Pistol", Damage = "3E/3B",
    Range = "15/35/80/200", Cost = "1500", Mass = "1", Shots = "3 PPS",
    AmmoCost = "5", AmmoMass = "0.25", AmmoCount = "4", Notes = "Burst 5", Count = "1"
});
character.RealLife = "Solaris VII Games";
character.BirthAffiliation = "Federated Suns";
character.BirthSubAffiliation = "Crucis March";
character.BirthYear = 3028;
character.GameYear = 3054;
character.ClanCaste = "MechWarrior";
character.ClanTrainingField = "MechWarrior";
character.Phenotype = "Phenotype/MechWarrior";
character.RealLifeHistory.Add("Tour of Duty - Inner Sphere");
character.RealLifeHistory.Add("Solaris VII Games");

using var output = new StringWriter();
LegacyCharacterSerializer.Write(character, output);
using var input = new StringReader(output.ToString());
var loaded = LegacyCharacterSerializer.Read(input);

Assert(loaded.Name == character.Name, "Names containing colons must round-trip.");
Assert(loaded.Notes == character.Notes, "Multiline notes must round-trip.");
Assert(loaded.Skills.Single().Name == "Piloting/'Mech", "Skills must round-trip.");
Assert(loaded.Equipment.Single().PatchCount == "2" &&
    loaded.Equipment.Single().Notes == "p.292",
    "All eight equipment fields must round-trip.");
var legacyEquipment = LegacyCharacterSerializer.Read(new StringReader(
    "equip:Flak/Jacket;150/10;4;Torso;1/5/1/3;legacy notes;2"));
Assert(legacyEquipment.Equipment.Single().PatchCount == "0" &&
    legacyEquipment.Equipment.Single().Notes == "legacy notes",
    "Older seven-field equipment rows must load with no purchased patches.");
Assert(loaded.Weapons.Single().AmmoCount == "4" &&
    loaded.Weapons.Single().Notes == "Burst 5",
    "All twelve weapon fields must round-trip.");
var legacyWeapon = LegacyCharacterSerializer.Read(new StringReader(
    "weapon:Small Arms;Hold-Out Pistol;1B/2;3/6/12/25;75;0.2;1;10;0.1;legacy notes;2"));
Assert(legacyWeapon.Weapons.Single().AmmoCount == "0" &&
    legacyWeapon.Weapons.Single().Notes == "legacy notes",
    "Older eleven-field weapon rows must load with no purchased ammo.");
Assert(loaded.RealLifeHistory.SequenceEqual(character.RealLifeHistory),
    "Stage 4 career history must round-trip in order.");
Assert(loaded.BirthAffiliation == character.BirthAffiliation &&
    loaded.BirthSubAffiliation == character.BirthSubAffiliation,
    "Order birth affiliation details must round-trip.");
Assert(loaded.BirthYear == character.BirthYear &&
    loaded.GameYear == character.GameYear &&
    loaded.Age == 26,
    "Birth year, game year, and calculated age must round-trip.");
var legacyAgeCharacter = LegacyCharacterSerializer.Read(new StringReader(
    "age:32"));
Assert(legacyAgeCharacter.GameYear == 3045 &&
    legacyAgeCharacter.BirthYear == 3013 &&
    legacyAgeCharacter.Age == 32,
    "Legacy age-only files must load as birth year and game year.");
Assert(loaded.ClanCaste == character.ClanCaste &&
    loaded.ClanTrainingField == character.ClanTrainingField &&
    loaded.Phenotype == character.Phenotype,
    "Clan caste, training, and phenotype must round-trip.");

CheckResourceCatalog();
CheckEraInference();
CheckEraAvailability();
CheckEraTemplates();

Assert(CharacterRules.AttributeValue(99) == 1, "Sub-100 attributes have value 1.");
Assert(CharacterRules.AttributeValue(400) == 4, "Attribute XP converts to its value.");
Assert(CharacterRules.LinkModifier(7) == 1, "Attribute values convert to link modifiers.");
Assert(CharacterRules.CarryingCapacity(400) == 20m, "Strength determines carrying capacity.");
Assert(CharacterRules.SkillLevel(80, []) == 3, "Normal skill thresholds match the legacy app.");
Assert(CharacterRules.SkillLevel(64, [new NamedValue("Fast Learner", 300)]) == 3,
    "Fast Learner reduces skill thresholds by 20 percent.");
Assert(CharacterRules.TraitLevel("Wealth", -95) == -1,
    "Negative trait XP uses floor semantics.");
Assert(CharacterRules.TraitLevel("Mutation", 450) == 3 &&
    CharacterRules.TraitLevel("Mutation", -650) == -5,
    "Companion Mutation trait XP must clamp to its printed range.");
Assert(CharacterRules.StartingCBills(3) == 10_000, "Wealth level determines starting C-Bills.");
Assert(CharacterRules.BasePurchaseCost("500/100") == 500,
    "Slash-separated catalog costs must expose the base purchase price.");
Assert(CharacterRules.SecondaryPurchaseCost("500/100") == 100,
    "Slash-separated catalog costs must expose the patch purchase price.");
Assert(CharacterRules.BasePurchaseCost("1,400,000*") == 1_400_000,
    "Comma and wildcard catalog costs must expose the numeric base price.");
Assert(CharacterRules.BasePurchaseCost("*") == 0,
    "Pure wildcard catalog costs must stay unresolved.");
Assert(!CharacterRules.HasUnresolvedPurchaseCost("1,400,000*") &&
    CharacterRules.HasUnresolvedPurchaseCost("*"),
    "Only pure wildcard catalog costs must be reported as unresolved.");
Assert(CharacterRules.CatalogMass("1,600") == 1600m,
    "Catalog mass parsing must accept grouped numbers.");
Assert(CharacterRules.ItemCount("0") == 1 &&
    CharacterRules.ItemCount("3") == 3,
    "Inventory item counts must default to one and accept positive counts.");
var inventoryCharacter = new Character();
inventoryCharacter.Equipment.Add(new EquipmentItem
{
    Name = "Test equipment", Cost = "100/7", Mass = "2", PatchCount = "5", Count = "3"
});
inventoryCharacter.Weapons.Add(new WeaponItem
{
    Name = "Test weapon", Cost = "200", Mass = "1.5", Count = "2",
    AmmoCost = "10", AmmoMass = "0.25", AmmoCount = "4"
});
var inventorySummary = CharacterRules.Calculate(inventoryCharacter);
Assert(inventorySummary.InventoryBaseCost == 775,
    "Inventory base cost must total equipment, weapons, purchased patches, and purchased ammo.");
Assert(inventorySummary.RemainingCBills == 225,
    "Inventory cost must multiply each item, patch, and ammo pack cost by quantity.");
Assert(inventorySummary.InventoryMass == 10m,
    "Inventory mass must multiply each item's and ammo pack's mass by quantity.");
var companionInventoryCharacter = new Character();
companionInventoryCharacter.Equipment.Add(new EquipmentItem
{
    Name = "Patched armor", Cost = "500/100", Mass = "3.1", Count = "2"
});
companionInventoryCharacter.Equipment.Add(new EquipmentItem
{
    Name = "Wildcard implant", Cost = "*", Mass = "0", Count = "4"
});
companionInventoryCharacter.Weapons.Add(new WeaponItem
{
    Name = "Expensive support weapon", Cost = "1,400,000*", Mass = "1,600", Count = "1",
    AmmoCost = "*", AmmoCount = "2"
});
var companionInventorySummary = CharacterRules.Calculate(companionInventoryCharacter);
Assert(companionInventorySummary.InventoryBaseCost == 1_401_000,
    "Inventory base cost must expose the parsed base total.");
Assert(companionInventorySummary.RemainingCBills == -1_400_000,
    "Inventory cost must use base prices from slash, comma, and wildcard formats.");
Assert(companionInventorySummary.InventoryMass == 1606.2m,
    "Inventory mass must continue parsing comma and decimal quantities.");
Assert(companionInventorySummary.UnresolvedInventoryPrices == 6,
    "Inventory summaries must count unresolved wildcard purchase prices by quantity.");
var patchPriceWarningCharacter = new Character();
patchPriceWarningCharacter.Equipment.Add(new EquipmentItem
{
    Name = "Patch warning armor", Cost = "500", Mass = "2", PatchCount = "3"
});
Assert(CharacterRules.PatchPurchasesNeedingPrice(patchPriceWarningCharacter) == 3,
    "Patch purchases must warn when no patch price is present.");
patchPriceWarningCharacter.Equipment.Clear();
patchPriceWarningCharacter.Equipment.Add(new EquipmentItem
{
    Name = "Priced patch armor", Cost = "500/100", Mass = "2", PatchCount = "3"
});
Assert(CharacterRules.PatchPurchasesNeedingPrice(patchPriceWarningCharacter) == 0,
    "Patch purchase warnings must clear when a patch price is present.");
patchPriceWarningCharacter.Equipment.Clear();
patchPriceWarningCharacter.Equipment.Add(new EquipmentItem
{
    Name = "Wildcard patch armor", Cost = "500/*", Mass = "2", PatchCount = "2"
});
Assert(CharacterRules.Calculate(patchPriceWarningCharacter).UnresolvedInventoryPrices == 2,
    "Wildcard patch purchase prices must count as unresolved inventory prices.");
var ammoDetailWarningCharacter = new Character();
ammoDetailWarningCharacter.Weapons.Add(new WeaponItem
{
    Name = "Missing ammo cost", Cost = "100", Mass = "1",
    AmmoCost = "", AmmoMass = "0.1", AmmoCount = "3"
});
Assert(CharacterRules.AmmoPurchasesNeedingDetails(ammoDetailWarningCharacter) == 3,
    "Ammo purchases must warn when ammo cost is missing.");
ammoDetailWarningCharacter.Weapons.Clear();
ammoDetailWarningCharacter.Weapons.Add(new WeaponItem
{
    Name = "Missing ammo mass", Cost = "100", Mass = "1",
    AmmoCost = "5", AmmoMass = "", AmmoCount = "2"
});
Assert(CharacterRules.AmmoPurchasesNeedingDetails(ammoDetailWarningCharacter) == 2,
    "Ammo purchases must warn when ammo mass is missing.");
ammoDetailWarningCharacter.Weapons.Clear();
ammoDetailWarningCharacter.Weapons.Add(new WeaponItem
{
    Name = "Complete ammo", Cost = "100", Mass = "1",
    AmmoCost = "5", AmmoMass = "0.1", AmmoCount = "2"
});
Assert(CharacterRules.AmmoPurchasesNeedingDetails(ammoDetailWarningCharacter) == 0,
    "Ammo purchase warnings must clear when ammo cost and mass are present.");
var reloadReviewCharacter = new Character();
reloadReviewCharacter.Weapons.Add(new WeaponItem
{
    Name = "Missing shot capacity", Cost = "100", Mass = "1", Shots = "",
    AmmoCost = "5", AmmoMass = "0.1", AmmoCount = "2"
});
Assert(CharacterRules.AmmoPurchasesNeedingReloadReview(reloadReviewCharacter) == 2,
    "Ammo purchases must warn when weapon shot capacity is missing.");
reloadReviewCharacter.Weapons.Clear();
reloadReviewCharacter.Weapons.Add(new WeaponItem
{
    Name = "Power-pack weapon", Cost = "100", Mass = "1", Shots = "5 PPS",
    AmmoCost = "5", AmmoMass = "0.1", AmmoCount = "3"
});
Assert(CharacterRules.AmmoPurchasesNeedingReloadReview(reloadReviewCharacter) == 3,
    "Ammo purchases must warn when weapon shots use power-pack notation.");
reloadReviewCharacter.Weapons.Clear();
reloadReviewCharacter.Weapons.Add(new WeaponItem
{
    Name = "Mixed ammo weapon", Cost = "100", Mass = "1", Shots = "10, 10 PPS",
    AmmoCost = "5", AmmoMass = "0.1", AmmoCount = "1"
});
Assert(CharacterRules.AmmoPurchasesNeedingReloadReview(reloadReviewCharacter) == 1,
    "Ammo purchases must warn when weapon shots combine ammunition and power-pack notation.");
reloadReviewCharacter.Weapons.Clear();
reloadReviewCharacter.Weapons.Add(new WeaponItem
{
    Name = "Numeric ammo weapon", Cost = "100", Mass = "1", Shots = "10",
    AmmoCost = "5", AmmoMass = "0.1", AmmoCount = "2"
});
Assert(CharacterRules.AmmoPurchasesNeedingReloadReview(reloadReviewCharacter) == 0,
    "Ammo reload review warnings must clear for numeric shot capacity.");
var prostheticWarningCharacter = new Character();
prostheticWarningCharacter.Equipment.Add(new EquipmentItem
{
    Name = "Prosthetic Enhancement - Vibroblade", Cost = "1000", Mass = "0",
    Locations = "Prosthetic", Count = "2"
});
Assert(CharacterRules.UnmountedProstheticEnhancements(prostheticWarningCharacter) == 2,
    "Prosthetic enhancements must warn when no prosthetic or implant host is present.");
prostheticWarningCharacter.Equipment.Add(new EquipmentItem
{
    Name = "Gill Implant", Cost = "8000", Mass = "0", Locations = "Implant", Count = "1"
});
Assert(CharacterRules.UnmountedProstheticEnhancements(prostheticWarningCharacter) == 0,
    "Prosthetic enhancement warnings must clear when an implant host is present.");
var vehicleWarningCharacter = new Character();
vehicleWarningCharacter.Equipment.Add(new EquipmentItem
{
    Name = "Hoodling Sensor HoverJeep", Cost = "92000", Mass = "0",
    Locations = "Vehicle", Count = "2"
});
Assert(CharacterRules.UnbackedVehiclePurchases(vehicleWarningCharacter) == 2,
    "Vehicle purchases must warn when no Vehicle or Custom Vehicle trait is present.");
vehicleWarningCharacter.Traits.Add(new NamedValue("Vehicle", 100));
Assert(CharacterRules.UnbackedVehiclePurchases(vehicleWarningCharacter) == 0,
    "Vehicle purchase warnings must clear when the Vehicle trait is present.");
vehicleWarningCharacter.Traits.Clear();
vehicleWarningCharacter.Traits.Add(new NamedValue("Custom Vehicle", 100));
Assert(CharacterRules.UnbackedVehiclePurchases(vehicleWarningCharacter) == 0,
    "Vehicle purchase warnings must clear when the Custom Vehicle trait is present.");

CheckSample("newchar.btcc", 2_900, 100, 6, 18, 36);
CheckSample("test.btcc", 4_212, 5_494, 6, 18, 36);
CheckSample("lisa.btcc", 3_610, 1_000, 3, 14, 28);
CheckLifePath();
CheckExpandedLifePaths();
CheckLateChildhoods();
CheckEducation();
CheckLifePathAccounting();
CheckFlexibleAllocations();
CheckRealLifeModules();

Console.WriteLine("All migration tests passed.");

static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static void CheckResourceCatalog()
{
    var resources = Path.Combine(AppContext.BaseDirectory, "Resources");
    var catalog = ResourceCatalog.Load(resources);
    var companionCatalog = ResourceCatalog.Load(
        resources,
        new ResourceCatalogOptions(IncludeCompanion: true));
    Assert(catalog.Equipment.Count == 187,
        "All 187 legacy equipment entries must be imported.");
    Assert(catalog.Weapons.Count == 209,
        "All 209 legacy weapon entries must be imported.");
    Assert(companionCatalog.Equipment.Count == 286,
        "Companion-enabled equipment must include the starter Companion import.");
    Assert(companionCatalog.Weapons.Count == 225,
        "Companion-enabled weapons must include the starter Companion import.");
    Assert(catalog.Skills.Count == 119 &&
        catalog.Traits.Count == 76 &&
        catalog.Careers.Count == 26,
        "Skill, trait, and career reference catalogs must be complete.");
    Assert(companionCatalog.Traits.Count == 82,
        "Companion-enabled traits must include the expanded trait import.");
    Assert(catalog.SkillDescriptions.Count == 92 &&
        catalog.TraitDescriptions.Count == 56,
        "All available skill and trait descriptions must be imported.");
    Assert(companionCatalog.TraitDescriptions.Count == 62,
        "Companion-enabled trait descriptions must include expanded trait notes.");
    Assert(catalog.Equipment.Any(item =>
            item.Category == "persarmor" &&
            item.Name == "Flak/Jacket" &&
            item.Armor == "1/5/1/3") &&
        catalog.Weapons.Any(item =>
            item.Category == "archmelee" &&
            item.Name == "Katana" &&
            item.Skill == "Melee Weapons"),
        "Equipment and weapon fields must retain their source values.");
    Assert(catalog.Skills.Single(item =>
            item.Name == "Animal Handling/Riding").Subskills.Contains("Riding") &&
        catalog.Skills.Single(item =>
            item.Name == "Acting").Description.Contains("Acting Skill"),
        "Skill descriptions and subskills must be linked to their entries.");
    var mw3ConversionTargets = new[]
    {
        "Acrobatics/Any", "Animal Handling/Any", "Art/Any", "Career/Any",
        "Comms/Any", "Driving/Ground Vehicle", "Driving/Sea Vehicle",
        "Interest/Any", "Interest/Chic", "Interest/Gambling",
        "Interest/Meditation", "Language/Any", "MedTech", "MedTech/Any",
        "Navigation/Any", "Protocol/Any", "Science/Any",
        "Science/Chemistry", "Science/Linguistics",
        "Security Systems/Any", "Streetwise/Any", "Survival/Any",
        "Surgery/Any", "Tactics/Any", "Technician/Any", "Thrown Weapons",
        "Tracking/Any"
    };
    Assert(mw3ConversionTargets.All(target =>
            catalog.Skills.Any(skill => skill.Name == target)),
        "Every MW3-to-AToW conversion-table skill target must be available.");
    Assert(catalog.Traits.Single(item =>
            item.Name == "Alternate ID").Reference == "p.108" &&
        catalog.Traits.Single(item =>
            item.Name == "Ambidextrous").Description.Contains("both hands"),
        "Trait references and descriptions must be linked to their entries.");
    Assert(catalog.Equipment.All(item => item.Source == RulebookSource.CoreRulebook) &&
        catalog.Weapons.All(item => item.Source == RulebookSource.CoreRulebook) &&
        catalog.Skills.All(item => item.Source == RulebookSource.CoreRulebook) &&
        catalog.Traits.All(item => item.Source == RulebookSource.CoreRulebook),
        "Legacy catalog entries must be tagged as core-rulebook content.");
    Assert(!catalog.Options.IncludeCompanion &&
        companionCatalog.Options.IncludeCompanion,
        "Companion catalog loading must be controlled by an explicit option.");
    Assert(ResourceCatalog.IsSourceEnabled(RulebookSource.CoreRulebook, catalog.Options) &&
        !ResourceCatalog.IsSourceEnabled(RulebookSource.Companion, catalog.Options) &&
        ResourceCatalog.IsSourceEnabled(RulebookSource.Companion, companionCatalog.Options),
        "Companion source filtering must be opt-in.");
    Assert(catalog.Equipment.All(item => item.Source != RulebookSource.Companion) &&
        catalog.Weapons.All(item => item.Source != RulebookSource.Companion) &&
        catalog.Equipment.All(item => item.Name != "Vintage Bulletproof Vest") &&
        catalog.Equipment.All(item => item.Name != "Gill Implant") &&
        catalog.Equipment.All(item => item.Name != "Flight Wings") &&
        catalog.Equipment.All(item => item.Name != "Prosthetic Enhancement - Vibroblade") &&
        catalog.Equipment.All(item => item.Name != "Mermaid Adaptation Kit") &&
        catalog.Equipment.All(item => item.Name != "Field Simulation Server") &&
        catalog.Equipment.All(item => item.Name != "Hoodling Sensor HoverJeep") &&
        catalog.Weapons.All(item => item.Name != "Shock Staff") &&
        catalog.Traits.All(item => item.Name != "Mutation") &&
        catalog.Traits.All(item => item.Name != "Rank - Expanded Rules"),
        "Companion equipment, weapons, and traits must be hidden by default.");
    var companionArmor = companionCatalog.Equipment.Single(item =>
        item.Name == "Vintage Bulletproof Vest");
    var companionImplant = companionCatalog.Equipment.Single(item =>
        item.Name == "Gill Implant");
    var companionCybernetic = companionCatalog.Equipment.Single(item =>
        item.Name == "Cybernetic Eye (IR)");
    var companionCommunicationImplant = companionCatalog.Equipment.Single(item =>
        item.Name == "Pheromone Effuser");
    var companionCombatImplant = companionCatalog.Equipment.Single(item =>
        item.Name == "Pain Shunt");
    var companionProsthetic = companionCatalog.Equipment.Single(item =>
        item.Name == "Flight Wings");
    var companionWeaponEnhancement = companionCatalog.Equipment.Single(item =>
        item.Name == "Prosthetic Enhancement - Vibroblade");
    var companionUtilityEnhancement = companionCatalog.Equipment.Single(item =>
        item.Name == "Prosthetic Enhancement - Microcomputer");
    var companionCosmetic = companionCatalog.Equipment.Single(item =>
        item.Name == "Cosmetic Beauty Enhancement");
    var companionAdaptationKit = companionCatalog.Equipment.Single(item =>
        item.Name == "Mermaid Adaptation Kit");
    var companionPracticeEquipment = companionCatalog.Equipment.Single(item =>
        item.Name == "Field Simulation Server");
    var companionVehicle = companionCatalog.Equipment.Single(item =>
        item.Name == "Hoodling Sensor HoverJeep");
    var companionWeapon = companionCatalog.Weapons.Single(item =>
        item.Name == "Shock Staff");
    var companionSupportWeapon = companionCatalog.Weapons.Single(item =>
        item.Name == "Snub-Nose Support PPC");
    var companionMutation = companionCatalog.Traits.Single(item =>
        item.Name == "Mutation");
    var companionRankRules = companionCatalog.Traits.Single(item =>
        item.Name == "Rank - Expanded Rules");
    var companionImplantRules = companionCatalog.Traits.Single(item =>
        item.Name == "Implant/Prosthetic - Expanded Rules");
    Assert(companionArmor.Source == RulebookSource.Companion &&
        companionArmor.Armor == "1/4/0/2" &&
        companionArmor.SourceLabel == "A Time of War Companion" &&
        companionImplant.Source == RulebookSource.Companion &&
        companionImplant.Cost == "8000" &&
        companionImplant.Armor == "D/A-A-A/C" &&
        companionCybernetic.Source == RulebookSource.Companion &&
        companionCybernetic.Cost == "450000" &&
        companionCommunicationImplant.Source == RulebookSource.Companion &&
        companionCommunicationImplant.Cost == "40000" &&
        companionCombatImplant.Source == RulebookSource.Companion &&
        companionCombatImplant.Armor == "F/X-X-F/F" &&
        companionProsthetic.Source == RulebookSource.Companion &&
        companionProsthetic.Cost == "115000" &&
        companionProsthetic.Armor == "F/X-X-F/F" &&
        companionWeaponEnhancement.Source == RulebookSource.Companion &&
        companionWeaponEnhancement.Cost == "1000" &&
        companionWeaponEnhancement.Armor == "E/F-F-E/E" &&
        companionUtilityEnhancement.Source == RulebookSource.Companion &&
        companionUtilityEnhancement.Cost == "350" &&
        companionCosmetic.Source == RulebookSource.Companion &&
        companionCosmetic.Cost == "15000" &&
        companionCosmetic.Armor == "E/C-C-C/A" &&
        companionAdaptationKit.Source == RulebookSource.Companion &&
        companionAdaptationKit.Cost == "750000" &&
        companionPracticeEquipment.Source == RulebookSource.Companion &&
        companionPracticeEquipment.Cost == "200000" &&
        companionPracticeEquipment.Armor == "D/D-F-E/D" &&
        companionVehicle.Source == RulebookSource.Companion &&
        companionVehicle.Cost == "92000" &&
        companionVehicle.Armor == "D/X-X-D/C" &&
        companionWeapon.Source == RulebookSource.Companion &&
        companionWeapon.Skill == "Melee Weapons" &&
        companionWeapon.Damage == "2E/6" &&
        companionSupportWeapon.Source == RulebookSource.Companion &&
        companionSupportWeapon.Skill == "Support Weapons" &&
        companionSupportWeapon.Mass == "1600" &&
        companionMutation.Source == RulebookSource.Companion &&
        companionMutation.Reference == "Companion p.55" &&
        companionMutation.Description.Contains("-5 to +3 TP") &&
        companionRankRules.Source == RulebookSource.Companion &&
        companionRankRules.Description.Contains("faction-specific military rank") &&
        companionImplantRules.Source == RulebookSource.Companion &&
        companionImplantRules.Description.Contains("advanced implant"),
        "Companion catalog entries must retain source-tagged fields and notes.");
    Assert(new EquipmentCatalogItem("", "Test", "", "", "", "", "",
            RulebookSource.Companion).SourceLabel == "A Time of War Companion",
        "Companion catalog entries must have a user-facing source label.");
}

static void CheckEraInference()
{
    Assert(EraPresetCatalog.Presets.Count == 7,
        "Every local Era Digest and Era Report source must map to an era.");
    Assert(EraPresetCatalog.Presets.Select(preset => preset.Name).SequenceEqual(
            ["Age of War", "Star League", "Golden Century", "Clan Invasion",
             "Civil War", "Dark Age", "Late Dark Age"]),
        "Era inference sources must remain in chronological order.");
    Assert(EraPresetCatalog.Presets.Single(preset =>
            preset.Name == "Age of War").DisplayName == "Age of War (2398-2571)" &&
        EraPresetCatalog.InferEra(3052)?.Name == "Clan Invasion" &&
        EraPresetCatalog.BuildInferredEraLabel(3062).Contains("Civil War", StringComparison.Ordinal) &&
        EraPresetCatalog.Presets.Single(preset =>
            preset.Name == "Late Dark Age").Source == "BattleTech: Era Report 3145",
        "Campaign years must infer source titles, ranges, and labels.");
}

static void CheckEraAvailability()
{
    var resources = Path.Combine(AppContext.BaseDirectory, "Resources");
    var companionCatalog = ResourceCatalog.Load(
        resources,
        new ResourceCatalogOptions(IncludeCompanion: true));
    var starLeagueAffiliations = EraAvailabilityCatalog.FilterAffiliations(
        LifePathCatalog.Affiliations,
        2750);
    Assert(!starLeagueAffiliations.Any(module => module.Id == "comstar") &&
        !starLeagueAffiliations.Any(module => module.Id == "invading-clan") &&
        !starLeagueAffiliations.Any(module => module.Id == "word-of-blake"),
        "Star League era availability must hide later affiliations.");

    var clanInvasionAffiliations = EraAvailabilityCatalog.FilterAffiliations(
        LifePathCatalog.Affiliations,
        3052);
    Assert(clanInvasionAffiliations.Any(module => module.Id == "comstar") &&
        clanInvasionAffiliations.Any(module => module.Id == "word-of-blake") &&
        clanInvasionAffiliations.Any(module => module.Id == "invading-clan"),
        "Clan Invasion era availability must reveal order and invading Clan affiliations.");

    Assert(EraAvailabilityCatalog.BuildAffiliationSummary(
            LifePathCatalog.Affiliations,
            2750).Contains("hidden for 2750", StringComparison.Ordinal) &&
        EraAvailabilityCatalog.EarliestAffiliationYear("homeworld-clan") == 2830,
        "Era availability summaries and earliest-year helpers must stay stable.");

    Assert(EraAvailabilityCatalog.BuildEquipmentAvailabilityNote(
            companionCatalog.Equipment.Single(item =>
                item.Name == "Hoodling Sensor HoverJeep"),
            3062).Contains("check availability", StringComparison.Ordinal) &&
        EraAvailabilityCatalog.BuildEquipmentAvailabilityNote(
            companionCatalog.Equipment.Single(item =>
                item.Name == "Hoodling Sensor HoverJeep"),
            3145).Contains("available", StringComparison.Ordinal) &&
        EraAvailabilityCatalog.BuildWeaponAvailabilityNote(
            companionCatalog.Weapons.Single(item =>
                item.Name == "Shock Staff"),
            3045).Contains("check availability", StringComparison.Ordinal),
        "Era equipment and weapon availability notes must track selected campaign years.");

    var rasalhague = LifePathCatalog.Affiliations.Single(module =>
        module.Id == "rasalhague");
    var preInvasionSubAffiliations = EraAvailabilityCatalog.FilterSubAffiliations(
        rasalhague.Id,
        rasalhague.SubAffiliations!,
        3045);
    var invasionSubAffiliations = EraAvailabilityCatalog.FilterSubAffiliations(
        rasalhague.Id,
        rasalhague.SubAffiliations!,
        3052);
    var civilWarSubAffiliations = EraAvailabilityCatalog.FilterSubAffiliations(
        rasalhague.Id,
        rasalhague.SubAffiliations!,
        3062);
    Assert(preInvasionSubAffiliations.Count == 0 &&
        invasionSubAffiliations.Any(module => module.Name == "Clan War Expatriate") &&
        !invasionSubAffiliations.Any(module => module.Name == "Ghost Bear Dominion") &&
        civilWarSubAffiliations.Any(module => module.Name == "Ghost Bear Dominion"),
        "Rasalhague sub-affiliation availability must follow the selected era.");
}

static void CheckEraTemplates()
{
    Assert(EraTemplateCatalog.Templates.Count == 4 &&
        EraTemplateCatalog.Templates.Select(template => template.Name).SequenceEqual(
            ["Star League Technician", "Clan Invasion Rasalhague Survivor",
             "Civil War Mercenary", "Late Dark Age Scout"]),
        "Era quick-start templates must remain stable and visible.");

    var character = new Character();
    var template = EraTemplateCatalog.Templates.Single(item =>
        item.Name == "Clan Invasion Rasalhague Survivor");
    EraTemplateCatalog.Apply(character, template);
    Assert(character.Name == template.Name &&
        character.GameYear == 3052 &&
        character.Affiliation == "Free Rasalhague Republic" &&
        character.SubAffiliation == "Clan War Expatriate" &&
        character.Notes.Contains("Clan Invasion", StringComparison.Ordinal) &&
        character.Skills.Any(skill =>
            skill.Name == "Survival/City" && skill.Value == 15) &&
        character.Traits.Any(trait =>
            trait.Name == "Connections" && trait.Value == 25),
        "Era quick-start templates must apply campaign year, affiliation, notes, skills, and traits.");
}

static void CheckSample(
    string fileName,
    int expectedSpentXp,
    int expectedCBills,
    int expectedWalk,
    int expectedRun,
    int expectedSprint)
{
    var path = Path.Combine(AppContext.BaseDirectory, "Samples", fileName);
    var character = LegacyCharacterSerializer.Load(path);
    var summary = CharacterRules.Calculate(character);
    Assert(summary.SpentXp == expectedSpentXp, $"{fileName} XP total changed.");
    Assert(summary.RemainingCBills == expectedCBills, $"{fileName} C-Bills changed.");
    Assert(summary.Walk == expectedWalk, $"{fileName} walk movement changed.");
    Assert(summary.Run == expectedRun, $"{fileName} run movement changed.");
    Assert(summary.Sprint == expectedSprint, $"{fileName} sprint movement changed.");
}

static void CheckLifePath()
{
    var affiliation = LifePathCatalog.Affiliations.Single(module => module.Id == "fed-suns");
    var childhood = LifePathCatalog.Childhoods.Single(module => module.Id == "back-woods");
    var character = LifePathEngine.CreateBase("Aidan", "Language/English");

    LifePathEngine.Apply(character, new ModuleSelection(
        affiliation,
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["aptitude"] = ["Natural Aptitude/Strategy"]
        }));
    LifePathEngine.Apply(character, new ModuleSelection(
        childhood,
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["survival"] = ["Survival/Desert"],
            ["flex"] = ["DEX", "Leadership"]
        }));

    Assert(character.Affiliation.Length == 0,
        "The engine changes stats without assuming presentation metadata.");
    Assert(character.Attributes.Single(item => item.Name == "STR").Value == 200,
        "Childhood attribute modifiers must accumulate.");
    Assert(character.Attributes.Single(item => item.Name == "DEX").Value == 125,
        "Flexible XP may target attributes.");
    Assert(character.Skills.Single(item => item.Name == "Perception").Value == 15,
        "Life-path skills must merge with base skills.");
    Assert(character.Skills.Single(item => item.Name == "Leadership").Value == 25,
        "Flexible XP may target skills.");
    Assert(character.Traits.Single(item => item.Name == "Natural Aptitude/Strategy").Value == 100,
        "Affiliation choices must apply traits.");
    Assert(character.PreAttributes.Single(item => item.Name == "BOD").Value == 500,
        "Childhood prerequisites must be retained.");

    var duplicateChoiceRejected = false;
    try
    {
        LifePathEngine.Apply(new Character(), new ModuleSelection(
            childhood,
            new Dictionary<string, IReadOnlyList<string>>
            {
                ["survival"] = ["Survival/Desert"],
                ["flex"] = ["DEX", "DEX"]
            }));
    }
    catch (InvalidOperationException)
    {
        duplicateChoiceRejected = true;
    }
    Assert(duplicateChoiceRejected, "Distinct life-path choices must reject duplicates.");
}

static void CheckExpandedLifePaths()
{
    Assert(LifePathCatalog.Affiliations.Count == 15,
        "All corrected affiliations migrated so far must be available.");
    Assert(LifePathCatalog.Childhoods.Count == 10,
        "All legacy early-childhood modules must be available.");

    var clan = LifePathCatalog.Affiliations.Single(module => module.Id == "invading-clan");
    Assert(clan.Castes?.Count == 11, "Every legacy Clan caste must be available.");
    Assert(clan.SubAffiliations?.Any(module => module.Name == "Ghost Bear") == true,
        "Clan sub-affiliations must be retained.");
    var allSubAffiliations = LifePathCatalog.Affiliations
        .SelectMany(module => module.SubAffiliations ?? [])
        .ToArray();
    Assert(allSubAffiliations.Length == 68,
        "All 68 corrected sub-affiliations must be available.");
    Assert(allSubAffiliations.All(module => module.Effects.Count > 0 || module.Choices.Count > 0),
        "Every sub-affiliation must provide its corrected modifiers.");
    var correctedNames = new[]
    {
        "Capellan March", "Crucis March", "Draconis March", "Outback",
        "Capellan Commonality", "Liao Commonality", "Sian Commonality",
        "St. Ives Commonality", "Victoria Commonality", "Azami",
        "Benjamin District", "Dieron District",
        "New Samarkand (Galedon) District", "Pesht District",
        "Marik Commonwealth", "Principality of Regulus", "Duchy of Oriente",
        "Duchy of Andurien", "Other FWL Worlds", "Alarion Province",
        "Bolan Province", "Coventry Province", "Donegal Province",
        "Skye Province", "Clan War Expatriate", "Ghost Bear Dominion",
        "Fiefdom of Randis", "Franklin Fiefs", "Mica Majority",
        "Niops Association", "Rim Collection", "Circinus Federation",
        "Magistracy of Canopus", "Marian Hegemony", "Outworlds Alliance",
        "Taurian Concordat", "Hanseatic League", "Castilian Principalities",
        "Umayyad Caliphate", "JàrnFòlk", "Diamond Shark", "Ghost Bear",
        "Hell's Horses", "Jade Falcon", "Nova Cat", "Snow Raven", "Wolf",
        "Blood Spirit", "Cloud Cobra", "Coyote", "Fire Mandrill",
        "Goliath Scorpion", "Ice Hellion", "Star Adder", "Steel Viper",
        "Belter", "Lunar Citizen", "Martian Citizen",
        "Outer System Citizen", "Terran Citizen", "Venusian Citizen",
        "Antallos", "Astrokaszy", "Generic", "Mercenary", "Pirate",
        "Spacer", "Tortuga"
    };
    Assert(correctedNames.Length == 68 &&
        correctedNames.All(name => allSubAffiliations.Any(module =>
            module.Name == name)),
        "The catalog must match the complete corrected sub-affiliation inventory.");

    CheckSubAffiliation("fed-suns", "Capellan March", "WIL", 40, "Connections", 25);
    CheckSubAffiliation("major-periphery", "Taurian Concordat", "WIL", 150,
        "Compulsion/Distrust FedSuns", -75);
    CheckSubAffiliation("homeworld-clan", "Blood Spirit", "WIL", 100,
        "Exceptional Attribute/WIL", 200);
    CheckSubAffiliation("terran", "Terran Citizen", "EDG", -100, "Connections", 100);
    CheckSubAffiliation("independent", "Spacer", "DEX", 10,
        "G-Tolerance", 20);

    var stIves = FindSubAffiliation("capellan", "St. Ives Commonality");
    Assert(!stIves.Effects.Any(effect => effect.Name == "Connections"),
        "St. Ives must not receive the legacy Connections award.");
    var azami = FindSubAffiliation("draconis", "Azami");
    Assert(azami.Effects.Any(effect =>
            effect.Target == EffectTarget.Attribute &&
            effect.Name == "WIL" && effect.Xp == 190) &&
        !azami.Effects.Any(effect => effect.Name == "Thick-Skinned"),
        "Azami modifiers must match the corrected printing.");

    var minorPeriphery = LifePathCatalog.Affiliations
        .Single(module => module.Id == "minor-periphery");
    var majorPeriphery = LifePathCatalog.Affiliations
        .Single(module => module.Id == "major-periphery");
    var deepPeriphery = LifePathCatalog.Affiliations
        .Single(module => module.Id == "deep-periphery");
    Assert(minorPeriphery.Choices.Single(choice => choice.Id == "flex") is
        { Xp: 25, Count: 3, FixedFlexibleSelections: true } &&
        majorPeriphery.ModuleCost == 100 &&
        majorPeriphery.Choices.Single(choice => choice.Id == "flex") is
        { Xp: 15, Count: 3, FixedFlexibleSelections: true } &&
        deepPeriphery.Choices.Single(choice => choice.Id == "flex") is
        { Xp: 10, Count: 2, FixedFlexibleSelections: true },
        "Periphery affiliation-wide flexible pools must match the corrected tables.");
    Assert(FindSubAffiliation("major-periphery", "Circinus Federation")
            .Choices.Single(choice => choice.Id == "skills").Xp == 20 &&
        FindSubAffiliation("deep-periphery", "Hanseatic League").Effects.Any(effect =>
            effect.Name == "Compulsion/Distrust Lyrans" && effect.Xp == -20),
        "Major and Deep Periphery sub-affiliation corrections must be preserved.");

    Assert(FindSubAffiliation("invading-clan", "Wolf")
            .Choices.Single(choice => choice.Id == "skills") is
        { Xp: 10, Count: 2 } &&
        FindSubAffiliation("homeworld-clan", "Fire Mandrill").Choices.Count == 5 &&
        FindSubAffiliation("homeworld-clan", "Star Adder")
            .Choices.Single(choice => choice.Id == "compulsion").Xp == -60,
        "Clan selectable awards must match the corrected printing.");

    var lunar = FindSubAffiliation("terran", "Lunar Citizen");
    var venusian = FindSubAffiliation("terran", "Venusian Citizen");
    Assert(lunar.Effects.Any(effect =>
            effect.Name == "WIL" && effect.Xp == 65) &&
        lunar.Choices.Any(choice =>
            choice.Id == "technician" && choice.Xp == 5) &&
        venusian.Effects.Any(effect =>
            effect.Name == "WIL" && effect.Xp == 110) &&
        !venusian.Effects.Any(effect => effect.Name == "Thick-Skinned"),
        "Terran sub-affiliation values must match the corrected printing.");

    var astrokaszy = FindSubAffiliation("independent", "Astrokaszy");
    var generic = FindSubAffiliation("independent", "Generic");
    Assert(astrokaszy.Effects.Any(effect =>
            effect.Name == "WIL" && effect.Xp == 25) &&
        astrokaszy.Choices.Single(choice => choice.Id == "skills").Xp == 15 &&
        generic.Choices.Single(choice => choice.Id == "skills").Count == 4,
        "Independent sub-affiliation awards must match the corrected printing.");

    var street = LifePathCatalog.Childhoods.Single(module => module.Id == "street");
    var character = LifePathEngine.CreateBase("Mara", "Language/English");
    character.Affiliation = "Federated Suns";
    character.EarlyChildhood = street.Name;
    LifePathEngine.Apply(character, new ModuleSelection(street,
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["flex"] = ["STR", "Toughness", "Stealth", "Leadership"]
        }));
    LifePathEngine.ApplyAffiliationContext(character,
        LifePathCatalog.Affiliations.Single(module => module.Id == "fed-suns"),
        street, "Language/English");

    Assert(character.Traits.Single(item => item.Name == "Toughness").Value == 210,
        "Flexible XP must classify traits as traits.");
    Assert(character.Skills.Single(item => item.Name == "Language/English").Value == 35,
        "Childhood affiliation-language modifiers must affect the selected language.");
    Assert(character.Skills.Single(item => item.Name == "Streetwise/FedSuns").Value == 10,
        "Childhood streetwise modifiers must use the affiliation skill.");

    var trueborn = LifePathCatalog.Childhoods.Single(module => module.Id == "trueborn-creche");
    var invalid = LifePathEngine.CreateBase("Invalid", "Language/English");
    invalid.Affiliation = "Federated Suns";
    invalid.EarlyChildhood = trueborn.Name;
    LifePathEngine.Apply(invalid, new ModuleSelection(trueborn,
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["phenotype"] = ["Phenotype/MechWarrior"],
            ["flex"] = ["STR", "BOD", "RFL", "DEX", "INT"]
        }));
    Assert(PrerequisiteRules.Evaluate(invalid)
            .Any(issue => issue.Category == "Affiliation"),
        "Trueborn Creche must require a Clan affiliation.");
}

static LifePathModule FindSubAffiliation(
    string affiliationId,
    string subAffiliationName) =>
    LifePathCatalog.Affiliations
        .Single(module => module.Id == affiliationId)
        .SubAffiliations!
        .Single(module => module.Name == subAffiliationName);

static void CheckSubAffiliation(
    string affiliationId,
    string subAffiliationName,
    string attributeName,
    int attributeXp,
    string traitName,
    int traitXp)
{
    var module = LifePathCatalog.Affiliations
        .Single(affiliation => affiliation.Id == affiliationId)
        .SubAffiliations!
        .Single(subAffiliation => subAffiliation.Name == subAffiliationName);
    var character = new Character();
    LifePathEngine.Apply(character, new ModuleSelection(module,
        module.Choices.ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)choice.Options.Take(choice.Count).ToArray())));

    Assert(character.Attributes.Single(item => item.Name == attributeName).Value ==
        100 + attributeXp, $"{subAffiliationName} attribute modifiers changed.");
    Assert(character.Traits.Single(item => item.Name == traitName).Value == traitXp,
        $"{subAffiliationName} trait modifiers changed.");
}

static void CheckLateChildhoods()
{
    Assert(LifePathCatalog.LateChildhoods.Count == 13,
        "All legacy late-childhood modules must be available.");
    Assert(LifePathCatalog.LateChildhoods
            .All(module => module.Effects.Count > 0 || module.Choices.Count > 0),
        "Every late-childhood module must provide effects or choices.");

    var affiliation = LifePathCatalog.Affiliations.Single(module => module.Id == "fed-suns");
    var warfare = LifePathCatalog.LateChildhoods
        .Single(module => module.Id == "late-adolescent-warfare");
    var character = LifePathEngine.CreateBase("Warfare", "Language/English");
    LifePathEngine.Apply(character, SelectDefaults(warfare));
    LifePathEngine.ApplyAffiliationContext(character, affiliation, warfare, "Language/English");
    Assert(character.Skills.Single(item => item.Name == "Streetwise/FedSuns").Value == 45,
        "Late-childhood affiliation streetwise must use the selected affiliation.");
    Assert(character.Skills.Single(item => item.Name == "Language/English").Value == 15,
        "Late-childhood language modifiers must accumulate with the base language.");

    var trueborn = LifePathCatalog.LateChildhoods
        .Single(module => module.Id == "late-trueborn-sibko");
    var truebornSelection = trueborn.Choices.ToDictionary(
        choice => choice.Id,
        choice => (IReadOnlyList<string>)(choice.Id == "branch"
            ? ["MechWarrior"]
            : choice.Options.Take(choice.Count).ToArray()));
    var clanCharacter = new Character
    {
        Affiliation = "Invading Clan",
        LateChildhood = "Trueborn Sibko"
    };
    LifePathEngine.Apply(clanCharacter, new ModuleSelection(trueborn, truebornSelection));
    Assert(clanCharacter.Traits.Single(item => item.Name == "Custom Vehicle").Value == 200,
        "Conditional Clan branch traits must apply.");
    Assert(clanCharacter.Skills.Single(item => item.Name == "Gunnery/Mech").Value == 15,
        "Conditional Clan branch skills must apply.");

    var invalid = new Character
    {
        Affiliation = "Federated Suns",
        LateChildhood = "Trueborn Sibko"
    };
    Assert(PrerequisiteRules.Evaluate(invalid)
            .Any(issue => issue.Category == "Affiliation"),
        "Clan late-childhood modules must reject non-Clan affiliations.");
}

static ModuleSelection SelectDefaults(LifePathModule module) =>
    new(module, module.Choices.ToDictionary(
        choice => choice.Id,
        choice => (IReadOnlyList<string>)choice.Options.Take(choice.Count).ToArray()));

static void CheckEducation()
{
    Assert(LifePathCatalog.EducationSchools.Count == 10,
        "All legacy education schools must be available.");
    Assert(LifePathCatalog.EducationSchools.All(school => school.BasicFields?.Count > 0),
        "Every education school must offer a basic field.");

    var technical = LifePathCatalog.EducationSchools
        .Single(module => module.Id == "technical-college");
    var communications = technical.BasicFields!
        .Single(module => module.Name == "Communications");
    var character = new Character();
    LifePathEngine.Apply(character, SelectDefaults(technical));
    LifePathEngine.Apply(character, SelectDefaults(communications));
    Assert(character.Attributes.Single(item => item.Name == "DEX").Value == 200,
        "Technical College attribute modifiers changed.");
    Assert(character.Skills.Single(item => item.Name == "Computers").Value == 50,
        "School and career-field skills must accumulate.");
    Assert(character.Skills.Single(item => item.Name == "Career/Communications").Value == 30,
        "Basic education fields must apply their complete skill package.");

    var university = LifePathCatalog.EducationSchools
        .Single(module => module.Id == "university");
    Assert(university.AdvancedFields?.Any(field => field.Name == "Engineer") == true,
        "University advanced fields must be available.");
    Assert(university.SpecialistFields?.Any(field => field.Name == "Doctor") == true,
        "University specialist fields must be available.");
    Assert(university.AdvancedFields?.Any(field => field.Name == "Technician - Aerospace") == true &&
        university.AdvancedFields?.Any(field => field.Name == "Technician - Vehicle") == true,
        "University advanced technical fields must match the corrected printing.");

    var analysis = university.AdvancedFields!
        .Single(module => module.Name == "Analysis");
    Assert(analysis.Choices.Single(choice => choice.Label == "Language").Count == 2,
        "Repeated education placeholders must become a multi-selection.");
    Assert(analysis.Effects.All(effect => !effect.Name.Contains("/Any")),
        "Education fields must not apply unresolved Any placeholders.");

    var generalStudies = university.BasicFields!
        .Single(module => module.Name == "General Studies");
    Assert(generalStudies.AffiliationProtocolXp == 30,
        "Affiliation education skills must retain their field XP.");
    var affiliatedCharacter = new Character();
    LifePathEngine.Apply(affiliatedCharacter, SelectDefaults(generalStudies));
    LifePathEngine.ApplyAffiliationContext(
        affiliatedCharacter,
        LifePathCatalog.Affiliations.Single(module => module.Id == "fed-suns"),
        generalStudies,
        "Language/English");
    Assert(affiliatedCharacter.Skills.Single(item => item.Name == "Protocol/FedSuns").Value == 30,
        "Education affiliation placeholders must resolve to the selected affiliation.");

    var familyTraining = LifePathCatalog.EducationSchools
        .Single(module => module.Id == "family-training");
    Assert(familyTraining.ModuleCost == 570,
        "Family Training must use the corrected 570 XP cost.");
    Assert(familyTraining.Effects.Single(effect =>
        effect.Target == EffectTarget.Attribute && effect.Name == "WIL").Xp == 50,
        "Family Training must award positive WIL XP.");
    Assert(familyTraining.Choices.Any(choice => choice.Id == "driving") &&
        familyTraining.Choices.Any(choice => choice.Id == "survival"),
        "Family Training must resolve Driving and Survival specialties.");

    var militaryAcademy = LifePathCatalog.EducationSchools
        .Single(module => module.Id == "military-academy");
    Assert(militaryAcademy.SpecialistFields?.Any(field => field.Name == "Pilot - WarShip") == true,
        "Military Academy must include Pilot/WarShip special training.");
    var mechWarrior = militaryAcademy.AdvancedFields!
        .Single(field => field.Name == "MechWarrior");
    Assert(mechWarrior.Effects.Any(effect => effect.Name == "Gunnery/'Mech") &&
        mechWarrior.Effects.Any(effect => effect.Name == "Piloting/'Mech"),
        "MechWarrior training must use the corrected 'Mech skill names.");
}

static void CheckLifePathAccounting()
{
    var baseCharacter = LifePathEngine.CreateBase("Base", "Language/German");
    Assert(baseCharacter.Skills.Single(item => item.Name == "Language/German").Value == 20,
        "Universal XP must grant the selected affiliation language.");
    Assert(baseCharacter.Skills.Single(item => item.Name == "Language/English").Value == 20,
        "Universal XP must grant English separately.");

    var englishCharacter = LifePathEngine.CreateBase("English", "Language/English");
    Assert(englishCharacter.Skills.Single(item => item.Name == "Language/English").Value == 40,
        "English receives both universal language awards when it is the affiliation language.");

    var affiliation = LifePathCatalog.Affiliations.Single(module => module.Id == "fed-suns");
    var childhood = LifePathCatalog.Childhoods.Single(module => module.Id == "back-woods");
    var modules = new[] { affiliation, childhood };
    var expectedCost = LifePathEngine.UniversalModuleCost +
        affiliation.ModuleCost + childhood.ModuleCost;
    Assert(LifePathEngine.CalculateModuleCost(modules) == expectedCost,
        "Life-path cost must include the universal module.");

    LifePathEngine.ApplyModuleAccounting(baseCharacter, modules);
    Assert(CharacterRules.Calculate(baseCharacter).FreeXp ==
        LifePathEngine.StartingXp - expectedCost,
        "Wizard characters must retain the module-based remaining XP.");

    var technical = LifePathCatalog.EducationSchools
        .Single(module => module.Id == "technical-college");
    var communications = technical.BasicFields!
        .Single(module => module.Name == "Communications");
    var awardedSkillCount = communications.Effects.Count +
        communications.Choices.Sum(choice => choice.Count) +
        communications.AffiliationProtocolXp / 30 +
        communications.AffiliationStreetwiseXp / 30;
    Assert(communications.ModuleCost == awardedSkillCount * 30,
        "Each education field must cost 30 XP per awarded skill.");
}

static void CheckFlexibleAllocations()
{
    var module = LifePathCatalog.LateChildhoods
        .Single(item => item.Id == "late-adolescent-warfare");
    var flex = module.Choices.Single(choice => choice.Target == EffectTarget.Flexible);
    var character = new Character();
    var regularChoices = module.Choices
        .Where(choice => choice.Target != EffectTarget.Flexible)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)choice.Options.Take(choice.Count).ToArray());
    LifePathEngine.Apply(character, new ModuleSelection(
        module,
        regularChoices,
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [flex.Id] =
            [
                new("DEX", 60),
                new("Swimming", flex.Xp * flex.Count - 60)
            ]
        }));
    Assert(character.Attributes.Single(item => item.Name == "DEX").Value == 160,
        "Flexible XP may be split into an attribute.");
    Assert(character.Skills.Single(item => item.Name == "Swimming").Value ==
        flex.Xp * flex.Count - 60,
        "Flexible XP may be split into a skill.");

    var invalidRejected = false;
    try
    {
        LifePathEngine.Apply(new Character(), new ModuleSelection(
            module,
            regularChoices,
            new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
            {
                [flex.Id] = [new("DEX", flex.Xp * flex.Count - 1)]
            }));
    }
    catch (InvalidOperationException)
    {
        invalidRejected = true;
    }
    Assert(invalidRejected, "Flexible allocations must spend their exact XP pool.");
}

static void CheckRealLifeModules()
{
    Assert(LifePathCatalog.RealLifeModules.Count == 46,
        "The Stage 4 catalog must contain forty-six corrected modules.");
    var requiredBaseModules = new[]
    {
        "Agitator", "Civilian Job", "Clan Watch Operative",
        "Clan Warrior Washout", "Cloister Training", "Combat Correspondent",
        "ComStar/Word of Blake Service", "Covert Operations", "Dark Caste",
        "Explorer", "Goliath Scorpion Seeker", "Guerilla Insurgent",
        "Merchant", "Ne'er-Do-Well", "Organized Crime",
        "Postgraduate Studies", "ProtoMech Pilot Training",
        "Scientist Caste Service", "Solaris Insider", "Solaris VII Games",
        "Think Tank", "Tour of Duty", "To Serve and Protect", "Travel"
    };
    foreach (var baseName in requiredBaseModules)
    {
        var represented = baseName == "ComStar/Word of Blake Service"
            ? LifePathCatalog.RealLifeModules.Any(module =>
                module.Name is "ComStar Service" or "Word of Blake Service")
            : LifePathCatalog.RealLifeModules.Any(module =>
                module.Name == baseName ||
                module.Name.StartsWith($"{baseName} - ",
                    StringComparison.Ordinal));
        Assert(represented,
            $"The corrected Stage 4 catalog must include {baseName}.");
    }
    var agitator = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-agitator");
    Assert(agitator.ModuleCost == 900 && agitator.TimeYears == 4,
        "Agitator cost and time must match the corrected printing.");
    Assert(agitator.AffiliationStreetwiseXp == 75,
        "Agitator Streetwise XP must resolve through affiliation.");

    var flex = agitator.Choices.Single(choice => choice.Target == EffectTarget.Flexible);
    var choices = agitator.Choices
        .Where(choice => choice.Target != EffectTarget.Flexible)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)choice.Options.Take(choice.Count).ToArray());
    var overAllocatedAttributeRejected = false;
    try
    {
        LifePathEngine.Apply(new Character(), new ModuleSelection(
            agitator,
            choices,
            new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
            {
                [flex.Id] =
                [
                    new("DEX", 30),
                    new("DEX", 21),
                    new("Leadership", 74)
                ]
            }));
    }
    catch (InvalidOperationException)
    {
        overAllocatedAttributeRejected = true;
    }
    Assert(overAllocatedAttributeRejected,
        "Agitator flexible XP must enforce its 50 XP Attribute limit across split rows.");
    var repeatedAgitator = new Character();
    repeatedAgitator.RealLifeHistory.Add(agitator.Name);
    var repeatedChoices = agitator.Choices
        .Where(choice => choice.Target is EffectTarget.Skill or EffectTarget.Flexible)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)choice.Options
                .Take(choice.Count).ToArray());
    var repeatedFlex = agitator.Choices.Single(choice =>
        choice.Target == EffectTarget.Flexible);
    LifePathEngine.ApplyStage4(repeatedAgitator, new ModuleSelection(
        agitator,
        repeatedChoices,
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [repeatedFlex.Id] = [new("Leadership", 125)]
        }));
    Assert(repeatedAgitator.Attributes.All(attribute => attribute.Value == 100),
        "A repeated Stage 4 module must not award fixed Attribute XP.");
    Assert(repeatedAgitator.Traits.Count == 0,
        "A repeated Stage 4 module must not award fixed Trait XP.");
    Assert(repeatedAgitator.Skills.Count > 0,
        "A repeated Stage 4 module must still award Skill and Flexible XP.");
    var nonRepeatableCareer = LifePathCatalog.RealLifeModules.Single(module =>
        module.Name == "Postgraduate Studies");
    var nonRepeatableCareerRejected = false;
    try
    {
        var returningGraduate = new Character();
        returningGraduate.RealLifeHistory.Add(nonRepeatableCareer.Name);
        LifePathEngine.ApplyStage4(returningGraduate, new ModuleSelection(
            nonRepeatableCareer,
            new Dictionary<string, IReadOnlyList<string>>()));
    }
    catch (InvalidOperationException)
    {
        nonRepeatableCareerRejected = true;
    }
    Assert(nonRepeatableCareerRejected,
        "A non-repeatable Stage 4 module must reject a second selection.");

    var covertModules = LifePathCatalog.RealLifeModules
        .Where(module => module.Id.StartsWith("real-covert-", StringComparison.Ordinal))
        .ToArray();
    Assert(covertModules.Length == 11 &&
        covertModules.All(module => module.ModuleCost == 900 && module.TimeYears == 6),
        "Covert Operations must provide all eleven corrected affiliation variants.");
    foreach (var orderAffiliation in new[] { "ComStar", "Word of Blake" })
    {
        var orderModule = covertModules.Single(module =>
            module.Name == $"Covert Operations - {orderAffiliation}");
        var orderAgent = new Character
        {
            Affiliation = orderAffiliation,
            AdvancedSchool = "Covert Operations",
            RealLife = orderModule.Name
        };
        Assert(!PrerequisiteRules.Evaluate(orderAgent)
                .Any(issue => issue.Category == "Affiliation"),
            $"{orderAffiliation} must qualify for its Covert Operations variant.");
    }
    var fedSunsCovert = covertModules
        .Single(module => module.Id == "real-covert-fed-suns");
    var underqualifiedVeteran = new Character
    {
        Affiliation = "Federated Suns",
        RealLife = fedSunsCovert.Name
    };
    underqualifiedVeteran.RealLifeHistory.Add("Tour of Duty - Inner Sphere");
    underqualifiedVeteran.RealLifeHistory.Add(fedSunsCovert.Name);
    underqualifiedVeteran.Traits.Add(new NamedValue("Connections", 140));
    underqualifiedVeteran.Skills.Add(new NamedValue("Leadership", 120));
    Assert(PrerequisiteRules.Evaluate(underqualifiedVeteran)
            .Any(issue => issue.Category == "Education"),
        "Covert Operations must require a prior Tour of Duty and 150 pre-module XP in Connections or Leadership.");
    underqualifiedVeteran.Traits.Single(item =>
        item.Name == "Connections").Value = 190;
    Assert(!PrerequisiteRules.Evaluate(underqualifiedVeteran)
            .Any(issue => issue.Category == "Education"),
        "A prior Tour of Duty and 150 pre-module Connections XP must satisfy Covert Operations.");
    var covertAgent = new Character
    {
        Affiliation = "Federated Suns",
        School = "University",
        AdvancedSchool = "Covert Operations",
        RealLife = fedSunsCovert.Name
    };
    var covertSkills =
        LifePathCatalog.ResolveCovertOperationsFieldSkills(covertAgent);
    Assert(covertSkills.Contains("Acting") && covertSkills.Contains("Tracking/Urban"),
        "Covert Operations must resolve the selected Intelligence Field skills.");
    var covertChoices = fedSunsCovert.Choices.ToDictionary(
        choice => choice.Id,
        choice => (IReadOnlyList<string>)(choice.Id == "field-skills"
            ? Enumerable.Repeat(covertSkills[0], choice.Count).ToArray()
            : choice.Options.Take(choice.Count).ToArray()));
    LifePathEngine.Apply(covertAgent,
        new ModuleSelection(fedSunsCovert, covertChoices));
    Assert(!PrerequisiteRules.Evaluate(covertAgent)
            .Any(issue => issue.Category is "Affiliation" or "Education" or "Trait"),
        "A trained Federated Suns agent must satisfy Covert Operations.");
    Assert(covertAgent.Skills.Single(skill => skill.Name == covertSkills[0]).Value >= 150,
        "Covert Operations may assign multiple 25-XP groups to one Field skill.");

    var invalidCovertAgent = new Character
    {
        Affiliation = "Federated Suns",
        RealLife = fedSunsCovert.Name
    };
    invalidCovertAgent.Traits.Add(new NamedValue("Combat Paralysis", -75));
    var covertIssues = PrerequisiteRules.Evaluate(invalidCovertAgent);
    Assert(covertIssues.Any(issue => issue.Category == "Education") &&
        covertIssues.Any(issue => issue.Name == "May not have Combat Paralysis"),
        "Covert Operations must enforce its training and Combat Paralysis restrictions.");

    var clanWatchModules = LifePathCatalog.RealLifeModules
        .Where(module => module.Id.StartsWith("real-clan-watch-",
            StringComparison.Ordinal))
        .ToArray();
    Assert(clanWatchModules.Length == 2 &&
        clanWatchModules.All(module =>
            module.ModuleCost == 1200 && module.TimeYears == 3),
        "Clan Watch Operative must provide both corrected Clan variants.");
    var homeworldWatch = clanWatchModules
        .Single(module => module.Id == "real-clan-watch-homeworld");
    var watchScientist = new Character
    {
        Affiliation = "Homeworld Clan",
        ClanCaste = "Scientist Caste",
        RealLife = homeworldWatch.Name
    };
    var watchChoices = homeworldWatch.Choices
        .Where(choice => choice.Target != EffectTarget.Flexible)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)choice.Options.Take(choice.Count).ToArray());
    var watchFlex = homeworldWatch.Choices
        .Single(choice => choice.Target == EffectTarget.Flexible);
    LifePathEngine.Apply(watchScientist, new ModuleSelection(
        homeworldWatch,
        watchChoices,
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [watchFlex.Id] = [new("INT", 175)]
        }));
    Assert(!PrerequisiteRules.Evaluate(watchScientist)
            .Any(issue => issue.Category is "Affiliation" or "Caste"),
        "A Homeworld Clan scientist must satisfy Clan Watch prerequisites.");
    Assert(watchScientist.Skills.Any(skill =>
            skill.Name == "Career/Soldier" && skill.Value == 60),
        "The Homeworld Clan variant must award its Soldier training.");

    var invadingWatch = clanWatchModules
        .Single(module => module.Id == "real-clan-watch-invading");
    Assert(invadingWatch.Effects
            .Where(effect => effect.Name == "Dark Secret")
            .Sum(effect => effect.Xp) == -100 &&
        invadingWatch.Effects
            .Where(effect => effect.Name == "Computers")
            .Sum(effect => effect.Xp) == 125 &&
        invadingWatch.Effects
            .Where(effect => effect.Name == "Perception")
            .Sum(effect => effect.Xp) == 125,
        "The Invading Clan variant must accumulate its common and variant awards.");
    var invalidWatch = new Character
    {
        Affiliation = "Invading Clan",
        ClanCaste = "Merchant Caste",
        RealLife = invadingWatch.Name
    };
    Assert(PrerequisiteRules.Evaluate(invalidWatch)
            .Any(issue => issue.Category == "Caste"),
        "Clan Watch Operative must reject ineligible Clan castes.");

    var washouts = LifePathCatalog.RealLifeModules
        .Where(module => module.Id.StartsWith("real-clan-warrior-washout-",
            StringComparison.Ordinal))
        .ToArray();
    Assert(washouts.Length == 4 &&
        washouts.All(module => module.ModuleCost == 400 &&
            module.TimeYears == 2 && !module.Repeatable),
        "Clan Warrior Washout must provide four non-repeatable caste outcomes.");
    var scientistWashout = washouts
        .Single(module => module.Id == "real-clan-warrior-washout-scientist");
    var washedOutWarrior = new Character
    {
        Affiliation = "Homeworld Clan",
        LateChildhood = "Trueborn Sibko",
        ClanCaste = "Scientist Caste",
        ClanTrainingField = "MechWarrior",
        RealLife = scientistWashout.Name
    };
    var washoutSkills =
        LifePathCatalog.ResolveClanWarriorFieldSkills(washedOutWarrior);
    var washoutChoices = scientistWashout.Choices
        .Where(choice => choice.Target != EffectTarget.Flexible)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)(choice.Id == "warrior-skills"
                ? washoutSkills.Take(choice.Count).ToArray()
                : choice.Options.Take(choice.Count).ToArray()));
    var washoutFlex = scientistWashout.Choices
        .Single(choice => choice.Target == EffectTarget.Flexible);
    LifePathEngine.Apply(washedOutWarrior, new ModuleSelection(
        scientistWashout,
        washoutChoices,
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [washoutFlex.Id] = [new("INT", 185)]
        }));
    Assert(!PrerequisiteRules.Evaluate(washedOutWarrior)
            .Any(issue => issue.Category is "Affiliation" or "Background" or
                "Caste" or "Training"),
        "An eligible sibko graduate must satisfy the selected washout caste.");
    Assert(washedOutWarrior.Skills.Any(skill =>
            skill.Name == washoutSkills[0] && skill.Value == -30),
        "Clan Warrior Washout must reduce two actual Warrior Field skills.");

    var technicianWashout = washouts
        .Single(module => module.Id == "real-clan-warrior-washout-technician");
    Assert(technicianWashout.Choices
            .Where(choice => choice.Id.StartsWith("technician-", StringComparison.Ordinal))
            .Sum(choice => choice.Xp) == 150,
        "Technician Caste washouts must receive all three Technician awards.");
    var protoWashout = new Character
    {
        Affiliation = "Invading Clan",
        LateChildhood = "Trueborn Sibko",
        ClanCaste = "Laborer Caste",
        ClanTrainingField = "ProtoMech",
        RealLife = "Clan Warrior Washout - Laborer Caste"
    };
    Assert(PrerequisiteRules.Evaluate(protoWashout)
            .Any(issue => issue.Category == "Training"),
        "ProtoMech washouts must use ProtoMech Pilot Training instead.");

    var comStarAffiliation = LifePathCatalog.Affiliations
        .Single(module => module.Id == "comstar");
    var wordAffiliation = LifePathCatalog.Affiliations
        .Single(module => module.Id == "word-of-blake");
    Assert(comStarAffiliation.ModuleCost == 50 &&
        wordAffiliation.ModuleCost == 50 &&
        comStarAffiliation.ProtocolSkill == "Protocol/ComStar" &&
        wordAffiliation.ProtocolSkill == "Protocol/Word of Blake",
        "ComStar and Word of Blake order affiliations must use the corrected 50 XP cost.");
    var fedSunsBirth = LifePathCatalog.Affiliations
        .Single(module => module.Id == "fed-suns");
    var orderCharacter = new Character
    {
        Affiliation = "ComStar",
        BirthAffiliation = fedSunsBirth.Name
    };
    LifePathEngine.Apply(orderCharacter, SelectDefaults(fedSunsBirth));
    LifePathEngine.Apply(orderCharacter, SelectDefaults(comStarAffiliation));
    Assert(LifePathEngine.CalculateModuleCost(
            [fedSunsBirth, comStarAffiliation]) ==
        LifePathEngine.UniversalModuleCost +
        fedSunsBirth.ModuleCost + comStarAffiliation.ModuleCost,
        "Order characters must pay the full birth and Order affiliation costs.");
    Assert(orderCharacter.Attributes.Single(attribute =>
            attribute.Name == "WIL").Value == 85 &&
        orderCharacter.Attributes.Single(attribute =>
            attribute.Name == "INT").Value == 125 &&
        orderCharacter.PreAttributes.Any(attribute =>
            attribute.Name == "INT" && attribute.Value == 400),
        "Order characters must receive full effects from both affiliations.");

    var comStarService = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-comstar-service");
    var wordService = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-word-of-blake-service");
    Assert(comStarService.ModuleCost == 900 && comStarService.TimeYears == 5 &&
        wordService.ModuleCost == 900 && wordService.TimeYears == 5,
        "Both order-service variants must cost 900 XP and take five years.");
    var adept = new Character
    {
        Affiliation = "ComStar",
        School = "University",
        BasicSchool = "General Studies",
        AdvancedSchool = "Analysis",
        RealLife = comStarService.Name
    };
    var adeptFieldSkills =
        LifePathCatalog.ResolveSelectedEducationFieldSkills(adept);
    var serviceChoices = comStarService.Choices
        .Where(choice => choice.Target != EffectTarget.Flexible)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)(choice.Id == "field-skills"
                ? adeptFieldSkills.Take(choice.Count).ToArray()
                : choice.Options.Take(choice.Count).ToArray()));
    var serviceFlex = comStarService.Choices
        .Single(choice => choice.Target == EffectTarget.Flexible);
    LifePathEngine.Apply(adept, new ModuleSelection(
        comStarService,
        serviceChoices,
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [serviceFlex.Id] = [new("INT", 50)]
        }));
    LifePathEngine.ApplyAffiliationContext(
        adept, comStarAffiliation, comStarService, "Language/English");
    Assert(!PrerequisiteRules.Evaluate(adept)
            .Any(issue => issue.Category is "Affiliation" or "Trait"),
        "An eligible ComStar adept must satisfy the service prerequisites.");
    Assert(adept.Skills.Any(skill =>
            skill.Name == "Protocol/ComStar" && skill.Value == 35),
        "ComStar Service must award its affiliation protocol XP.");

    var invalidAdept = new Character
    {
        Affiliation = "Word of Blake",
        RealLife = comStarService.Name
    };
    invalidAdept.Traits.Add(new NamedValue("Poor Vision", -150));
    invalidAdept.Traits.Add(new NamedValue("Property", 100));
    var serviceIssues = PrerequisiteRules.Evaluate(invalidAdept);
    Assert(serviceIssues.Any(issue => issue.Category == "Affiliation") &&
        serviceIssues.Any(issue => issue.Name.Contains("Poor Vision")) &&
        serviceIssues.Any(issue => issue.Name == "May not have Property"),
        "Order service and affiliation restrictions must reject invalid characters.");

    var darkCaste = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-dark-caste");
    Assert(darkCaste.ModuleCost == 700 && darkCaste.TimeYears == 4,
        "Dark Caste must cost 700 XP and take four years.");
    Assert(darkCaste.Choices
            .Where(choice => choice.Id.StartsWith("technician-", StringComparison.Ordinal))
            .Sum(choice => choice.Xp) == 70,
        "Dark Caste must preserve both Technician specialty awards.");
    var outcast = new Character
    {
        Affiliation = "Invading Clan",
        ClanCaste = "Dark Caste",
        RealLife = darkCaste.Name
    };
    var darkChoices = darkCaste.Choices
        .Where(choice => choice.Target != EffectTarget.Flexible)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)choice.Options.Take(choice.Count).ToArray());
    var darkFlex = darkCaste.Choices
        .Single(choice => choice.Target == EffectTarget.Flexible);
    LifePathEngine.Apply(outcast, new ModuleSelection(
        darkCaste,
        darkChoices,
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [darkFlex.Id] = [new("DEX", 115)]
        }));
    Assert(!PrerequisiteRules.Evaluate(outcast)
            .Any(issue => issue.Category is "Affiliation" or "Caste"),
        "A Clan outcast must satisfy Dark Caste prerequisites.");
    Assert(outcast.Skills.Any(skill =>
            skill.Name == "Protocol/Clan" && skill.Value == -25),
        "Dark Caste must reduce Clan protocol.");
    var nonClanOutcast = new Character
    {
        Affiliation = "Independent",
        ClanCaste = "Dark Caste",
        RealLife = darkCaste.Name
    };
    Assert(PrerequisiteRules.Evaluate(nonClanOutcast)
            .Any(issue => issue.Category == "Affiliation"),
        "Dark Caste must require Clan origin.");

    var civilianJob = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-civilian-job");
    Assert(civilianJob.ModuleCost == 600 && civilianJob.TimeYears == 6 &&
        civilianJob.AffiliationProtocolXp == 50,
        "Civilian Job cost, time, and affiliation protocol must match the rulebook.");
    var careerFieldChoice = civilianJob.Choices
        .Single(choice => choice.Id == "career-field");
    Assert(careerFieldChoice.FixedFlexibleSelections &&
        careerFieldChoice.Count == 4 && careerFieldChoice.Xp == 20,
        "Civilian Job must award four fixed 20-XP career-field selections.");
    var civilian = new Character
    {
        Affiliation = "Federated Suns",
        School = "University",
        BasicSchool = "General Studies",
        AdvancedSchool = "Engineer",
        RealLife = civilianJob.Name
    };
    var civilianFieldSkills =
        LifePathCatalog.ResolveSelectedEducationFieldSkills(civilian);
    var civilianChoices = civilianJob.Choices
        .Where(choice => choice.Target != EffectTarget.Flexible ||
            choice.FixedFlexibleSelections)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)(choice.Id == "career-field"
                ? civilianFieldSkills.Take(choice.Count).ToArray()
                : choice.Options.Take(choice.Count).ToArray()));
    var civilianFlex = civilianJob.Choices
        .Single(choice => choice.Id == "flex");
    LifePathEngine.Apply(civilian, new ModuleSelection(
        civilianJob,
        civilianChoices,
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [civilianFlex.Id] = [new("CHA", 85)]
        }));
    Assert(civilianFieldSkills.Take(4).All(name =>
        civilian.Skills.Any(skill => skill.Name == name && skill.Value >= 20)),
        "Civilian Job must award its selected Field skills.");

    var fieldlessCivilian = new Character { RealLife = civilianJob.Name };
    var fieldlessChoices = civilianJob.Choices
        .Where(choice => choice.Target != EffectTarget.Flexible ||
            choice.FixedFlexibleSelections)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)choice.Options.Take(choice.Count).ToArray());
    LifePathEngine.Apply(fieldlessCivilian, new ModuleSelection(
        civilianJob,
        fieldlessChoices,
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [civilianFlex.Id] = [new("CHA", 85)]
        }));
    Assert(fieldlessCivilian.Attributes.Any(attribute =>
            attribute.Name == "STR" && attribute.Value == 120),
        "Without a Field, Civilian Job's four awards must become flexible XP.");

    var clanScientistWorker = new Character
    {
        Affiliation = "Homeworld Clan",
        ClanCaste = "Scientist Caste",
        RealLife = civilianJob.Name
    };
    Assert(PrerequisiteRules.Evaluate(clanScientistWorker)
            .Any(issue => issue.Category == "Caste"),
        "Clan scientists and warriors may not use Civilian Job.");

    var insurgents = LifePathCatalog.RealLifeModules
        .Where(module => module.Id.StartsWith("real-guerilla-insurgent-",
            StringComparison.Ordinal))
        .ToArray();
    Assert(insurgents.Length == 2 &&
        insurgents.All(module => module.ModuleCost == 900 && module.TimeYears == 6),
        "Guerilla Insurgent must provide both corrected affiliation variants.");
    var rasalhagueInsurgent = insurgents
        .Single(module => module.Id == "real-guerilla-insurgent-rasalhague");
    Assert(rasalhagueInsurgent.Effects
            .Where(effect => effect.Name == "Bloodmark")
            .Sum(effect => effect.Xp) == -85 &&
        rasalhagueInsurgent.Effects
            .Where(effect => effect.Name == "Combat Sense")
            .Sum(effect => effect.Xp) == 70,
        "The Rasalhague variant must accumulate its common and special Traits.");
    var freedomFighter = new Character
    {
        Affiliation = "Free Rasalhague Republic",
        RealLife = rasalhagueInsurgent.Name
    };
    var insurgentChoices = rasalhagueInsurgent.Choices
        .Where(choice => choice.Target != EffectTarget.Flexible)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)choice.Options.Take(choice.Count).ToArray());
    var insurgentFlex = rasalhagueInsurgent.Choices
        .Single(choice => choice.Target == EffectTarget.Flexible);
    LifePathEngine.Apply(freedomFighter, new ModuleSelection(
        rasalhagueInsurgent,
        insurgentChoices,
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [insurgentFlex.Id] = [new("WIL", 180)]
        }));
    Assert(!PrerequisiteRules.Evaluate(freedomFighter)
            .Any(issue => issue.Category == "Affiliation"),
        "A Free Rasalhague character must satisfy its insurgent variant.");
    Assert(freedomFighter.Traits.Any(trait =>
            trait.Name == "Implant/Prosthetic" && trait.Value == 50),
        "The Rasalhague insurgent must receive the corrected Prosthetic Trait.");

    var generalInsurgent = insurgents
        .Single(module => module.Id == "real-guerilla-insurgent-general");
    Assert(generalInsurgent.AffiliationLanguageXp == 35,
        "The general insurgent must award affiliation-language XP.");
    var invalidClanInsurgent = new Character
    {
        Affiliation = "Invading Clan",
        RealLife = generalInsurgent.Name
    };
    Assert(PrerequisiteRules.Evaluate(invalidClanInsurgent)
            .Any(issue => issue.Name == "Non-Clan affiliation"),
        "Clan characters may not take Guerilla Insurgent.");

    var merchants = LifePathCatalog.RealLifeModules
        .Where(module => module.Id.StartsWith("real-merchant-",
            StringComparison.Ordinal))
        .ToArray();
    Assert(merchants.Length == 4 &&
        merchants.All(module => module.ModuleCost == 900 && module.TimeYears == 4),
        "Merchant must provide all four corrected career variants.");
    var freeTrader = merchants
        .Single(module => module.Id == "real-merchant-free-trader");
    var merchantGraduate = new Character
    {
        Affiliation = "Independent",
        School = "Trade School",
        BasicSchool = "Merchant",
        RealLife = freeTrader.Name
    };
    var freeTraderField = freeTrader.Choices.Single(choice =>
        choice.Id == "field-skills");
    Assert(freeTraderField.Count == 5 && freeTraderField.Xp == 20 &&
        freeTraderField.Options.Contains("Career/Merchant") &&
        freeTraderField.Options.Contains("Career/Merchant Marine"),
        "Free Traders must receive five Merchant-field skill awards.");
    var freeTraderChoices = freeTrader.Choices
        .Where(choice => choice.Target != EffectTarget.Flexible)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)choice.Options.Take(choice.Count).ToArray());
    var merchantFlex = freeTrader.Choices.Single(choice =>
        choice.Target == EffectTarget.Flexible);
    LifePathEngine.Apply(merchantGraduate, new ModuleSelection(
        freeTrader,
        freeTraderChoices,
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [merchantFlex.Id] = [new("CHA", 200)]
        }));
    Assert(!PrerequisiteRules.Evaluate(merchantGraduate)
            .Any(issue => issue.Category == "Education"),
        "A character with the Merchant Field must satisfy Merchant prerequisites.");

    var sharkMerchant = merchants
        .Single(module => module.Id == "real-merchant-diamond-shark");
    var shark = new Character
    {
        Affiliation = "Invading Clan",
        SubAffiliation = "Diamond Shark",
        ClanCaste = "Merchant Caste",
        ClanTrainingField = "MechWarrior",
        BasicSchool = "Merchant",
        RealLife = sharkMerchant.Name
    };
    var sharkChoice = sharkMerchant.Choices.Single(choice =>
        choice.Id == "field-skills");
    var sharkOptions = sharkChoice.Options
        .Concat(LifePathCatalog.ResolveClanWarriorFieldSkills(shark))
        .Distinct(StringComparer.Ordinal)
        .ToArray();
    Assert(sharkOptions.Contains("Career/Management") &&
        sharkOptions.Contains("Gunnery/'Mech"),
        "Diamond Shark merchants must choose commercial or Warrior Field skills.");
    Assert(!PrerequisiteRules.Evaluate(shark)
            .Any(issue => issue.Category is "Affiliation" or "Caste" or "Trait"),
        "An eligible Diamond Shark merchant must satisfy its special prerequisites.");

    var invalidShark = new Character
    {
        Affiliation = "Invading Clan",
        SubAffiliation = "Wolf",
        ClanCaste = "Scientist Caste",
        BasicSchool = "Merchant",
        RealLife = sharkMerchant.Name
    };
    invalidShark.Traits.Add(new NamedValue("TDS", -50));
    var sharkIssues = PrerequisiteRules.Evaluate(invalidShark);
    Assert(sharkIssues.Any(issue => issue.Category == "Affiliation") &&
        sharkIssues.Any(issue => issue.Category == "Caste") &&
        sharkIssues.Any(issue => issue.Name == "May not have TDS"),
        "Diamond Shark merchant restrictions must all be enforced.");

    var organizedCrime = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-organized-crime");
    var clanCrime = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-organized-crime-clan");
    Assert(organizedCrime.ModuleCost == 1000 && organizedCrime.TimeYears == 5 &&
        clanCrime.ModuleCost == 1000 && clanCrime.TimeYears == 5,
        "Both Organized Crime paths must cost 1000 XP and take five years.");
    Assert(organizedCrime.Effects.Any(effect =>
            effect.Target == EffectTarget.Attribute && effect.Name == "EDG") &&
        organizedCrime.Effects.Any(effect =>
            effect.Target == EffectTarget.Trait && effect.Name == "Alternate ID") &&
        clanCrime.Effects.All(effect =>
            effect.Target is not (EffectTarget.Attribute or EffectTarget.Trait)),
        "The Clan path must suppress every Organized Crime Attribute and Trait award.");

    var criminal = new Character
    {
        Affiliation = "Draconis Combine",
        RealLife = organizedCrime.Name
    };
    var crimeChoices = organizedCrime.Choices
        .Where(choice => choice.Target != EffectTarget.Flexible)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)choice.Options.Take(choice.Count).ToArray());
    var crimeFlex = organizedCrime.Choices
        .Single(choice => choice.Target == EffectTarget.Flexible);
    LifePathEngine.Apply(criminal, new ModuleSelection(
        organizedCrime,
        crimeChoices,
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [crimeFlex.Id] = [new("EDG", 100)]
        }));
    Assert(criminal.Skills.Any(skill =>
            skill.Name == "Career/Mafia" && skill.Value == 100) &&
        criminal.Skills.Any(skill =>
            skill.Name == "Language/Mafia" && skill.Value == 50),
        "The syndicate choice must set matching Career and Language skills.");
    Assert(!PrerequisiteRules.Evaluate(criminal)
            .Any(issue => issue.Category is "Affiliation" or "Caste"),
        "A non-Clan criminal must satisfy Organized Crime prerequisites.");

    var darkCasteCriminal = new Character
    {
        Affiliation = "Homeworld Clan",
        ClanCaste = "Dark Caste",
        RealLife = clanCrime.Name
    };
    Assert(!PrerequisiteRules.Evaluate(darkCasteCriminal)
            .Any(issue => issue.Category is "Affiliation" or "Caste"),
        "A prior Clan Dark Caster must satisfy the Clan Organized Crime path.");
    var invalidClanCriminal = new Character
    {
        Affiliation = "Invading Clan",
        ClanCaste = "Merchant Caste",
        RealLife = clanCrime.Name
    };
    Assert(PrerequisiteRules.Evaluate(invalidClanCriminal)
            .Any(issue => issue.Category == "Caste"),
        "Clan Organized Crime must require prior Dark Caste service.");

    var solarisInsider = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-solaris-insider");
    Assert(solarisInsider.ModuleCost == 825 && solarisInsider.TimeYears == 4,
        "Solaris Insider must cost 825 XP and take four years.");
    Assert(solarisInsider.RepeatEffects?.Single() ==
        new ModuleEffect(EffectTarget.Trait, "In For Life", -100),
        "Repeated Solaris Insider service must add 100 negative XP to In For Life.");
    var insiderFieldChoice = solarisInsider.Choices
        .Single(choice => choice.Id == "field-skills");
    Assert(insiderFieldChoice.Count == 6 && insiderFieldChoice.Xp == 25 &&
        insiderFieldChoice.SolarisInternshipFieldSkillsOnly,
        "Solaris Insider must award six 25-XP Field-skill selections.");
    var intern = new Character
    {
        Affiliation = "Lyran Alliance",
        School = "Solaris Internship",
        BasicSchool = "Communications",
        AdvancedSchool = "MechWarrior",
        RealLife = solarisInsider.Name
    };
    var internshipSkills =
        LifePathCatalog.ResolveSolarisInternshipFieldSkills(intern);
    Assert(internshipSkills.Contains("Gunnery/'Mech") &&
        internshipSkills.Contains("Career/Communications"),
        "Solaris Insider must use the character's actual Internship Fields.");
    var insiderChoices = solarisInsider.Choices
        .Where(choice => choice.Target != EffectTarget.Flexible)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)(choice.Id == "field-skills"
                ? internshipSkills.Take(choice.Count).ToArray()
                : choice.Options.Take(choice.Count).ToArray()));
    var insiderFlex = solarisInsider.Choices
        .Single(choice => choice.Target == EffectTarget.Flexible);
    LifePathEngine.Apply(intern, new ModuleSelection(
        solarisInsider,
        insiderChoices,
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [insiderFlex.Id] = [new("CHA", 100)]
        }));
    Assert(!PrerequisiteRules.Evaluate(intern)
            .Any(issue => issue.Name.StartsWith("Solaris Internship",
                StringComparison.Ordinal)),
        "A Solaris Internship graduate must satisfy Insider prerequisites.");
    var repeatedInsider = new Character
    {
        Affiliation = "Lyran Alliance",
        School = "Solaris Internship",
        BasicSchool = "Communications",
        AdvancedSchool = "MechWarrior"
    };
    repeatedInsider.RealLifeHistory.Add(solarisInsider.Name);
    LifePathEngine.ApplyStage4(repeatedInsider, new ModuleSelection(
        solarisInsider,
        insiderChoices,
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [insiderFlex.Id] = [new("CHA", 100)]
        }));
    Assert(repeatedInsider.Traits.Single(item =>
            item.Name == "In For Life").Value == -100,
        "The Stage 4 engine must apply Solaris Insider's repeat penalty.");

    var connectedInsider = new Character
    {
        Affiliation = "Federated Suns",
        RealLife = solarisInsider.Name
    };
    connectedInsider.Traits.Add(new NamedValue("Connections", 200));
    var fallbackSkills = insiderFieldChoice.Options;
    Assert(fallbackSkills.Contains("Career/Communications") &&
        fallbackSkills.Contains("Career/Management") &&
        fallbackSkills.Contains("Career/Politician"),
        "Connections-only Insiders must use Communications, Manager, or Politician skills.");
    LifePathEngine.Apply(connectedInsider, new ModuleSelection(
        solarisInsider,
        solarisInsider.Choices
            .Where(choice => choice.Target != EffectTarget.Flexible)
            .ToDictionary(
                choice => choice.Id,
                choice => (IReadOnlyList<string>)choice.Options.Take(choice.Count).ToArray()),
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [insiderFlex.Id] = [new("CHA", 100)]
        }));
    Assert(!PrerequisiteRules.Evaluate(connectedInsider)
            .Any(issue => issue.Name.StartsWith("Solaris Internship",
                StringComparison.Ordinal)),
        "Two hundred pre-module Connections XP must satisfy Solaris Insider.");

    var solarisGames = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-solaris-vii-games");
    Assert(solarisGames.ModuleCost == 900 && solarisGames.TimeYears == 4,
        "Solaris VII Games must cost 900 XP and take four years.");
    Assert(solarisGames.RepeatEffects?.Single() ==
        new ModuleEffect(EffectTarget.Trait, "In For Life", -150),
        "Repeated Solaris VII Games service must add 150 negative XP to In For Life.");
    Assert(solarisGames.Choices.Single(choice => choice.Id == "assets").Count == 3 &&
        solarisGames.Choices.Single(choice => choice.Id == "field-skills").Count == 6,
        "Solaris VII Games must provide three assets and six Field-skill awards.");
    var gladiator = new Character
    {
        Affiliation = "Lyran Alliance",
        School = "Solaris Internship",
        BasicSchool = "Technician - Military",
        AdvancedSchool = "MechWarrior",
        RealLife = solarisGames.Name
    };
    var arenaSkills =
        LifePathCatalog.ResolveSolarisGamesFieldSkills(gladiator);
    Assert(arenaSkills.Contains("Gunnery/'Mech") &&
        arenaSkills.Contains("Technician/Weapons") &&
        !arenaSkills.Contains("Leadership"),
        "Solaris Games must use actual Tech and Military Fields while excluding Officer.");
    var gamesChoices = solarisGames.Choices
        .Where(choice => choice.Target != EffectTarget.Flexible)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)(choice.Id == "field-skills"
                ? arenaSkills.Take(choice.Count).ToArray()
                : choice.Options.Take(choice.Count).ToArray()));
    var gamesFlex = solarisGames.Choices
        .Single(choice => choice.Target == EffectTarget.Flexible);
    LifePathEngine.Apply(gladiator, new ModuleSelection(
        solarisGames,
        gamesChoices,
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [gamesFlex.Id] = [new("EDG", 125)]
        }));
    Assert(!PrerequisiteRules.Evaluate(gladiator)
            .Any(issue => issue.Category is "Training" or "Education"),
        "A Solaris-trained MechWarrior must satisfy Games prerequisites.");
    Assert(gladiator.Traits.Count(trait =>
            trait.Name is "Custom Vehicle" or "Design Quirk" or "Equipped") == 3,
        "Solaris VII Games must apply all three selected arena assets.");
    var veteranGladiator = new Character
    {
        Affiliation = "Federated Suns",
        RealLife = solarisGames.Name
    };
    veteranGladiator.RealLifeHistory.Add("Tour of Duty - Inner Sphere");
    veteranGladiator.RealLifeHistory.Add(solarisGames.Name);
    Assert(!PrerequisiteRules.Evaluate(veteranGladiator)
            .Any(issue => issue.Category == "Training"),
        "A prior Tour of Duty must satisfy Solaris VII Games training.");

    var careerHistoryCriminal = new Character
    {
        Affiliation = "Invading Clan",
        RealLife = "Organized Crime - Clan Dark Caste"
    };
    careerHistoryCriminal.RealLifeHistory.Add("Dark Caste");
    careerHistoryCriminal.RealLifeHistory.Add(
        "Organized Crime - Clan Dark Caste");
    Assert(!PrerequisiteRules.Evaluate(careerHistoryCriminal)
            .Any(issue => issue.Category == "Caste"),
        "A prior Dark Caste career must unlock Clan Organized Crime.");

    var untrainedGladiator = new Character
    {
        Affiliation = "Independent",
        School = "University",
        AdvancedSchool = "Officer",
        RealLife = solarisGames.Name
    };
    var gamesIssues = PrerequisiteRules.Evaluate(untrainedGladiator);
    Assert(gamesIssues.Any(issue => issue.Category == "Training") &&
        gamesIssues.Any(issue => issue.Category == "Education"),
        "Solaris VII Games must reject untrained characters and Officer-only fields.");

    var explorer = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-explorer");
    Assert(explorer.ModuleCost == 900 && explorer.TimeYears == 6,
        "Explorer cost and time must match the corrected printing.");
    Assert(explorer.AffiliationLanguageXp == 25,
        "Explorer must award its affiliation language XP.");

    var neerDoWell = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-neer-do-well");
    var neerFlex = neerDoWell.Choices.Single(choice => choice.Target == EffectTarget.Flexible);
    Assert(!neerFlex.Options.Any(option =>
            option == "Wealth" || option == "Toughness" || option == "Connections"),
        "Ne'er-Do-Well flexible XP may not target Traits.");
    Assert(!neerDoWell.AwardFlexibleXpOnRepeat,
        "Ne'er-Do-Well must not award flexible XP when repeated.");
    var repeatedNeerDoWell = new Character();
    repeatedNeerDoWell.RealLifeHistory.Add(neerDoWell.Name);
    var repeatedNeerChoices = neerDoWell.Choices
        .Where(choice => choice.Target == EffectTarget.Skill)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)choice.Options
                .Take(choice.Count).ToArray());
    LifePathEngine.ApplyStage4(repeatedNeerDoWell,
        new ModuleSelection(neerDoWell, repeatedNeerChoices));
    Assert(repeatedNeerDoWell.Attributes.All(attribute => attribute.Value == 100) &&
        repeatedNeerDoWell.Traits.Count == 0,
        "A repeated Ne'er-Do-Well module must award Skills without Attribute, Trait, or flexible XP.");

    var correspondent = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-combat-correspondent");
    var invalidCorrespondent = new Character
    {
        Affiliation = "Federated Suns",
        RealLife = correspondent.Name
    };
    Assert(PrerequisiteRules.Evaluate(invalidCorrespondent)
            .Any(issue => issue.Category == "Education"),
        "Combat Correspondent must require the Journalist Field.");

    var seeker = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-goliath-scorpion-seeker");
    var seekerFlex = seeker.Choices.Single(choice => choice.Target == EffectTarget.Flexible);
    var seekerChoices = seeker.Choices
        .Where(choice => choice.Target != EffectTarget.Flexible)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)choice.Options.Take(choice.Count).ToArray());
    var seekerRestrictionRejected = false;
    try
    {
        LifePathEngine.Apply(new Character(), new ModuleSelection(
            seeker,
            seekerChoices,
            new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
            {
                [seekerFlex.Id] = [new("Leadership", 160)]
            }));
    }
    catch (InvalidOperationException)
    {
        seekerRestrictionRejected = true;
    }
    Assert(seekerRestrictionRejected,
        "Goliath Scorpion Seeker must spend at least 100 flexible XP on Attributes or Traits.");

    var protoMech = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-protomech-pilot");
    Assert(!protoMech.Repeatable && protoMech.TimeYears == 2,
        "ProtoMech Pilot Training must be non-repeatable and take two years.");
    Assert(protoMech.Effects.Any(effect =>
            effect.Name == "Gunnery/ProtoMech" && effect.Xp == 25),
        "ProtoMech Pilot Training must award its Clan field skills.");

    var scientist = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-scientist-caste-service");
    Assert(scientist.ModuleCost == 1200 && scientist.TimeYears == 4,
        "Scientist Caste Service cost and time must match the corrected printing.");
    var traitPair = scientist.Choices.Single(choice => choice.Id == "trait-pair");
    Assert(traitPair.OptionEffects?.Count > 1,
        "Scientist Caste Service must provide both corrected Trait-pair paths.");
    var scientistCharacter = new Character
    {
        Affiliation = "Homeworld Clan",
        ClanCaste = "Scientist Caste",
        RealLife = scientist.Name
    };
    LifePathEngine.Apply(scientistCharacter, new ModuleSelection(
        scientist,
        scientist.Choices.ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)choice.Options.Take(choice.Count).ToArray())));
    Assert(scientistCharacter.Traits.Any(item => item.Name == "Fast Learner") &&
        scientistCharacter.Traits.Any(item => item.Name == "Combat Paralysis"),
        "Scientist Caste Service must apply the selected Trait pair.");
    Assert(!PrerequisiteRules.Evaluate(scientistCharacter)
            .Any(issue => issue.Category is "Affiliation" or "Caste"),
        "A Clan scientist must satisfy Scientist Caste Service prerequisites.");

    var thinkTank = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-think-tank");
    Assert(thinkTank.ModuleCost == 900 && thinkTank.TimeYears == 4,
        "Think Tank cost and time must match the corrected printing.");
    var thinkTankFlex = thinkTank.Choices
        .Single(choice => choice.Target == EffectTarget.Flexible);
    Assert(!thinkTankFlex.Options.Contains("Small Arms") &&
        !thinkTankFlex.Options.Contains("Combat Sense"),
        "Think Tank flexible XP must exclude combat options.");
    var underqualifiedThinker = new Character
    {
        Affiliation = "Federated Suns",
        AdvancedSchool = "Analysis",
        RealLife = thinkTank.Name
    };
    underqualifiedThinker.Attributes.Single(item =>
        item.Name == "INT").Value = 700;
    underqualifiedThinker.Traits.Add(new NamedValue("Connections", 300));
    var thinkerIssues = PrerequisiteRules.Evaluate(underqualifiedThinker);
    Assert(thinkerIssues.Any(issue => issue.Name == "INT before Think Tank") &&
        thinkerIssues.Any(issue => issue.Name == "Connections before Think Tank"),
        "Think Tank must not count its own INT and Connections awards toward prerequisites.");
    underqualifiedThinker.Attributes.Single(item =>
        item.Name == "INT").Value = 790;
    underqualifiedThinker.Traits.Single(item =>
        item.Name == "Connections").Value = 400;
    Assert(!PrerequisiteRules.Evaluate(underqualifiedThinker)
            .Any(issue => issue.Category is "Attribute" or "Trait" or "Education"),
        "A character meeting Think Tank prerequisites before its awards must qualify.");

    var travel = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-travel");
    Assert(travel.ModuleCost == 700 && travel.TimeYears == 6,
        "Travel must use the corrected 700 XP cost and six-year duration.");
    Assert(travel.AffiliationLanguageXp == 50,
        "Travel must award affiliation-language XP separately.");
    var invalidTraveler = new Character { RealLife = travel.Name };
    Assert(PrerequisiteRules.Evaluate(invalidTraveler)
            .Any(issue => issue.Name == "Extra Income or Wealth"),
        "Travel must require sufficient Extra Income or Wealth.");

    var serveProtect = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-serve-protect");
    var policeCharacter = new Character
    {
        Affiliation = "Federated Suns",
        BasicSchool = "Police Officer",
        RealLife = serveProtect.Name
    };
    var policeSkills = LifePathCatalog.ResolveEducationFieldSkills(
        policeCharacter, ["Police Officer", "Police Tactical Officer", "Detective"]);
    Assert(policeSkills.Contains("Small Arms") &&
        policeSkills.Contains("Streetwise/FedSuns"),
        "Field-linked choices must include fixed and affiliation Police Field skills.");
    var serveChoices = serveProtect.Choices.ToDictionary(
        choice => choice.Id,
        choice => (IReadOnlyList<string>)(choice.Id == "field-skills"
            ? policeSkills.Take(choice.Count).ToArray()
            : choice.Options.Take(choice.Count).ToArray()));
    LifePathEngine.Apply(policeCharacter, new ModuleSelection(serveProtect, serveChoices));
    Assert(!PrerequisiteRules.Evaluate(policeCharacter)
            .Any(issue => issue.Category == "Education"),
        "A Police Officer Field must satisfy To Serve and Protect.");

    var invalidFieldSkillRejected = false;
    serveChoices["field-skills"] =
        ["Small Arms", "Computers", "Perception", "Piloting/Aerospace"];
    try
    {
        LifePathEngine.Apply(new Character
        {
            Affiliation = "Federated Suns",
            BasicSchool = "Police Officer"
        }, new ModuleSelection(serveProtect, serveChoices));
    }
    catch (InvalidOperationException)
    {
        invalidFieldSkillRejected = true;
    }
    Assert(invalidFieldSkillRejected,
        "Field-linked choices must reject skills outside the selected Fields.");

    var postgraduate = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-postgraduate");
    Assert(!postgraduate.Repeatable && postgraduate.ModuleCost == 700,
        "Postgraduate Studies must cost 700 XP and be non-repeatable.");
    var graduate = new Character
    {
        Affiliation = "Federated Suns",
        School = "University",
        BasicSchool = "General Studies",
        AdvancedSchool = "Analysis",
        RealLife = postgraduate.Name
    };
    var postFlex = postgraduate.Choices
        .Single(choice => choice.Target == EffectTarget.Flexible);
    var graduateFieldSkills = LifePathCatalog.ResolveEducationFieldSkills(
        graduate, postFlex.EducationFieldNames!);
    var postChoices = postgraduate.Choices
        .Where(choice => choice.Target != EffectTarget.Flexible)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)choice.Options.Take(choice.Count).ToArray());
    LifePathEngine.Apply(graduate, new ModuleSelection(
        postgraduate,
        postChoices,
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [postFlex.Id] =
            [
                new(graduateFieldSkills[0], 50),
                new(graduateFieldSkills[1], 50),
                new("DEX", 75)
            ]
        }));
    Assert(!PrerequisiteRules.Evaluate(graduate)
            .Any(issue => issue.Category == "Education"),
        "A University graduate must satisfy Postgraduate Studies.");

    var insufficientFieldXpRejected = false;
    try
    {
        LifePathEngine.Apply(new Character
        {
            Affiliation = "Federated Suns",
            School = "University",
            BasicSchool = "General Studies"
        }, new ModuleSelection(
            postgraduate,
            postChoices,
            new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
            {
                [postFlex.Id] =
                [
                    new(graduateFieldSkills[0], 75),
                    new("DEX", 100)
                ]
            }));
    }
    catch (InvalidOperationException)
    {
        insufficientFieldXpRejected = true;
    }
    Assert(insufficientFieldXpRejected,
        "Postgraduate Studies must assign at least 100 flexible XP to University Field skills.");

    var cloister = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-cloister-training");
    Assert(!cloister.Repeatable && cloister.TimeYears == 3,
        "Cloister Training must be non-repeatable and take three years.");
    var clanWarrior = new Character
    {
        Affiliation = "Homeworld Clan",
        SubAffiliation = "Cloud Cobra",
        ClanCaste = "MechWarrior",
        ClanTrainingField = "MechWarrior",
        RealLife = cloister.Name
    };
    clanWarrior.Attributes.Single(item => item.Name == "WIL").Value = 500;
    var clanSkills = LifePathCatalog.ResolveClanWarriorFieldSkills(clanWarrior);
    Assert(clanSkills.Contains("Gunnery/'Mech") &&
        clanSkills.Contains("Piloting/'Mech"),
        "Clan warrior branch metadata must resolve its actual Field skills.");
    var cloisterChoices = cloister.Choices.ToDictionary(
        choice => choice.Id,
        choice => (IReadOnlyList<string>)(choice.Id == "warrior-skills"
            ? clanSkills.Take(choice.Count).ToArray()
            : choice.Options.Take(choice.Count).ToArray()));
    LifePathEngine.Apply(clanWarrior, new ModuleSelection(cloister, cloisterChoices));
    Assert(!PrerequisiteRules.Evaluate(clanWarrior)
            .Any(issue => issue.Category is "Affiliation" or "Caste" or "Education" or "Attribute"),
        "An eligible Cloud Cobra warrior must satisfy Cloister Training.");

    var innerTour = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-tour-duty-inner-sphere");
    Assert(innerTour.ModuleCost == 800 && innerTour.TimeYears == 3,
        "The Inner Sphere Tour of Duty must cost 800 XP and take three years.");
    var soldier = new Character
    {
        Affiliation = "Federated Suns",
        School = "Military Academy",
        BasicSchool = "Basic Training",
        AdvancedSchool = "MechWarrior",
        RealLife = innerTour.Name
    };
    var militarySkills = LifePathCatalog.ResolveMilitaryFieldSkills(soldier);
    Assert(militarySkills.Contains("Gunnery/'Mech") &&
        militarySkills.Contains("Piloting/'Mech"),
        "Tour of Duty must resolve skills from the selected Military Fields.");
    var tourChoices = innerTour.Choices
        .Where(choice => choice.Target != EffectTarget.Flexible)
        .ToDictionary(
            choice => choice.Id,
            choice => (IReadOnlyList<string>)(choice.Id == "field-skills"
                ? militarySkills.Take(choice.Count).ToArray()
                : choice.Options.Take(choice.Count).ToArray()));
    var tourFlex = innerTour.Choices
        .Single(choice => choice.Target == EffectTarget.Flexible);
    LifePathEngine.Apply(soldier, new ModuleSelection(
        innerTour,
        tourChoices,
        new Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        {
            [tourFlex.Id] = [new("DEX", 100)]
        }));
    Assert(!PrerequisiteRules.Evaluate(soldier)
            .Any(issue => issue.Category is "Affiliation" or "Education"),
        "A trained Inner Sphere soldier must satisfy Tour of Duty.");
    var comStarSoldier = new Character
    {
        Affiliation = "ComStar",
        School = "Military Academy",
        BasicSchool = "Basic Training",
        AdvancedSchool = "MechWarrior",
        RealLife = innerTour.Name
    };
    Assert(!PrerequisiteRules.Evaluate(comStarSoldier)
            .Any(issue => issue.Category == "Affiliation"),
        "ComStar must qualify for the Inner Sphere Tour of Duty.");

    var clanTour = LifePathCatalog.RealLifeModules
        .Single(module => module.Id == "real-tour-duty-clan");
    Assert(clanTour.ModuleCost == 1000 &&
        clanTour.Choices.Single(choice => choice.Id == "field-skills").Count == 10,
        "Clan Tour of Duty must cost 1000 XP and award ten Field-skill bonuses.");
    var clanTourSkills = LifePathCatalog.ResolveMilitaryFieldSkills(clanWarrior);
    Assert(clanTourSkills.SequenceEqual(clanSkills),
        "Clan Tour of Duty must use the warrior's actual training branch.");

    var militaryScientist = new Character
    {
        Affiliation = "Federated Suns",
        School = "University",
        AdvancedSchool = "Military Scientist",
        RealLife = innerTour.Name
    };
    Assert(!PrerequisiteRules.Evaluate(militaryScientist)
            .Any(issue => issue.Name == "Military Skill Field"),
        "A Military Skill Field qualifies regardless of which school taught it.");

    var field = LifePathCatalog.EducationSchools
        .Single(module => module.Id == "technical-college")
        .BasicFields!.Single(module => module.Name == "Communications");
    var fastLearner = new Character();
    fastLearner.Traits.Add(new NamedValue("Fast Learner", 300));
    Assert(LifePathEngine.CalculateModuleCost(fastLearner, [field]) ==
        LifePathEngine.UniversalModuleCost + field.Effects.Count * 24 +
        field.Choices.Sum(choice => choice.Count) * 24,
        "Fast Learner must reduce education Field costs to 24 XP per Skill.");
}
