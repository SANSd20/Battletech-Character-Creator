# Battletech Character Creator

## Modern .NET migration

A new WPF implementation is being developed in `src/`. It targets .NET 10 and
keeps compatibility with existing `.btcc` character files. The original Qt
application remains in the repository as a reference during migration.

The new Create flow includes affiliations, early and late childhood, higher
education, basic, advanced, and specialist education fields, and all 46
Stage 4 real-life module variants, representing all 24 base modules in the
corrected printing. Module effects, choices, prerequisites, repeat restrictions,
and module-specific repeat penalties are implemented in the Core library.
The wizard supports two ordered careers, including legal repeat modules and
career-to-career prerequisites.
ComStar and Word of Blake characters also select a full-cost birth affiliation.
All 68 corrected Stage 0 sub-affiliation XP packages are included.
The legacy equipment, weapon, skill, trait, career, subskill, and description
catalogs are imported as structured .NET data for the editor.

Rules are verified against *A Time of War: The BattleTech RPG, Corrected Third
Printing*. See `docs/RULES_SOURCE.md` for the authoritative page map and
migration policy. The original Qt application is an implementation reference,
not the final authority when it conflicts with the corrected rulebook.

Build the new application with:

```powershell
dotnet build BattletechCharacterCreator.sln
dotnet run --project src/BattletechCharacterCreator.App
```

Run the dependency-free migration checks with:

```powershell
dotnet run --project tests/BattletechCharacterCreator.Tests
```

The program is created according to the rules “BATTLETECH A Time of War” and include Wizard for Life Modules and 
character editor. It allows you to quickly and easily create a new character to play Battletech RPG.

## Last Stable Version is 0.8.9
Development is closed now. Last stable version is 0.8.9.

## Download
You can download install file from releases.
## Installation
Run file as Administrator.
Next, Next, Next, Enjoy

## License
Only for Non-commercial use.
Battletech Character Creator is free software, and is released under the terms of the Creative Commons license version 3 or (at your option) any later version. 
See https://creativecommons.org/
