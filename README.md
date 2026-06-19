# Battletech Character Creator

## Modern .NET migration

A new WPF implementation is being developed in `src/`. It targets .NET 10 and
keeps compatibility with existing `.btcc` character files. The original Qt
application remains in the repository as a reference during migration.

This project is based on bearchik's original
[BattleTech Character Creator](https://github.com/bearchik/Battletech-Character-Creator).
The original Qt application, data, and design provide the foundation for this
modern .NET migration.

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
Optional *A Time of War Companion* catalog entries are source-tagged and hidden
behind an explicit Companion toggle in the equipment and weapon editor tabs.
Imported Companion content currently includes vintage armor, First SLDF armor
kit entries, archaic, vintage, and advanced personal weapons, fixed-cost
advanced implants and cybernetics, extreme prosthetics, and prosthetic
enhancements, cosmetic adaptation kits, advanced combat practice equipment, and
light support vehicles that fit the current catalog model.

Rules are verified against *A Time of War: The BattleTech RPG, Corrected Third
Printing*. See `docs/RULES_SOURCE.md` for the authoritative page map and
migration policy. Optional material from *A Time of War Companion* is tracked in
`docs/COMPANION_AUDIT.md`. The original Qt application is an implementation
reference, not the final authority when it conflicts with the corrected
rulebook.

Build the new application with:

```powershell
dotnet build BattletechCharacterCreator.sln
dotnet run --project src/BattletechCharacterCreator.App
```

Run the dependency-free migration checks with:

```powershell
dotnet run --project tests/BattletechCharacterCreator.Tests
```

The application provides a Life Module character wizard and a detailed
character editor for *A Time of War*.

## Interface mockups

These development mockups show the current direction of the .NET interface.
Layouts and details may continue to change before the first stable release.

### Start screen

![Start screen mockup](docs/images/start-screen-mockup.png)

### Character wizard

![Stage 0 character wizard mockup](docs/images/wizard-stage0-mockup.png)

### Character editor

![Character editor summary mockup](docs/images/editor-summary-mockup.png)

## Development status

The .NET migration is approximately **94% complete**. There is not yet a stable
or packaged release.

### Completed

- .NET 10 and WPF application foundation
- Character wizard from basic information through Stage 4
- All affiliations and 68 corrected sub-affiliations
- Life-path effects, prerequisites, repeat rules, and flexible XP allocation
- Character editor with guided Attribute, Trait, and Skill XP controls
- Searchable equipment and weapon catalogs with visible quantity-aware
  base-price totals, purchased patch and ammo totals, and unresolved-price counts
- Optional Companion catalog toggle with source-tagged imported equipment,
  weapons, implants, cybernetics, prosthetics, prosthetic enhancements, and
  cosmetic adaptation kits, plus advanced combat practice equipment and light
  support vehicles
- Legacy `.btcc` character save and load compatibility
- Official character-sheet PDF preview and export, including purchased patch
  and ammo inventory details
- Error report generation for unexpected UI failures and recoverable editor
  operation failures
- Windows x64 folder publish profile documented in `docs/RELEASE.md`
- Locally compile-verified per-user NSIS installer script for the .NET publish
  output
- Installer smoke-test script for install, launch, and uninstall verification
- Automated tests covering major Inner Sphere, Periphery, ComStar, and Clan paths

### Remaining

- Continue importing and modeling selected optional content from
  *A Time of War Companion*
- Expand reload, patch repair rules, ammunition modifier, vehicle, and prosthetic enhancement
  purchasing details
- Continue interface polish and usability testing
- Continue strengthening error handling and recovery
- Complete the final rulebook audit
- Run the installer smoke test outside the sandbox on a clean machine and cut a
  packaged release

## License
Only for Non-commercial use.
Battletech Character Creator is free software, and is released under the terms of the Creative Commons license version 3 or (at your option) any later version. 
See https://creativecommons.org/
