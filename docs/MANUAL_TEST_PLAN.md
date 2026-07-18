# Manual Preview Test Plan

Use this checklist after `scripts\Run-ReleaseChecks.ps1`,
`scripts\Test-Installer.ps1`, and `scripts\Publish-GitHubRelease.ps1 -DryRun`
pass.

## Install and Launch

- Install the packaged `atow-character-creator-0.1.27-preview-<commit>-setup.exe`.
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
- Confirm Stage 3 can add a second or third education entry.
- Confirm Officer Candidate School can be selected after another education path
  when the required basic and advanced fields are present.
- Confirm Stage 4 shows the full first-career candidate list and summarizes
  unmet prerequisites without hiding those careers.
- Confirm changing Stage 4 careers removes the previous career's unique
  Attribute, Trait, and Skill awards from the character totals.
- Confirm selecting a Stage 4 career with unassigned Flexible XP still shows
  the newly selected career's Attribute, Trait, and Skill totals.
- Confirm Covert Operations and other Stage 4 field-skill choices show
  nonblank dropdown options even when prerequisites are being fixed through
  alternate routes.
- Confirm the Free XP page uses one target dropdown for Attributes, Traits, and
  Skills.
- Confirm an assigned Free XP target disappears from the Free XP target dropdown,
  then reappears after removing that allocation row.

## Editor Workflows

- Save and reload a newly created character.
- Load at least one legacy `.btcc` character.
- Increase and decrease one Attribute, one Trait, and one Skill.
- Confirm prerequisite warnings are visible when a required XP target is short.
- Search/filter Skills and confirm unrelated rows are hidden.

## Inventory and Companion Content

- Add and remove one core equipment item and one core weapon.
- Filter the equipment and weapon catalogs and confirm unrelated rows are hidden.
- Confirm the equipment and weapon category filters make the catalog lists easier
  to browse.
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

## Imported GitHub Issues

- Review `docs\ISSUE_AUDIT.md`.
- Confirm every issue marked "Implemented locally" has a matching manual-test
  result.
- Close or update the corresponding GitHub issue after the installed-app test
  confirms the workflow.
