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
10 Desktop Runtime installed. A future installer should use this folder as its
input, create Start Menu shortcuts for `BattletechCharacterCreator.App.exe`,
and include the bundled `Resources` and `Assets` folders produced by publish.

Before a packaged release, run:

```powershell
dotnet run --project tests\BattletechCharacterCreator.Tests
dotnet run --project src\BattletechCharacterCreator.App -- --smoke-inventory
dotnet run --project src\BattletechCharacterCreator.App -- --smoke-sheet-export=artifacts\smoke-sheet-export.pdf
dotnet publish src\BattletechCharacterCreator.App /p:PublishProfile=win-x64-folder
```
