# Manual Preview Test Plan

Use this checklist after `scripts\Run-ReleaseChecks.ps1`,
`scripts\Test-Installer.ps1`, and `scripts\Publish-GitHubRelease.ps1 -DryRun`
pass.

## Install and Launch

- Install the packaged `atow-character-creator-0.1.3-preview-<commit>-setup.exe`.
- Launch the app from the Start Menu shortcut.
- Confirm the first window offers Character Wizard and Character Editor.
- Open the Character Editor directly and confirm it starts without an error.
- Open the Character Wizard from the first window and create a character.
- Confirm Basic Information asks for campaign year, not Home planet, year of
  birth, or age.

## Wizard Paths

- Create one Inner Sphere character through Stage 4.
- Create one Periphery character through Stage 4.
- Create one ComStar or Word of Blake character through Stage 4.
- Create one invading Clan character through Stage 4.
- Create one Homeworld Clan character through Stage 4.
- Confirm free XP, attribute totals, trait totals, and skill totals update on
  each stage screen except Basic Information.

## Editor Workflows

- Save and reload a newly created character.
- Load at least one legacy `.btcc` character.
- Increase and decrease one Attribute, one Trait, and one Skill.
- Confirm prerequisite warnings are visible when a required XP target is short.
- Search/filter Skills and confirm unrelated rows are hidden.

## Inventory and Companion Content

- Add and remove one core equipment item and one core weapon.
- Filter the equipment and weapon catalogs and confirm unrelated rows are hidden.
- Select one equipment item and one weapon and confirm their detail panels show
  source, cost, mass, and notes before adding them.
- Confirm C-Bill, mass, and remaining carrying-capacity totals update.
- Add an item with wildcard/manual pricing and confirm the warning is visible.
- Enable Companion content and confirm Companion equipment, weapons, and traits
  are source-tagged as `A Time of War Companion`.
- Disable Companion content and confirm Companion-only catalog rows are hidden.

## Character Sheet

- Preview a character sheet PDF.
- Export a character sheet PDF.
- Confirm purchased armor patches and ammo details appear in exported inventory
  notes when present.

## Error and Recovery

- Confirm no unexpected application error appears during normal use.
- If an error appears, save the generated report with the app version and
  command-line diagnostics intact.
- Uninstall the app and confirm the installed executable is removed.
