# A Time of War Character Creator 0.1.16-preview Beta

This is a beta refresh of the .NET/WPF migration of the original BattleTech
Character Creator. It is ready for broader manual testing and feedback, but it
is not the final stable release.

## Download

Attach these files from `artifacts\release\0.1.16-preview`:

- `atow-character-creator-0.1.16-preview-<commit>-setup.exe`
- `atow-character-creator-0.1.16-preview-<commit>-setup.exe.sha256`
- `PREVIEW_RELEASE_NOTES.md`
- `release-manifest.txt`

## Requirements

- Windows
- .NET 10 Desktop Runtime

## Install Location

This beta installs under `%LOCALAPPDATA%\A Time of War Character Creator Beta`
and registers a separate beta uninstaller.

## Verification

See `release-manifest.txt` for the exact packaged commit.

SHA-256:

```text
{{INSTALLER_SHA256}}
```

Before packaging, the automated release checks passed:

- Migration tests
- Start window smoke
- Wizard smoke
- Clan round-trip smoke
- Representative life-path smoke
- Editor allocation smoke
- Inventory smoke
- Character-sheet export smoke
- Error report smokes
- Solution build
- Folder publish
- NSIS installer build
- Installer dry-run

The full installer install, launch-smoke, and uninstall test passed on
July 7, 2026.

Manual installed-app testing should follow `docs/MANUAL_TEST_PLAN.md`.

## Highlights

- Free XP spending now uses one unified target dropdown for Attributes, Traits, and Skills
- The Free XP allocation list now shows the target type alongside each spend
- Module flexible XP must now be fully assigned before advancing to the next Life Module, matching the page 61 spending rule
- Stage 2 running Free XP now visibly subtracts the selected late-childhood module cost
- Wizard age calculation now starts from age 16 after late childhood and adds only selected Stage 3 education time and Stage 4 career time
- Stage 3 now supports repeated education selections, including second and
  third education entries for paths such as University plus Officer Candidate
  School
- Education prerequisite checks now account for all selected education fields
- Equipment and weapon catalogs now include category filters in addition to
  text search
- Life Module character wizard from basic information through Stage 4
- First-launch choice window for opening the Character Wizard or Character Editor
- Basic information uses campaign year, with age calculated from life-path choices
- Campaign year infers the matching era from the imported `Eras.xlsx` chronology
- Era-aware affiliation availability filters with visible source notes
- Era-aware Rasalhague sub-affiliation filters with visible source notes
- Pre-invasion eras after the Golden Century show both Invading Clan and
  Homeworld Clan origins for Clan characters
- Era quick-start templates for common campaign starts in the editor
- All corrected-printing affiliations and 68 sub-affiliations
- All 24 corrected-printing Stage 4 base modules, including variants
- Two ordered Stage 4 careers with repeat and prerequisite handling
- Stage 2, Stage 3, and Stage 4 flexible XP pools preserve user allocations
  while navigating between wizard pages
- Flexible XP target dropdowns group Attributes first, then Traits, then Skills
- Stage 1 flexible XP target lists include cataloged Compulsion traits
- Wizard character totals remain visible while flexible XP pools still have
  unallocated or overallocated XP
- Review Character keeps the last valid totals during temporary wizard refreshes
- Review Character can spend remaining Free XP to fix Attribute, Trait, and Skill prerequisite gaps
- Wizard now has a dedicated Free XP step before final Review, with rule-check fixes moved there
- Free XP step can manually spend remaining XP on selected Traits and Skills
- Character editor with XP controls for attributes, skills, and traits, including
  5 XP and 10 XP adjustment buttons
- Legacy `.btcc` save/load compatibility
- Searchable equipment and weapon catalogs with C-Bill and mass summaries
- Equipment and weapon catalog filters for faster inventory selection
- Equipment and weapon catalog dropdowns group results by category for easier scanning
- Equipment and weapon catalog filters show visible result counts while browsing
- Selected equipment and weapon detail panels for source and rules context
- Selected equipment and weapon detail panels show campaign-year era warnings
- Inventory status warnings for over-budget, overloaded, manually priced wildcard items,
  armor patches that need patch pricing, ammo purchases that need ammo cost or
  mass details, and prosthetic enhancements that need a prosthetic or implant host
- Inventory status warnings for ammo purchases that need reload or power-pack review
- Inventory status warnings for vehicle purchases that need Vehicle or Custom Vehicle
  trait support
- Inventory status warnings now name the inventory rows that need pricing, ammo,
  reload, prosthetic host, or vehicle trait attention
- Weapon inventory rows now support purchased ammo modifier names plus per-pack
  modifier cost and mass adjustments
- Headless inventory smoke checks catalog and inventory-rule behavior without
  constructing the editor window
- Headless character-sheet export smoke checks PDF output without constructing
  wizard/editor windows
- Opt-in *A Time of War Companion* equipment and weapon catalog content
- Opt-in *A Time of War Companion* expanded trait reference entries
- Skill and Trait editor reference panels with source labels and rule notes
- MW3-to-AToW conversion skill targets from the Companion conversion table
- Local Era Digest and Era Report source audit for era-aware behavior
- Official character-sheet PDF preview and export
- Release checks verify that character-sheet export produces a nonempty PDF
- Error report generation for unexpected and recoverable app failures
- Error reports include app version, runtime, process, and launch diagnostics
- Wizard smoke failures now write an error report and exit cleanly
- Release checks validate diagnostic report versions against the requested version
- Release checks verify the app project version matches the requested version
- Release checks verify Windows assembly/file versions match the release version
- Release checks verify the installer dry-run still covers installed start,
  diagnostic, sheet export, and uninstall checks
- Release checks close repo-launched app instances around app smoke, build, and
  publish steps to prevent stale file locks
- Release checks build once before app smoke steps and avoid rebuilding between
  smoke launches
- Wizard smoke headlessly validates era-aware wizard behavior and a representative
  character path without the slower exhaustive UI selection sweep
- Start-window smoke runs headlessly before WPF startup so release automation
  validates launch choices without hanging on window construction
- Start-window smoke validates compiled WPF start-screen resources in installed
  builds
- Installer smoke validates the installed start-window choices and required
  launch-screen assets
- Installer smoke validates diagnostic metadata from the installed app
- Installer smoke validates character-sheet PDF export from the installed app
- Per-user Windows installer
- Release packaging guard against stale installers when release checks are skipped
- Release package filenames include the packaged build commit
- GitHub release validation cross-checks manifest installer metadata
- GitHub release validation cross-checks the packaged release version
- GitHub release validation checks the draft notes match the requested version
- GitHub release validation checks preview notes match the requested version
- Manual preview test plan and generated run logs for installed-app testing

## Known Gaps

- Remaining optional *A Time of War Companion* content is still being modeled
- Deeper reload behavior, deeper patch repair, ammunition modifier rules, and
  deeper vehicle purchasing rules need richer first-class support
- Interface polish and usability testing are still ongoing
- The final rulebook audit is not complete
