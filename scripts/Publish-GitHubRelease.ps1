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

$manifestText = Get-Content -LiteralPath $manifestPath -Raw
if (!$AllowDirtyManifest -and $manifestText -match "Dirty working tree:\s*True") {
    throw "Release manifest was built from a dirty working tree. Rebuild the package from a clean commit."
}

$currentCommit = git rev-parse --short HEAD
if ($manifestText -notmatch "Commit:\s*$currentCommit\b") {
    throw "Release manifest commit does not match HEAD ($currentCommit). Rebuild the package after committing."
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
