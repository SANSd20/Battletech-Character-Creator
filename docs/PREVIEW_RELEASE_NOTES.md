# A Time of War Character Creator 0.1.0-preview

This is the first packaged preview of the .NET/WPF migration. It is intended
for testing, feedback, and continued rule audit work, not as the final stable
release.

## Highlights

- Life Module character wizard from basic information through Stage 4
- First-launch choice window for opening the Character Wizard or Character Editor
- Basic information uses year of birth and game year, with calculated age
- Campaign year infers the matching era from the local Era Digest and Era Report audit
- Era-aware affiliation availability filters with visible source notes
- Era-aware Rasalhague sub-affiliation filters with visible source notes
- All corrected-printing affiliations and 68 sub-affiliations
- All 24 corrected-printing Stage 4 base modules, including variant paths
- Two ordered Stage 4 careers with repeat and prerequisite handling
- Character editor with XP controls for attributes, skills, and traits
- Legacy `.btcc` save/load compatibility
- Searchable equipment and weapon catalogs with C-Bill and mass summaries
- Equipment and weapon catalog filters for faster inventory selection
- Selected equipment and weapon detail panels for source and rules context
- Inventory status warnings for over-budget, overloaded, and manually priced wildcard items
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
- Installer smoke validates diagnostic metadata from the installed app
- Installer smoke validates character-sheet PDF export from the installed app
- Per-user Windows installer and release package manifest/checksum output
- Release packaging guard against stale installers when release checks are skipped
- GitHub release validation cross-checks manifest installer metadata
- GitHub release validation cross-checks the packaged release version
- GitHub release validation checks the draft notes match the requested version
- GitHub release validation checks preview notes match the requested version
- Manual preview test plan for installed-app testing

## Verification

Before this preview was packaged, the automated release checks passed:

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
June 18, 2026.

Manual installed-app testing should follow `docs/MANUAL_TEST_PLAN.md`.

## Known Gaps

- Remaining optional *A Time of War Companion* content is still being modeled
- Reload, patch repair, ammunition modifier, vehicle, and prosthetic purchasing
  rules need richer first-class support
- Interface polish and usability testing are still ongoing
- The final rulebook audit is not complete
