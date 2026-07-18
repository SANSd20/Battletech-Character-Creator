param(
    [string]$InstallerPath = "niss\atow-character-creator-0.1.28-preview-setup.exe",
    [string]$InstallDir = (Join-Path $env:TEMP "A-Time-of-War-Installer-Smoke"),
    [string]$ExpectedVersion = "0.1.28-preview",
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

function Resolve-RepoPath([string]$Path) {
    if ([System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }
    return Join-Path (Get-Location) $Path
}

$installer = Resolve-RepoPath $InstallerPath
$installTarget = Resolve-RepoPath $InstallDir
$installedExe = Join-Path $installTarget "BattletechCharacterCreator.App.exe"
$resourceFile = Join-Path $installTarget "Resources\equiplist.dat"
$sheetFile = Join-Path $installTarget "Assets\Sheets\CharacterRecordSheet.png"
$smokeReport = Join-Path $installTarget "installer-smoke-report.txt"
$sheetExportPath = Join-Path $installTarget "installer-smoke-sheet-export.pdf"
$sheetExportErrorPath = $sheetExportPath + ".error.txt"
$uninstaller = Join-Path $installTarget "uninstall.exe"

if ($DryRun) {
    Write-Host "Installer: $installer"
    if (!(Test-Path -LiteralPath $installer)) {
        Write-Host "Installer is not present; dry run will only show the planned checks."
    }
    Write-Host "Install target: $installTarget"
    Write-Host "Would run: $installer /S /D=$installTarget"
    Write-Host "Would verify: $installedExe"
    Write-Host "Would verify: $resourceFile"
    Write-Host "Would verify: $sheetFile"
    Write-Host "Would smoke: $installedExe --smoke-start"
    Write-Host "Would smoke: $installedExe --smoke-error-report=$smokeReport"
    Write-Host "Would verify smoke report diagnostic metadata"
    Write-Host "Would smoke: $installedExe --smoke-sheet-export=$sheetExportPath"
    Write-Host "Would verify sheet export output"
    Write-Host "Would uninstall: $uninstaller /S"
    exit 0
}

if (!(Test-Path -LiteralPath $installer)) {
    throw "Installer not found: $installer"
}

if (Test-Path -LiteralPath $installTarget) {
    Remove-Item -LiteralPath $installTarget -Recurse -Force
}

$installProcess = Start-Process -FilePath $installer `
    -ArgumentList @("/S", "/D=$installTarget") `
    -WindowStyle Hidden `
    -PassThru
$installProcess.WaitForExit()
if ($installProcess.ExitCode -ne 0) {
    throw "Installer exited with code $($installProcess.ExitCode)."
}

foreach ($path in @($installedExe, $resourceFile, $sheetFile, $uninstaller)) {
    if (!(Test-Path -LiteralPath $path)) {
        throw "Installed file missing: $path"
    }
}

$startSmokeProcess = Start-Process -FilePath $installedExe `
    -ArgumentList "--smoke-start" `
    -WindowStyle Hidden `
    -PassThru
if (!$startSmokeProcess.WaitForExit(30000)) {
    $startSmokeProcess.Kill()
    throw "Installed app start-window smoke test timed out."
}
if ($startSmokeProcess.ExitCode -ne 0) {
    throw "Installed app start-window smoke test exited with code $($startSmokeProcess.ExitCode)."
}

$smokeProcess = Start-Process -FilePath $installedExe `
    -ArgumentList "--smoke-error-report=$smokeReport" `
    -WindowStyle Hidden `
    -PassThru
if (!$smokeProcess.WaitForExit(30000)) {
    $smokeProcess.Kill()
    throw "Installed app smoke test timed out."
}
if ($smokeProcess.ExitCode -ne 0) {
    throw "Installed app smoke test exited with code $($smokeProcess.ExitCode)."
}
if (!(Test-Path -LiteralPath $smokeReport)) {
    throw "Installed app smoke report was not created."
}
$report = Get-Content -LiteralPath $smokeReport -Raw
$requiredReportText = @(
    "Process architecture:",
    "Command line:"
)
if (![string]::IsNullOrWhiteSpace($ExpectedVersion)) {
    $requiredReportText += "Version: $ExpectedVersion"
}
foreach ($text in $requiredReportText) {
    if (!$report.Contains($text)) {
        throw "Installed app smoke report did not include diagnostic metadata: $text"
    }
}

$sheetExportProcess = Start-Process -FilePath $installedExe `
    -ArgumentList "--smoke-sheet-export=$sheetExportPath" `
    -WindowStyle Hidden `
    -PassThru
if (!$sheetExportProcess.WaitForExit(30000)) {
    $sheetExportProcess.Kill()
    throw "Installed app sheet export smoke test timed out."
}
if ($sheetExportProcess.ExitCode -ne 0) {
    throw "Installed app sheet export smoke test exited with code $($sheetExportProcess.ExitCode)."
}
if (Test-Path -LiteralPath $sheetExportErrorPath) {
    $exportError = Get-Content -LiteralPath $sheetExportErrorPath -Raw
    throw "Installed app sheet export wrote an error report: $exportError"
}
if (!(Test-Path -LiteralPath $sheetExportPath)) {
    throw "Installed app sheet export output was not created."
}
if ((Get-Item -LiteralPath $sheetExportPath).Length -le 0) {
    throw "Installed app sheet export output was empty."
}

$uninstallProcess = Start-Process -FilePath $uninstaller `
    -ArgumentList "/S" `
    -WindowStyle Hidden `
    -PassThru
$uninstallProcess.WaitForExit()
if ($uninstallProcess.ExitCode -ne 0) {
    throw "Uninstaller exited with code $($uninstallProcess.ExitCode)."
}

for ($attempt = 1; $attempt -le 20; $attempt++) {
    if (!(Test-Path -LiteralPath $installedExe)) {
        break
    }
    Start-Sleep -Milliseconds 250
}

if (Test-Path -LiteralPath $installedExe) {
    $remaining = Get-ChildItem -LiteralPath $installTarget -Recurse -Force |
        Select-Object -ExpandProperty FullName
    throw "Installed app remained after uninstall: $installedExe`nRemaining files:`n$($remaining -join "`n")"
}

Write-Host "Installer smoke test passed."
