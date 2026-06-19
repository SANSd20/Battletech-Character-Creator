param(
    [string]$Version = "0.1.0-preview",
    [string]$Repository = "SANSd20/Battletech-Character-Creator",
    [string]$TagName = "",
    [string]$Title = "",
    [switch]$DryRun,
    [switch]$AllowDirtyManifest
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

if ([string]::IsNullOrWhiteSpace($TagName)) {
    $TagName = "v$Version"
}

if ([string]::IsNullOrWhiteSpace($Title)) {
    $Title = "A Time of War Character Creator $Version"
}

$releaseDir = Join-Path $repoRoot "artifacts\release\$Version"
$installerName = "atow-character-creator-$Version-setup.exe"
$installerPath = Join-Path $releaseDir $installerName
$checksumPath = Join-Path $releaseDir "$installerName.sha256"
$notesPath = Join-Path $releaseDir "GITHUB_RELEASE.md"
$releaseNotesPath = Join-Path $releaseDir "PREVIEW_RELEASE_NOTES.md"
$manifestPath = Join-Path $releaseDir "release-manifest.txt"

$requiredFiles = @(
    $installerPath,
    $checksumPath,
    $notesPath,
    $releaseNotesPath,
    $manifestPath
)

foreach ($path in $requiredFiles) {
    if (!(Test-Path -LiteralPath $path)) {
        throw "Required release artifact is missing: $path"
    }
}

$actualHash = (Get-FileHash -LiteralPath $installerPath -Algorithm SHA256).Hash
$checksumText = Get-Content -LiteralPath $checksumPath -Raw
if (!$checksumText.Contains($actualHash)) {
    throw "Checksum file does not match installer hash. Expected $actualHash."
}

$notesText = Get-Content -LiteralPath $notesPath -Raw
if (!$notesText.Contains($actualHash)) {
    throw "GitHub release draft does not include the current installer hash. Rebuild the package."
}
if ($notesText.Contains("{{INSTALLER_SHA256}}")) {
    throw "GitHub release draft still contains the installer hash placeholder. Rebuild the package."
}

$manifestText = Get-Content -LiteralPath $manifestPath -Raw
if (!$AllowDirtyManifest -and $manifestText -match "Dirty working tree:\s*True") {
    throw "Release manifest was built from a dirty working tree. Rebuild the package from a clean commit."
}

if ($manifestText -notmatch "Version:\s*$([regex]::Escape($Version))\b") {
    throw "Release manifest version does not match requested version ($Version). Rebuild the package."
}

$currentCommit = git rev-parse --short HEAD
if ($manifestText -notmatch "Commit:\s*$currentCommit\b") {
    throw "Release manifest commit does not match HEAD ($currentCommit). Rebuild the package after committing."
}

$installerSize = (Get-Item -LiteralPath $installerPath).Length
if ($manifestText -notmatch "SHA256:\s*$actualHash\b") {
    throw "Release manifest SHA256 does not match installer hash. Rebuild the package."
}
if ($manifestText -notmatch "Installer bytes:\s*$installerSize\b") {
    throw "Release manifest installer size does not match installer. Rebuild the package."
}
if ($manifestText -notmatch "Installer built:\s*\d{4}-\d{2}-\d{2}") {
    throw "Release manifest does not include the installer build timestamp. Rebuild the package."
}

$assetPaths = @(
    $installerPath,
    $checksumPath,
    $releaseNotesPath,
    $manifestPath
)

$arguments = @(
    "release",
    "create",
    $TagName,
    "--repo",
    $Repository,
    "--title",
    $Title,
    "--notes-file",
    $notesPath,
    "--prerelease"
) + $assetPaths

Write-Host "Release package validated:"
Write-Host $releaseDir
Write-Host "Tag: $TagName"
Write-Host "Repository: $Repository"

if ($DryRun) {
    Write-Host "Dry run only. GitHub release command:"
    Write-Host "gh $($arguments -join ' ')"
    exit 0
}

$ghCommand = Get-Command gh -ErrorAction SilentlyContinue
if ($null -eq $ghCommand) {
    throw "GitHub CLI was not found. Install gh or publish the release manually from the files in $releaseDir."
}

& gh @arguments
if ($LASTEXITCODE -ne 0) {
    throw "GitHub release creation failed with exit code $LASTEXITCODE."
}
