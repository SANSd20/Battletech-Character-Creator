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
makensis /DVERSION=0.1.0-preview niss\atow_setup.nsi
```

If `makensis` is not on `PATH`, the default NSIS install location can be used
directly:

```powershell
& 'C:\Program Files (x86)\NSIS\makensis.exe' /DVERSION=0.1.0-preview niss\atow_setup.nsi
```

The installer uses the published folder as input, installs under the user's
local app data folder, creates Start Menu shortcuts for
`BattletechCharacterCreator.App.exe`, and registers an uninstaller under the
current user.

To smoke-test the built installer on a normal Windows session:

```powershell
.\scripts\Test-Installer.ps1
```

In restricted environments, use dry-run mode to validate paths and commands
without executing the installer:

```powershell
.\scripts\Test-Installer.ps1 -DryRun
```

Before a packaged release, run the automated release check:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\Run-ReleaseChecks.ps1
```

Then, outside restricted sandboxes, run the full installer install/uninstall
smoke:

```powershell
.\scripts\Test-Installer.ps1
```

Status note: the full installer smoke test passed on June 18, 2026. Repeat it
before each packaged release after rebuilding the installer.

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
artifacts\release\0.1.0-preview
```

Validate the prepared GitHub release without publishing it:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\Publish-GitHubRelease.ps1 -DryRun
```

To publish the preview with GitHub CLI after signing in with `gh auth login`:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\Publish-GitHubRelease.ps1
```

The publish helper checks that the installer, checksum, release notes, GitHub
release draft, and manifest are present. It also verifies that the checksum
matches the installer, that the GitHub release draft includes the current
installer hash, and that the manifest was generated from the current clean
commit before creating the prerelease.
