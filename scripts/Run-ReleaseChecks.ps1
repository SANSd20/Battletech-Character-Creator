param(
    [string]$Version = "0.1.14-preview",
    [string]$Configuration = "Release",
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

function Stop-RepoAppProcesses([string]$RepoRoot, [string]$Reason) {
    $repoPrefix = [System.IO.Path]::GetFullPath($RepoRoot)
    $processes = Get-Process BattletechCharacterCreator.App -ErrorAction SilentlyContinue |
        Where-Object {
            try {
                $_.Path -and
                    [System.IO.Path]::GetFullPath($_.Path).StartsWith(
                        $repoPrefix,
                        [System.StringComparison]::OrdinalIgnoreCase)
            } catch {
                $false
            }
        }

    if ($null -eq $processes) {
        return
    }

    Write-Host "Closing repo app process(es) before $Reason..."
    foreach ($process in $processes) {
        $process.CloseMainWindow() | Out-Null
    }
    Start-Sleep -Seconds 2

    foreach ($process in $processes) {
        $running = Get-Process -Id $process.Id -ErrorAction SilentlyContinue
        if ($null -ne $running) {
            Stop-Process -Id $process.Id -Force
        }
    }
}

function Invoke-AppSmoke([string]$Name, [string[]]$Arguments) {
    Invoke-Step $Name {
        $safeName = $Name -replace '[^A-Za-z0-9]+', '-'
        $failureReportPath = Join-Path $appData "$safeName-error-report.txt"
        Remove-Item -LiteralPath $failureReportPath -ErrorAction SilentlyContinue
        try {
            Stop-RepoAppProcesses $repoRoot $Name
            dotnet run --configuration $Configuration --no-build --project src\BattletechCharacterCreator.App `
                /p:UseSharedCompilation=false -- @Arguments "--smoke-failure-report=$failureReportPath"
            if ($LASTEXITCODE -ne 0) {
                if (Test-Path -LiteralPath $failureReportPath) {
                    $failureReport = Get-Content -LiteralPath $failureReportPath -Raw
                    throw "$Name failed with exit code $LASTEXITCODE. Failure report: $failureReport"
                }
                throw "$Name failed with exit code $LASTEXITCODE."
            }
        } finally {
            Stop-RepoAppProcesses $repoRoot $Name
        }
    }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot
Stop-RepoAppProcesses $repoRoot "release checks"

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

Invoke-Step "App smoke build" {
    Stop-RepoAppProcesses $repoRoot "app smoke build"
    dotnet build src\BattletechCharacterCreator.App --configuration $Configuration --no-restore `
        /p:UseSharedCompilation=false
}

Invoke-AppSmoke "Start window smoke" @("--smoke-start")

Invoke-AppSmoke "Wizard smoke" @("--smoke-wizard")

Invoke-AppSmoke "Affiliation-filtered childhood smoke" @("--smoke-affiliation-filtered-childhoods")

Invoke-AppSmoke "Clan round-trip smoke" @("--smoke-clan-roundtrip")

Invoke-AppSmoke "Representative life paths smoke" @("--smoke-complete-life-paths")

Invoke-AppSmoke "Editor allocation smoke" @("--smoke-editor-allocation")

Invoke-AppSmoke "Inventory smoke" @("--smoke-inventory")

Invoke-Step "Sheet export smoke" {
    try {
        Stop-RepoAppProcesses $repoRoot "sheet export smoke"
        dotnet run --configuration $Configuration --no-build --project src\BattletechCharacterCreator.App `
            /p:UseSharedCompilation=false -- --smoke-sheet-export=$sheetExportPath
        if ($LASTEXITCODE -ne 0) {
            throw "Sheet export smoke failed with exit code $LASTEXITCODE."
        }
    } finally {
        Stop-RepoAppProcesses $repoRoot "sheet export smoke"
    }
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
    try {
        Stop-RepoAppProcesses $repoRoot "error report smoke"
        dotnet run --configuration $Configuration --no-build --project src\BattletechCharacterCreator.App `
            /p:UseSharedCompilation=false -- --smoke-error-report=$errorReportPath
        if ($LASTEXITCODE -ne 0) {
            throw "Error report smoke failed with exit code $LASTEXITCODE."
        }
    } finally {
        Stop-RepoAppProcesses $repoRoot "error report smoke"
    }
    Assert-DiagnosticReport $errorReportPath "Error report smoke"
}

Invoke-Step "Operation report smoke" {
    try {
        Stop-RepoAppProcesses $repoRoot "operation report smoke"
        dotnet run --configuration $Configuration --no-build --project src\BattletechCharacterCreator.App `
            /p:UseSharedCompilation=false -- --smoke-operation-error-report=$operationReportPath
        if ($LASTEXITCODE -ne 0) {
            throw "Operation report smoke failed with exit code $LASTEXITCODE."
        }
    } finally {
        Stop-RepoAppProcesses $repoRoot "operation report smoke"
    }
    Assert-DiagnosticReport $operationReportPath "Operation report smoke"
}

Invoke-Step "Solution build" {
    Stop-RepoAppProcesses $repoRoot "solution build"
    dotnet build BattletechCharacterCreator.sln --configuration $Configuration /p:UseSharedCompilation=false
}

Invoke-Step "Folder publish" {
    Stop-RepoAppProcesses $repoRoot "folder publish"
    dotnet publish src\BattletechCharacterCreator.App --configuration $Configuration `
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
