param(
    [string]$Version = "0.1.1-preview",
    [string]$Tester = $env:USERNAME,
    [string]$InstallerPath = "",
    [string]$OutputDirectory = "artifacts\manual-tests"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

$planPath = Join-Path $repoRoot "docs\MANUAL_TEST_PLAN.md"
if (!(Test-Path -LiteralPath $planPath)) {
    throw "Manual test plan not found: $planPath"
}

$commit = git rev-parse --short HEAD
if ([string]::IsNullOrWhiteSpace($InstallerPath)) {
    $InstallerPath = "artifacts\release\$Version\atow-character-creator-$Version-$commit-setup.exe"
}
$created = Get-Date -Format "yyyy-MM-dd HH:mm:ss K"
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputRoot = Join-Path $repoRoot $OutputDirectory
New-Item -ItemType Directory -Force -Path $outputRoot | Out-Null

$safeTester = if ([string]::IsNullOrWhiteSpace($Tester)) {
    "tester"
} else {
    $Tester -replace "[^A-Za-z0-9._-]", "-"
}
$outputPath = Join-Path $outputRoot "manual-preview-$Version-$safeTester-$stamp.md"
$installerFullPath = if ([System.IO.Path]::IsPathRooted($InstallerPath)) {
    $InstallerPath
} else {
    Join-Path $repoRoot $InstallerPath
}
$installerStatus = if (Test-Path -LiteralPath $installerFullPath) {
    "Present"
} else {
    "Not found yet"
}

$plan = Get-Content -LiteralPath $planPath -Raw
$body = @(
    "# Manual Preview Test Run",
    "",
    "Version: $Version",
    "Commit: $commit",
    "Created: $created",
    "Tester: $Tester",
    "Installer: $installerFullPath",
    "Installer status: $installerStatus",
    "",
    "Result: Pending",
    "",
    "Notes:",
    "",
    "- ",
    "",
    $plan
) -join "`n"

$body | Set-Content -LiteralPath $outputPath

Write-Host "Manual test run created:"
Write-Host $outputPath
