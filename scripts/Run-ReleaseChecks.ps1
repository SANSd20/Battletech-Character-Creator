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

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

$appData = Join-Path $repoRoot ".appdata"
New-Item -ItemType Directory -Force -Path $appData | Out-Null
$env:APPDATA = $appData

$sheetExportPath = Join-Path $appData "release-smoke-sheet-export.pdf"
$errorReportPath = Join-Path $appData "release-smoke-error-report.txt"
$operationReportPath = Join-Path $appData "release-smoke-operation-error-report.txt"

Remove-Item -LiteralPath $sheetExportPath -ErrorAction SilentlyContinue
Remove-Item -LiteralPath ($sheetExportPath + ".error.txt") -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $errorReportPath -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $operationReportPath -ErrorAction SilentlyContinue

Invoke-Step "Migration tests" {
    dotnet run --project tests\BattletechCharacterCreator.Tests `
        /p:UseSharedCompilation=false
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
}

Invoke-Step "Error report smoke" {
    dotnet run --project src\BattletechCharacterCreator.App `
        /p:UseSharedCompilation=false -- --smoke-error-report=$errorReportPath
    if (!(Test-Path -LiteralPath $errorReportPath)) {
        throw "Error report smoke output was not created."
    }
    $report = Get-Content -LiteralPath $errorReportPath -Raw
    if (!$report.Contains("Version: 0.1.0-preview") -or
        !$report.Contains("Process architecture:") -or
        !$report.Contains("Command line:")) {
        throw "Error report smoke output did not include diagnostic metadata."
    }
}

Invoke-Step "Operation report smoke" {
    dotnet run --project src\BattletechCharacterCreator.App `
        /p:UseSharedCompilation=false -- --smoke-operation-error-report=$operationReportPath
    if (!(Test-Path -LiteralPath $operationReportPath)) {
        throw "Operation report smoke output was not created."
    }
    $report = Get-Content -LiteralPath $operationReportPath -Raw
    if (!$report.Contains("Version: 0.1.0-preview") -or
        !$report.Contains("Process architecture:") -or
        !$report.Contains("Command line:")) {
        throw "Operation report smoke output did not include diagnostic metadata."
    }
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
        powershell -NoProfile -ExecutionPolicy Bypass `
            -File scripts\Test-Installer.ps1 -DryRun `
            -InstallerPath "niss\atow-character-creator-$Version-setup.exe"
    }
}

Write-Host ""
Write-Host "Release checks passed."
