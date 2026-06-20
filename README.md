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
light support vehicles that fit the current catalog model. Companion expanded
trait reference entries are also available behind the Companion toggle.

Rules are verified against *A Time of War: The BattleTech RPG, Corrected Third
Printing*. See `docs/RULES_SOURCE.md` for the authoritative page map and
migration policy. Optional material from *A Time of War Companion* is tracked in
`docs/COMPANION_AUDIT.md`. Local Era Digest and Era Report references are
tracked in `docs/ERA_SOURCE_AUDIT.md` for future era-aware defaults and
campaign context. The original Qt application is an implementation reference,
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

The .NET migration is approximately **99% complete**. There is not yet a stable
release. Automated release checks and installer smoke tests are passing; the
next gate is manual preview testing with `docs/MANUAL_TEST_PLAN.md`.

### Completed

- .NET 10 and WPF application foundation
- First-launch choice window for opening the Character Wizard or Character
  Editor
- Character wizard from basic information through Stage 4
- Basic information uses year of birth and game year, with age calculated for
  summaries and exports
- Era presets set the game year from the local Era Digest and Era Report
  source audit
- All affiliations and 68 corrected sub-affiliations
- Life-path effects, prerequisites, repeat rules, and flexible XP allocation
- Character editor with guided Attribute, Trait, and Skill XP controls
- Searchable equipment and weapon catalogs with visible quantity-aware
  base-price totals, purchased patch and ammo totals, and unresolved-price counts
- Equipment and weapon catalog filters for faster core and Companion inventory
  selection
- Selected equipment and weapon detail panels showing source, cost, mass, and
  combat notes before adding items
- Inventory status warnings for over-budget, overloaded, and manually priced wildcard items
- Optional Companion catalog toggle with source-tagged imported equipment,
  weapons, implants, cybernetics, prosthetics, prosthetic enhancements, and
  cosmetic adaptation kits, plus advanced combat practice equipment, light
  support vehicles, and expanded trait references
- Skill and Trait editor reference panels with source labels and rule notes
- MW3-to-AToW conversion skill targets from the Companion conversion table
- Local Era Digest and Era Report source audit for era-aware behavior
- Legacy `.btcc` character save and load compatibility
- Official character-sheet PDF preview and export, including purchased patch
  and ammo inventory details
- Error report generation for unexpected UI failures and recoverable editor
  operation failures
- Error reports include app version, runtime, process, and launch diagnostics
- Release checks validate diagnostic report versions against the requested
  release version
- Release checks verify the app project version matches the requested release
  version
- Release checks verify Windows assembly/file versions match the release
  version number
- Windows x64 folder publish profile documented in `docs/RELEASE.md`
- Locally compile-verified per-user NSIS installer script for the .NET publish
  output
- Installer smoke-test script for install, launch diagnostics, and uninstall
  verification, including installed-app sheet export
- Full installer install, launch-smoke, and uninstall verification
- Automated release-check script covering tests, start window, app smokes,
  sheet export output, publish, and installer dry-run
- Preview release packaging script with installer checksum, stale-installer
  guard, and manifest output
- Preview release notes included in packaged release artifacts
- GitHub-ready preview release draft included in packaged release artifacts
- GitHub release publish helper with package, checksum, manifest, and installer
  metadata/version/notes validation
- Manual preview test plan in `docs/MANUAL_TEST_PLAN.md`
- Automated tests covering major Inner Sphere, Periphery, ComStar, and Clan paths

### Remaining

- Run the manual preview test plan on the installed app
- Continue importing and modeling selected optional mechanics from
  *A Time of War Companion*
- Add deeper era-aware notes and availability filters from the local Era Digest
  and Era Report source audit
- Expand reload, patch repair rules, ammunition modifier, vehicle, and prosthetic enhancement
  purchasing details
- Continue interface polish and usability testing
- Continue strengthening error handling and recovery
- Complete the final rulebook audit
- Publish the prepared preview release on GitHub after account authentication

## License
Only for Non-commercial use.
Battletech Character Creator is free software, and is released under the terms of the Creative Commons license version 3 or (at your option) any later version. 
See https://creativecommons.org/
