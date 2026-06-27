param(
    [string]$Version = "0.1.0-preview",
    [switch]$SkipInstallerBuild
)

$ErrorActionPreference = "Stop"

function Invoke-Step([string]$Name, [scriptblock]$Action) {
    Write-Host ""
    Write-Host "== $Name =="
    & $Action
}

function Find-MakeNsis {
    $command = Get-Command makensis -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $defaultPath = "C:\Program Files (x86)\NSIS\makensis.exe"
    if (Test-Path -LiteralPath $defaultPath) {
        return $defaultPath
    }

    throw "makensis was not found. Install NSIS or pass -SkipInstallerBuild."
}

function Assert-DiagnosticReport([string]$Path, [string]$Name) {
    if (!(Test-Path -LiteralPath $Path)) {
        throw "$Name output was not created."
    }
    $report = Get-Content -LiteralPath $Path -Raw
    if (!$report.Contains("Version: $Version") -or
        !$report.Contains("Process architecture:") -or
        !$report.Contains("Command line:")) {
        throw "$Name output did not include diagnostic metadata for version $Version."
    }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

$appProjectPath = Join-Path $repoRoot "src\BattletechCharacterCreator.App\BattletechCharacterCreator.App.csproj"
[xml]$appProject = Get-Content -LiteralPath $appProjectPath
$appVersion = $appProject.Project.PropertyGroup.InformationalVersion
if ($appVersion -ne $Version) {
    throw "App InformationalVersion ($appVersion) does not match release version ($Version)."
}
$numericVersion = ($Version -split "-", 2)[0]
$expectedAssemblyVersion = "$numericVersion.0"
$assemblyVersion = $appProject.Project.PropertyGroup.AssemblyVersion
$fileVersion = $appProject.Project.PropertyGroup.FileVersion
if ($assemblyVersion -ne $expectedAssemblyVersion) {
    throw "App AssemblyVersion ($assemblyVersion) does not match expected release assembly version ($expectedAssemblyVersion)."
}
if ($fileVersion -ne $expectedAssemblyVersion) {
    throw "App FileVersion ($fileVersion) does not match expected release file version ($expectedAssemblyVersion)."
}

$appData = Join-Path $repoRoot ".appdata"
New-Item -ItemType Directory -Force -Path $appData | Out-Null
$env:APPDATA = $appData

$sheetExportPath = Join-Path $appData "release-smoke-sheet-export.pdf"
$sheetExportErrorPath = $sheetExportPath + ".error.txt"
$errorReportPath = Join-Path $appData "release-smoke-error-report.txt"
$operationReportPath = Join-Path $appData "release-smoke-operation-error-report.txt"

Remove-Item -LiteralPath $sheetExportPath -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $sheetExportErrorPath -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $errorReportPath -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $operationReportPath -ErrorAction SilentlyContinue

Invoke-Step "Migration tests" {
    dotnet run --project tests\BattletechCharacterCreator.Tests `
        /p:UseSharedCompilation=false
}

Invoke-Step "Start window smoke" {
    dotnet run --project src\BattletechCharacterCreator.App `
        /p:UseSharedCompilation=false -- --smoke-start
}

Invoke-Step "Wizard smoke" {
    dotnet run --project src\BattletechCharacterCreator.App `
        /p:UseSharedCompilation=false -- --smoke-wizard
}

Invoke-Step "Clan round-trip smoke" {
    dotnet run --project src\BattletechCharacterCreator.App `
        /p:UseSharedCompilation=false -- --smoke-clan-roundtrip
}

Invoke-Step "Representative life paths smoke" {
    dotnet run --project src\BattletechCharacterCreator.App `
        /p:UseSharedCompilation=false -- --smoke-complete-life-paths
}

Invoke-Step "Editor allocation smoke" {
    dotnet run --project src\BattletechCharacterCreator.App `
        /p:UseSharedCompilation=false -- --smoke-editor-allocation
}

Invoke-Step "Inventory smoke" {
    dotnet run --project src\BattletechCharacterCreator.App `
        /p:UseSharedCompilation=false -- --smoke-inventory
}

Invoke-Step "Sheet export smoke" {
    dotnet run --project src\BattletechCharacterCreator.App `
        /p:UseSharedCompilation=false -- --smoke-sheet-export=$sheetExportPath
    if (Test-Path -LiteralPath $sheetExportErrorPath) {
        $exportError = Get-Content -LiteralPath $sheetExportErrorPath -Raw
        throw "Sheet export smoke wrote an error report: $exportError"
    }
    if (!(Test-Path -LiteralPath $sheetExportPath)) {
        throw "Sheet export smoke output was not created."
    }
    if ((Get-Item -LiteralPath $sheetExportPath).Length -le 0) {
        throw "Sheet export smoke output was empty."
    }
}

Invoke-Step "Error report smoke" {
    dotnet run --project src\BattletechCharacterCreator.App `
        /p:UseSharedCompilation=false -- --smoke-error-report=$errorReportPath
    Assert-DiagnosticReport $errorReportPath "Error report smoke"
}

Invoke-Step "Operation report smoke" {
    dotnet run --project src\BattletechCharacterCreator.App `
        /p:UseSharedCompilation=false -- --smoke-operation-error-report=$operationReportPath
    Assert-DiagnosticReport $operationReportPath "Operation report smoke"
}

Invoke-Step "Solution build" {
    dotnet build BattletechCharacterCreator.sln /p:UseSharedCompilation=false
}

Invoke-Step "Folder publish" {
    dotnet publish src\BattletechCharacterCreator.App `
        /p:PublishProfile=win-x64-folder `
        /p:UseSharedCompilation=false
}

if (!$SkipInstallerBuild) {
    Invoke-Step "NSIS installer build" {
        $makeNsis = Find-MakeNsis
        & $makeNsis "/DVERSION=$Version" niss\atow_setup.nsi
    }

    Invoke-Step "Installer smoke dry-run" {
        $installerDryRunOutput = powershell -NoProfile -ExecutionPolicy Bypass `
            -File scripts\Test-Installer.ps1 -DryRun `
            -InstallerPath "niss\atow-character-creator-$Version-setup.exe" `
            -ExpectedVersion $Version
        if ($LASTEXITCODE -ne 0) {
            throw "Installer smoke dry-run failed with exit code $LASTEXITCODE."
        }
        $installerDryRunOutput | Write-Host
        $requiredDryRunText = @(
            "--smoke-start",
            "--smoke-error-report=",
            "--smoke-sheet-export=",
            "Would verify smoke report diagnostic metadata",
            "Would uninstall:"
        )
        $installerDryRunText = $installerDryRunOutput -join "`n"
        foreach ($text in $requiredDryRunText) {
            if (!$installerDryRunText.Contains($text)) {
                throw "Installer smoke dry-run did not include planned check: $text"
            }
        }
    }
}

Write-Host ""
Write-Host "Release checks passed."
