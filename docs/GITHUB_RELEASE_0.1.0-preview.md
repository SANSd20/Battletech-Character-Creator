# A Time of War Character Creator 0.1.0-preview

This is the first packaged preview of the .NET/WPF migration of the original
BattleTech Character Creator. It is ready for testing and feedback, but it is
not the final stable release.

## Download

Attach these files from `artifacts\release\0.1.0-preview`:

- `atow-character-creator-0.1.0-preview-setup.exe`
- `atow-character-creator-0.1.0-preview-setup.exe.sha256`
- `PREVIEW_RELEASE_NOTES.md`
- `release-manifest.txt`

## Requirements

- Windows
- .NET 10 Desktop Runtime

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
June 18, 2026.

Manual installed-app testing should follow `docs/MANUAL_TEST_PLAN.md`.

## Highlights

- Life Module character wizard from basic information through Stage 4
- First-launch choice window for opening the Character Wizard or Character Editor
- All corrected-printing affiliations and 68 sub-affiliations
- All 24 corrected-printing Stage 4 base modules, including variants
- Two ordered Stage 4 careers with repeat and prerequisite handling
- Character editor with XP controls for attributes, skills, and traits
- Legacy `.btcc` save/load compatibility
- Searchable equipment and weapon catalogs with C-Bill and mass summaries
- Inventory status warnings for over-budget, overloaded, and manually priced wildcard items
- Opt-in *A Time of War Companion* equipment and weapon catalog content
- Opt-in *A Time of War Companion* expanded trait reference entries
- Skill and Trait editor reference panels with source labels and rule notes
- Official character-sheet PDF preview and export
- Release checks verify that character-sheet export produces a nonempty PDF
- Error report generation for unexpected and recoverable app failures
- Error reports include app version, runtime, process, and launch diagnostics
- Release checks validate diagnostic report versions against the requested version
- Release checks verify the app project version matches the requested version
- Release checks verify Windows assembly/file versions match the release version
- Installer smoke validates diagnostic metadata from the installed app
- Installer smoke validates character-sheet PDF export from the installed app
- Per-user Windows installer
- Release packaging guard against stale installers when release checks are skipped
- GitHub release validation cross-checks manifest installer metadata
- GitHub release validation cross-checks the packaged release version
- GitHub release validation checks the draft notes match the requested version
- GitHub release validation checks preview notes match the requested version
- Manual preview test plan for installed-app testing

## Known Gaps

- Remaining optional *A Time of War Companion* content is still being modeled
- Reload, patch repair, ammunition modifier, vehicle, and prosthetic purchasing
  rules need richer first-class support
- Interface polish and usability testing are still ongoing
- The final rulebook audit is not complete
