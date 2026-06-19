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

Before a packaged release, run:

```powershell
dotnet run --project tests\BattletechCharacterCreator.Tests
dotnet run --project src\BattletechCharacterCreator.App -- --smoke-inventory
dotnet run --project src\BattletechCharacterCreator.App -- --smoke-sheet-export=artifacts\smoke-sheet-export.pdf
dotnet publish src\BattletechCharacterCreator.App /p:PublishProfile=win-x64-folder
makensis /DVERSION=0.1.0-preview niss\atow_setup.nsi
```
