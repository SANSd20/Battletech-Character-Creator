# Release Notes

The .NET app can be published as a Windows x64 folder build from the repository
root:

```powershell
dotnet publish src\BattletechCharacterCreator.App `
  /p:PublishProfile=win-x64-folder `
  /p:UseSharedCompilation=false
```

The publish output is written to:

```text
artifacts\publish\win-x64
```

This profile creates a framework-dependent build. Test machines need the .NET
10 Desktop Runtime installed.

After publishing, build the per-user NSIS installer with:

```powershell
makensis /DVERSION=0.1.5-preview niss\atow_setup.nsi
```

If `makensis` is not on `PATH`, the default NSIS install location can be used
directly:

```powershell
& 'C:\Program Files (x86)\NSIS\makensis.exe' /DVERSION=0.1.5-preview niss\atow_setup.nsi
```

The beta installer uses the published folder as input, installs under
`%LOCALAPPDATA%\A Time of War Character Creator Beta`, creates Start Menu
shortcuts under `A Time of War Character Creator Beta`, and registers a
per-user beta uninstaller.

To smoke-test the built installer on a normal Windows session:

```powershell
.\scripts\Test-Installer.ps1
```

In restricted environments, use dry-run mode to validate paths and commands
without executing the installer. Dry-run mode can be used before the installer
has been rebuilt; it reports the planned checks even when the installer file is
not present yet:

```powershell
.\scripts\Test-Installer.ps1 -DryRun
```

Before a packaged release, run the automated release check:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\Run-ReleaseChecks.ps1
```

The release check verifies that the character-sheet export smoke creates a
nonempty PDF and does not write an export error report. It also validates that
the app project informational, assembly, and file versions match the requested
release version, that smoke-test diagnostic reports include that version, and
that the installer dry-run still covers installed start-window, diagnostic,
sheet export, and uninstall checks. To avoid stale build-output file locks, it
also closes `BattletechCharacterCreator.App` processes launched from this
repository around app smoke, build, and publish steps. App smoke steps use a
single pre-smoke app build and then run without rebuilding between launches.
The installed start-window smoke validates compiled WPF resources for the
start-screen XAML, image, and font because those assets are embedded in the
published app rather than copied as loose files.
The wizard smoke is headless and intentionally focused on era-aware behavior and a
representative character path; broader path coverage is handled by the
representative life paths smoke.
The character-sheet export smoke is also headless and exports from a
representative character without constructing wizard or editor windows.

Then, outside restricted sandboxes, run the full installer install/uninstall
smoke:

```powershell
.\scripts\Test-Installer.ps1
```

Status note: the full installer smoke test passed on July 7, 2026. Repeat it
before each packaged release after rebuilding the installer. The smoke test
also checks that the installed app can validate the start window assets, write
diagnostic metadata to its launch report, and export a nonempty character-sheet
PDF.

After automated checks pass, run the installed-app manual preview checklist:

```text
docs\MANUAL_TEST_PLAN.md
```

To create a dated manual test run log from that checklist:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\New-ManualTestRun.ps1
```

The run log is written under `artifacts\manual-tests` with the current commit,
version, tester, installer path, and checklist copied in for notes and results.

Package the preview release with:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\Build-PreviewRelease.ps1
```

Run this from a clean working tree. The package script records the current Git
commit in the release manifest and stops if uncommitted changes are present.
When `-SkipReleaseChecks` is used, the script also verifies that the installer
was rebuilt after the current commit unless `-AllowStaleInstaller` is passed for
an intentional repackaging.

The package script writes the installer, a SHA-256 checksum, preview release
notes, a GitHub release draft with the current installer hash injected, and a
manifest to:

```text
artifacts\release\0.1.5-preview
```

The packaged installer and checksum include the current short Git commit in
their filenames, for example:

```text
atow-character-creator-0.1.5-preview-<commit>-setup.exe
atow-character-creator-0.1.5-preview-<commit>-setup.exe.sha256
```

Validate the prepared GitHub release without publishing it:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\Publish-GitHubRelease.ps1 -DryRun
```

To publish the preview with GitHub CLI after signing in with `gh auth login`:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\Publish-GitHubRelease.ps1
```

If the prerelease already exists and you want to refresh its notes and attached
assets from the current package, rerun with:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\Publish-GitHubRelease.ps1 -UpdateExisting
```

The publish helper checks that the commit-suffixed installer, checksum, release
notes, GitHub release draft, and manifest are present. It also verifies that the
checksum matches the installer, that the preview notes and GitHub release draft
include the requested version, that the draft includes the current installer
hash, and that the manifest was generated from the current clean commit and
requested version with matching installer name, hash, size, and build timestamp
before creating the prerelease. If `-UpdateExisting` is used, the helper updates
the existing prerelease notes and uploads the current assets with `--clobber`.
