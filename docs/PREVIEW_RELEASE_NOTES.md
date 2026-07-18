# A Time of War Character Creator 0.1.28-preview Beta

This is a beta refresh of the .NET/WPF migration. It is intended for manual
testing, feedback, and continued rule audit work, not as the final stable
release.

## Highlights

- Free XP spending now uses one unified target dropdown for Attributes, Traits, and Skills
- The Free XP allocation list now shows the target type alongside each spend
- Free XP allocation rows can now be removed individually without resetting all Free XP spending
- Free XP target choices now hide targets already assigned on the Free XP step and restore them after removal
- Free XP target choices now include current character Traits and Skills even
  when they come from generated, imported, or life-module-specific names
- Free XP rule-check fixes now navigate back to the relevant wizard stage for
  non-XP prerequisites, including Education issues returning to Stage 3
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
- Stage 3 education interest choices hide Clan-history options from pre-invasion
  Inner Sphere characters
- Era-aware Rasalhague sub-affiliation filters with visible source notes
- Pre-invasion eras after the Golden Century show both Invading Clan and
  Homeworld Clan origins for Clan characters
- Era quick-start templates for common campaign starts in the editor
- All corrected-printing affiliations and 68 sub-affiliations
- All 24 corrected-printing Stage 4 base modules, including variant paths
- Two ordered Stage 4 careers with repeat and prerequisite handling
- Stage 4 career dropdowns show candidate careers while prerequisite gaps are summarized on the Stage 4 screen
- Stage 4 now shows a compact visible list of unmet career prerequisite details once a career slot is enabled
- Stage 4 career field-skill dropdowns now fall back to the allowed field-skill pool instead of appearing blank when the character qualifies through an alternate prerequisite route
- Stage 4 character totals no longer reuse the previous career totals during career-switch refreshes
- Stage 4 character totals stay visible for the newly selected career while that career's flexible XP is still being assigned
- Stage 2, Stage 3, and Stage 4 flexible XP pools preserve user allocations
  while navigating between wizard pages
- Flexible XP target dropdowns group Attributes first, then Traits, then Skills
- Flexible XP target dropdowns hide targets already selected in the same pool
- Stage 1 flexible XP target lists include cataloged Compulsion traits
- Wizard character totals remain visible while flexible XP pools still have
  unallocated or overallocated XP
- Review Character keeps the last valid totals during temporary wizard refreshes
- Review Character can spend remaining Free XP to fix Attribute, Trait, and Skill prerequisite gaps
- Wizard now has a dedicated Free XP step before final Review, with rule-check fixes moved there
- Free XP step can manually spend remaining XP on selected Attributes, Traits, and Skills
- Character editor with XP controls for attributes, skills, and traits, including
  5 XP and 10 XP adjustment buttons
- Character editor Rule Check now shows each prerequisite's missing XP gap
  alongside required and current XP
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
- Core-rulebook Power Packs and Rechargers are now imported into the equipment
  catalog, including standard, Clan, high-capacity, quick-charge, and support
  PPC power packs
- Simple power-pack weapon rows now clear the reload/power-pack warning once
  the character owns a power pack, satchel battery, or portable power unit
- Inventory status warnings for vehicle purchases that need Vehicle or Custom Vehicle
  trait support
- Inventory status warnings now name the inventory rows that need pricing, ammo,
  reload, prosthetic host, or vehicle trait attention
- Weapon inventory rows now support purchased ammo modifier names plus per-pack
  modifier cost and mass adjustments
- Specialty ammunition modifiers from the core rulebook can now be selected
  and applied to weapon inventory rows
- Specialty ammunition choices now filter by the selected weapon row category
  and weapon category survives save/load
- Specialty ammunition filtering now honors rifle-only, shotgun-only,
  and support-Gauss restrictions from the modifier table
- Specialty ammunition now tracks support gear requirements for Air-Burst,
  Guided Gyrojet, and Radioactive Tracker ammunition
- Inventory status warns when specialty ammunition purchases are missing
  required support gear and clears once the matching support item is present
- Guided Rifle Modules and Radioactive Tracker Scanner are now imported as core
  equipment catalog entries so specialty ammo support warnings can be resolved
  through normal inventory purchases
- Applied specialty ammunition now stores AP/BD and range effects directly on
  weapon inventory rows
- Character-sheet export includes applied specialty ammunition AP/BD and range
  effect notes in weapon inventory details
- Applied specialty ammunition now calculates effective AP/BD for simple numeric
  modifiers and effective range for half-range ammunition
- Weapon inventory rows now preserve calculated specialty ammunition effective
  damage and range through save/load
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
- Preview packaging now stops immediately if release checks fail
- Release checks verify the installer dry-run still covers installed start,
  diagnostic, sheet export, and uninstall checks
- Release checks close repo-launched app instances around app smoke, build, and
  publish steps to prevent stale file locks
- Release checks build once before app smoke steps and avoid rebuilding between
  smoke launches
- Wizard smoke headlessly validates era-aware wizard behavior and a representative
  character path without the slower exhaustive UI selection sweep
- Affiliation-filtered childhood, Clan round-trip, and editor allocation smokes
  now run headlessly to avoid local WPF window/font-cache startup failures
  during release checks
- Start-window smoke runs headlessly before WPF startup so release automation
  validates launch choices without hanging on window construction
- Start-window smoke validates compiled WPF start-screen resources in installed
  builds
- Installer smoke validates the installed start-window choices and required
  launch-screen assets
- Installer smoke validates diagnostic metadata from the installed app
- Installer smoke validates character-sheet PDF export from the installed app
- Per-user Windows installer and release package manifest/checksum output
- Beta installer uses `%LOCALAPPDATA%\A Time of War Character Creator Beta`
  with separate beta Start Menu and uninstall entries
- Release packaging guard against stale installers when release checks are skipped
- Release package filenames include the packaged build commit
- GitHub release validation cross-checks manifest installer metadata
- GitHub release validation cross-checks the packaged release version
- GitHub release validation checks the draft notes match the requested version
- GitHub release validation checks preview notes match the requested version
- Manual preview test plan and generated run logs for installed-app testing

## Verification

Before this preview was packaged, the automated release checks passed:

- Migration tests
- Start window smoke
- Wizard smoke
- Stage preview smoke
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
June 18, 2026.

Manual installed-app testing should follow `docs/MANUAL_TEST_PLAN.md`.

## Known Gaps

- Remaining optional *A Time of War Companion* content is still being modeled
- Deeper mixed-reload behavior, deeper patch repair, broader ammunition effect math, and
  deeper vehicle purchasing rules need richer first-class support
- Interface polish and usability testing are still ongoing
- The final rulebook audit is not complete
