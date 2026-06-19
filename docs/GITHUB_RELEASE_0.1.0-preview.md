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
FC526AF2E16E621538409A5BA41B6F3F1B20BCB7ED721258C2A837520A310284
```

Before packaging, the automated release checks passed:

- Migration tests
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

## Highlights

- Life Module character wizard from basic information through Stage 4
- All corrected-printing affiliations and 68 sub-affiliations
- All 24 corrected-printing Stage 4 base modules, including variants
- Two ordered Stage 4 careers with repeat and prerequisite handling
- Character editor with XP controls for attributes, skills, and traits
- Legacy `.btcc` save/load compatibility
- Searchable equipment and weapon catalogs with C-Bill and mass summaries
- Opt-in *A Time of War Companion* equipment and weapon catalog content
- Official character-sheet PDF preview and export
- Error report generation for unexpected and recoverable app failures
- Per-user Windows installer

## Known Gaps

- Remaining optional *A Time of War Companion* content is still being modeled
- Reload, patch repair, ammunition modifier, vehicle, and prosthetic purchasing
  rules need richer first-class support
- Interface polish and usability testing are still ongoing
- The final rulebook audit is not complete
