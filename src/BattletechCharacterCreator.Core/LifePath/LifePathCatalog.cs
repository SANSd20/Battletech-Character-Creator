using BattletechCharacterCreator.Core.Models;

namespace BattletechCharacterCreator.Core.LifePath;

public static class LifePathCatalog
{
    private static readonly string[] Attributes =
        ["STR", "BOD", "RFL", "DEX", "INT", "WIL", "CHA", "EDG"];

    private static readonly string[] Languages =
    [
        "Language/English", "Language/Mandarin Chinese", "Language/Russian",
        "Language/Cantonese", "Language/Vietnamese", "Language/Japanese",
        "Language/Arabic", "Language/Swedenese", "Language/French", "Language/German",
        "Language/Hindi", "Language/Greek", "Language/Italian", "Language/Mongolian",
        "Language/Romanian", "Language/Slovak", "Language/Spanish", "Language/Urdu",
        "Language/Scots Gaelic", "Language/Swedish"
    ];

    private static readonly string[] Interests =
    [
        "Interests/BattleMechs", "Interests/Battlesuits", "Interests/Aerospace",
        "Interests/Spacecraft", "Interests/History (Star League)", "Interests/History (Clan)",
        "Interests/Literature", "Interests/Holo-Games", "Interests/Military History"
    ];

    private static readonly string[] Arts =
        ["Art/Dance", "Art/Drawing", "Art/Music", "Art/Painting", "Art/Poetry",
            "Art/Sculpture", "Art/Songwriting", "Art/Writing"];

    private static readonly string[] Careers =
        ["Career/Agriculture", "Career/Doctor", "Career/Engineer", "Career/Journalist",
            "Career/Lawyer", "Career/Management", "Career/MedTech", "Career/Merchant",
            "Career/Politician", "Career/Scientist", "Career/Soldier", "Career/Technician"];

    private static readonly string[] Technicians =
        ["Technician/Aeronautics", "Technician/Cybernetics", "Technician/Electronic",
            "Technician/Jets", "Technician/Mechanical", "Technician/Myomer",
            "Technician/Nuclear", "Technician/Weapons"];

    private static readonly string[] SurvivalSkills =
        ["Survival/Arctic", "Survival/City", "Survival/Desert", "Survival/Forests",
            "Survival/Jungle", "Survival/Martian Desert", "Survival/Mountain",
            "Survival/Ocean", "Survival/Steppe"];

    private static readonly string[] NavigationSkills =
        ["Navigation/Air", "Navigation/Ground", "Navigation/K-F Jump",
            "Navigation/Sea", "Navigation/Space"];

    private static readonly string[] PilotingSkills =
        ["Piloting/Aerospace", "Piloting/Air Vehicle", "Piloting/Battlesuit",
            "Piloting/'Mech", "Piloting/ProtoMech", "Piloting/Spacecraft"];

    private static readonly string[] GunnerySkills =
        ["Gunnery/Aerospace", "Gunnery/Air Vehicle", "Gunnery/Battlesuit",
            "Gunnery/Ground Vehicle", "Gunnery/'Mech", "Gunnery/ProtoMech",
            "Gunnery/Sea Vehicle", "Gunnery/Spacecraft", "Gunnery/Turret"];

    private static readonly string[] PrestidigitationSkills =
        ["Prestidigitation/Pick Pocket", "Prestidigitation/Quickdraw",
            "Prestidigitation/Sleight of Hand"];

    private static readonly string[] MedTechSkills =
        ["MedTech/General", "MedTech/Veterinary"];

    private static readonly string[] CommunicationsSkills =
        ["Communications/Black Box", "Communications/Conventional",
            "Communications/HPG"];

    private static readonly string[] DrivingSkills =
        ["Driving/Ground Vehicles", "Driving/Rail Vehicles", "Driving/Sea Vehicles"];

    private static readonly string[] SecuritySystemsSkills =
        ["Security Systems/Electronic", "Security Systems/Mechanical"];

    private static readonly string[] TacticsSkills =
        ["Tactics/Infantry", "Tactics/Land", "Tactics/Sea", "Tactics/Air",
            "Tactics/Space"];

    private static readonly string[] TrackingSkills =
        ["Tracking/Urban", "Tracking/Wilds"];

    private static readonly string[] ProtocolSkills =
        ["Protocol/Capellan", "Protocol/Combine", "Protocol/FedSuns",
            "Protocol/Lyran", "Protocol/ComStar", "Protocol/Word of Blake",
            "Protocol/Free Worlds", "Protocol/Rasalhague", "Protocol/Clan"];

    private static readonly string[] InnerSphereProtocols =
        ["Protocol/Capellan", "Protocol/Combine", "Protocol/FedSuns",
            "Protocol/Lyran", "Protocol/Free Worlds", "Protocol/Rasalhague",
            "Protocol/Terran"];

    private static readonly string[] AddictionTraits =
        ["Compulsion/Alcohol Addiction", "Compulsion/Drug Addiction",
            "Compulsion/Smoking Addiction", "Compulsion/Gambling"];

    private static readonly string[] CompulsionTraits =
    [
        "Compulsion/Adder Arrogance", "Compulsion/Alcohol Addiction",
        "Compulsion/Arrogance", "Compulsion/Atrean Opponent",
        "Compulsion/Berserker", "Compulsion/Blood Spirit Fanaticism",
        "Compulsion/Burrock Forever!", "Compulsion/Castilian Honor Code",
        "Compulsion/Catatonia", "Compulsion/Chemical Addiction",
        "Compulsion/Clan Honor", "Compulsion/Confusion",
        "Compulsion/Distrust FedSuns", "Compulsion/Distrust Lyrans",
        "Compulsion/Distrust of Inner Sphere",
        "Compulsion/Distrust of Non-Terrans", "Compulsion/Drug Addiction",
        "Compulsion/Falcon Pride", "Compulsion/Fire Mandrill Fanaticism",
        "Compulsion/Flashbacks", "Compulsion/Gambling", "Compulsion/Greedy",
        "Compulsion/Hate Ghost Bears", "Compulsion/Hate Hell's Horses",
        "Compulsion/Hate Invading Clans", "Compulsion/Hate Jade Falcons",
        "Compulsion/Hate Snow Ravens", "Compulsion/Hate Star Adder",
        "Compulsion/Hate Steel Vipers", "Compulsion/Hatred for Authority",
        "Compulsion/Hatred of Capellan Confederation",
        "Compulsion/Hatred of Clans", "Compulsion/Hatred of ComStar",
        "Compulsion/Hatred of Draconis Combine",
        "Compulsion/Hatred of Federated Suns",
        "Compulsion/Hatred of House Liao", "Compulsion/Hatred of House Marik",
        "Compulsion/Hatred of Umayyads", "Compulsion/Hatred of Word of Blake",
        "Compulsion/Hysteria", "Compulsion/Kindraa Fanaticism",
        "Compulsion/Loyalty to Crime Boss",
        "Compulsion/Loyalty to Draconis Combine",
        "Compulsion/Loyalty to House Kurita",
        "Compulsion/Necrosia Addiction", "Compulsion/Nostalgic",
        "Compulsion/Paranoia", "Compulsion/Paranoid",
        "Compulsion/Paranoid of Combine Government",
        "Compulsion/Rasalhague Pride", "Compulsion/Raven Pride",
        "Compulsion/Regression", "Compulsion/Religious Faith",
        "Compulsion/Smoking Addiction", "Compulsion/Split Personality",
        "Compulsion/Stubborn", "Compulsion/Traumatic Memories",
        "Compulsion/Wolf Pride", "Compulsion/Xenophobia",
        "Compulsion/Xenophobic"
    ];

    private static readonly string[] ClanProtocols =
        ["Protocol/Blood Spirit", "Protocol/Cloud Cobra", "Protocol/Coyote",
            "Protocol/Diamond Shark", "Protocol/Fire Mandrill",
            "Protocol/Ghost Bear", "Protocol/Goliath Scorpion",
            "Protocol/Hell's Horses", "Protocol/Ice Hellion",
            "Protocol/Jade Falcon", "Protocol/Nova Cat", "Protocol/Snow Raven",
            "Protocol/Star Adder", "Protocol/Steel Viper", "Protocol/Wolf"];

    private static readonly string[] HomeworldClanStreetwiseSkills =
        ["Streetwise/Blood Spirit", "Streetwise/Cloud Cobra", "Streetwise/Coyote",
            "Streetwise/Fire Mandrill", "Streetwise/Goliath Scorpion",
            "Streetwise/Ice Hellion", "Streetwise/Star Adder",
            "Streetwise/Steel Viper"];

    private static readonly string[] InvadingClanOrInnerSphereStreetwiseSkills =
        ["Streetwise/Diamond Shark", "Streetwise/Ghost Bear",
            "Streetwise/Hell's Horses", "Streetwise/Jade Falcon",
            "Streetwise/Nova Cat", "Streetwise/Snow Raven", "Streetwise/Wolf",
            "Streetwise/Capellan", "Streetwise/Combine", "Streetwise/FedSuns",
            "Streetwise/Free Worlds", "Streetwise/Lyran",
            "Streetwise/Rasalhague", "Streetwise/Terran"];

    private static readonly string[] AcademicInterests =
        ["Interests/Archaeology", "Interests/Biology", "Interests/Chemistry",
            "Interests/Geology", "Interests/History", "Interests/Law",
            "Interests/Literature", "Interests/Mathematics",
            "Interests/Pharmacology", "Interests/Physics",
            "Interests/Star League History"];

    private static readonly string[] NonCombatFlexibleOptions =
        ["Alternate ID", "Animal Empathy", "Attractive", "Connections", "Extra Income",
            "Fast Learner", "Good Hearing", "Good Vision", "Patient", "Property",
            "Reputation", "Wealth", "Acting", "Administration", "Animal Handling",
            "Appraisal", "Computers", "Leadership", "MedTech", "Negotiation",
            "Perception", "Strategy", "Training"];

    private static readonly string[] UniversityFieldNames =
        ["Cartographer", "Communications", "General Studies", "Manager", "Scientist",
            "Technician - Civilian", "Analysis", "Anthropologist", "Archaeologist",
            "Detective", "Engineer", "HPG Technician", "Planetary Surveyor",
            "Medical Assistant", "Politician", "Technician - Aerospace",
            "Technician - Vehicle", "Doctor", "Lawyer", "Military Scientist",
            "Technician - Mech", "Technician - Military"];

    private static readonly string[] MilitaryFieldNames =
        ["Basic Training", "Basic Training (Naval)", "Cavalry", "Infantry", "Marine",
            "MechWarrior", "Pilot - Aerospace (Combat)", "Pilot - Aircraft (Combat)",
            "Pilot - DropShip (Civilian)", "Scientist", "Scout", "Ship's Crew",
            "Doctor", "Infantry - Anti-'Mech", "Military Scientist",
            "Pilot - Battle Armor", "Pilot - JumpShip", "Pilot - WarShip",
            "Special Forces", "Medical Assistant", "Police Officer",
            "Technician - Military", "Police Tactical Officer",
            "Technician - Aerospace", "Technician - Mech", "Technician - Vehicle",
            "Detective", "Officer"];

    private static readonly string[] IntelligencePoliceFieldNames =
        ["Analysis", "Covert Operations", "Detective", "Intelligence",
            "Police Officer", "Police Tactical Officer"];

    private static readonly string[] TechFieldNames =
        ["Technician - Civilian", "Technician - Aerospace", "Technician - Mech",
            "Technician - Vehicle", "Technician - Military", "Engineer",
            "HPG Technician"];

    private static readonly string[] FedSunsLanguages =
        ["Language/English", "Language/French", "Language/German", "Language/Hindi",
            "Language/Russian"];

    private static readonly string[] LyranLanguages =
        ["Language/German", "Language/English", "Language/Italian",
            "Language/Scots Gaelic", "Language/Swedish"];

    private static readonly string[] FlexibleTraits =
    [
        "Alternate ID", "Ambidextrous", "Animal Empathy", "Attractive", "Combat Sense",
        "Connections", "Equipped", "Exceptional Attribute/STR", "Extra Income", "Fast Learner",
        "Fit", "Good Hearing", "Good Vision", "Natural Aptitude/Perception", "Pain Resistance",
        "Patient", "Property", "Reputation", "Sixth Sense", "Thick-Skinned", "Toughness", "Wealth",
        ..CompulsionTraits
    ];

    private static readonly string[] FlexibleSkills =
    [
        "Acting", "Administration", "Animal Handling", "Appraisal", "Climbing", "Computers",
        "Leadership", "Martial Arts", "MedTech", "Melee Weapons", "Negotiation", "Perception",
        "Running", "Small Arms", "Stealth", "Strategy", "Swimming", "Tracking", "Training"
    ];

    public static IReadOnlyList<LifePathModule> Affiliations { get; } =
    [
        Affiliation("fed-suns", "Federated Suns", 150,
            [Skill("Protocol/FedSuns", 10), PreAttribute("INT", 400)],
            [Choice("aptitude", "Natural Aptitude", EffectTarget.Trait, 100, 1,
                ["Natural Aptitude/Protocol", "Natural Aptitude/Strategy"])],
            ["Language/English", "Language/French", "Language/German", "Language/Hindi", "Language/Russian"],
            "FedSuns", ["Capellan March", "Crucis March", "Draconis March", "Outback"]),
        Affiliation("capellan", "Capellan Confederation", 150,
            [Attribute("WIL", 50), Trait("Exceptional Attribute/EDG", 100),
                Trait("Compulsion/Paranoia", -100), Skill("Protocol/Capellan", 10),
                Skill("Martial Arts", 5)],
            [Choice("secondary-language", "Secondary language", EffectTarget.Skill, 10, 1,
                ["Language/Russian", "Language/Cantonese", "Language/Vietnamese", "Language/English"])],
            ["Language/Mandarin Chinese", "Language/Russian", "Language/Cantonese",
                "Language/Vietnamese", "Language/English"],
            "Capellan", ["Capellan Commonality", "Liao Commonality", "Sian Commonality",
                "St. Ives Commonality", "Victoria Commonality"]),
        Affiliation("draconis", "Draconis Combine", 150,
            [Attribute("WIL", 50), Trait("Compulsion/Xenophobia", -100), Trait("Wealth", -50),
                Skill("Art/Oral Tradition", 15), Skill("Martial Arts", 10), Skill("Protocol/Combine", 15)],
            [Choice("combat-trait", "Combat trait", EffectTarget.Trait, 100, 1,
                ["Pain Resistance", "Combat Sense"])],
            ["Language/Japanese", "Language/Arabic", "Language/English", "Language/Swedenese"],
            "Combine", ["Azami", "Benjamin District", "Dieron District",
                "New Samarkand (Galedon) District", "Pesht District"]),
        Affiliation("free-worlds", "Free Worlds League", 150, [],
            [Choice("secondary-language", "Secondary language", EffectTarget.Skill, 15, 1,
                ["Language/Greek", "Language/Hindi", "Language/Italian", "Language/Mandarin Chinese",
                    "Language/Mongolian", "Language/Romanian", "Language/Slovak",
                    "Language/Spanish", "Language/Urdu"]),
                Choice("art", "Art specialty", EffectTarget.Skill, 10, 1, Arts)],
            ["Language/English", "Language/Greek", "Language/Hindi", "Language/Italian",
                "Language/Mandarin Chinese", "Language/Mongolian", "Language/Romanian",
                "Language/Slovak", "Language/Spanish", "Language/Urdu"],
            "Free Worlds", ["Marik Commonwealth", "Principality of Regulus", "Duchy of Oriente",
                "Duchy of Andurien", "Other FWL Worlds"]),
        Affiliation("lyran", "Lyran Alliance", 150,
            [Attribute("WIL", -50), Attribute("EDG", -50), Trait("Equipped", 100),
                Trait("Extra Income", 50), Trait("Wealth", 100), Skill("Negotiation", 15),
                Skill("Appraisal", 10), Skill("Protocol/Lyran", 15)],
            [Choice("drawback", "Drawback", EffectTarget.Trait, -100, 1,
                ["Combat Paralysis", "Glass Jaw"])],
            ["Language/German", "Language/English", "Language/Italian",
                "Language/Scots Gaelic", "Language/Swedish"],
            "Lyran", ["Alarion Province", "Bolan Province", "Coventry Province",
                "Donegal Province", "Skye Province"]),
        Affiliation("rasalhague", "Free Rasalhague Republic", 100,
            [Attribute("WIL", 25), Attribute("EDG", -25), Skill("Negotiation", 15)],
            [Choice("interest", "Interest", EffectTarget.Skill, 10, 1, Interests)],
            ["Language/Swedish", "Language/English", "Language/Japanese",
                "Language/Swedenese", "Language/German"],
            "Rasalhague", ["Clan War Expatriate", "Ghost Bear Dominion"]),
        Affiliation("minor-periphery", "Minor Periphery", 75,
            [Trait("Equipped", -150), Skill("Perception", 15), Skill("Survival", 20)],
            [Choice("flex", "Attributes or traits", EffectTarget.Flexible, 25, 3,
                Attributes.Concat(FlexibleTraits).ToArray())
                with { FixedFlexibleSelections = true }],
            Languages, "Periphery", ["Fiefdom of Randis", "Franklin Fiefs", "Mica Majority",
                "Niops Association", "Rim Collection"]),
        Affiliation("major-periphery", "Major Periphery State", 100,
            [Trait("Equipped", -50)],
            [FlexibleChoice("flex", "Attributes, traits, or skills", 15, 3)
                with { FixedFlexibleSelections = true }],
            ["Language/English"], "Periphery",
            ["Circinus Federation", "Magistracy of Canopus", "Marian Hegemony",
                "Outworlds Alliance", "Taurian Concordat"]),
        Affiliation("deep-periphery", "Deep Periphery", 50,
            [Attribute("WIL", 60), Trait("Equipped", -80)],
            [FlexibleChoice("flex", "Attributes, traits, or skills", 10, 2)
                with { FixedFlexibleSelections = true }],
            Languages, "Periphery",
            ["Hanseatic League", "Castilian Principalities", "Umayyad Caliphate", "JarnFolk"]),
        Affiliation("invading-clan", "Invading Clan", 75,
            [Trait("Compulsion/Arrogance", -50), Trait("Compulsion/Distrust of Inner Sphere", -100),
                Skill("Interests/Clan Remembrance", 25), Skill("Protocol/Clan", 25)], [],
            ["Language/English"], "Clan",
            ["Diamond Shark", "Ghost Bear", "Hell's Horses", "Jade Falcon", "Nova Cat", "Snow Raven", "Wolf"],
            CreateClanCastes()),
        Affiliation("homeworld-clan", "Homeworld Clan", 50,
            [Trait("Compulsion/Distrust of Inner Sphere", -100),
                Trait("Compulsion/Hate Invading Clans", -100),
                Skill("Interests/Clan Remembrance", 25), Skill("Protocol/Clan", 25)], [],
            ["Language/English"], "Clan",
            ["Blood Spirit", "Cloud Cobra", "Coyote", "Fire Mandrill", "Goliath Scorpion",
                "Ice Hellion", "Star Adder", "Steel Viper"], CreateClanCastes()),
        new("comstar", "ComStar",
            "ComStar order affiliation, combined with a full-cost birth affiliation.",
            50,
            [Trait("Enemy", -100), Trait("Equipped", 100), Trait("Rank", 50),
                Trait("Reputation", -50), Attribute("INT", 25), Attribute("WIL", -15),
                Trait("Connections", 50), Trait("Enemy/Word of Blake", -100),
                Trait("Reputation", 20), Skill("Communications/Conventional", 10),
                Skill("Interests/Writings of Jerome Blake", 10),
                Skill("Negotiation", 10), Skill("Protocol/ComStar", 15)],
            [Choice("nearest-protocol", "Nearest-state protocol",
                    EffectTarget.Skill, 15, 1, InnerSphereProtocols),
                Choice("technician", "Technician specialty", EffectTarget.Skill,
                    10, 1, Technicians)],
            Languages: ["Language/English", .. Languages],
            ProtocolSkill: "Protocol/ComStar",
            StreetwiseSkill: "Streetwise/ComStar"),
        new("word-of-blake", "Word of Blake",
            "Word of Blake order affiliation, combined with a full-cost birth affiliation.",
            50,
            [Trait("Enemy", -100), Trait("Equipped", 130), Trait("Rank", 50),
                Trait("Reputation", -50), Attribute("WIL", 50), Attribute("CHA", -50),
                Trait("Compulsion/Paranoid", -50), Trait("Connections", 75),
                Trait("Enemy/ComStar", -100),
                Skill("Communications/Conventional", 10),
                Skill("Interests/Writings of Jerome Blake", 25),
                Skill("Interests/Writings of the Master", 15),
                Skill("Negotiation", 20), Skill("Protocol/Word of Blake", 10)],
            [Choice("nearest-protocol", "Nearest-state protocol",
                    EffectTarget.Skill, 5, 1, InnerSphereProtocols),
                Choice("technician", "Technician specialty", EffectTarget.Skill,
                    10, 1, Technicians)],
            Languages: ["Language/English", .. Languages],
            ProtocolSkill: "Protocol/Word of Blake",
            StreetwiseSkill: "Streetwise/Word of Blake"),
        Affiliation("terran", "Terran", 240,
            [Attribute("INT", 100), Attribute("EDG", -150),
                Trait("Compulsion/Distrust of Non-Terrans", -75), Trait("Reputation", 100),
                Skill("Language/English", 25)],
            [Choice("attributes", "Other attributes", EffectTarget.Attribute, 50, 2,
                ["STR", "BOD", "RFL", "DEX", "WIL", "CHA"]),
                Choice("languages", "Other languages", EffectTarget.Skill, 15, 2,
                    Languages.Where(language => language != "Language/English").ToArray())],
            Languages, "Terran", ["Belter", "Lunar Citizen", "Martian Citizen",
                "Outer System Citizen", "Terran Citizen", "Venusian Citizen"]),
        Affiliation("independent", "Independent", 50,
            [Attribute("WIL", 20), Attribute("EDG", 20), Trait("Equipped", -20),
                Trait("Reputation", -10), Trait("Wealth", -10)], [],
            Languages, "Independent", ["Antallos", "Astrokaszy", "Generic", "Mercenary",
                "Pirate", "Spacer", "Tortuga"])
    ];

    public static IReadOnlyList<LifePathModule> Childhoods { get; } =
    [
        new("back-woods", "Back Woods", "A hardy upbringing in a remote frontier community.", 290,
            [Attribute("STR", 100), Attribute("BOD", 100), Attribute("RFL", 75),
                Attribute("INT", -25), Attribute("CHA", -50), Trait("Equipped", -50),
                Trait("Fit", 100), Trait("Illiterate", -75), Trait("Toughness", 75),
                Trait("Wealth", -75), Skill("Martial Arts", 10), Skill("Melee Weapons", 10),
                Skill("Navigation/Ground", 10), Skill("Perception", 5), Skill("Running", 10),
                Skill("Tracking/Wilds", 10), PreAttribute("STR", 400), PreAttribute("BOD", 500)],
            [Choice("survival", "Survival specialty", EffectTarget.Skill, 15, 1,
                ["Survival/Arctic", "Survival/Forests", "Survival/Desert", "Survival/Jungle",
                    "Survival/Ocean", "Survival/Mountain", "Survival/Steppe"]),
                FlexibleAttributeOrTraitChoice("flex", "Flexible XP", 25, 2)]),
        new("mercenary-brat", "Born Mercenary Brat", "Raised in the mobile culture of a mercenary command.", 270,
            [Attribute("STR", 75), Attribute("BOD", 50), Attribute("RFL", 100),
                Attribute("WIL", 25), Attribute("CHA", -25), Attribute("EDG", 25),
                Trait("Equipped", 50), Trait("Illiterate", -50), Trait("Reputation", -50),
                Skill("Career/Soldier", 10), Skill("Interests/Military History", 5),
                Skill("Martial Arts", 15), Skill("Melee Weapons", 10), Skill("Negotiation", 5),
                Skill("Perception", 5), PreAttribute("STR", 400), PreAttribute("BOD", 400),
                PreAttribute("WIL", 400)],
            [Choice("language", "Language", EffectTarget.Skill, 10, 1, Languages),
                Choice("streetwise", "Streetwise", EffectTarget.Skill, 10, 1, StreetwiseOptions)]),
        new("farm", "Farm", "A rural upbringing built around agriculture and hard work.", 275,
            [Attribute("STR", 100), Attribute("BOD", 100), Attribute("DEX", 25),
                Attribute("CHA", -50), Trait("Animal Empathy", 25), Trait("Illiterate", -25),
                Trait("Toughness", 50), Trait("Wealth", -25), Skill("Career/Agriculture", 10),
                Skill("Animal Handling", 15)],
            [Choice("interests", "Interests", EffectTarget.Skill, 5, 2, Interests),
                FlexibleAttributeOrTraitChoice("flex", "Flexible XP", 10, 4)]),
        new("fugitives", "Fugitives", "A childhood spent hiding, moving, and learning self-reliance.", 225,
            [Attribute("STR", 25), Attribute("RFL", 100), Attribute("WIL", 100),
                Attribute("EDG", 100), Trait("Connections", 75), Trait("Dark Secret", -100),
                Trait("Illiterate", -50), Trait("Introvert", -100), Trait("Wealth", -100),
                Skill("Acting", 5), Skill("Perception", 10), Skill("Running", 10),
                Skill("Stealth", 10), Skill("Zero-G Operations", 5)],
            [Choice("survival-trait", "Survival trait", EffectTarget.Trait, 100, 1,
                ["Combat Sense", "Fit", "Good Hearing", "Good Vision", "Patient",
                    "Thick-Skinned", "Toughness"]),
                Choice("language", "Language", EffectTarget.Skill, 5, 1, Languages),
                Choice("streetwise", "Streetwise", EffectTarget.Skill, 10, 1, StreetwiseOptions),
                FlexibleAttributeOrTraitChoice("flex", "Flexible XP", 5, 4)]),
        new("nobility", "Nobility", "A privileged upbringing shaped by status and political expectation.", 215,
            [Trait("Title/Inner Sphere", 300), PreTrait("Wealth", 500),
                PreTrait("Title", 500), PreTrait("Property", 500)],
            [FlexibleAttributeOrTraitChoice("flex", "Flexible XP", 5, 4)]),
        new("slave", "Slave", "A harsh childhood under forced labor and rigid control.", 45,
            [Attribute("STR", 100), Attribute("BOD", 75), Attribute("DEX", 100),
                Attribute("INT", -50), Attribute("WIL", -50), Trait("Equipped", -100),
                Trait("Illiterate", -90), Trait("Patient", 100), Trait("Reputation", -100),
                Trait("Wealth", -200), Skill("Stealth", 15), PreAttribute("STR", 400),
                PreAttribute("BOD", 400)],
            [Choice("career", "Career", EffectTarget.Skill, 15, 1, Careers),
                Choice("interest", "Interest", EffectTarget.Skill, 10, 1, Interests),
                Choice("technician", "Technician specialty", EffectTarget.Skill, 5, 1, Technicians),
                Choice("gift", "Exceptional gift", EffectTarget.Trait, 90, 1,
                    ["Exceptional Attribute/STR", "Exceptional Attribute/BOD",
                        "Exceptional Attribute/RFL", "Exceptional Attribute/DEX",
                        "Exceptional Attribute/INT", "Exceptional Attribute/WIL",
                        "Exceptional Attribute/CHA", "Exceptional Attribute/EDG",
                        "Natural Aptitude/Perception"]),
                FlexibleAttributeOrTraitChoice("flex", "Flexible XP", 25, 4)],
            AffiliationLanguageXp: -5, AffiliationProtocolXp: 15, AffiliationStreetwiseXp: 15),
        new("street", "Street", "A precarious urban childhood built on instinct and local connections.", 250,
            [Attribute("STR", 25), Attribute("BOD", -20), Attribute("RFL", 100),
                Attribute("INT", -50), Attribute("WIL", 100), Attribute("CHA", -25),
                Attribute("EDG", 100), Trait("Connections", 75), Trait("Compulsion/Paranoid", -50),
                Trait("Enemy", -100), Trait("Illiterate", -75), Trait("Reputation", -100),
                Trait("Toughness", 200), Trait("Wealth", -75), Skill("Martial Arts", 15),
                Skill("Melee Weapons", 5), Skill("Perception", 10), Skill("Running", 10),
                Skill("Stealth", 10)],
            [FlexibleAttributeOrTraitChoice("flex", "Flexible XP", 10, 4)],
            AffiliationLanguageXp: -5, AffiliationStreetwiseXp: 10),
        new("trueborn-creche", "Trueborn Creche", "A Clan trueborn upbringing among a sibko.", 300,
            [Attribute("STR", 100), Attribute("BOD", 125), Attribute("RFL", 125),
                Attribute("WIL", 100), Attribute("CHA", -75), Trait("Compulsion/Clan Honor", -100),
                Trait("Slow Learner", -300), Trait("Trueborn", 200),
                Skill("Interests/Clan Remembrance", 10), Skill("Martial Arts", 10),
                Skill("Melee Weapons", 5), Skill("Protocol/Clan", 10), Skill("Small Arms", 5),
                Skill("Swimming", 10), PreTrait("Phenotype", 0), PreTrait("Trueborn", 0)],
            [Choice("phenotype", "Phenotype", EffectTarget.Trait, 0, 1,
                ["Phenotype/Aerospace", "Phenotype/Elemental", "Phenotype/MechWarrior"]),
                FlexibleAttributeOrTraitChoice("flex", "Flexible XP", 15, 5)]),
        new("war-orphan", "War Orphan", "Loss and conflict forced an early independence.", 170,
            [Attribute("INT", 50), Attribute("WIL", 100), Attribute("EDG", 100),
                Trait("Compulsion/Traumatic Memories", -100), Trait("Illiterate", -25),
                Trait("Introvert", -50), Trait("Reputation", -50), Trait("Sixth Sense", 150),
                Trait("Wealth", -100), Skill("Perception", 10), Skill("Stealth", 5)],
            [FlexibleAttributeOrTraitChoice("flex", "Flexible XP", 25, 3)],
            AffiliationLanguageXp: -5, AffiliationStreetwiseXp: 10),
        new("white-collar", "White Collar", "A sheltered upbringing among prosperous professionals.", 170,
            [Attribute("STR", -50), Attribute("BOD", -50), Attribute("INT", 75),
                Attribute("WIL", -50), Attribute("CHA", 75), Trait("Equipped", 75),
                Trait("Enemy", -100), Trait("Extra Income", 50), Trait("Glass Jaw", -50),
                Trait("Reputation", 50), Trait("Wealth", 100), PreTrait("Property", 300),
                PreTrait("Wealth", 300)],
            [Choice("art", "Art specialty", EffectTarget.Skill, 10, 1, Arts),
                Choice("interest", "Interest", EffectTarget.Skill, 10, 1, Interests),
                FlexibleAttributeOrTraitChoice("flex", "Flexible XP", 5, 3)])
    ];

    public static IReadOnlyList<LifePathModule> LateChildhoods { get; } =
    [
        new("late-adolescent-warfare", "Adolescent Warfare",
            "Teenage years spent surviving gangs or an active war zone.", 500,
            [Attribute("BOD", 40), Attribute("RFL", 40), Attribute("WIL", 50),
                Attribute("INT", -30), Trait("Combat Sense", 80), Trait("Connections", 30),
                Trait("Compulsion/Paranoid", -20), Trait("Enemy", -40), Trait("Wealth", -20),
                Skill("Leadership", 25), Skill("MedTech", 25), Skill("Melee Weapons", 25),
                Skill("Negotiation", 15), Skill("Perception", 25), Skill("Running", 40),
                Skill("Small Arms", 20), Skill("Stealth", 30)],
            [Choice("survival", "Survival", EffectTarget.Skill, 25, 1, SurvivalSkills),
                FlexiblePoolChoice("flex", "Flexible XP", 130)],
            AffiliationLanguageXp: -25, AffiliationProtocolXp: -10,
            AffiliationStreetwiseXp: 45),
        new("late-back-woods", "Back Woods",
            "A teenage survivalist life far from modern society.", 500,
            [Attribute("BOD", 60), Attribute("WIL", 70), Attribute("INT", -20),
                Trait("Animal Empathy", 80), Trait("Good Hearing", 40),
                Trait("Introvert", -20), Trait("Wealth", -20), Skill("Climbing", 30),
                Skill("MedTech", 20), Skill("Melee Weapons", 20), Skill("Perception", 45),
                Skill("Small Arms", 20), Skill("Stealth", 40),
                Skill("Survival/Forests", 25), Skill("Tracking/Wilds", 30)],
            [FlexiblePoolChoice("flex", "Flexible XP", 125)],
            AffiliationProtocolXp: -15),
        new("late-clan-apprenticeship", "Clan Apprenticeship",
            "A lower-caste Clan apprenticeship in an assigned trade.", 500,
            [Skill("Administration", 35), Skill("Interests/Clan History", 30)],
            [Choice("computers", "Computers", EffectTarget.Skill, 50, 1,
                ["Computers/Programming", "Computers/Hacking"]),
                Choice("interest", "Interest", EffectTarget.Skill, 30, 1, Interests),
                ConditionalChoice("caste", "Apprenticeship caste",
                    new Dictionary<string, IReadOnlyList<ModuleEffect>>
                    {
                        ["Laborer Caste"] =
                        [
                            Attribute("BOD", 30), Skill("Career/Agriculture", 50),
                            Skill("Computers/Programming", 40),
                            Skill("Driving/Ground Vehicle", 20), PreAttribute("BOD", 400)
                        ],
                        ["Merchant Caste"] =
                        [
                            Attribute("CHA", 30), Skill("Administration", 50),
                            Skill("Appraisal", 40), Skill("Negotiation", 20),
                            PreAttribute("CHA", 400)
                        ],
                        ["Scientist Caste"] =
                        [
                            Attribute("INT", 30), Skill("Computers/Programming", 30),
                            Skill("Interests/Clan History", 10), Skill("MedTech", 20),
                            Skill("Science/Physics", 50), PreAttribute("INT", 400)
                        ],
                        ["Technician Caste"] =
                        [
                            Attribute("DEX", 30), Skill("Computers/Programming", 30),
                            Skill("Perception", 20), Skill("Technician/Mechanical", 60),
                            PreAttribute("DEX", 400)
                        ]
                    }),
                FlexiblePoolChoice("flex", "Flexible XP", 165)]),
        new("late-farm", "Farm", "Teenage years spent learning agricultural work.", 400,
            [Attribute("DEX", 40), Attribute("CHA", -20), Trait("Animal Empathy", 30),
                Skill("Administration", 35), Skill("Animal Handling", 30),
                Skill("Career/Agriculture", 50), Skill("Driving/Ground Vehicle", 30),
                Skill("Small Arms", 30)],
            [Choice("interest-major", "Primary interest", EffectTarget.Skill, 40, 1, Interests),
                Choice("interest-minor", "Secondary interest", EffectTarget.Skill, 20, 1, Interests),
                FlexiblePoolChoice("flex", "Flexible XP", 115)]),
        new("late-freeborn-sibko", "Freeborn Sibko",
            "Clan military training for an aspiring freeborn warrior.", 950,
            [Attribute("BOD", 50), Attribute("WIL", 50), Attribute("CHA", -30),
                Trait("Compulsion/Clan Honor", -30), Trait("Rank", 100),
                Trait("Reputation", 40), Skill("Career/Soldier", 50),
                Skill("Interests/Clan Remembrance", 20), Skill("Negotiation", 50),
                PreAttribute("BOD", 300), PreAttribute("DEX", 400),
                PreAttribute("RFL", 300), PreAttribute("WIL", 400)],
            [ConditionalChoice("branch", "Branch of service", CreateFreebornBranches())]),
        new("late-high-school", "High School",
            "A general academic education preparing a teenager for adult life.", 400,
            [Attribute("CHA", 25), Attribute("INT", 25), Trait("Connections", 20),
                Skill("Computers", 20), Skill("Swimming", 20)],
            [Choice("interest", "Interest", EffectTarget.Skill, 35, 1, Interests),
                FlexiblePoolChoice("flex", "Flexible XP", 185)],
            AffiliationLanguageXp: 10, AffiliationStreetwiseXp: 20),
        new("late-mercenary-brat", "Mercenary Brat",
            "Teenage years traveling and working with a mercenary command.", 600,
            [Attribute("WIL", 35), Attribute("EDG", 50), Attribute("INT", -20),
                Attribute("CHA", -20), Trait("Connections", 40), Trait("Tech Empathy", 20),
                Skill("Career/Soldier", 50), Skill("Driving/Ground Vehicle", 15),
                Skill("Swimming", 20), Skill("Martial Arts", 30), Skill("MedTech", 10),
                Skill("Negotiation", 50), Skill("Perception", 30)],
            [Choice("interest", "Interest", EffectTarget.Skill, 30, 1, Interests),
                Choice("languages", "Languages", EffectTarget.Skill, 30, 2, Languages),
                Choice("streetwise", "Streetwise", EffectTarget.Skill, 30, 1, StreetwiseOptions),
                Choice("tactics", "Tactics", EffectTarget.Skill, 10, 1,
                    ["Tactics/Infantry", "Tactics/Land", "Tactics/Sea", "Tactics/Air",
                        "Tactics/Space"]),
                Choice("technician", "Technician", EffectTarget.Skill, 30, 1, Technicians),
                Choice("second-interest", "Additional interest", EffectTarget.Skill, 20, 1, Interests),
                FlexiblePoolChoice("flex", "Flexible XP", 150)]),
        new("late-military-school", "Military School",
            "A disciplined military education during the teenage years.", 500,
            [Attribute("CHA", 50), Trait("Connections", 15), Trait("Fit", 15),
                Trait("Rank", 20), Skill("Career/Soldier", 25), Skill("Computers", 35),
                Skill("Interests/Military History", 40), Skill("Leadership", 20),
                Skill("Martial Arts", 30), Skill("MedTech", 10), Skill("Melee Weapons", 20),
                Skill("Running", 30), Skill("Small Arms", 50), Skill("Strategy", 10),
                Skill("Swimming", 30), PreAttribute("WIL", 300)],
            [Choice("interest", "Interest", EffectTarget.Skill, 30, 1, Interests),
                FlexiblePoolChoice("flex", "Flexible XP", 40)],
            AffiliationProtocolXp: 30),
        new("late-preparatory-school", "Preparatory School",
            "An elite education aimed at university and influential careers.", 500,
            [Attribute("CHA", 60), Trait("Connections", 40), Trait("Extra Income", 20),
                Trait("Gregarious", 20), Skill("Archery", 20), Skill("Computers", 25),
                Skill("MedTech", 10), Skill("Melee Weapons", 15)],
            [Choice("interest-one", "Primary interest", EffectTarget.Skill, 30, 1, Interests),
                Choice("interest-two", "Secondary interest", EffectTarget.Skill, 20, 1, Interests),
                Choice("interest-three", "Additional interest", EffectTarget.Skill, 20, 1, Interests),
                Choice("language", "Language", EffectTarget.Skill, 20, 1, Languages),
                FlexiblePoolChoice("flex", "Flexible XP", 160)],
            AffiliationProtocolXp: 40),
        new("late-spacer-family", "Spacer Family",
            "Raised aboard DropShips and JumpShips in low gravity.", 490,
            [Attribute("RFL", 40), Attribute("DEX", 30), Attribute("BOD", -20),
                Attribute("STR", -20), Trait("Equipped", 20), Trait("G-Tolerance", 40),
                Trait("Natural Aptitude/Zero-G Operations", 20), Trait("Introvert", -25),
                Skill("Career/Ship's Crew", 30), Skill("Communications/Conventional", 20),
                Skill("Computers", 20), Skill("Gunnery/Spacecraft", 10),
                Skill("Navigation/Space", 20), Skill("Perception", 15),
                Skill("Piloting/Spacecraft", 15), Skill("Sensor Operations", 15),
                Skill("Technician/Aeronautics", 20), Skill("Technician/Electronic", 20),
                Skill("Zero-G Operations", 15), PreAttribute("RFL", 400),
                PreAttribute("DEX", 400), PreAttribute("INT", 400),
                PreSkill("Zero-G Operations", 50)],
            [Choice("language", "Language", EffectTarget.Skill, 15, 1, Languages),
                FlexiblePoolChoice("flex", "Flexible XP", 175)]),
        new("late-street", "Street", "Teenage survival among gangs and authorities.", 400,
            [Attribute("BOD", 20), Attribute("EDG", 40), Attribute("WIL", 10),
                Attribute("CHA", -20), Trait("Combat Sense", 15), Trait("Connections", 20),
                Trait("Enemy", -20), Trait("Illiterate", -20), Trait("Reputation", -20),
                Skill("Acting", 20), Skill("Climbing", 15), Skill("Disguise", 20),
                Skill("Escape Artist", 20), Skill("Interrogation", 20),
                Skill("Martial Arts", 20), Skill("MedTech", 10), Skill("Melee Weapons", 25),
                Skill("Negotiation", 20), Skill("Perception", 25), Skill("Running", 25),
                Skill("Small Arms", 20), Skill("Stealth", 15)],
            [Choice("interest", "Interest", EffectTarget.Skill, 20, 1, Interests),
                FlexiblePoolChoice("flex", "Flexible XP", 60)],
            AffiliationStreetwiseXp: 40),
        new("late-trueborn-sibko", "Trueborn Sibko",
            "The intense military training regimen of a trueborn Clan warrior.", 1500,
            [PreAttribute("BOD", 300), PreAttribute("DEX", 400),
                PreAttribute("RFL", 300), PreAttribute("WIL", 300)],
            [ConditionalChoice("branch", "Branch of service", CreateTruebornBranches())]),
        new("late-civilian-job", "Civilian Job",
            "Honest work within the infrastructure of ordinary society.", 600,
            [Skill("Administration", 75), Skill("Computers", 40),
                Skill("Leadership", 40), Skill("Negotiation", 30)],
            [Choice("career", "Non-military career", EffectTarget.Skill, 40, 1,
                Careers.Where(career => career != "Career/Soldier").ToArray()),
                Choice("driving", "Driving", EffectTarget.Skill, 60, 1,
                    ["Driving/Ground Vehicles", "Driving/Rail Vehicles", "Driving/Sea Vehicles"]),
                Choice("interest", "Interest", EffectTarget.Skill, 100, 1, Interests),
                FlexiblePoolChoice("flex", "Flexible XP", 85)],
            AffiliationProtocolXp: 50)
    ];

    public static IReadOnlyList<LifePathModule> EducationSchools { get; } =
    [
        School("technical-college", "Technical College", 600,
            [Attribute("DEX", 100), Attribute("INT", 100), Trait("Equipped", 150),
                Skill("Computers", 20)],
            [Choice("interest", "Interest", EffectTarget.Skill, 30, 1, Interests),
                FlexiblePoolChoice("flex", "Flexible XP", 200)],
            ["Communications", "Pilot - Aerospace (Civilian)", "Pilot - Aircraft (Civilian)",
                "Pilot - DropShip (Civilian)", "Pilot - Exoskeleton", "Technician - Civilian"],
            ["Cartographer", "Engineer", "Merchant Marine", "Pilot - IndustrialMech",
                "Pilot - JumpShip", "Technician - Aerospace", "Technician - Mech",
                "Technician - Vehicle"]),
        School("trade-school", "Trade School", 560,
            [Attribute("INT", 50), Trait("Connections", 50), Trait("Equipped", 100)],
            [Choice("skills", "General skills", EffectTarget.Skill, 20, 3, FlexibleSkills),
                Choice("attribute", "Attribute", EffectTarget.Attribute, 100, 1,
                    ["STR", "BOD", "RFL", "DEX", "WIL", "CHA", "EDG"]),
                FlexiblePoolChoice("flex", "Flexible XP", 200)],
            ["General Studies", "Merchant"],
            ["Analysis", "Anthropologist", "Archaeologist", "Cartographer", "Communications",
                "HPG Technician", "Journalist", "Manager", "Medical Assistant", "Merchant Marine"]),
        School("university", "University", 710,
            [Attribute("INT", 150), Attribute("WIL", 75), Attribute("CHA", 25),
                Attribute("EDG", 25), Trait("Connections", 200), Trait("Equipped", 50),
                Trait("Reputation", 75), Trait("Wealth", -200), Skill("Computers", 25),
                Skill("Perception", 25), PreAttribute("INT", 400)],
            [Choice("interest", "Interest", EffectTarget.Skill, 20, 1, Interests),
                FlexiblePoolChoice("flex", "Flexible XP", 220)],
            ["Cartographer", "Communications", "General Studies", "Manager", "Scientist",
                "Technician - Civilian"],
            ["Analysis", "Anthropologist", "Archaeologist", "Detective", "Engineer",
                "HPG Technician", "Planetary Surveyor", "Medical Assistant", "Politician",
                "Technician - Aerospace", "Technician - Vehicle"],
            ["Doctor", "Lawyer", "Military Scientist", "Technician - Mech",
                "Technician - Military"], specialistFieldYears: 2, protocolXp: 20),
        School("solaris-internship", "Solaris Internship", 700,
            [Attribute("CHA", 150), Attribute("EDG", 50), Trait("Connections", 100),
                Trait("Enemy", -50), Trait("Reputation", 100), Skill("Acting", 25),
                Skill("Interests/Solaris Games", 30), Skill("Perception", 20),
                PreTrait("Connections", 200)],
            [Choice("attribute", "Attribute", EffectTarget.Attribute, 50, 1,
                    ["STR", "BOD", "RFL", "DEX", "INT", "WIL"]),
                Choice("equipment", "Equipment benefit", EffectTarget.Trait, 100, 1,
                    ["Equipped", "Vehicle"]),
                Choice("streetwise", "Streetwise", EffectTarget.Skill, 25, 1, StreetwiseOptions),
                FlexiblePoolChoice("flex", "Flexible XP", 100)],
            ["Communications", "Manager", "Technician - Military"],
            ["Cavalry", "Journalist", "MechWarrior", "Pilot - Battle Armor", "Politician",
                "Technician - Mech"],
            basicFieldYears: 2),
        School("police-academy", "Police Academy", 680,
            [Attribute("RFL", 100), Attribute("WIL", 100), Trait("Connections", 50),
                Trait("Rank", 100), Trait("Reputation", 100), Skill("Computers", 15),
                ],
            [Choice("driving", "Driving specialty", EffectTarget.Skill, 20, 1,
                    ["Driving/Ground Vehicles", "Driving/Rail Vehicles",
                        "Driving/Sea Vehicles"]),
                FlexiblePoolChoice("flex", "Flexible XP", 140)],
            ["Police Officer"],
            ["Analysis", "Communications", "Detective", "Intelligence",
                "Technician - Military"],
            ["Covert Operations", "Police Tactical Officer", "Special Forces",
                "Technician - Aerospace", "Technician - Vehicle"],
            basicFieldYears: 0, advancedFieldYears: 1, specialistFieldYears: 2,
            protocolXp: 25, streetwiseXp: 30),
        School("intelligence-training", "Intelligence Operative Training", 760,
            [Attribute("INT", 100), Attribute("WIL", 150), Trait("Alternate ID", 50),
                Trait("Connections", 200), Trait("In For Life", -300), Trait("Rank", 250),
                Trait("Wealth", 50), Skill("Acting", 20), Skill("Computers", 20),
                PreAttribute("INT", 400), PreAttribute("WIL", 500),
                PreTrait("Connections", 200)],
            [Choice("attribute", "Attribute", EffectTarget.Attribute, 50, 1,
                    ["STR", "BOD", "RFL", "DEX", "CHA", "EDG"]),
                FlexiblePoolChoice("flex", "Flexible XP", 150)],
            ["Basic Training"],
            ["Analysis", "Covert Operations", "Detective", "Intelligence", "Police Officer",
                "Scout"],
            ["Police Tactical Officer", "Special Forces"],
            advancedFieldYears: 1, specialistFieldYears: 2, protocolXp: 20),
        School("military-academy", "Military Academy", 830,
            [Attribute("STR", 50), Attribute("BOD", 100), Attribute("RFL", 125),
                Attribute("WIL", 100), Trait("Equipped", 100), Trait("Rank", 200),
                Skill("Interests/Military History", 15), Skill("Leadership", 10),
                Skill("Swimming", 15)],
            [FlexiblePoolChoice("flex", "Flexible XP", 100)],
            ["Basic Training", "Basic Training (Naval)"],
            ["Analysis", "Cavalry", "Infantry", "Marine", "MechWarrior",
                "Pilot - Aerospace (Combat)", "Pilot - Aircraft (Combat)",
                "Pilot - DropShip (Civilian)", "Scientist", "Scout", "Ship's Crew"],
            ["Doctor", "Infantry - Anti-'Mech", "Military Scientist", "Pilot - Battle Armor",
                "Pilot - JumpShip", "Pilot - WarShip", "Special Forces"],
            advancedFieldYears: 1, specialistFieldYears: 2, protocolXp: 15),
        School("military-enlistment", "Military Enlistment", 720,
            [Attribute("STR", 125), Attribute("BOD", 125), Attribute("RFL", 100),
                Attribute("WIL", 100), Attribute("CHA", -100), Trait("Equipped", 50),
                Trait("Rank", 100), Skill("Swimming", 20)],
            [FlexiblePoolChoice("flex", "Flexible XP", 200)],
            ["Basic Training", "Basic Training (Naval)"],
            ["Cavalry", "Infantry", "Marine", "Medical Assistant", "Police Officer", "Scout",
                "Ship's Crew", "Technician - Military"],
            ["Police Tactical Officer", "Infantry - Anti-'Mech", "Special Forces",
                "Technician - Aerospace", "Technician - Mech", "Technician - Vehicle"],
            basicFieldYears: 0, advancedFieldYears: 1, specialistFieldYears: 1),
        School("family-training", "Family Training", 570,
            [Attribute("STR", 75), Attribute("BOD", 75), Attribute("RFL", 50),
                Attribute("WIL", 50), Trait("Equipped", 50), Trait("Rank", 100),
                Skill("Interests/Homeworld History", 20)],
            [Choice("driving", "Driving specialty", EffectTarget.Skill, 15, 1,
                    ["Driving/Ground Vehicles", "Driving/Rail Vehicles",
                        "Driving/Sea Vehicles"]),
                Choice("survival", "Survival specialty", EffectTarget.Skill, 20, 1,
                    SurvivalSkills),
                FlexiblePoolChoice("flex", "Flexible XP", 100)],
            ["Basic Training", "Basic Training (Naval)"],
            ["Cavalry", "Infantry", "Marine", "MechWarrior", "Pilot - Aerospace (Combat)",
                "Pilot - Aircraft (Combat)", "Pilot - DropShip (Civilian)", "Scout",
                "Ship's Crew"],
            ["Infantry - Anti-'Mech", "Pilot - Battle Armor", "Pilot - JumpShip"],
            basicFieldYears: 0, advancedFieldYears: 1, specialistFieldYears: 2,
            protocolXp: 15),
        School("officer-candidate", "Officer Candidate School", 550,
            [Attribute("CHA", 100), Attribute("EDG", -200), Trait("Connections", 50),
                Trait("Equipped", 50), Trait("Rank", 250), Trait("Reputation", 50),
                Trait("Wealth", 100), Skill("Leadership", 10)],
            [FlexiblePoolChoice("flex", "Flexible XP", 115)],
            ["Officer"], [], protocolXp: 25)
    ];

    public static IReadOnlyList<LifePathModule> RealLifeModules { get; } =
    [
        new("real-agitator", "Agitator",
            "A dangerous adult life spent challenging authority and rallying followers.",
            900,
            [Attribute("WIL", 75), Trait("Bloodmark", -50), Trait("Gregarious", 80),
                Trait("Toughness", 80), Trait("Reputation", -150), Skill("Acting", 50),
                Skill("Disguise", 75), Skill("Leadership", 60), Skill("Negotiation", 80),
                Skill("Perception", 70), Skill("Small Arms", 75),
                Skill("Tactics/Infantry", 40), Skill("Training", 50)],
            [Choice("driving", "Driving specialty", EffectTarget.Skill, 65, 1,
                    ["Driving/Ground Vehicles", "Driving/Rail Vehicles",
                        "Driving/Sea Vehicles"]),
                Choice("prestidigitation", "Prestidigitation specialty",
                    EffectTarget.Skill, 100, 1,
                    ["Prestidigitation/Pick Pocket", "Prestidigitation/Quickdraw",
                        "Prestidigitation/Sleight of Hand"]),
                FlexibleChoice("flex", "Flexible XP", 125, 1, 50)],
            AffiliationStreetwiseXp: 75,
            TimeYears: 4),
        .. CreateCovertOperationsModules(),
        .. CreateClanWatchModules(),
        .. CreateClanWarriorWashoutModules(),
        .. CreateComStarServiceModules(),
        .. CreateGuerillaInsurgentModules(),
        .. CreateMerchantModules(),
        .. CreateOrganizedCrimeModules(),
        new("real-solaris-insider", "Solaris Insider",
            "A dealmaker and fixer within the dangerous Solaris VII stable scene.",
            825,
            [Attribute("WIL", 50), Attribute("CHA", 45), Attribute("EDG", 50),
                Trait("Compulsion/Gambling", -75), Trait("Connections", 150),
                Trait("Enemy", -200), Trait("Fit", 50), Trait("Property", 75),
                Trait("Reputation", 100), Trait("Wealth", 100),
                Skill("Administration", 30), Skill("Escape Artist", 15),
                Skill("Forgery", 15), Skill("Interests/Solaris Games", 20),
                Skill("Interests/Solaris Night Life", 25), Skill("Stealth", 20),
                Skill("Streetwise/Solaris VII", 25)],
            [Choice("computers", "Computers specialty", EffectTarget.Skill, 25, 1,
                    ["Computers/Programming", "Computers/Hacking"]),
                Choice("interest", "Interest", EffectTarget.Skill, 15, 1, Interests),
                Choice("prestidigitation", "Prestidigitation specialty",
                    EffectTarget.Skill, 15, 1, PrestidigitationSkills),
                Choice("security", "Security Systems specialty",
                    EffectTarget.Skill, 25, 1, SecuritySystemsSkills),
                new ModuleChoice("field-skills",
                    "Solaris Internship or fallback Field skills",
                    EffectTarget.Skill, 25, 6,
                    ResolveFieldDefinitionSkills(
                        "Communications", "Manager", "Politician"),
                    SolarisInternshipFieldSkillsOnly: true),
                FlexibleChoice("flex", "Flexible XP", 100, 1)],
            TimeYears: 4,
            RepeatEffects: [Trait("In For Life", -100)]),
        new("real-solaris-vii-games", "Solaris VII Games",
            "A gladiatorial career in the arenas and intrigues of Solaris VII.",
            900,
            [Attribute("EDG", 100), Trait("Bloodmark", -25),
                Trait("Enemy", -250), Trait("Reputation", 150),
                Skill("Acting", 25), Skill("Administration", 10),
                Skill("Computers", 10), Skill("Escape Artist", 15),
                Skill("Interests/Solaris Games", 30),
                Skill("Interests/Solaris Night Life", 35),
                Skill("Martial Arts", 20), Skill("Streetwise/Solaris VII", 25)],
            [Choice("attribute", "Other Attribute", EffectTarget.Attribute, 100, 1,
                    ["STR", "BOD", "RFL", "DEX", "INT", "WIL", "CHA"]),
                Choice("addiction", "Addiction", EffectTarget.Trait, -50, 1,
                    AddictionTraits),
                Choice("assets", "Arena assets", EffectTarget.Trait, 100, 3,
                    ["Custom Vehicle", "Design Quirk", "Equipped", "Extra Income",
                        "Property", "Tech Empathy", "Vehicle"]),
                Choice("interest", "Interest", EffectTarget.Skill, 10, 1, Interests),
                new ModuleChoice("field-skills", "Tech or Military Field skills",
                    EffectTarget.Skill, 45, 6, [],
                    EducationFieldNames:
                        TechFieldNames.Concat(MilitaryFieldNames)
                            .Where(field => field != "Officer")
                            .Distinct(StringComparer.Ordinal).ToArray(),
                    ClanWarriorFieldSkillsOnly: true),
                FlexibleChoice("flex", "Flexible XP", 125, 1)],
            TimeYears: 4,
            RepeatEffects: [Trait("In For Life", -150)]),
        new("real-dark-caste", "Dark Caste",
            "An outcast Clan life among fugitives, drifters, and the criminal underworld.",
            700,
            [Attribute("BOD", 25), Attribute("DEX", 25),
                Trait("Compulsion/Distrust of Inner Sphere", 75),
                Trait("Reputation", -100), Trait("Wealth", -25),
                Skill("Acting", 30), Skill("Disguise", 50),
                Skill("Escape Artist", 50), Skill("Martial Arts", 60),
                Skill("Negotiation", 25), Skill("Perception", 40),
                Skill("Protocol/Clan", -25), Skill("Running", 30),
                Skill("Stealth", 40)],
            [Choice("gunnery", "Gunnery specialty", EffectTarget.Skill, 75, 1,
                    GunnerySkills),
                Choice("navigation", "Navigation specialty", EffectTarget.Skill, 50, 1,
                    NavigationSkills),
                Choice("piloting", "Piloting specialty", EffectTarget.Skill, 20, 1,
                    PilotingSkills),
                Choice("prestidigitation", "Prestidigitation specialty",
                    EffectTarget.Skill, 25, 1, PrestidigitationSkills),
                Choice("survival", "Survival specialty", EffectTarget.Skill, 45, 1,
                    SurvivalSkills),
                Choice("technician-45", "Primary Technician specialty",
                    EffectTarget.Skill, 45, 1, Technicians),
                Choice("technician-25", "Secondary Technician specialty",
                    EffectTarget.Skill, 25, 1, Technicians),
                FlexibleChoice("flex", "Flexible XP", 115, 1)],
            TimeYears: 4),
        new("real-civilian-job", "Civilian Job",
            "An ordinary working life supporting the infrastructure of society.",
            600,
            [Skill("Administration", 75), Skill("Computers", 40),
                Skill("Leadership", 40), Skill("Negotiation", 30)],
            [Choice("career", "Non-military career", EffectTarget.Skill, 40, 1,
                    Careers.Where(career => career != "Career/Soldier").ToArray()),
                Choice("driving", "Driving specialty", EffectTarget.Skill, 60, 1,
                    DrivingSkills),
                Choice("interest-1", "Interest", EffectTarget.Skill, 50, 1,
                    Interests),
                Choice("interest-2", "Second interest", EffectTarget.Skill, 50, 1,
                    Interests),
                new ModuleChoice("career-field", "Career Field or flexible XP",
                    EffectTarget.Flexible, 20, 4,
                    Attributes.Concat(FlexibleTraits).Concat(FlexibleSkills).ToArray(),
                    EducationFieldNames: null,
                    SelectedEducationFieldSkillsOnly: true,
                    FixedFlexibleSelections: true),
                FlexibleChoice("flex", "Flexible XP", 85, 1)],
            AffiliationProtocolXp: 50,
            TimeYears: 6),
        new("real-combat-correspondent", "Combat Correspondent",
            "Front-line journalism combining battlefield survival with investigative reporting.",
            700,
            [Attribute("WIL", 50), Attribute("CHA", 75), Trait("Extra Income", 40),
                Trait("Reputation", 30), Skill("Art/Writing", 35),
                Skill("Career/Journalist", 50), Skill("Communications/Conventional", 30),
                Skill("Computers", 20), Skill("Investigation", 35),
                Skill("Negotiation", 40), Skill("Perception", 30),
                Skill("Technician/Electronic", 35)],
            [Choice("language", "Additional language", EffectTarget.Skill, 30, 1,
                    Languages),
                Choice("navigation", "Navigation specialty", EffectTarget.Skill, 25, 1,
                    NavigationSkills),
                Choice("survival", "Survival specialty", EffectTarget.Skill, 35, 1,
                    SurvivalSkills),
                FlexibleChoice("flex", "Flexible XP", 90, 1)],
            AffiliationLanguageXp: 50,
            TimeYears: 4),
        new("real-explorer", "Explorer",
            "A life spent surveying lost worlds, ruins, resources, and the unknown.",
            900,
            [Attribute("BOD", 20), Attribute("RFL", 30), Attribute("INT", 30),
                Attribute("WIL", 30), Trait("G-Tolerance", 50),
                Trait("Good Hearing", 35), Trait("Vehicle", 35),
                Trait("Introvert", -40), Trait("Wealth", -50), Skill("Appraisal", 35),
                Skill("Climbing", 25), Skill("Computers", 20),
                Skill("Investigation", 35), Skill("Martial Arts", 25),
                Skill("Melee Weapons", 30), Skill("Sensor Operations", 55),
                Skill("Zero-G Operations", 15)],
            [Choice("communications", "Communications specialty", EffectTarget.Skill, 35, 1,
                    CommunicationsSkills),
                Choice("language", "Additional language", EffectTarget.Skill, 40, 1,
                    Languages),
                Choice("medtech", "MedTech specialty", EffectTarget.Skill, 15, 1,
                    MedTechSkills),
                Choice("navigation", "Navigation specialty", EffectTarget.Skill, 50, 1,
                    NavigationSkills),
                Choice("piloting", "Piloting specialty", EffectTarget.Skill, 50, 1,
                    PilotingSkills),
                Choice("survival", "Survival specialty", EffectTarget.Skill, 75, 1,
                    SurvivalSkills),
                Choice("streetwise", "Streetwise specialty", EffectTarget.Skill, 35, 1,
                    StreetwiseOptions),
                Choice("tracking", "Tracking specialty", EffectTarget.Skill, 25, 1,
                    ["Tracking/Urban", "Tracking/Wilds"]),
                FlexibleChoice("flex", "Flexible XP", 170, 1)],
            AffiliationLanguageXp: 25,
            TimeYears: 6),
        new("real-neer-do-well", "Ne'er-Do-Well",
            "An aimless but eventful adult life of odd jobs, travel, and improvisation.",
            700,
            [Attribute("EDG", 75), Trait("Extra Income", 75), Trait("Reputation", -25),
                Trait("Wealth", -50), Skill("Acting", 25), Skill("Appraisal", 25),
                Skill("Art/Cooking", 35), Skill("Disguise", 15),
                Skill("Escape Artist", 35), Skill("Martial Arts", 20),
                Skill("Negotiation", 35), Skill("Prestidigitation/Pick Pocket", 25),
                Skill("Running", 35), Skill("Swimming", 10)],
            [Choice("attribute", "Other Attribute", EffectTarget.Attribute, 75, 1,
                    ["STR", "BOD", "RFL", "DEX", "INT", "WIL", "CHA"]),
                Choice("interest-1", "Interest", EffectTarget.Skill, 40, 1, Interests),
                Choice("interest-2", "Second interest", EffectTarget.Skill, 20, 1,
                    Interests),
                Choice("language", "Language", EffectTarget.Skill, 25, 1, Languages),
                Choice("survival", "Survival specialty", EffectTarget.Skill, 35, 1,
                    SurvivalSkills),
                new ModuleChoice("flex", "Flexible XP", EffectTarget.Flexible, 145, 1,
                    Attributes.Concat(FlexibleSkills).ToArray())],
            AffiliationStreetwiseXp: 25,
            TimeYears: 4,
            AwardFlexibleXpOnRepeat: false),
        new("real-goliath-scorpion-seeker", "Goliath Scorpion Seeker",
            "A Goliath Scorpion warrior's solitary search for lost Star League relics.",
            700,
            [Trait("Connections", 75), Trait("In For Life", -25),
                Skill("Appraisal", 50), Skill("Computers", 65),
                Skill("Interests/Archaeology", 55),
                Skill("Interests/Star League History", 60),
                Skill("Interests/Pre-Star League History", 35),
                Skill("Navigation/Space", 35), Skill("Perception", 50),
                Skill("Zero-G Operations", 25)],
            [Choice("language", "Language", EffectTarget.Skill, 35, 1, Languages),
                Choice("medtech", "MedTech specialty", EffectTarget.Skill, 40, 1,
                    MedTechSkills),
                Choice("survival", "Survival specialty", EffectTarget.Skill, 40, 1,
                    SurvivalSkills),
                FlexibleChoice("flex", "Flexible XP", 160, 1,
                    minimumAttributeOrTraitXp: 100)],
            TimeYears: 4),
        new("real-protomech-pilot", "ProtoMech Pilot Training",
            "Retraining for selected Clan aerospace washouts entering ProtoMech service.",
            600,
            [Attribute("RFL", 50), Trait("Fast Learner", 50),
                Trait("Toughness", 80), Trait("Vehicle", 75),
                Trait("Compulsion/Chemical Addiction", -80),
                Trait("Implant/EI Neural Implant", 150), Trait("Reputation", -75),
                Skill("Career/Soldier", 15), Skill("Escape Artist", 20),
                Skill("Interests/Neural Implants", 15), Skill("Martial Arts", 30),
                Skill("Melee Weapons", 15), Skill("Tactics/Infantry", 50),
                Skill("Tactics/Land", 75), Skill("Gunnery/ProtoMech", 25),
                Skill("Navigation/Ground", 25), Skill("Piloting/ProtoMech", 25),
                Skill("Sensor Operations", 25)],
            [FlexibleChoice("flex", "Flexible XP", 30, 1)],
            TimeYears: 2,
            Repeatable: false),
        new("real-scientist-caste-service", "Scientist Caste Service",
            "Clan scientist-caste service in genetics, research, and advanced technology.",
            1200,
            [Attribute("INT", 75), Attribute("WIL", 50), Attribute("BOD", -75),
                Skill("Acting", 50), Skill("Administration", 75),
                Skill("Career/Scientist", 100), Skill("Computers", 70),
                Skill("Cryptography", 50), Skill("Interests/Clan Genetics", 85),
                Skill("Investigation", 95), Skill("Leadership", 35),
                Skill("Perception", 85), Skill("Training", 85)],
            [ConditionalChoice("trait-pair", "Trait pair",
                    CreateScientistCasteTraitPairs()),
                Choice("interest", "Interest", EffectTarget.Skill, 75, 1, Interests),
                Choice("language", "Language", EffectTarget.Skill, 45, 1, Languages),
                Choice("medtech", "MedTech specialty", EffectTarget.Skill, 65, 1,
                    MedTechSkills),
                Choice("clan-protocol", "Clan protocol", EffectTarget.Skill, 35, 1,
                    ClanProtocols),
                Choice("science", "Science specialty", EffectTarget.Skill, 85, 1,
                    ["Science/Biology", "Science/Chemistry", "Science/Mathematics",
                        "Science/Physics"]),
                FlexibleChoice("flex", "Flexible XP", 50, 1)],
            AffiliationProtocolXp: 65,
            TimeYears: 4),
        new("real-think-tank", "Think Tank",
            "High-level analytical work for a government, corporation, or military organization.",
            900,
            [Attribute("STR", -75), Attribute("BOD", -75), Attribute("INT", 90),
                Attribute("WIL", 75), Trait("Connections", 100),
                Trait("Exceptional Attribute/INT", 75), Trait("Rank", 75),
                Trait("Wealth", 100), Trait("In For Life", -100),
                Skill("Administration", 50), Skill("Computers", 50),
                Skill("Training", 50)],
            [Choice("academic-interest", "Academic interest", EffectTarget.Skill, 120, 1,
                    AcademicInterests),
                Choice("interest", "Interest", EffectTarget.Skill, 85, 1, Interests),
                Choice("science", "Science specialty", EffectTarget.Skill, 30, 1,
                    ["Science/Biology", "Science/Chemistry", "Science/Mathematics",
                        "Science/Physics"]),
                Choice("technician", "Technician specialty", EffectTarget.Skill, 30, 1,
                    Technicians),
                new ModuleChoice("flex", "Non-combat flexible XP",
                    EffectTarget.Flexible, 190, 1, NonCombatFlexibleOptions)],
            AffiliationProtocolXp: 30,
            TimeYears: 4),
        new("real-travel", "Travel",
            "Years spent wandering among worlds, cultures, and unfamiliar environments.",
            700,
            [Attribute("INT", 45), Attribute("EDG", 45), Skill("Art/Cooking", 30),
                Skill("Climbing", 35), Skill("Swimming", 50),
                Skill("Zero-G Operations", 50)],
            [Choice("art", "Art specialty", EffectTarget.Skill, 35, 1, Arts),
                Choice("driving", "Driving specialty", EffectTarget.Skill, 50, 1,
                    ["Driving/Ground Vehicles", "Driving/Rail Vehicles",
                        "Driving/Sea Vehicles"]),
                Choice("interest-1", "Interest", EffectTarget.Skill, 75, 1, Interests),
                Choice("interest-2", "Second interest", EffectTarget.Skill, 45, 1,
                    Interests),
                Choice("interest-3", "Third interest", EffectTarget.Skill, 20, 1,
                    Interests),
                Choice("language", "Language", EffectTarget.Skill, 35, 1, Languages),
                Choice("survival", "Survival specialty", EffectTarget.Skill, 25, 1,
                    SurvivalSkills),
                FlexibleChoice("flex", "Flexible XP", 110, 1)],
            AffiliationLanguageXp: 50,
            TimeYears: 6),
        new("real-serve-protect", "To Serve and Protect",
            "Professional police or security service protecting a civilian population.",
            900,
            [Attribute("BOD", 100), Attribute("RFL", 100), Attribute("WIL", 100),
                Trait("Connections", 50), Trait("Enemy", -75),
                Skill("Administration", 25), Skill("Computers", 35),
                Skill("Cryptography", 15), Skill("Interrogation", 25),
                Skill("Investigation", 25), Skill("Leadership", 25),
                Skill("Melee Weapons", 45), Skill("Perception", 45),
                Skill("Small Arms", 50), Skill("Support Weapons", 15),
                Skill("Tactics/Infantry", 25), Skill("Training", 10)],
            [ConditionalChoice("trait-pair", "Trait pair",
                    new Dictionary<string, IReadOnlyList<ModuleEffect>>
                    {
                        ["Attractive and Handicap"] =
                            [Trait("Attractive", 50), Trait("Handicap", -50)],
                        ["Fit and Dependent"] =
                            [Trait("Fit", 50), Trait("Dependent", -50)]
                    }),
                Choice("medtech", "MedTech specialty", EffectTarget.Skill, 30, 1,
                    MedTechSkills),
                Choice("navigation", "Navigation specialty", EffectTarget.Skill, 35, 1,
                    NavigationSkills),
                new ModuleChoice("field-skills", "Police Field skills",
                    EffectTarget.Skill, 25, 4, [],
                    EducationFieldNames:
                    [
                        "Police Officer", "Police Tactical Officer", "Detective"
                    ]),
                FlexibleChoice("flex", "Flexible XP", 50, 1)],
            AffiliationProtocolXp: 25,
            AffiliationStreetwiseXp: 45,
            TimeYears: 4),
        new("real-postgraduate", "Postgraduate Studies",
            "Advanced academic fieldwork following a University education.",
            700,
            [Attribute("INT", 50), Attribute("WIL", -50), Trait("Connections", 75),
                Trait("Extra Income", 25), Trait("Wealth", -100),
                Skill("Appraisal", 50), Skill("Training", 75),
                Skill("Zero-G Operations", 25)],
            [Choice("academic-interest", "Academic interest", EffectTarget.Skill, 120, 1,
                    AcademicInterests),
                Choice("interest", "Interest", EffectTarget.Skill, 85, 1, Interests),
                Choice("language", "Language", EffectTarget.Skill, 50, 1, Languages),
                Choice("survival", "Survival specialty", EffectTarget.Skill, 35, 1,
                    SurvivalSkills),
                FlexibleChoice("flex", "Flexible XP", 175, 1) with
                {
                    EducationFieldNames = UniversityFieldNames,
                    MinimumEducationFieldSkillXp = 100,
                    MaximumEducationFieldSkillTargets = 4
                }],
            AffiliationLanguageXp: 85,
            TimeYears: 4,
            Repeatable: false),
        new("real-cloister-training", "Cloister Training",
            "Clan religious and martial study inspired by the Cloud Cobra Cloisters.",
            700,
            [Attribute("WIL", 75), Trait("Connections", 50),
                Trait("In For Life", -75), Skill("Interests/Clan Remembrance", 80),
                Skill("Interests/Theology", 100), Skill("Melee Weapons", 50),
                Skill("Perception", 35), Skill("Training", 85)],
            [Choice("interest", "Interest", EffectTarget.Skill, 75, 1, Interests),
                new ModuleChoice("warrior-skills", "Clan Warrior Field skills",
                    EffectTarget.Skill, 25, 3, [],
                    ClanWarriorFieldSkillsOnly: true),
                FlexibleChoice("flex", "Flexible XP", 150, 1)],
            TimeYears: 3,
            Repeatable: false),
        new("real-tour-duty-periphery", "Tour of Duty - Periphery",
            "Military service with a Periphery force.",
            700,
            [Trait("Connections", 25), Trait("Enemy", -50), Trait("Toughness", 50),
                Skill("Career/Soldier", 50), Skill("Martial Arts", 40),
                Skill("Leadership", 15), Skill("MedTech/General", 30),
                Skill("Negotiation", 25), Skill("Perception", 15)],
            [Choice("attribute", "Attribute", EffectTarget.Attribute, 50, 1, Attributes),
                Choice("equipment", "Equipment award", EffectTarget.Trait, 100, 1,
                    ["Equipped", "Vehicle"]),
                Choice("interest", "Interest", EffectTarget.Skill, 20, 1, Interests),
                Choice("navigation", "Navigation specialty", EffectTarget.Skill, 40, 1,
                    NavigationSkills),
                new ModuleChoice("field-skills", "Military Field skills",
                    EffectTarget.Skill, 25, 6, [],
                    EducationFieldNames: MilitaryFieldNames,
                    ClanWarriorFieldSkillsOnly: true),
                FlexibleChoice("flex", "Flexible XP", 100, 1)],
            AffiliationProtocolXp: 40,
            TimeYears: 3),
        new("real-tour-duty-inner-sphere", "Tour of Duty - Inner Sphere",
            "Military service with an Inner Sphere force.",
            800,
            [Trait("Connections", 25), Trait("Rank", 50),
                Skill("Career/Soldier", 50), Skill("Martial Arts", 50),
                Skill("Leadership", 15), Skill("MedTech/General", 20),
                Skill("Perception", 20)],
            [Choice("attributes", "Attributes", EffectTarget.Attribute, 50, 2, Attributes),
                Choice("equipment", "Equipment award", EffectTarget.Trait, 100, 1,
                    ["Equipped", "Vehicle"]),
                Choice("additional-equipment", "Additional equipment award",
                    EffectTarget.Trait, 50, 1, ["Equipped", "Vehicle"]),
                Choice("drawback", "Service drawback", EffectTarget.Trait, -50, 1,
                    ["Compulsion/Alcohol Addiction", "Compulsion/Drug Addiction",
                        "Compulsion/Smoking Addiction", "Unlucky"]),
                Choice("navigation", "Navigation specialty", EffectTarget.Skill, 40, 1,
                    NavigationSkills),
                new ModuleChoice("field-skills", "Military Field skills",
                    EffectTarget.Skill, 25, 7, [],
                    EducationFieldNames: MilitaryFieldNames,
                    ClanWarriorFieldSkillsOnly: true),
                FlexibleChoice("flex", "Flexible XP", 100, 1)],
            AffiliationLanguageXp: 15,
            AffiliationProtocolXp: 40,
            TimeYears: 3),
        new("real-tour-duty-clan", "Tour of Duty - Clan",
            "Military service with a Clan force.",
            1000,
            [Trait("Connections", 25), Trait("Bloodname", 50),
                Trait("Combat Sense", 75), Trait("Enemy", -75),
                Skill("Career/Soldier", 50), Skill("Computers", 15),
                Skill("Martial Arts", 40), Skill("Perception", 20)],
            [Choice("attributes", "Attributes", EffectTarget.Attribute, 75, 2, Attributes),
                Choice("equipment", "Equipment award", EffectTarget.Trait, 100, 1,
                    ["Equipped", "Vehicle"]),
                Choice("additional-equipment", "Additional equipment award",
                    EffectTarget.Trait, 75, 1, ["Equipped", "Vehicle"]),
                Choice("communications", "Communications specialty", EffectTarget.Skill,
                    15, 1, CommunicationsSkills),
                Choice("interest", "Interest", EffectTarget.Skill, 20, 1, Interests),
                Choice("navigation", "Navigation specialty", EffectTarget.Skill, 40, 1,
                    NavigationSkills),
                Choice("technician", "Technician specialty", EffectTarget.Skill, 10, 1,
                    Technicians),
                new ModuleChoice("field-skills", "Military Field skills",
                    EffectTarget.Skill, 25, 10, [],
                    EducationFieldNames: MilitaryFieldNames,
                    ClanWarriorFieldSkillsOnly: true),
                FlexibleChoice("flex", "Flexible XP", 100, 1)],
            AffiliationProtocolXp: 40,
            TimeYears: 3)
    ];

    public static IReadOnlyList<string> ResolveEducationFieldSkills(
        Character character,
        IReadOnlyList<string> allowedFieldNames)
    {
        var selectedNames = new[]
            {
                character.BasicSchool, character.AdvancedSchool, character.SpecialSchool
            }
            .Where(name => name.Length > 0 && allowedFieldNames.Contains(name))
            .ToHashSet(StringComparer.Ordinal);
        if (selectedNames.Count == 0) return [];

        var skills = new HashSet<string>(StringComparer.Ordinal);
        foreach (var field in EducationSchools
                     .SelectMany(school =>
                         (school.BasicFields ?? [])
                         .Concat(school.AdvancedFields ?? [])
                         .Concat(school.SpecialistFields ?? []))
                     .Where(field => selectedNames.Contains(field.Name)))
        {
            foreach (var effect in field.Effects.Where(effect =>
                         effect.Target == EffectTarget.Skill))
            {
                skills.Add(effect.Name);
            }
            foreach (var option in field.Choices.SelectMany(choice => choice.Options))
            {
                skills.Add(option);
            }
            var affiliation = Affiliations.FirstOrDefault(module =>
                module.Name == character.Affiliation);
            if (field.AffiliationProtocolXp != 0 && affiliation?.ProtocolSkill is not null)
            {
                skills.Add(affiliation.ProtocolSkill);
            }
            if (field.AffiliationStreetwiseXp != 0 &&
                affiliation?.StreetwiseSkill is not null)
            {
                skills.Add(affiliation.StreetwiseSkill);
            }
        }
        return skills.OrderBy(name => name).ToArray();
    }

    public static IReadOnlyList<string> ResolveClanWarriorFieldSkills(Character character) =>
        character.ClanTrainingField switch
        {
            "Aerospace" or "Aerospace Warrior" =>
                ["Gunnery/Aerospace", "Navigation/Space", "Piloting/Aerospace",
                    "Sensor Operations", "Tactics/Space"],
            "Cavalry" =>
                ["Artillery", "Driving/Ground Vehicles", "Driving/Rail Vehicles",
                    "Driving/Sea Vehicles", "Piloting/Air Vehicle",
                    "Gunnery/Air Vehicle", "Gunnery/Ground Vehicle",
                    "Gunnery/Sea Vehicle", "Sensor Operations", "Tactics/Land",
                    "Tactics/Air"],
            "Elemental" or "Elemental (Advanced)" =>
                ["Climbing", "Gunnery/Battlesuit", "Melee Weapons",
                    "Piloting/Battlesuit", "Sensor Operations", "Small Arms",
                    "Tactics/Infantry"],
            "Infantry" =>
                ["Martial Arts", "MedTech/General", "Melee Weapons",
                    "Navigation/Ground", "Small Arms"],
            "MechWarrior" =>
                ["Gunnery/'Mech", "Leadership", "Navigation/Ground",
                    "Piloting/'Mech", "Sensor Operations", "Tactics/Land"],
            "ProtoMech" or "ProtoMech (Advanced)" =>
                ["Gunnery/ProtoMech", "Navigation/Ground", "Piloting/ProtoMech",
                    "Sensor Operations", "Tactics/Land"],
            _ => []
        };

    public static IReadOnlyList<string> ResolveMilitaryFieldSkills(Character character)
    {
        var skills = new HashSet<string>(
            ResolveEducationFieldSkills(character, MilitaryFieldNames),
            StringComparer.Ordinal);
        skills.UnionWith(ResolveClanWarriorFieldSkills(character));
        return skills.OrderBy(name => name).ToArray();
    }

    public static IReadOnlyList<string> ResolveCovertOperationsFieldSkills(
        Character character) =>
        ResolveEducationFieldSkills(character,
            MilitaryFieldNames.Concat(IntelligencePoliceFieldNames)
                .Distinct(StringComparer.Ordinal).ToArray());

    public static IReadOnlyList<string> ResolveSelectedEducationFieldSkills(
        Character character)
    {
        var allFieldNames = EducationSchools
            .SelectMany(school =>
                (school.BasicFields ?? [])
                .Concat(school.AdvancedFields ?? [])
                .Concat(school.SpecialistFields ?? []))
            .Select(field => field.Name)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return ResolveEducationFieldSkills(character, allFieldNames);
    }

    public static IReadOnlyList<string> ResolveSolarisInternshipFieldSkills(
        Character character)
    {
        if (character.School != "Solaris Internship") return [];
        var solaris = EducationSchools.Single(school =>
            school.Name == "Solaris Internship");
        var fieldNames = (solaris.BasicFields ?? [])
            .Concat(solaris.AdvancedFields ?? [])
            .Concat(solaris.SpecialistFields ?? [])
            .Select(field => field.Name)
            .ToArray();
        return ResolveEducationFieldSkills(character, fieldNames);
    }

    public static IReadOnlyList<string> ResolveSolarisGamesFieldSkills(
        Character character)
    {
        var fields = TechFieldNames.Concat(MilitaryFieldNames)
            .Where(field => field != "Officer")
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var skills = new HashSet<string>(
            ResolveEducationFieldSkills(character, fields),
            StringComparer.Ordinal);
        skills.UnionWith(ResolveClanWarriorFieldSkills(character));
        return skills.OrderBy(name => name).ToArray();
    }

    private static IReadOnlyList<string> ResolveFieldDefinitionSkills(
        params string[] fieldNames)
    {
        var skills = new HashSet<string>(StringComparer.Ordinal);
        foreach (var field in EducationSchools
                     .SelectMany(school =>
                         (school.BasicFields ?? [])
                         .Concat(school.AdvancedFields ?? [])
                         .Concat(school.SpecialistFields ?? []))
                     .Where(field => fieldNames.Contains(
                         field.Name, StringComparer.Ordinal)))
        {
            foreach (var effect in field.Effects.Where(effect =>
                         effect.Target == EffectTarget.Skill))
            {
                skills.Add(effect.Name);
            }
            foreach (var option in field.Choices.SelectMany(choice => choice.Options))
            {
                skills.Add(option);
            }
        }
        return skills.OrderBy(name => name).ToArray();
    }

    private static IReadOnlyList<LifePathModule> CreateCovertOperationsModules()
    {
        var commonEffects = new[]
        {
            Trait("Alternate ID", 85), Trait("Enemy", -25),
            Trait("In For Life", -110), Trait("Sixth Sense", 50),
            Skill("Acting", 25), Skill("Perception", 50)
        };
        var commonChoices = new[]
        {
            Choice("attributes", "Attributes", EffectTarget.Attribute, 50, 2,
                ["BOD", "RFL", "WIL", "EDG"]),
            Choice("survival", "Survival specialty", EffectTarget.Skill, 75, 1,
                SurvivalSkills),
            new ModuleChoice("field-skills", "Military or Intelligence/Police Field skills",
                EffectTarget.Skill, 25, 6, [],
                Distinct: false,
                EducationFieldNames:
                    MilitaryFieldNames.Concat(IntelligencePoliceFieldNames)
                        .Distinct(StringComparer.Ordinal).ToArray())
        };
        var terranOrderEffects = new[]
        {
            Attribute("INT", 25), Trait("Combat Sense", 50),
            Trait("Fast Learner", 50), Trait("Impatient", -50),
            Trait("Reputation", -50), Skill("Administration", 50),
            Skill("Computers", 50), Skill("Cryptography", 60),
            Skill("Interrogation", 75), Skill("Small Arms", 50),
            Skill("Strategy", 50)
        };
        var terranOrderChoices = new[]
        {
            Choice("communications", "Communications specialty",
                EffectTarget.Skill, 50, 1, CommunicationsSkills),
            Choice("tactics", "Tactics specialty", EffectTarget.Skill, 40, 1,
                TacticsSkills),
            Choice("technician", "Technician specialty", EffectTarget.Skill,
                50, 1, Technicians)
        };

        return
        [
            Covert("capellan", "Capellan Confederation",
                [Attribute("DEX", 25), Trait("Citizenship", 75),
                    Trait("Dark Secret", -50), Trait("Fit", 25),
                    Trait("Reputation", -50), Skill("Climbing", 50),
                    Skill("Demolitions", 60), Skill("Escape Artist", 35),
                    Skill("Interrogation", 50), Skill("Investigation", 15),
                    Skill("Martial Arts", 75), Skill("Perception", 25),
                    Skill("Science/Chemistry", 40), Skill("Stealth", 50),
                    Skill("Thrown Weapons/Blade", 45)],
                [Choice("tactics", "Tactics specialty", EffectTarget.Skill, 30, 1,
                    TacticsSkills)]),
            Covert("comstar", "ComStar", terranOrderEffects, terranOrderChoices),
            Covert("word-of-blake", "Word of Blake",
                terranOrderEffects, terranOrderChoices),
            Covert("terran", "Terran", terranOrderEffects, terranOrderChoices),
            Covert("draconis", "Draconis Combine",
                [Attribute("INT", 50), Attribute("WIL", 50),
                    Attribute("CHA", -50), Attribute("EDG", -50),
                    Trait("Connections", 75), Trait("Enemy", -25),
                    Trait("Equipped", 50), Trait("Reputation", -75),
                    Skill("Acrobatics/Gymnastics", 35), Skill("Climbing", 50),
                    Skill("Computers", 25), Skill("Cryptography", 40),
                    Skill("Interrogation", 60), Skill("Investigation", 25),
                    Skill("Leadership", 40), Skill("Martial Arts", 55),
                    Skill("Melee Weapons", 30), Skill("Stealth", 30),
                    Skill("Training", 25)],
                [Choice("loyalty", "Loyalty", EffectTarget.Trait, -50, 1,
                        ["Compulsion/Loyalty to House Kurita",
                            "Compulsion/Loyalty to Draconis Combine"]),
                    Choice("tactics", "Tactics specialty", EffectTarget.Skill, 35, 1,
                        TacticsSkills)],
                protocolXp: 40, streetwiseXp: 35),
            Covert("fed-suns", "Federated Suns",
                [Trait("Combat Sense", 50), Trait("Connections", 40),
                    Trait("Enemy", -75), Trait("Rank", 50),
                    Skill("Acrobatics/Free-Fall", 20), Skill("Climbing", 35),
                    Skill("Computers", 45), Skill("Cryptography", 30),
                    Skill("Interrogation", 25), Skill("Investigation", 65),
                    Skill("Leadership", 20), Skill("Support Weapons", 30)],
                [Choice("addiction", "Addiction", EffectTarget.Trait, -25, 1,
                        AddictionTraits),
                    Choice("driving", "Driving specialty", EffectTarget.Skill, 25, 1,
                        DrivingSkills),
                    Choice("navigation", "Navigation specialty", EffectTarget.Skill,
                        35, 1, NavigationSkills),
                    Choice("security", "Security Systems specialty",
                        EffectTarget.Skill, 50, 1, SecuritySystemsSkills),
                    Choice("tactics", "Tactics specialty", EffectTarget.Skill, 30, 1,
                        TacticsSkills)],
                streetwiseXp: 50),
            Covert("free-worlds", "Free Worlds League",
                [Trait("Slow Learner", -50), Trait("Tech Empathy", 50),
                    Skill("Administration", 45), Skill("Computers", 30),
                    Skill("Disguise", 35), Skill("Forgery", 45),
                    Skill("Interrogation", 35), Skill("Investigation", 40),
                    Skill("Martial Arts", 25), Skill("Swimming", 50)],
                [ConditionalChoice("trait-pair", "Trait pair",
                        new Dictionary<string, IReadOnlyList<ModuleEffect>>
                        {
                            ["Prosthetic and Lost Limb"] =
                                [Trait("Implant/Prosthetic", 50),
                                    Trait("Lost Limb", -50)],
                            ["Attractive and Dark Secret"] =
                                [Trait("Attractive", 50), Trait("Dark Secret", -50)]
                        }),
                    Choice("driving", "Driving specialty", EffectTarget.Skill, 35, 1,
                        DrivingSkills),
                    Choice("medtech", "MedTech specialty", EffectTarget.Skill, 35, 1,
                        MedTechSkills),
                    Choice("security", "Security Systems specialty",
                        EffectTarget.Skill, 50, 1, SecuritySystemsSkills),
                    Choice("tracking", "Tracking specialty", EffectTarget.Skill, 65, 1,
                        TrackingSkills)],
                protocolXp: 60),
            Covert("rasalhague", "Free Rasalhague Republic",
                [Attribute("WIL", 50), Trait("Alternate ID", 35),
                    Trait("Compulsion/Rasalhague Pride", -75),
                    Trait("Equipped", 35), Trait("Impatient", -25),
                    Skill("Cryptography", 35), Skill("Demolitions", 50),
                    Skill("Disguise", 35), Skill("Interrogation", 25),
                    Skill("MedTech/General", 50), Skill("Melee Weapons", 35),
                    Skill("Perception", 25), Skill("Small Arms", 50),
                    Skill("Stealth", 50), Skill("Tactics/Infantry", 35)],
                [Choice("security", "Security Systems specialty",
                        EffectTarget.Skill, 40, 1, SecuritySystemsSkills),
                    Choice("technician", "Technician specialty", EffectTarget.Skill,
                        25, 1, Technicians),
                    Choice("tracking", "Tracking specialty", EffectTarget.Skill, 25, 1,
                        TrackingSkills)]),
            Covert("lyran", "Lyran Alliance",
                [Attribute("WIL", 30), Trait("Connections", 85),
                    Trait("Enemy", -45), Trait("Introvert", -85),
                    Trait("Property", 25), Trait("Rank", 85),
                    Trait("Reputation", -20), Skill("Acting", 30),
                    Skill("Computers", 50), Skill("Cryptography", 35),
                    Skill("Forgery", 20), Skill("Investigation", 50),
                    Skill("Martial Arts", 40), Skill("Negotiation", 50),
                    Skill("Strategy", 25), Skill("Support Weapons", 25),
                    Skill("Training", 40)],
                [Choice("driving", "Driving specialty", EffectTarget.Skill, 35, 1,
                        DrivingSkills),
                    Choice("tactics", "Tactics specialty", EffectTarget.Skill, 25, 1,
                        TacticsSkills)]),
            Covert("periphery", "Periphery",
                [Attribute("BOD", 25), Attribute("WIL", 50),
                    Trait("Alternate ID", 50), Trait("Compulsion/Gambling", -35),
                    Trait("Connections", 25), Trait("Introvert", -35),
                    Skill("Administration", 50), Skill("Disguise", 35),
                    Skill("Interrogation", 65), Skill("Melee Weapons", 25),
                    Skill("Negotiation", 25), Skill("Small Arms", 60),
                    Skill("Thrown Weapons", 35)],
                [Choice("driving", "Driving specialty", EffectTarget.Skill, 50, 1,
                        DrivingSkills),
                    Choice("security", "Security Systems specialty",
                        EffectTarget.Skill, 25, 1, SecuritySystemsSkills),
                    Choice("survival-extra", "Additional Survival specialty",
                        EffectTarget.Skill, 50, 1, SurvivalSkills)]),
            Covert("independent", "Independent",
                [Attribute("RFL", 75), Attribute("CHA", -50),
                    Trait("Bloodmark", -50), Trait("Connections", 75),
                    Trait("Extra Income", 25), Trait("Reputation", -75),
                    Trait("Wealth", -50), Skill("Acting", 50),
                    Skill("Climbing", 35), Skill("Cryptography", 40),
                    Skill("Escape Artist", 75), Skill("Martial Arts", 35),
                    Skill("Science/Chemistry", 25), Skill("Small Arms", 25)],
                [Choice("interest", "Interest", EffectTarget.Skill, 50, 1, Interests),
                    Choice("language", "Language", EffectTarget.Skill, 75, 1, Languages),
                    Choice("navigation", "Navigation specialty", EffectTarget.Skill,
                        25, 1, NavigationSkills),
                    Choice("protocol", "Protocol specialty", EffectTarget.Skill, 45, 1,
                        ProtocolSkills),
                    Choice("streetwise", "Streetwise specialty", EffectTarget.Skill,
                        35, 1, StreetwiseOptions),
                    Choice("tactics", "Tactics specialty", EffectTarget.Skill, 35, 1,
                        TacticsSkills)])
        ];

        LifePathModule Covert(
            string id,
            string affiliation,
            IReadOnlyList<ModuleEffect> variantEffects,
            IReadOnlyList<ModuleChoice> variantChoices,
            int protocolXp = 0,
            int streetwiseXp = 0) =>
            new($"real-covert-{id}", $"Covert Operations - {affiliation}",
                "Espionage and clandestine service for an Inner Sphere or Periphery power.",
                900,
                commonEffects.Concat(variantEffects).ToArray(),
                commonChoices.Concat(variantChoices).ToArray(),
                AffiliationProtocolXp: protocolXp,
                AffiliationStreetwiseXp: streetwiseXp,
                TimeYears: 6);
    }

    private static IReadOnlyList<LifePathModule> CreateClanWatchModules()
    {
        var commonEffects = new[]
        {
            Attribute("INT", 70), Attribute("RFL", 50), Attribute("CHA", -75),
            Trait("Connections", 100), Trait("Dark Secret", -50),
            Trait("In For Life", -100), Trait("Reputation", -50),
            Skill("Acting", 30), Skill("Computers", 75),
            Skill("Cryptography", 50), Skill("Demolitions", 40),
            Skill("Martial Arts", 75), Skill("Perception", 75),
            Skill("Small Arms", 80), Skill("Stealth", 50)
        };
        var commonChoices = new[]
        {
            Choice("tracking", "Tracking specialty", EffectTarget.Skill, 80, 1,
                TrackingSkills),
            FlexibleChoice("flex", "Flexible XP", 175, 1)
        };

        return
        [
            Watch("homeworld", "Homeworld Clan",
                [Skill("Career/Soldier", 60), Skill("Interrogation", 50),
                    Skill("Investigation", 50), Skill("Melee Weapons", 40)],
                [Choice("clan-protocol", "Clan protocol", EffectTarget.Skill, 75, 1,
                        ClanProtocols),
                    Choice("security", "Security Systems specialty",
                        EffectTarget.Skill, 35, 1, SecuritySystemsSkills),
                    Choice("streetwise", "Homeworld Clan Streetwise",
                        EffectTarget.Skill, 50, 1, HomeworldClanStreetwiseSkills),
                    Choice("survival", "Survival specialty", EffectTarget.Skill, 25, 1,
                        SurvivalSkills),
                    Choice("technician", "Technician specialty", EffectTarget.Skill,
                        40, 1, Technicians)]),
            Watch("invading", "Invading Clan",
                [Trait("Equipped", 50), Trait("Dark Secret", -50),
                    Skill("Computers", 50), Skill("Disguise", 35),
                    Skill("Interrogation", 75), Skill("Investigation", 75),
                    Skill("Negotiation", 40), Skill("Perception", 50)],
                [Choice("streetwise", "Invading Clan or Inner Sphere Streetwise",
                        EffectTarget.Skill, 50, 1,
                        InvadingClanOrInnerSphereStreetwiseSkills),
                    Choice("survival", "Survival specialty", EffectTarget.Skill, 50, 1,
                        SurvivalSkills)])
        ];

        LifePathModule Watch(
            string id,
            string affiliation,
            IReadOnlyList<ModuleEffect> variantEffects,
            IReadOnlyList<ModuleChoice> variantChoices) =>
            new($"real-clan-watch-{id}", $"Clan Watch Operative - {affiliation}",
                "Clan intelligence service operating through espionage and covert action.",
                1200,
                commonEffects.Concat(variantEffects).ToArray(),
                commonChoices.Concat(variantChoices).ToArray(),
                AffiliationProtocolXp: 50,
                AffiliationStreetwiseXp: 50,
                TimeYears: 3);
    }

    private static IReadOnlyList<LifePathModule> CreateClanWarriorWashoutModules()
    {
        var commonEffects = new[]
        {
            Attribute("CHA", -25), Attribute("WIL", -50),
            Trait("Reputation", -150), Skill("Career/Soldier", -30),
            Skill("Computers", 25)
        };
        var commonChoices = new[]
        {
            Choice("compulsion", "Compulsion", EffectTarget.Trait, -25, 1,
                ["Compulsion/Berserker", "Compulsion/Catatonia",
                    "Compulsion/Confusion", "Compulsion/Flashbacks",
                    "Compulsion/Hysteria", "Compulsion/Paranoia",
                    "Compulsion/Regression", "Compulsion/Split Personality"]),
            Choice("survival", "Survival specialty", EffectTarget.Skill, 75, 1,
                SurvivalSkills),
            new ModuleChoice("warrior-skills", "Reduced Clan Warrior Field skills",
                EffectTarget.Skill, -30, 2, [],
                ClanWarriorFieldSkillsOnly: true),
            FlexibleChoice("flex", "Flexible XP", 185, 1)
        };

        return
        [
            Washout("scientist", "Scientist Caste",
                [Attribute("INT", 50), Attribute("RFL", 25),
                    Trait("Dark Secret", -50), Skill("Administration", 75),
                    Skill("Investigation", 75), Skill("Surgery", 25)],
                [Choice("interest", "Interest", EffectTarget.Skill, 50, 1, Interests),
                    Choice("medtech", "MedTech specialty", EffectTarget.Skill, 50, 1,
                        MedTechSkills),
                    Choice("science", "Science specialty", EffectTarget.Skill, 75, 1,
                        ["Science/Biology", "Science/Chemistry",
                            "Science/Mathematics", "Science/Physics"])]),
            Washout("technician", "Technician Caste",
                [Attribute("RFL", 25), Attribute("STR", 50),
                    Trait("Impatient", -50), Skill("Perception", 75)],
                [Choice("communications", "Communications specialty",
                        EffectTarget.Skill, 75, 1, CommunicationsSkills),
                    Choice("interest", "Interest", EffectTarget.Skill, 50, 1, Interests),
                    Choice("technician-75", "Primary Technician specialty",
                        EffectTarget.Skill, 75, 1, Technicians),
                    Choice("technician-50", "Secondary Technician specialty",
                        EffectTarget.Skill, 50, 1, Technicians),
                    Choice("technician-25", "Additional Technician specialty",
                        EffectTarget.Skill, 25, 1, Technicians)]),
            Washout("merchant", "Merchant Caste",
                [Attribute("WIL", 70), Skill("Acting", 45),
                    Skill("Administration", 40), Skill("Appraisal", 70),
                    Skill("Leadership", 40), Skill("Negotiation", 50)],
                [Choice("interest", "Interest", EffectTarget.Skill, 35, 1, Interests)],
                streetwiseXp: 25),
            Washout("laborer", "Laborer Caste",
                [Attribute("BOD", 75), Trait("Dependent", -50),
                    Skill("Computers", 50)],
                [Choice("career", "Appropriate career", EffectTarget.Skill, 75, 1,
                        Careers.Where(career => career != "Career/Soldier").ToArray()),
                    Choice("driving", "Driving specialty", EffectTarget.Skill, 75, 1,
                        DrivingSkills),
                    Choice("interest-75", "Primary interest", EffectTarget.Skill,
                        75, 1, Interests),
                    Choice("interest-50", "Secondary interest", EffectTarget.Skill,
                        50, 1, Interests)],
                streetwiseXp: 25)
        ];

        LifePathModule Washout(
            string id,
            string caste,
            IReadOnlyList<ModuleEffect> casteEffects,
            IReadOnlyList<ModuleChoice> casteChoices,
            int streetwiseXp = 0) =>
            new($"real-clan-warrior-washout-{id}",
                $"Clan Warrior Washout - {caste}",
                $"A failed Clan warrior reassigned to the {caste}.",
                400,
                commonEffects.Concat(casteEffects).ToArray(),
                commonChoices.Concat(casteChoices).ToArray(),
                AffiliationProtocolXp: 80,
                AffiliationStreetwiseXp: streetwiseXp,
                TimeYears: 2,
                Repeatable: false);
    }

    private static IReadOnlyList<LifePathModule> CreateComStarServiceModules()
    {
        var commonEffects = new[]
        {
            Attribute("DEX", 50), Attribute("INT", 50),
            Trait("Combat Sense", 75), Trait("In For Life", -100),
            Trait("Tech Empathy", 35), Skill("Administration", 40),
            Skill("Communications/HPG", 55), Skill("Computers", 35),
            Skill("Martial Arts", 45)
        };
        var commonChoices = new[]
        {
            Choice("asset", "Service asset", EffectTarget.Trait, 100, 1,
                ["Equipped", "Vehicle", "Wealth"]),
            Choice("communications", "Communications specialty",
                EffectTarget.Skill, 35, 1, CommunicationsSkills),
            Choice("language", "Language", EffectTarget.Skill, 25, 1, Languages),
            Choice("protocol", "Protocol specialty", EffectTarget.Skill, 20, 1,
                ProtocolSkills),
            new ModuleChoice("field-skills", "Selected Field skills",
                EffectTarget.Skill, 40, 4, FlexibleSkills,
                SelectedEducationFieldSkillsOnly: true),
            FlexibleChoice("flex", "Flexible XP", 50, 1)
        };

        return
        [
            Service("comstar", "ComStar",
                [Attribute("WIL", 25), Attribute("EDG", -25),
                    Trait("Equipped", 80), Trait("Rank", 80),
                    Trait("Compulsion/Hatred of Word of Blake", -50),
                    Skill("Communications/HPG", 15), Skill("Leadership", 20),
                    Skill("Negotiation", 15), Skill("Training", 15)],
                [Choice("technician", "Technician specialty", EffectTarget.Skill,
                    15, 1, Technicians)]),
            Service("word-of-blake", "Word of Blake",
                [Attribute("INT", 25), Attribute("CHA", -25),
                    Trait("Equipped", 70), Trait("Rank", 70),
                    Trait("Compulsion/Hatred of ComStar", -75),
                    Trait("Compulsion/Hatred of Clans", -100),
                    Skill("Computers", 35), Skill("Cryptography", 25),
                    Skill("Interests/Writings of Jerome Blake", 50),
                    Skill("Interrogation", 40), Skill("Perception", 25)],
                [Choice("communications-extra",
                    "Additional Communications specialty",
                    EffectTarget.Skill, 50, 1, CommunicationsSkills)])
        ];

        LifePathModule Service(
            string id,
            string affiliation,
            IReadOnlyList<ModuleEffect> variantEffects,
            IReadOnlyList<ModuleChoice> variantChoices) =>
            new($"real-{id}-service", $"{affiliation} Service",
                $"Professional service in the {affiliation} order.",
                900,
                commonEffects.Concat(variantEffects).ToArray(),
                commonChoices.Concat(variantChoices).ToArray(),
                AffiliationProtocolXp: 35,
                TimeYears: 5);
    }

    private static IReadOnlyList<LifePathModule> CreateGuerillaInsurgentModules()
    {
        var commonEffects = new[]
        {
            Attribute("STR", 100), Attribute("WIL", 100),
            Trait("Bloodmark", -50), Trait("Combat Sense", 30),
            Trait("Connections", 50), Trait("Equipped", 30),
            Trait("Compulsion/Hatred for Authority", -100),
            Trait("Dependent", -25), Trait("Unlucky", -35),
            Skill("Computers", 45), Skill("Demolitions", 65),
            Skill("Disguise", 40), Skill("Escape Artist", 25),
            Skill("Melee Weapons", 20), Skill("Perception", 25),
            Skill("Small Arms", 35), Skill("Support Weapons", 35)
        };
        var commonChoices = new[]
        {
            Choice("prestidigitation", "Prestidigitation specialty",
                EffectTarget.Skill, 50, 1, PrestidigitationSkills),
            Choice("security", "Security Systems specialty",
                EffectTarget.Skill, 25, 1, SecuritySystemsSkills),
            Choice("survival", "Survival specialty", EffectTarget.Skill, 35, 1,
                SurvivalSkills),
            FlexibleChoice("flex", "Flexible XP", 180, 1)
        };

        return
        [
            Insurgent("rasalhague", "Free Rasalhague",
                [Trait("Bloodmark", -35), Trait("Combat Sense", 40),
                    Trait("Lost Limb", -50), Trait("Implant/Prosthetic", 50),
                    Skill("Demolitions", 40), Skill("Forgery", 40),
                    Skill("Leadership", 35), Skill("Protocol/Rasalhague", 35),
                    Skill("Streetwise/Rasalhague", 40)],
                [Choice("tactics", "Tactics specialty", EffectTarget.Skill, 25, 1,
                    TacticsSkills)]),
            Insurgent("general", "General",
                [Attribute("EDG", -25), Trait("Dark Secret", -50),
                    Trait("Enemy", -50), Trait("Reputation", 25),
                    Trait("Toughness", 25), Skill("Climbing", 40),
                    Skill("Interrogation", 50), Skill("Small Arms", 25),
                    Skill("Swimming", 45), Skill("Tactics/Infantry", 50)],
                [Choice("driving", "Driving specialty", EffectTarget.Skill, 50, 1,
                    DrivingSkills)],
                languageXp: 35)
        ];

        LifePathModule Insurgent(
            string id,
            string variant,
            IReadOnlyList<ModuleEffect> variantEffects,
            IReadOnlyList<ModuleChoice> variantChoices,
            int languageXp = 0) =>
            new($"real-guerilla-insurgent-{id}",
                $"Guerilla Insurgent - {variant}",
                "An outlaw fighter resisting occupation, tyranny, or foreign rule.",
                900,
                commonEffects.Concat(variantEffects).ToArray(),
                commonChoices.Concat(variantChoices).ToArray(),
                AffiliationLanguageXp: languageXp,
                TimeYears: 6);
    }

    private static IReadOnlyList<LifePathModule> CreateMerchantModules()
    {
        var merchantSkills =
            ResolveFieldDefinitionSkills("Merchant", "Merchant Marine");
        var merchantMasterSkills =
            ResolveFieldDefinitionSkills("Manager", "Merchant", "Merchant Marine");
        var commonEffects = new[]
        {
            Attribute("CHA", 50), Trait("Enemy", -75),
            Trait("Reputation", 50), Trait("Wealth", 50),
            Skill("Acting", 20), Skill("Appraisal", 20),
            Skill("Computers", 15), Skill("Negotiation", 20),
            Skill("Perception", 30), Skill("Zero-G Operations", 10)
        };
        var commonChoices = new[]
        {
            Choice("interest", "Interest", EffectTarget.Skill, 35, 1, Interests),
            Choice("language", "Language", EffectTarget.Skill, 25, 1, Languages),
            Choice("protocol-35", "Primary protocol", EffectTarget.Skill, 35, 1,
                ProtocolSkills),
            Choice("protocol-15", "Secondary protocol", EffectTarget.Skill, 15, 1,
                ProtocolSkills),
            FlexibleChoice("flex", "Flexible XP", 200, 1)
        };

        return
        [
            Merchant("free-trader", "Free Trader",
                [Attribute("WIL", 50), Trait("Connections", 50),
                    Trait("Extra Income", 50), Trait("Gregarious", 25),
                    Skill("Administration", 25), Skill("Appraisal", 15),
                    Skill("Leadership", 15), Skill("Martial Arts", 15),
                    Skill("Melee Weapons", 15), Skill("Small Arms", 20)],
                [new ModuleChoice("field-skills",
                    "Merchant or Merchant Marine Field skills",
                    EffectTarget.Skill, 20, 5, merchantSkills)]),
            Merchant("merchant-master", "Merchant Master",
                [Trait("Connections", 50), Trait("Enemy", -125),
                    Trait("Extra Income", 50), Trait("Reputation", 75),
                    Skill("Administration", 15), Skill("Career/Merchant", 20),
                    Skill("Communications/Conventional", 10),
                    Skill("Interests/Antiques", 10), Skill("Negotiation", 15)],
                [Choice("master-interest", "Additional interest",
                        EffectTarget.Skill, 25, 1, Interests),
                    Choice("master-language", "Additional language",
                        EffectTarget.Skill, 25, 1, Languages),
                    new ModuleChoice("field-skills",
                        "Manager, Merchant, or Merchant Marine Field skills",
                        EffectTarget.Skill, 35, 6, merchantMasterSkills)]),
            Merchant("deep-periphery", "Deep Periphery Trader",
                [Attribute("BOD", -50), Attribute("WIL", 75),
                    Trait("Connections", 20), Trait("Enemy", -100),
                    Trait("Exceptional Attribute/EDG", 75),
                    Trait("G-Tolerance", 75), Skill("Administration", 20),
                    Skill("Leadership", 20), Skill("Martial Arts", 25),
                    Skill("Melee Weapons", 30), Skill("Small Arms", 25),
                    Skill("Zero-G Operations", 15)],
                [Choice("deep-language", "Additional language",
                        EffectTarget.Skill, 25, 1, Languages),
                    new ModuleChoice("field-skills",
                        "Merchant or Merchant Marine Field skills",
                        EffectTarget.Skill, 25, 5, merchantSkills)]),
            Merchant("diamond-shark", "Diamond Shark Warrior-Merchant",
                [Attribute("WIL", 75), Attribute("CHA", 25),
                    Trait("Enemy", -150), Trait("G-Tolerance", 50),
                    Trait("Reputation", -35), Trait("Wealth", 25),
                    Skill("Administration", 25), Skill("Career/Merchant", 20),
                    Skill("Communications/Conventional", 25),
                    Skill("Computers", 25), Skill("Negotiation", 20)],
                [Choice("shark-interest", "Additional interest",
                        EffectTarget.Skill, 25, 1, Interests),
                    Choice("shark-language", "Additional language",
                        EffectTarget.Skill, 25, 1, Languages),
                    Choice("navigation", "Navigation specialty",
                        EffectTarget.Skill, 15, 1, NavigationSkills),
                    new ModuleChoice("field-skills",
                        "Commercial or Clan Warrior Field skills",
                        EffectTarget.Skill, 35, 6, merchantMasterSkills,
                        ClanWarriorFieldSkillsOnly: true)])
        ];

        LifePathModule Merchant(
            string id,
            string variant,
            IReadOnlyList<ModuleEffect> variantEffects,
            IReadOnlyList<ModuleChoice> variantChoices) =>
            new($"real-merchant-{id}", $"Merchant - {variant}",
                $"A commercial career as a {variant}.",
                900,
                commonEffects.Concat(variantEffects).ToArray(),
                commonChoices.Concat(variantChoices).ToArray(),
                AffiliationLanguageXp: 20,
                TimeYears: 4);
    }

    private static IReadOnlyList<LifePathModule> CreateOrganizedCrimeModules()
    {
        var skillEffects = new[]
        {
            Skill("Acting", 60), Skill("Computers", 15),
            Skill("Demolitions", 50), Skill("Escape Artist", 35),
            Skill("Forgery", 35), Skill("Interrogation", 85),
            Skill("Leadership", 25), Skill("Martial Arts", 30),
            Skill("Melee Weapons", 45), Skill("Negotiation", 35),
            Skill("Perception", 35), Skill("Small Arms", 75),
            Skill("Stealth", 35)
        };
        var skillChoices = new[]
        {
            ConditionalChoice("syndicate", "Syndicate identity",
                CreateSyndicateIdentities()),
            Choice("driving", "Driving specialty", EffectTarget.Skill, 30, 1,
                DrivingSkills),
            Choice("sport", "Sport interest", EffectTarget.Skill, 55, 1,
                ["Interests/Football", "Interests/Ice Hockey", "Interests/Soccer",
                    "Interests/Tennis", "Interests/Volleyball",
                    "Interests/Basketball", "Interests/Baseball"]),
            Choice("prestidigitation", "Prestidigitation specialty",
                EffectTarget.Skill, 35, 1, PrestidigitationSkills),
            Choice("security", "Security Systems specialty",
                EffectTarget.Skill, 45, 1, SecuritySystemsSkills),
            FlexibleChoice("flex", "Flexible XP", 100, 1)
        };

        return
        [
            new("real-organized-crime", "Organized Crime",
                "A life inside an organized criminal syndicate.",
                1000,
                [Attribute("EDG", 85), Trait("Alternate ID", 100),
                    Trait("In For Life", -150), .. skillEffects],
                [Choice("loyalty", "Syndicate burden", EffectTarget.Trait, -85, 1,
                        ["Dark Secret", "Compulsion/Loyalty to Crime Boss"]),
                    .. skillChoices],
                AffiliationProtocolXp: 25,
                AffiliationStreetwiseXp: 50,
                TimeYears: 5),
            new("real-organized-crime-clan", "Organized Crime - Clan Dark Caste",
                "A Clan Dark Caster serving an organized criminal syndicate.",
                1000,
                skillEffects,
                skillChoices,
                AffiliationProtocolXp: 25,
                AffiliationStreetwiseXp: 50,
                TimeYears: 5)
        ];
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<ModuleEffect>>
        CreateSyndicateIdentities() =>
        new Dictionary<string, IReadOnlyList<ModuleEffect>>
        {
            ["Mafia"] =
                [Skill("Career/Mafia", 100), Skill("Language/Mafia", 50)],
            ["Yakuza"] =
                [Skill("Career/Yakuza", 100), Skill("Language/Yakuza", 50)],
            ["Tong or Triad"] =
                [Skill("Career/Tong or Triad", 100),
                    Skill("Language/Tong or Triad", 50)],
            ["Dark Caste"] =
                [Skill("Career/Dark Caste", 100),
                    Skill("Language/Dark Caste", 50)],
            ["Other Syndicate"] =
                [Skill("Career/Syndicate", 100),
                    Skill("Language/Syndicate", 50)]
        };

    private static IReadOnlyList<LifePathModule> CreateClanCastes() =>
    [
        Child("caste-mechwarrior", "MechWarrior", [Attribute("DEX", 75), Attribute("RFL", 75),
            Attribute("WIL", 75), Attribute("CHA", -25), Attribute("EDG", -50),
            Trait("Fit", 25), Trait("Impatient", -50)]),
        Child("caste-elemental", "Elemental", [Attribute("BOD", 125), Attribute("STR", 125),
            Attribute("DEX", -75), Attribute("CHA", -75), Skill("Martial Arts", 25)]),
        Child("caste-elemental-advanced", "Elemental-Advanced", [Attribute("BOD", 200),
            Attribute("STR", 175), Attribute("DEX", -100), Attribute("RFL", -75),
            Attribute("CHA", -100), Attribute("EDG", -100), Trait("Patient", 25),
            Trait("Reputation", 100)]),
        Child("caste-aerospace", "Aerospace", [Attribute("BOD", -50), Attribute("STR", -50),
            Attribute("DEX", 150), Attribute("RFL", 150), Attribute("CHA", -25),
            Attribute("EDG", -25), Trait("Fit", 25), Trait("Impatient", -50)]),
        Child("caste-protomech", "ProtoMech", [Attribute("BOD", -50), Attribute("STR", -50),
            Attribute("DEX", 150), Attribute("RFL", 150), Attribute("CHA", -25),
            Attribute("EDG", -25), Trait("Fit", 25), Trait("Impatient", -50)]),
        Child("caste-aerospace-naval", "Aerospace-Naval", [Attribute("BOD", -50),
            Attribute("STR", -50), Attribute("DEX", 125), Attribute("RFL", 125),
            Attribute("INT", 50), Attribute("CHA", -25), Attribute("EDG", -100),
            Trait("Compulsion/Arrogance", -100), Trait("Patient", 75), Trait("Reputation", 75)]),
        Child("caste-warrior-other", "Warrior Caste (Other)", [Attribute("BOD", 75),
            Attribute("STR", 50), Attribute("DEX", 50), Attribute("RFL", 50),
            Attribute("CHA", -25), Trait("Reputation", -75)]),
        new("caste-scientist", "Scientist Caste", "", 0,
            [Attribute("STR", -50), Attribute("INT", 100), Trait("Compulsion/Arrogance", -25),
                Trait("Patient", 100), Trait("Reputation", -25)],
            [Choice("interest", "Interest", EffectTarget.Skill, 10, 1, Interests),
                Choice("science", "Science", EffectTarget.Skill, 15, 1,
                    ["Science/Biology", "Science/Chemistry", "Science/Mathematics", "Science/Physics"])]),
        new("caste-technician", "Technician Caste", "", 0,
            [Attribute("DEX", 100), Attribute("INT", 20), Attribute("CHA", -50),
                Trait("Patient", 100), Trait("Reputation", -75)],
            [Choice("interest", "Interest", EffectTarget.Skill, 15, 1, Interests),
                Choice("technician", "Technician", EffectTarget.Skill, 15, 1, Technicians)]),
        new("caste-merchant", "Merchant Caste", "", 0,
            [Attribute("BOD", -50), Attribute("INT", 25), Attribute("CHA", 75),
                Trait("Gregarious", 100), Trait("Reputation", -75), Skill("Appraisal", 10),
                Skill("Negotiation", 15), Skill("Streetwise/Clan", 15)],
            [Choice("protocol", "Protocol", EffectTarget.Skill, 10, 1,
                ["Protocol/Capellan", "Protocol/Combine", "Protocol/FedSuns", "Protocol/Lyran",
                    "Protocol/Clan", "Protocol/Free Worlds", "Protocol/Rasalhague"])]),
        new("caste-laborer", "Laborer Caste", "", 0,
            [Attribute("BOD", 100), Attribute("STR", 125), Attribute("DEX", 50),
                Attribute("RFL", 50), Attribute("INT", -50), Attribute("CHA", -50),
                Trait("Reputation", -125)],
            [Choice("interest", "Interest", EffectTarget.Skill, 10, 1, Interests),
                Choice("career", "Career", EffectTarget.Skill, 15, 1, Careers)])
    ];

    private static string[] StreetwiseOptions =>
        ["Streetwise/Capellan", "Streetwise/Combine", "Streetwise/FedSuns",
            "Streetwise/Lyran", "Streetwise/Clan", "Streetwise/Free Worlds",
            "Streetwise/Rasalhague", "Streetwise/Periphery", "Streetwise/Terran",
            "Streetwise/Independent"];

    private static LifePathModule Affiliation(
        string id, string name, int cost, IReadOnlyList<ModuleEffect> effects,
        IReadOnlyList<ModuleChoice> choices, IReadOnlyList<string> languages,
        string skillSuffix, IReadOnlyList<string> subAffiliations,
        IReadOnlyList<LifePathModule>? castes = null) =>
        new(id, name, $"Affiliation with {name}.", cost, effects, choices, languages,
            $"Protocol/{skillSuffix}", $"Streetwise/{skillSuffix}",
            CreateSubAffiliations(id, subAffiliations),
            castes);

    private static IReadOnlyList<LifePathModule> CreateSubAffiliations(
        string affiliationId, IReadOnlyList<string> names) =>
        affiliationId switch
        {
            "fed-suns" =>
            [
                Sub("Capellan March",
                    [Attribute("WIL", 40), Trait("Connections", 25),
                        Trait("Compulsion/Hatred of Capellan Confederation", -50),
                        Skill("Protocol/FedSuns", 10), Skill("Interests/FedSuns History", 10)],
                    [Choice("language", "Border language", EffectTarget.Skill, 5, 1,
                        ["Language/Cantonese", "Language/German", "Language/Mandarin Chinese",
                            "Language/Spanish", "Language/Russian"])]),
                Sub("Crucis March",
                    [Attribute("WIL", 50), Attribute("EDG", -50),
                        Skill("Interests/FedSuns History", 15), Skill("Protocol/FedSuns", 15)],
                    [Choice("art", "Art", EffectTarget.Skill, 10, 1, Arts)]),
                Sub("Draconis March",
                    [Attribute("EDG", 25), Trait("Connections", 20),
                        Trait("Compulsion/Hatred of Draconis Combine", -30),
                        Skill("Interests/FedSuns History", 10), Skill("Protocol/FedSuns", 5)],
                    [Choice("art", "Art", EffectTarget.Skill, 10, 1, Arts)]),
                Sub("Outback",
                    [Attribute("STR", 50), Attribute("BOD", 150), Attribute("WIL", 100),
                        Attribute("INT", -100), Trait("Illiterate", -50), Trait("Reputation", -50),
                        Trait("Wealth", -100), Skill("Streetwise/FedSuns", 10)],
                    [Choice("culture", "Art or interest", EffectTarget.Skill, 10, 1,
                        Arts.Concat(Interests).ToArray()),
                        Choice("survival", "Survival", EffectTarget.Skill, 20, 1, SurvivalSkills)])
            ],
            "capellan" =>
            [
                Sub("Capellan Commonality",
                    [Attribute("EDG", 50), Trait("Wealth", 15), Skill("Protocol/FedSuns", 5)],
                    [Choice("language", "FedSuns language", EffectTarget.Skill, 5, 1, FedSunsLanguages)]),
                Sub("Liao Commonality",
                    [Attribute("INT", 50), Trait("Reputation", -25), Skill("Martial Arts", 15)],
                    [Choice("language", "FedSuns or Lyran language", EffectTarget.Skill, 15, 1,
                        FedSunsLanguages.Concat(LyranLanguages).Distinct().ToArray()),
                        Choice("protocol", "FedSuns or Lyran protocol", EffectTarget.Skill, 10, 1,
                            ["Protocol/FedSuns", "Protocol/Lyran"]),
                        Choice("art", "Art", EffectTarget.Skill, 10, 1, Arts)]),
                Sub("Sian Commonality",
                    [Attribute("WIL", 75), Trait("Compulsion/Hatred of Federated Suns", -135),
                        Trait("Citizenship", 50), Trait("Connections", 50),
                        Skill("Interests/Capellan History", 10), Skill("Protocol/Capellan", 15)],
                    [Choice("language", "Capellan secondary language", EffectTarget.Skill, 10, 1,
                        ["Language/Russian", "Language/Cantonese", "Language/Vietnamese",
                            "Language/English"])]),
                Sub("St. Ives Commonality",
                    [Attribute("WIL", 50), Attribute("EDG", 50), Trait("Reputation", -100),
                        Trait("Wealth", 50),
                        Skill("Protocol/Capellan", -15), Skill("Protocol/FedSuns", 10),
                        Skill("Martial Arts", 10)],
                    [Choice("language", "FedSuns language", EffectTarget.Skill, 15, 1, FedSunsLanguages),
                        Choice("art", "Art", EffectTarget.Skill, 5, 1, Arts)]),
                Sub("Victoria Commonality",
                    [Attribute("WIL", 35), Trait("Connections", 50), Trait("Wealth", -50),
                        Skill("Negotiation", 10), Skill("Martial Arts", 15)],
                    [Choice("language", "Language", EffectTarget.Skill, 15, 1, Languages)])
            ],
            "draconis" =>
            [
                Sub("Azami",
                    [Attribute("WIL", 190), Trait("Compulsion/Xenophobia", -50),
                        Trait("Equipped", -50), Trait("Wealth", -25),
                        Skill("Language/Arabic", 10), Skill("Language/Japanese", -10),
                        Skill("Martial Arts", 10), Skill("Melee Weapons", 10),
                        Skill("Animal Handling/Riding", 5)],
                    [Choice("survival", "Survival", EffectTarget.Skill, 10, 1, SurvivalSkills),
                        Choice("weapon", "Traditional weapon", EffectTarget.Skill, 10, 1,
                            ["Archery", "Melee Weapons", "Thrown Weapons"])]),
                Sub("Benjamin District",
                    [Trait("Compulsion/Paranoid of Combine Government", -50),
                        Trait("Connections", 50), Trait("Patient", 25), Trait("Wealth", 35),
                        Skill("Art/Oral Tradition", 5), Skill("Martial Arts", 10),
                        Skill("Protocol/Combine", 15), Skill("Streetwise/Combine", 10)],
                    [Choice("weapon", "Traditional weapon", EffectTarget.Skill, 10, 1,
                        ["Archery", "Melee Weapons", "Thrown Weapons"])]),
                Sub("Dieron District",
                    [Attribute("INT", 50), Attribute("WIL", -50),
                        Trait("Compulsion/Xenophobia", 50), Trait("Connections", 60),
                        Trait("Enemy", -100), Trait("Wealth", 50),
                        Skill("Interests/Star League History", 5), Skill("Negotiation", 5)],
                    [Choice("art", "Art", EffectTarget.Skill, 15, 1, Arts),
                        Choice("language", "Language", EffectTarget.Skill, 15, 1, Languages),
                        Choice("weapon", "Traditional weapon", EffectTarget.Skill, 10, 1,
                            ["Archery", "Melee Weapons", "Thrown Weapons"])]),
                Sub("New Samarkand (Galedon) District",
                    [Attribute("WIL", 100), Attribute("CHA", -50),
                        Trait("Compulsion/Hatred of Federated Suns", -50), Trait("Connections", 50),
                        Skill("Interests/Combine History", 10), Skill("Melee Weapons", 15),
                        Skill("Negotiation", 5), Skill("Protocol/Combine", 10),
                        Skill("Streetwise/Combine", 10)],
                    [Choice("weapon", "Traditional weapon", EffectTarget.Skill, 10, 1,
                        ["Archery", "Melee Weapons", "Thrown Weapons"])]),
                Sub("Pesht District",
                    [Attribute("WIL", 100), Attribute("EDG", -25),
                        Trait("Compulsion/Hatred of Clans", -100), Trait("Connections", 20),
                        Trait("Wealth", 50), Skill("Martial Arts", 10), Skill("Melee Weapons", 15),
                        Skill("Protocol/Combine", 20), Skill("Streetwise/Combine", 10)],
                    [Choice("weapon", "Traditional weapon", EffectTarget.Skill, 10, 1,
                        ["Archery", "Melee Weapons", "Thrown Weapons"])])
            ],
            "free-worlds" =>
            [
                Sub("Marik Commonwealth",
                    [Trait("Wealth", 100), Trait("Equipped", 100), Trait("Reputation", -100),
                        Skill("Appraisal", 5), Skill("Negotiation", 10),
                        Skill("Protocol/Free Worlds", 10)]),
                Sub("Principality of Regulus",
                    [Attribute("WIL", 75), Trait("Gregarious", 75),
                        Trait("Compulsion/Atrean Opponent", -50), Trait("Reputation", -50),
                        Skill("Interests/Regulan History", 20), Skill("Negotiation", 25),
                        Skill("Perception", 15), Skill("Protocol/Free Worlds", 15)]),
                Sub("Duchy of Oriente",
                    [Trait("Reputation", 100), Skill("Appraisal", 5), Skill("Negotiation", 15)],
                    [Choice("technician", "Technician", EffectTarget.Skill, 5, 1, Technicians)]),
                Sub("Duchy of Andurien",
                    [Attribute("WIL", 50), Trait("Combat Sense", 215),
                        Trait("Compulsion/Hatred of House Liao", -100),
                        Trait("Compulsion/Atrean Opponent", -50), Trait("Reputation", -30),
                        Skill("Negotiation", 15), Skill("Perception", 10),
                        Skill("Protocol/Free Worlds", 15)]),
                Sub("Other FWL Worlds",
                    [Skill("Appraisal", 15), Skill("Protocol/Free Worlds", 10)],
                    [Choice("trait", "Trait", EffectTarget.Trait, 35, 1, FlexibleTraits),
                        Choice("language", "Language", EffectTarget.Skill, 20, 1, Languages),
                        Choice("skills", "Other skills", EffectTarget.Skill, 10, 2, FlexibleSkills),
                        FlexibleChoice("flex", "Attribute, trait, or language", 25, 1)])
            ],
            "lyran" =>
            [
                Sub("Alarion Province",
                    [Attribute("CHA", -50), Trait("Wealth", 70), Skill("Administration", 10),
                        Skill("Negotiation", 10)],
                    [Choice("interest", "Interest", EffectTarget.Skill, 10, 1, Interests),
                        Choice("language", "Language", EffectTarget.Skill, 10, 1, Languages)]),
                Sub("Bolan Province",
                    [Trait("Compulsion/Hatred of House Marik", -50), Trait("Connections", 50),
                        Trait("Wealth", 25), Skill("Administration", 5), Skill("Negotiation", 15),
                        Skill("Protocol/Lyran", 10), Skill("Streetwise/Lyran", 5)]),
                Sub("Coventry Province",
                    [Attribute("WIL", 100), Trait("Compulsion/Hatred of Clans", -95),
                        Trait("Wealth", 25), Skill("Administration", 10), Skill("Negotiation", 10),
                        Skill("Protocol/Lyran", 10)]),
                Sub("Donegal Province",
                    [Attribute("WIL", 50), Trait("Compulsion/Greedy", -75),
                        Trait("Connections", 50), Trait("Reputation", -50), Trait("Wealth", 50),
                        Skill("Appraisal", 10), Skill("Negotiation", 10),
                        Skill("Protocol/Lyran", 15)]),
                Sub("Skye Province",
                    [Attribute("WIL", 100), Trait("Connections", 85), Trait("Reputation", -150),
                        Skill("Language/Scots Gaelic", 10), Skill("Negotiation", 15),
                        Skill("Protocol/Lyran", -15), Skill("Streetwise/Lyran", 15)])
            ],
            "rasalhague" =>
            [
                Sub("Clan War Expatriate",
                    [Attribute("WIL", 125), Attribute("EDG", 100),
                        Trait("Compulsion/Hatred of Clans", -150), Trait("Wealth", -50),
                        Skill("Martial Arts", 10), Skill("Small Arms", 15)],
                    [Choice("language", "Lyran or Draconis language", EffectTarget.Skill, 15, 1,
                        LyranLanguages.Concat(["Language/Japanese", "Language/Arabic",
                            "Language/Swedenese"]).Distinct().ToArray()),
                        Choice("protocol", "Lyran or Draconis protocol", EffectTarget.Skill, 10, 1,
                            ["Protocol/Lyran", "Protocol/Combine"])]),
                Sub("Ghost Bear Dominion",
                    [Trait("Equipped", 50), Trait("Introvert", -25), Trait("Reputation", -25),
                        Skill("Protocol/Clan (Ghost Bear)", 20), Skill("Interests/Remembrance", 10),
                        Skill("Negotiation", 10), Skill("Martial Arts", 15),
                        Skill("Melee Weapons", 10)],
                    [Choice("interest", "Interest", EffectTarget.Skill, 10, 1, Interests)])
            ],
            "minor-periphery" => CreateMinorPeriphery(),
            "major-periphery" => CreateMajorPeriphery(),
            "deep-periphery" => CreateDeepPeriphery(),
            "invading-clan" => CreateInvadingClanSubAffiliations(),
            "homeworld-clan" => CreateHomeworldClanSubAffiliations(),
            "terran" => CreateTerranSubAffiliations(),
            "independent" => CreateIndependentSubAffiliations(),
            _ => names.Select((name, index) => Child($"{affiliationId}-sub-{index}", name, [])).ToArray()
        };

    private static LifePathModule Sub(
        string name, IReadOnlyList<ModuleEffect> effects,
        IReadOnlyList<ModuleChoice>? choices = null) =>
        new($"sub-{Slug(name)}", name, "", 0, effects, choices ?? []);

    private static string Slug(string value) =>
        new string(value.ToLowerInvariant().Select(character =>
            char.IsLetterOrDigit(character) ? character : '-').ToArray()).Trim('-');

    private static IReadOnlyList<LifePathModule> CreateMinorPeriphery() =>
    [
        Sub("Fiefdom of Randis",
            [Attribute("BOD", 125), Attribute("EDG", 50), Trait("Illiterate", -75),
                Trait("Wealth", -50), Skill("Martial Arts", 10), Skill("Melee Weapons", 10),
                Skill("Negotiation", 10), Skill("Streetwise/Periphery", 15)],
            [Choice("survival", "Survival", EffectTarget.Skill, 20, 1, SurvivalSkills)]),
        Sub("Franklin Fiefs",
            [Attribute("BOD", 150), Attribute("INT", -100), Attribute("WIL", 50),
                Trait("Equipped", -60), Trait("Illiterate", -90), Trait("Toughness", 100),
                Skill("Martial Arts", 15), Skill("MedTech", 10),
                Skill("Protocol/Novo Franklin", 10), Skill("Streetwise/Periphery", 10)],
            [Choice("survival", "Survival", EffectTarget.Skill, 10, 1, SurvivalSkills),
                Choice("specialty", "Specialty", EffectTarget.Skill, 10, 1,
                    ["Archery", "Melee Weapons", "Negotiation"])]),
        Sub("Mica Majority",
            [Attribute("BOD", 100), Attribute("RFL", 100), Attribute("EDG", -100),
                Trait("Equipped", -25), Trait("Toughness", 100), Trait("Wealth", -100),
                Skill("Career/Mining", 10), Skill("Language/Japanese", 10),
                Skill("Negotiation", 10), Skill("Survival/Arctic", 10)]),
        Sub("Niops Association",
            [Attribute("INT", 125), Attribute("WIL", -110), Trait("Equipped", 200),
                Trait("Introvert", -125)],
            [Choice("interest", "Interest", EffectTarget.Skill, 10, 1, Interests),
                Choice("technician", "Technician", EffectTarget.Skill, 15, 1, Technicians)]),
        Sub("Rim Collection",
            [Attribute("CHA", -50), Attribute("EDG", 100), Trait("Fit", 75),
                Trait("Wealth", -50), Skill("Negotiation", 15), Skill("Small Arms", 5)],
            [Choice("skills", "Frontier skills", EffectTarget.Skill, 10, 2,
                ["Animal Handling", "Archery", "Martial Arts", "Melee Weapons",
                    "Streetwise/Rim Collection", .. SurvivalSkills])])
    ];

    private static IReadOnlyList<LifePathModule> CreateMajorPeriphery() =>
    [
        Sub("Circinus Federation",
            [Attribute("STR", 100), Attribute("BOD", 75), Attribute("INT", -100),
                Attribute("WIL", 70), Trait("Illiterate", -75), Trait("Reputation", -200),
                Trait("Toughness", 300), Trait("Wealth", -125)],
            [Choice("skills", "Frontier skills", EffectTarget.Skill, 20, 3,
                    ["Animal Handling", "Martial Arts", "MedTech", "Small Arms",
                        "Streetwise/Periphery", .. SurvivalSkills, "Tracking/Wilds"])]),
        Sub("Magistracy of Canopus",
            [Attribute("CHA", 100), Attribute("EDG", 50), Trait("Gregarious", 50),
                Trait("Illiterate", -25), Trait("Reputation", -125), Trait("Wealth", 25),
                Skill("Streetwise/Magistracy", 15)],
            [Choice("profession", "Acting or MedTech", EffectTarget.Skill, 15, 1,
                    ["Acting", "MedTech"])]),
        Sub("Marian Hegemony",
            [Attribute("WIL", 100), Trait("Compulsion/Paranoid", -50),
                Trait("Connections", 25), Trait("Reputation", -150), Trait("Toughness", 125),
                Skill("Interests/Marian History", 15), Skill("Interests/Roman History", 10),
                Skill("Language/Latin", 15), Skill("Protocol/Marian", 10),
                Skill("Strategy", 5)]),
        Sub("Outworlds Alliance",
            [Attribute("EDG", 75), Trait("Equipped", -55), Trait("G-Tolerance", 125),
                Trait("Wealth", -75), Skill("Streetwise/Outworlds", 10)],
            [Choice("survival", "Survival", EffectTarget.Skill, 10, 1, SurvivalSkills),
                Choice("profession", "Professional skill", EffectTarget.Skill, 15, 1,
                    ["Martial Arts", "MedTech", "Small Arms"])]),
        Sub("Taurian Concordat",
            [Attribute("WIL", 150), Attribute("EDG", 50),
                Trait("Compulsion/Distrust FedSuns", -75), Trait("Compulsion/Stubborn", -75),
                Skill("Martial Arts", 10), Skill("Negotiation", 10), Skill("Small Arms", 15),
                Skill("Streetwise/Taurian", 15)],
            [Choice("survival", "Survival", EffectTarget.Skill, 5, 1, SurvivalSkills)])
    ];

    private static IReadOnlyList<LifePathModule> CreateDeepPeriphery() =>
    [
        Sub("Hanseatic League",
            [Trait("Citizenship", 30), Trait("Compulsion/Distrust Lyrans", -20),
                Skill("Appraisal", 10), Skill("Negotiation", 20),
                Skill("Protocol/Hanseatic", 10)]),
        Sub("Castilian Principalities",
            [Attribute("DEX", 25), Trait("Compulsion/Castilian Honor Code", -20),
                Trait("Compulsion/Hatred of Umayyads", -20), Skill("Martial Arts", 15),
                Skill("Melee Weapons", 15), Skill("Negotiation", 10),
                Skill("Protocol/Castilian", 25)]),
        Sub("Umayyad Caliphate",
            [Attribute("DEX", 20), Trait("Compulsion/Xenophobic", -10),
                Skill("Protocol/Umayyad", 20)],
            [Choice("art", "Art", EffectTarget.Skill, 10, 1, Arts),
                Choice("interest", "Interest", EffectTarget.Skill, 10, 1, Interests)]),
        Sub("JàrnFòlk",
            [Attribute("RFL", 20), Trait("Compulsion/Xenophobic", -10),
                Trait("Natural Aptitude/Martial Arts", 10), Trait("Wealth", -10),
                Skill("Negotiation", 15), Skill("Protocol/JàrnFòlk Families", 15)],
            [Choice("specialty", "Art, interest, or technician specialty", EffectTarget.Skill,
                    10, 1, Arts.Concat(Interests).Concat(Technicians).ToArray())])
    ];

    private static IReadOnlyList<LifePathModule> CreateInvadingClanSubAffiliations() =>
    [
        Sub("Diamond Shark",
            [Attribute("STR", -45), Attribute("INT", 25), Attribute("EDG", -50),
                Trait("Connections", 25), Trait("Equipped", 25), Trait("Wealth", 30),
                Skill("Perception", 10), Skill("Negotiation", 20),
                Skill("Protocol/Diamond Shark", 10)]),
        Sub("Ghost Bear",
            [Attribute("STR", 25), Attribute("BOD", 25),
                Trait("Compulsion/Hate Hell's Horses", -25),
                Trait("Exceptional Attribute/STR", 50), Trait("Slow Learner", -50),
                Skill("Protocol/Ghost Bear", 10), Skill("Streetwise/Rasalhague", 5)],
            [Choice("art", "Art", EffectTarget.Skill, 10, 1, Arts)]),
        Sub("Hell's Horses",
            [Attribute("STR", 25), Attribute("BOD", 25),
                Trait("Compulsion/Hate Ghost Bears", -25), Trait("Introvert", -30),
                Skill("Melee Weapons", 10), Skill("Navigation/Ground", 15),
                Skill("Protocol/Hell's Horses", 15), Skill("Survival/Desert", 15)]),
        Sub("Jade Falcon",
            [Attribute("WIL", 25), Trait("Compulsion/Falcon Pride", -75),
                Trait("Compulsion/Hate Steel Vipers", -50), Trait("Reputation", 100),
                Skill("Acting", 10), Skill("Martial Arts", 15),
                Skill("Protocol/Jade Falcon", 15), Skill("Survival/Forests", 10)]),
        Sub("Nova Cat",
            [Attribute("EDG", 120), Trait("Enemy/The Clans", -100),
                Trait("Enemy/Draconis Combine", -50), Trait("Equipped", 50),
                Trait("Reputation", -100), Trait("Sixth Sense", 100),
                Skill("Interests/Nova Cat Vision Quest", 10), Skill("Language/Japanese", 5),
                Skill("Protocol/Draconis Combine", 5), Skill("Protocol/Nova Cat", 10)]),
        Sub("Snow Raven",
            [Attribute("INT", 20), Trait("Compulsion/Raven Pride", -50),
                Trait("Connections", 50), Skill("Negotiation", 10),
                Skill("Protocol/Snow Raven", 10), Skill("Zero-G Operations", 10)]),
        Sub("Wolf",
            [Attribute("INT", 25), Attribute("WIL", 25), Trait("Compulsion/Wolf Pride", -50),
                Trait("Equipped", 50), Trait("Enemy", -100), Trait("Reputation", 70),
                Skill("Protocol/Wolf", 10)],
            [Choice("skills", "Clan Wolf skills", EffectTarget.Skill, 10, 2,
                ["Leadership", "Negotiation", "Perception", "Strategy", .. Interests])])
    ];

    private static IReadOnlyList<LifePathModule> CreateHomeworldClanSubAffiliations() =>
    [
        Sub("Blood Spirit",
            [Attribute("BOD", 25), Attribute("WIL", 100), Attribute("CHA", -50),
                Trait("Combat Sense", 100), Trait("Compulsion/Blood Spirit Fanaticism", -100),
                Trait("Compulsion/Hate Star Adder", -100), Trait("Equipped", -65),
                Trait("Exceptional Attribute/WIL", 200), Trait("Introvert", -50),
                Trait("Reputation", -50), Skill("Interests/Clan History", 25),
                Skill("Martial Arts", 15), Skill("Small Arms", 15),
                Skill("Protocol/Blood Spirit", 10)]),
        Sub("Cloud Cobra",
            [Attribute("INT", 50), Attribute("WIL", 50),
                Trait("Compulsion/Religious Faith", -75), Trait("Equipped", -25),
                Trait("Patient", 100), Trait("Reputation", -75),
                Skill("Interests/Theology/Any", 20), Skill("Protocol/Cloud Cobra", 20)],
            [Choice("skill", "Other skill", EffectTarget.Skill, 10, 1,
                FlexibleSkills)]),
        Sub("Coyote",
            [Attribute("INT", 100), Attribute("WIL", -60), Attribute("EDG", 25),
                Trait("Equipped", 25), Trait("Reputation", -60),
                Skill("Interests/Coyote Rituals", 15), Skill("Protocol/Coyote", 10)],
            [Choice("trait", "Technical aptitude", EffectTarget.Trait, 10, 1,
                ["Custom Vehicle", "Natural Aptitude/Computers",
                    "Natural Aptitude/Technician/Electronic",
                    "Natural Aptitude/Technician/Mechanical",
                    "Natural Aptitude/Technician/Myomer",
                    "Natural Aptitude/Technician/Nuclear", "Vehicle"]),
                Choice("survival", "Survival", EffectTarget.Skill, 10, 1,
                    SurvivalSkills)]),
        Sub("Fire Mandrill",
            [Attribute("WIL", 25), Trait("Compulsion/Fire Mandrill Fanaticism", -100),
                Trait("Compulsion/Kindraa Fanaticism", -100), Trait("Enemy/Rival Kindraa", -25),
                Trait("Reputation", -25),
                Skill("Martial Arts", 15), Skill("Protocol/Fire Mandrill", 15),
                Skill("Protocol/Kindraa", 25)],
            [Choice("attribute-up", "Other attribute increase", EffectTarget.Attribute,
                    75, 1, ["STR", "BOD", "RFL", "DEX", "INT", "CHA", "EDG"]),
                Choice("attribute-down", "Other attribute reduction",
                    EffectTarget.Attribute, -20, 1,
                    ["STR", "BOD", "RFL", "DEX", "INT", "CHA", "EDG"]),
                Choice("trait", "Clan talent", EffectTarget.Trait, 150, 1,
                    ["Combat Sense", "Exceptional Attribute/STR",
                        "Exceptional Attribute/BOD", "Exceptional Attribute/RFL",
                        "Exceptional Attribute/DEX", "Exceptional Attribute/INT",
                        "Exceptional Attribute/WIL", "Exceptional Attribute/CHA",
                        "Exceptional Attribute/EDG", "Fast Learner",
                        "Natural Aptitude/Perception", "Sixth Sense"]),
                Choice("language", "Secondary language", EffectTarget.Skill, 20, 1,
                    ["Language/Chinese", "Language/French", "Language/German",
                        "Language/Japanese", "Language/Russian", "Language/Spanish"]),
                Choice("skills", "Kindraa skills", EffectTarget.Skill, 10, 2,
                    ["Leadership", "Melee Weapons", "Negotiation", "Perception",
                        .. TacticsSkills])]),
        Sub("Goliath Scorpion",
            [Attribute("DEX", 50), Attribute("INT", 50), Attribute("WIL", -50),
                Attribute("EDG", -50), Trait("Compulsion/Necrosia Addiction", -50),
                Trait("Compulsion/Nostalgic", -50), Trait("Fit", 55),
                Trait("Reputation", -25), Skill("Interests/Star League History", 20),
                Skill("Melee Weapons", 15), Skill("Protocol/Goliath Scorpion", 10)],
            [Choice("trait", "Scorpion aptitude", EffectTarget.Trait, 100, 1,
                ["Exceptional Attribute/INT", "Natural Aptitude/Gunnery/'Mech",
                    "Natural Aptitude/Gunnery/Aerospace",
                    "Natural Aptitude/Gunnery/Battlesuit",
                    "Natural Aptitude/Gunnery/Ground Vehicle",
                    "Natural Aptitude/Melee Weapons",
                    .. Interests.Select(name => $"Natural Aptitude/{name}")])]),
        Sub("Ice Hellion",
            [Attribute("DEX", 75), Attribute("RFL", 100), Attribute("WIL", 50),
                Attribute("CHA", -75), Trait("Combat Sense", 50), Trait("Impatient", -100),
                Trait("Reputation", -95), Skill("Interests/Clan Remembrance", 15),
                Skill("Martial Arts", 10), Skill("Negotiation", 15),
                Skill("Protocol/Ice Hellion", 10), Skill("Swimming", 10),
                Skill("Survival/Arctic", 10)]),
        Sub("Star Adder",
            [Attribute("INT", 50), Attribute("WIL", 75), Attribute("CHA", -70),
                Trait("Combat Sense", 50), Trait("Compulsion/Clan Honor", -50),
                Trait("Equipped", 25), Trait("Reputation", 25), Skill("Leadership", 10),
                Skill("Perception", 10), Skill("Protocol/Star Adder", 10)],
            [Choice("compulsion", "Clan compulsion", EffectTarget.Trait, -60, 1,
                ["Compulsion/Adder Arrogance",
                    "Compulsion/Burrock Forever!"])]),
        Sub("Steel Viper",
            [Attribute("INT", 75), Attribute("WIL", 100), Attribute("CHA", -50),
                Trait("Compulsion/Hate Jade Falcons", -100),
                Trait("Compulsion/Clan Honor", -100),
                Trait("Compulsion/Hate Snow Ravens", -50), Trait("Connections", 50),
                Trait("Equipped", 50), Trait("Reputation", 50), Skill("Negotiation", 15),
                Skill("Protocol/Steel Viper", 15)],
            [Choice("survival", "Survival", EffectTarget.Skill, 20, 1, SurvivalSkills)])
    ];

    private static IReadOnlyList<LifePathModule> CreateTerranSubAffiliations() =>
    [
        Sub("Belter",
            [Attribute("STR", -25), Attribute("BOD", -25),
                Trait("Compulsion/Xenophobia", -100), Trait("Reputation", -50),
                Trait("Wealth", 50), Skill("Navigation/Space", 15),
                Skill("Survival/Asteroids", 10),
                Skill("Zero-G Operations", 15)],
            [Choice("attribute", "Adapted attribute", EffectTarget.Attribute, 100, 1,
                ["STR", "BOD", "RFL", "DEX", "INT", "WIL"]),
                Choice("traits", "Adaptation traits", EffectTarget.Trait, 50, 2,
                ["Ambidextrous", "Attractive", "Exceptional Attribute/STR",
                    "Exceptional Attribute/BOD", "Exceptional Attribute/RFL",
                    "Exceptional Attribute/DEX", "Exceptional Attribute/INT",
                    "Exceptional Attribute/WIL", "Exceptional Attribute/CHA",
                    "Exceptional Attribute/EDG", "Fast Learner",
                    "G-Tolerance", "Good Hearing", "Good Vision", "Implant/Prosthetic",
                    "Pain Resistance", "Toughness"]),
                Choice("skills", "Space skills", EffectTarget.Skill, 10, 2,
                    ["Driving/Ground Vehicles", "Driving/Rail Vehicles",
                        "Driving/Sea Vehicles", .. Interests, .. Languages,
                        "MedTech/General", "Piloting/Air Vehicle", "Piloting/Aerospace",
                        "Piloting/Battlesuit", "Piloting/Spacecraft",
                        "Sensor Operations", .. Technicians])]),
        Sub("Lunar Citizen",
            [Attribute("STR", -20), Attribute("BOD", -25), Attribute("INT", 50),
                Attribute("WIL", 65), Attribute("EDG", -25), Trait("Equipped", 25),
                Trait("Poor Vision", -25), Trait("Wealth", 50)],
            [Choice("technician", "Technician", EffectTarget.Skill, 5, 1,
                    Technicians),
                Choice("skill", "Lunar skill", EffectTarget.Skill, 10, 1,
                    ["Computers", "Zero-G Operations", .. Arts, .. Interests,
                        .. SurvivalSkills])]),
        Sub("Martian Citizen",
            [Attribute("STR", -15), Attribute("WIL", 75), Trait("Equipped", 65),
                Trait("Introvert", -25), Trait("Poor Vision", -25), Trait("Reputation", -25),
                Trait("Wealth", 35), Skill("Negotiation", 10),
                Skill("Survival/Martian Desert", 10)],
            [Choice("language", "Previously selected secondary language",
                    EffectTarget.Skill, -10, 1, Languages),
                Choice("skill", "Martian skill", EffectTarget.Skill, 15, 1,
                    ["Computers", "Martial Arts", "MedTech/General",
                        "Melee Weapons", "Small Arms", .. Arts, .. Interests])]),
        Sub("Outer System Citizen",
            [Attribute("STR", -15), Attribute("BOD", -15), Attribute("RFL", 20),
                Trait("Equipped", 75), Trait("Wealth", 25), Trait("Reputation", -20),
                Skill("Negotiation", 10)],
            [Choice("technician", "Technician", EffectTarget.Skill, 10, 1,
                    Technicians),
                Choice("skills", "Outer-system skills", EffectTarget.Skill, 10, 2,
                    ["Computers", "Driving/Ground Vehicles", "Driving/Rail Vehicles",
                        "Driving/Sea Vehicles", "Martial Arts", "Zero-G Operations",
                        .. Arts, .. Interests, .. SurvivalSkills])]),
        Sub("Terran Citizen",
            [Attribute("EDG", -100), Trait("Compulsion/Distrust of Non-Terrans", -75),
                Trait("Connections", 100), Trait("Equipped", 100), Trait("Impatient", -50),
                Trait("Wealth", 110), Skill("Interests/Terran History", 10),
                Skill("Perception", -5)],
            [Choice("skills", "Other skills", EffectTarget.Skill, 5, 4,
                FlexibleSkills)]),
        Sub("Venusian Citizen",
            [Attribute("BOD", 35), Attribute("INT", -25), Attribute("WIL", 110),
                Trait("Equipped", 25), Trait("Introvert", -50), Trait("Reputation", -25),
                Trait("Wealth", 15)],
            [Choice("language", "Secondary language", EffectTarget.Skill, -10, 1, Languages),
                Choice("survival", "Survival", EffectTarget.Skill, 20, 1, SurvivalSkills),
                Choice("specialty", "Specialty", EffectTarget.Skill, 15, 1,
                    ["Melee Weapons", "MedTech/General", "Martial Arts", "Computers",
                        "Small Arms",
                        .. Arts, .. Interests, .. Technicians])])
    ];

    private static IReadOnlyList<LifePathModule> CreateIndependentSubAffiliations() =>
    [
        Sub("Antallos",
            [Attribute("BOD", 20), Attribute("WIL", 10), Attribute("CHA", -10),
                Trait("Illiterate", -20), Trait("Pain Resistance", 10),
                Trait("Reputation", -20), Trait("Toughness", 10),
                Skill("Language/Japanese", 10), Skill("Perception", 10),
                Skill("Streetwise/Periphery", 10)],
            [Choice("skills", "Survival skills", EffectTarget.Skill, 10, 2,
                ["Acting", "Escape Artist", "Martial Arts", "Melee Weapons",
                    "Small Arms", "Survival/Desert"])]),
        Sub("Astrokaszy",
            [Attribute("BOD", 15), Attribute("WIL", 25), Attribute("EDG", -10),
                Attribute("CHA", -10), Trait("Fit", 20), Trait("Compulsion/Xenophobic", -20),
                Trait("Illiterate", -20), Trait("Reputation", -10),
                Skill("Perception", 10),
                Skill("Protocol/Astrokaszy", 10), Skill("Streetwise/Periphery", 10)],
            [Choice("skills", "Survival skills", EffectTarget.Skill, 15, 2,
                ["Acting", "Martial Arts", "Melee Weapons", "Small Arms",
                    "Survival/Desert", "Thrown Weapons"])]),
        Sub("Generic",
            [Trait("Introvert", -10), Skill("Negotiation", 10)],
            [Choice("interest", "Interest", EffectTarget.Skill, 10, 1, Interests),
                Choice("skills", "Other skills", EffectTarget.Skill, 10, 4,
                    FlexibleSkills)]),
        Sub("Mercenary",
            [Attribute("CHA", -20), Trait("Equipped", 20), Trait("Rank", 20),
                Skill("Negotiation", 10), Skill("Protocol/Mercenary", 10)],
            [Choice("skill", "Other skill", EffectTarget.Skill, 10, 1, FlexibleSkills)]),
        Sub("Pirate",
            [Attribute("BOD", 20), Attribute("WIL", 10), Attribute("CHA", -30),
                Trait("Pain Resistance", 10), Trait("Reputation", -30), Trait("Toughness", 10),
                Skill("Negotiation", 5), Skill("Perception", 15)],
            [Choice("language", "Language", EffectTarget.Skill, 10, 1, Languages),
                Choice("skills", "Pirate skills", EffectTarget.Skill, 10, 3,
                    ["Acting", "Escape Artist", "Martial Arts", "Melee Weapons",
                        "Small Arms", .. SurvivalSkills])]),
        Sub("Spacer",
            [Attribute("BOD", -20), Attribute("STR", -10), Attribute("DEX", 10),
                Attribute("RFL", 10), Trait("Equipped", 10), Trait("G-Tolerance", 20),
                Trait("Introvert", -20), Trait("Natural Aptitude/Zero-G Operations", 20),
                Skill("Career/Ship's Crew", 10), Skill("Zero-G Operations", 10)],
            [Choice("specialty", "Shipboard specialty", EffectTarget.Skill, 10, 1,
                ["Appraisal", "Navigation/Space", "Negotiation", "Sensor Operations",
                    .. Interests])]),
        Sub("Tortuga",
            [Attribute("BOD", 10), Attribute("STR", 10), Attribute("WIL", 20),
                Attribute("CHA", -40), Trait("Pain Resistance", 10),
                Trait("Reputation", -50), Trait("Toughness", 10), Skill("Martial Arts", 10),
                Skill("Negotiation", 10), Skill("Perception", 10),
                Skill("Streetwise/Periphery", 10)],
            [Choice("language", "Language", EffectTarget.Skill, 10, 1, Languages),
                Choice("skills", "Survival skills", EffectTarget.Skill, 10, 3,
                    ["Acting", "Escape Artist", "Melee Weapons", "Small Arms",
                        .. SurvivalSkills])])
    ];

    private static LifePathModule Child(string id, string name, IReadOnlyList<ModuleEffect> effects) =>
        new(id, name, "", 0, effects, []);

    private static LifePathModule School(
        string id, string name, int cost, IReadOnlyList<ModuleEffect> effects,
        IReadOnlyList<ModuleChoice> choices, IReadOnlyList<string> basicFields,
        IReadOnlyList<string> advancedFields, IReadOnlyList<string>? specialistFields = null,
        int basicFieldYears = 1, int advancedFieldYears = 2, int specialistFieldYears = 0,
        int protocolXp = 0, int streetwiseXp = 0) =>
        new(id, name, $"Education at {name}.", cost, effects, choices,
            BasicFields: basicFields.Select(field =>
                CreateEducationField(field, basicFieldYears)).ToArray(),
            AdvancedFields: advancedFields.Select(field =>
                CreateEducationField(field, advancedFieldYears)).ToArray(),
            SpecialistFields: (specialistFields ?? []).Select(field =>
                CreateEducationField(field, specialistFieldYears)).ToArray(),
            AffiliationProtocolXp: protocolXp, AffiliationStreetwiseXp: streetwiseXp);

    private static LifePathModule CreateEducationField(string name, int timeYears)
    {
        var skills = EducationFieldData.Skills.TryGetValue(name, out var fieldSkills)
            ? fieldSkills
            : [$"Career/{name}"];
        var effects = new List<ModuleEffect>();
        var choices = new List<ModuleChoice>();
        var affiliationProtocolXp = 0;
        var affiliationStreetwiseXp = 0;

        foreach (var group in skills.GroupBy(skill => skill))
        {
            if (group.Key == "Protocol/Affiliation")
            {
                affiliationProtocolXp += group.Count() * 30;
            }
            else if (group.Key == "Streetwise/Affiliation")
            {
                affiliationStreetwiseXp += group.Count() * 30;
            }
            else if (EducationFieldData.ChoiceOptions.TryGetValue(group.Key, out var options))
            {
                choices.Add(Choice(
                    $"specialty-{Slug(group.Key)}",
                    EducationFieldData.ChoiceLabels.GetValueOrDefault(group.Key, group.Key),
                    EffectTarget.Skill,
                    30,
                    group.Count(),
                    options));
            }
            else
            {
                effects.AddRange(group.Select(_ => Skill(group.Key, 30)));
            }
        }

        return new($"field-{Slug(name)}", name, "", skills.Length * 30,
            effects, choices,
            AffiliationProtocolXp: affiliationProtocolXp,
            AffiliationStreetwiseXp: affiliationStreetwiseXp,
            TimeYears: timeYears);
    }

    private static class EducationFieldData
    {
        internal static readonly IReadOnlyDictionary<string, string> ChoiceLabels =
            new Dictionary<string, string>
        {
            ["Career/Any"] = "Career",
            ["Career/Pilot or Ship's Crew"] = "Career",
            ["Driving/Any"] = "Driving specialty",
            ["Gunnery/Any Vehicle"] = "Vehicle gunnery specialty",
            ["Interests/Any"] = "Interest",
            ["Interests/History (Any one culture)"] = "Cultural history",
            ["Interests/History (any)"] = "History",
            ["Language/Any"] = "Language",
            ["Piloting/Aircraft or VTOL"] = "Aircraft piloting specialty",
            ["Protocol/Any"] = "Protocol specialty",
            ["Science/Any"] = "Science specialty",
            ["Security Systems"] = "Security Systems specialty",
            ["Security Systems/Any"] = "Security Systems specialty",
            ["Streetwise/Any"] = "Streetwise specialty",
            ["Surgery/Any"] = "Surgery specialty",
            ["Survival/Any"] = "Survival specialty",
            ["Tactics/Any"] = "Tactics specialty",
            ["Tactics/Land or Sea"] = "Tactics specialty",
            ["Technician/Any"] = "Technician specialty",
            ["Tracking/Any"] = "Tracking specialty"
        };

        internal static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> ChoiceOptions =
            new Dictionary<string, IReadOnlyList<string>>
        {
            ["Career/Any"] = Careers,
            ["Career/Pilot or Ship's Crew"] =
                ["Career/Pilot", "Career/Ship's Crew"],
            ["Driving/Any"] =
                ["Driving/Ground Vehicles", "Driving/Rail Vehicles", "Driving/Sea Vehicles"],
            ["Gunnery/Any Vehicle"] =
                ["Gunnery/Air Vehicle", "Gunnery/Ground Vehicle", "Gunnery/Sea Vehicle"],
            ["Interests/Any"] = Interests,
            ["Interests/History (Any one culture)"] =
                ["Interests/Star League History", "Interests/Combine History",
                    "Interests/FedSuns History", "Interests/Regulan History",
                    "Interests/Marian History", "Interests/Roman History",
                    "Interests/Clan History", "Interests/Terran History"],
            ["Interests/History (any)"] =
                ["Interests/History", "Interests/Star League History",
                    "Interests/Combine History", "Interests/FedSuns History",
                    "Interests/Clan History", "Interests/Terran History"],
            ["Language/Any"] = Languages,
            ["Piloting/Aircraft or VTOL"] =
                ["Piloting/Air Vehicle", "Piloting/Aerospace"],
            ["Protocol/Any"] =
                ["Protocol/Capellan", "Protocol/Combine", "Protocol/FedSuns",
                    "Protocol/Lyran", "Protocol/Clan", "Protocol/ComStar",
                    "Protocol/Word of Blake", "Protocol/Free Worlds",
                    "Protocol/Rasalhague"],
            ["Science/Any"] =
                ["Science/Biology", "Science/Chemistry", "Science/Mathematics",
                    "Science/Physics"],
            ["Security Systems"] =
                ["Security Systems/Electronic", "Security Systems/Mechanical"],
            ["Security Systems/Any"] =
                ["Security Systems/Electronic", "Security Systems/Mechanical"],
            ["Streetwise/Any"] = StreetwiseOptions,
            ["Surgery/Any"] = ["Surgery/General", "Surgery/Veterinary"],
            ["Survival/Any"] = SurvivalSkills,
            ["Tactics/Any"] =
                ["Tactics/Infantry", "Tactics/Land", "Tactics/Sea", "Tactics/Air",
                    "Tactics/Space"],
            ["Tactics/Land or Sea"] = ["Tactics/Land", "Tactics/Sea"],
            ["Technician/Any"] = Technicians,
            ["Tracking/Any"] = ["Tracking/Urban", "Tracking/Wilds"]
        };

        internal static readonly IReadOnlyDictionary<string, string[]> Skills =
            new Dictionary<string, string[]>
        {
            ["Communications"] = ["Acting", "Career/Communications", "Communications/Conventional", "Computers", "Protocol/Any", "Sensor Operations"],
            ["Pilot - Aerospace (Civilian)"] = ["Career/Aerospace Pilot", "Communications/Conventional", "Navigation/Air", "Navigation/Space", "Piloting/Aerospace", "Sensor Operations"],
            ["Pilot - Aircraft (Civilian)"] = ["Career/Aircraft Pilot", "Communications/Conventional", "Navigation/Air", "Piloting/Air Vehicle", "Sensor Operations"],
            ["Pilot - DropShip (Civilian)"] = ["Career/DropShip Pilot", "Communications/Conventional", "Navigation/Space", "Piloting/Spacecraft", "Sensor Operations", "Zero-G Operations"],
            ["Pilot - Exoskeleton"] = ["Piloting/Battlesuit", "Sensor Operations", "Technician/Electronic", "Technician/Mechanical", "Technician/Myomer"],
            ["Technician - Civilian"] = ["Appraisal", "Career/Technician", "Technician/Electronic", "Technician/Mechanical", "Technician/Nuclear"],
            ["Cartographer"] = ["Career/Cartographer", "Computers", "Navigation/Air", "Navigation/Ground", "Perception", "Sensor Operations"],
            ["Engineer"] = ["Appraisal", "Career/Engineer", "Perception", "Technician/Nuclear", "Technician/Any"],
            ["Merchant Marine"] = ["Career/Merchant Marine", "Protocol/Any", "Technician/Aeronautics", "Technician/Any", "Zero-G Operations"],
            ["Pilot - IndustrialMech"] = ["Piloting/'Mech", "Sensor Operations", "Technician/Electronic", "Technician/Mechanical", "Technician/Myomer"],
            ["Pilot - JumpShip"] = ["Administration", "Computers", "Navigation/K-F Jump", "Navigation/Space"],
            ["Technician - Aerospace"] = ["Computers", "Technician/Aeronautics", "Technician/Nuclear", "Technician/Jets", "Zero-G Operations"],
            ["Technician - Mech"] = ["Technician/Electronic", "Technician/Jets", "Technician/Mechanical", "Technician/Myomer", "Technician/Nuclear"],
            ["Technician - Vehicle"] = ["Computers", "Technician/Electronic", "Technician/Mechanical", "Technician/Nuclear"],
            ["General Studies"] = ["Computers", "Career/Any", "Interests/Any", "Perception", "Protocol/Affiliation"],
            ["Merchant"] = ["Administration", "Appraisal", "Career/Merchant", "Negotiation", "Protocol/Any", "Streetwise/Any"],
            ["Analysis"] = ["Computers", "Investigation", "Language/Any", "Language/Any", "Sensor Operations", "Strategy", "Tactics/Any"],
            ["Anthropologist"] = ["Career/Anthropologist", "Interests/History (Any one culture)", "Investigation", "Language/Any", "Language/Any", "Protocol/Any"],
            ["Archaeologist"] = ["Career/Archaeologist", "Appraisal", "Interests/Geology", "Interests/History (any)", "Navigation/Ground", "Perception"],
            ["HPG Technician"] = ["Administration", "Communications/Conventional", "Communications/HPG", "Computers", "Cryptography"],
            ["Journalist"] = ["Acting", "Art/Writing", "Career/Journalist", "Computers", "Investigation", "Perception"],
            ["Manager"] = ["Administration", "Career/Management", "Leadership", "Negotiation", "Protocol/Affiliation", "Training"],
            ["Medical Assistant"] = ["Computers", "Career/Medtech", "Interests/Pharmacology", "MedTech/General", "Perception"],
            ["Detective"] = ["Career/Detective", "Computers", "Interrogation", "Investigation", "Perception", "Security Systems", "Streetwise/Affiliation"],
            ["Planetary Surveyor"] = ["Appraisal", "Driving/Any", "Navigation/Ground", "Survival/Any", "Tracking/Wilds"],
            ["Politician"] = ["Acting", "Career/Politician", "Leadership", "Negotiation", "Protocol/Affiliation"],
            ["Technician - Military"] = ["Appraisal", "Career/Technician", "Technician/Electronic", "Technician/Mechanical", "Technician/Nuclear", "Technician/Weapons"],
            ["Doctor"] = ["Administration", "Career/Doctor", "MedTech/General", "Protocol/Affiliation", "Surgery/Any"],
            ["Lawyer"] = ["Acting", "Administration", "Career/Lawyer", "Interests/Law", "Negotiation", "Protocol/Any"],
            ["Military Scientist"] = ["Career/Military Scientist", "Computers", "Cryptography", "Interests/Military History", "Strategy", "Tactics/Any"],
            ["Cavalry"] = ["Artillery", "Driving/Any", "Gunnery/Any Vehicle", "Sensor Operations", "Tactics/Land or Sea", "Technician/Mechanical"],
            ["MechWarrior"] = ["Gunnery/'Mech", "Piloting/'Mech", "Sensor Operations", "Tactics/Land", "Technician/Any"],
            ["Pilot - Battle Armor"] = ["Climbing", "Gunnery/Battlesuit", "Martial Arts", "Piloting/Battlesuit", "Sensor Operations", "Tactics/Land"],
            ["Police Officer"] = ["Acting", "Career/Police", "Driving/Any", "Martial Arts", "MedTech/General", "Small Arms", "Streetwise/Affiliation"],
            ["Intelligence"] = ["Communications/Conventional", "Computers", "Cryptography", "Language/Any", "Sensor Operations"],
            ["Covert Operations"] = ["Acting", "Escape Artist", "Language/Any", "Perception", "Protocol/Any", "Streetwise/Any", "Tracking/Any"],
            ["Police Tactical Officer"] = ["Climbing", "Demolitions", "Running", "Support Weapons", "Tactics/Infantry", "Thrown Weapons", "Tracking/Urban"],
            ["Special Forces"] = ["Acrobatics/Free-Fall", "Demolitions", "Small Arms", "Stealth", "Survival/Any", "Tracking/Any"],
            ["Basic Training"] = ["Career/Soldier", "Martial Arts", "MedTech/General", "Navigation/Ground", "Small Arms"],
            ["Scout"] = ["Communications/Conventional", "Disguise", "Language/Any", "Security Systems/Any", "Stealth", "Streetwise/Any", "Tracking/Any"],
            ["Infantry"] = ["Acrobatics/Free-Fall", "Artillery", "Climbing", "Communications/Conventional", "Support Weapons", "Tactics/Infantry"],
            ["Marine"] = ["Acrobatics/Free-Fall", "Demolitions", "Gunnery/Spacecraft", "Communications/Conventional", "Security Systems/Any", "Zero-G Operations"],
            ["Pilot - Aerospace (Combat)"] = ["Gunnery/Aerospace", "Navigation/Air", "Navigation/Space", "Piloting/Aerospace", "Sensor Operations", "Tactics/Space", "Zero-G Operations"],
            ["Pilot - Aircraft (Combat)"] = ["Gunnery/Air Vehicle", "Navigation/Air", "Piloting/Air Vehicle", "Sensor Operations", "Tactics/Air"],
            ["Scientist"] = ["Career/Scientist", "Computers", "Interests/Any", "Investigation", "Perception", "Science/Any", "Training"],
            ["Ship's Crew"] = ["Career/Ship's Crew", "Computers", "Gunnery/Spacecraft", "Technician/Any", "Zero-G Operations"],
            ["Infantry - Anti-'Mech"] = ["Acrobatics/Gymnastics", "Demolitions", "Perception", "Security Systems/Electronic", "Technician/Mechanical", "Technician/Myomer"],
            ["Basic Training (Naval)"] = ["Career/Pilot or Ship's Crew", "Martial Arts", "MedTech/General", "Navigation/Space", "Small Arms", "Zero-G Operations"],
            ["Pilot - WarShip"] = ["Computers", "Leadership", "Navigation/K-F Jump",
                "Navigation/Space", "Protocol/Affiliation", "Strategy", "Tactics/Space"],
            ["Officer"] = ["Administration", "Leadership", "Melee Weapons", "Protocol/Affiliation", "Training"]
        };
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<ModuleEffect>> CreateFreebornBranches() =>
        new Dictionary<string, IReadOnlyList<ModuleEffect>>
        {
            ["Aerospace"] = [Trait("Vehicle", 100)],
            ["Cavalry"] = [Trait("Vehicle", 100)],
            ["Elemental"] = [Trait("Vehicle", 100)],
            ["Infantry"] = [Trait("Equipped", 100)],
            ["MechWarrior"] = [Trait("Vehicle", 100)]
        };

    private static IReadOnlyDictionary<string, IReadOnlyList<ModuleEffect>> CreateTruebornBranches() =>
        new Dictionary<string, IReadOnlyList<ModuleEffect>>
        {
            ["Aerospace"] =
            [
                Trait("Custom Vehicle", 200), Skill("Gunnery/Spacecraft", 20),
                Skill("Piloting/Spacecraft", 20), Skill("Navigation/Air", 40)
            ],
            ["Elemental"] = [Trait("Vehicle", 120)],
            ["Elemental (Advanced)"] =
            [
                Attribute("CHA", -30), Attribute("EDG", -30), Trait("Equipped", 50),
                Trait("Vehicle", 185), Skill("Gunnery/Battlesuit", 15),
                Skill("Piloting/Battlesuit", 10), Skill("Swimming", 10),
                Skill("Tactics/Infantry", 10), PreAttribute("BOD", 600),
                PreAttribute("DEX", 400), PreAttribute("RFL", 400), PreAttribute("WIL", 400)
            ],
            ["ProtoMech"] =
            [
                Trait("Compulsion/Chemical Addiction", -100),
                Trait("Implant/EI Neural Implant", 200), Trait("Reputation", -100),
                Trait("Toughness", 100), Trait("Vehicle", 100),
                Skill("Navigation/Ground", 30), Skill("Tactics/Infantry", 30),
                Skill("Tactics/Land", 30)
            ],
            ["ProtoMech (Advanced)"] =
            [
                Trait("Compulsion/Chemical Addiction", -100),
                Trait("Implant/EI Neural Implant", 200), Trait("Reputation", -100),
                Trait("Toughness", 125), Trait("Vehicle", 125),
                Skill("Navigation/Ground", 30), Skill("Tactics/Infantry", 30),
                Skill("Tactics/Land", 30)
            ],
            ["MechWarrior"] =
            [
                Trait("Custom Vehicle", 200), Skill("Gunnery/Mech", 15),
                Skill("Piloting/BattleMech", 15)
            ]
        };

    private static IReadOnlyDictionary<string, IReadOnlyList<ModuleEffect>>
        CreateScientistCasteTraitPairs()
    {
        var pairs = new Dictionary<string, IReadOnlyList<ModuleEffect>>
        {
            ["Fast Learner and Combat Paralysis"] =
                [Trait("Fast Learner", 75), Trait("Combat Paralysis", -75)]
        };
        foreach (var interest in Interests)
        {
            pairs[$"Natural Aptitude/{interest} and Dark Secret"] =
                [Trait($"Natural Aptitude/{interest}", 75), Trait("Dark Secret", -75)];
        }
        foreach (var science in new[]
                 {
                     "Science/Biology", "Science/Chemistry", "Science/Mathematics",
                     "Science/Physics"
                 })
        {
            pairs[$"Natural Aptitude/{science} and Dark Secret"] =
                [Trait($"Natural Aptitude/{science}", 75), Trait("Dark Secret", -75)];
        }
        return pairs;
    }

    private static ModuleChoice ConditionalChoice(
        string id, string label,
        IReadOnlyDictionary<string, IReadOnlyList<ModuleEffect>> optionEffects) =>
        new(id, label, EffectTarget.None, 0, 1, optionEffects.Keys.ToArray(),
            OptionEffects: optionEffects);

    private static ModuleChoice FlexibleChoice(
        string id, string label, int xp, int count, int? attributeMaximumXp = null,
        int minimumAttributeOrTraitXp = 0) =>
        Choice(id, label, EffectTarget.Flexible, xp, count,
            Attributes.Concat(FlexibleTraits).Concat(FlexibleSkills).ToArray())
        with
        {
            AttributeMaximumXp = attributeMaximumXp,
            MinimumAttributeOrTraitXp = minimumAttributeOrTraitXp,
            FixedFlexibleSelections = true
        };

    private static ModuleChoice FlexiblePoolChoice(
        string id, string label, int xp, int count = 1,
        int? attributeMaximumXp = 200,
        int? traitMaximumXp = 200,
        int? skillMaximumXp = 35,
        int minimumAttributeOrTraitXp = 0) =>
        Choice(id, label, EffectTarget.Flexible, xp, count,
            Attributes.Concat(FlexibleTraits).Concat(FlexibleSkills).ToArray())
        with
        {
            AttributeMaximumXp = attributeMaximumXp,
            TraitMaximumXp = traitMaximumXp,
            SkillMaximumXp = skillMaximumXp,
            MinimumAttributeOrTraitXp = minimumAttributeOrTraitXp
        };

    private static ModuleChoice FlexibleAttributeOrTraitChoice(
        string id, string label, int xp, int count, int? attributeMaximumXp = null,
        int minimumAttributeOrTraitXp = 0) =>
        Choice(id, label, EffectTarget.Flexible, xp, count,
            Attributes.Concat(FlexibleTraits).ToArray())
        with
        {
            AttributeMaximumXp = attributeMaximumXp,
            MinimumAttributeOrTraitXp = minimumAttributeOrTraitXp,
            FixedFlexibleSelections = true
        };

    private static ModuleChoice Choice(
        string id, string label, EffectTarget target, int xp, int count, IReadOnlyList<string> options) =>
        new(id, label, target, xp, count, options);

    private static ModuleEffect Attribute(string name, int xp) => new(EffectTarget.Attribute, name, xp);
    private static ModuleEffect Skill(string name, int xp) => new(EffectTarget.Skill, name, xp);
    private static ModuleEffect Trait(string name, int xp) => new(EffectTarget.Trait, name, xp);
    private static ModuleEffect PreAttribute(string name, int xp) => new(EffectTarget.PreAttribute, name, xp);
    private static ModuleEffect PreSkill(string name, int xp) => new(EffectTarget.PreSkill, name, xp);
    private static ModuleEffect PreTrait(string name, int xp) => new(EffectTarget.PreTrait, name, xp);
}
