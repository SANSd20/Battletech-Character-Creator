param(
    [string]$Version = "0.1.0-preview",
    [switch]$SkipReleaseChecks,
    [switch]$AllowDirty
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

$installerName = "atow-character-creator-$Version-setup.exe"
$installerPath = Join-Path $repoRoot "niss\$installerName"
$releaseDir = Join-Path $repoRoot "artifacts\release\$Version"
$releaseInstaller = Join-Path $releaseDir $installerName
$checksumPath = Join-Path $releaseDir "$installerName.sha256"
$manifestPath = Join-Path $releaseDir "release-manifest.txt"
$notesSourcePath = Join-Path $repoRoot "docs\PREVIEW_RELEASE_NOTES.md"
$notesOutputPath = Join-Path $releaseDir "PREVIEW_RELEASE_NOTES.md"
$releaseDraftSourcePath = Join-Path $repoRoot "docs\GITHUB_RELEASE_$Version.md"
$releaseDraftOutputPath = Join-Path $releaseDir "GITHUB_RELEASE.md"

if (!$SkipReleaseChecks) {
    powershell -NoProfile -ExecutionPolicy Bypass `
        -File scripts\Run-ReleaseChecks.ps1 `
        -Version $Version
}

$gitStatus = git status --porcelain
$isDirty = $gitStatus.Length -gt 0
if ($isDirty -and !$AllowDirty) {
    throw "Working tree has uncommitted changes. Commit them or rerun with -AllowDirty."
}

if (!(Test-Path -LiteralPath $installerPath)) {
    throw "Installer not found: $installerPath"
}

if (Test-Path -LiteralPath $releaseDir) {
    Remove-Item -LiteralPath $releaseDir -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $releaseDir | Out-Null

Copy-Item -LiteralPath $installerPath -Destination $releaseInstaller
Copy-Item -LiteralPath $notesSourcePath -Destination $notesOutputPath
Copy-Item -LiteralPath $releaseDraftSourcePath -Destination $releaseDraftOutputPath

$hash = Get-FileHash -LiteralPath $releaseInstaller -Algorithm SHA256
"$($hash.Hash)  $installerName" | Set-Content -LiteralPath $checksumPath

$commit = git rev-parse --short HEAD
$created = Get-Date -Format "yyyy-MM-dd HH:mm:ss K"
$installerSize = (Get-Item -LiteralPath $releaseInstaller).Length

@(
    "A Time of War Character Creator Preview Release",
    "Version: $Version",
    "Commit: $commit",
    "Dirty working tree: $isDirty",
    "Created: $created",
    "Installer: $installerName",
    "Installer bytes: $installerSize",
    "Release notes: PREVIEW_RELEASE_NOTES.md",
    "GitHub release draft: GITHUB_RELEASE.md",
    "SHA256: $($hash.Hash)",
    "",
    "Prerequisites:",
    "- .NET 10 Desktop Runtime",
    "",
    "Verification:",
    "- Run scripts\Run-ReleaseChecks.ps1 before packaging",
    "- Run scripts\Test-Installer.ps1 on a normal Windows session"
) | Set-Content -LiteralPath $manifestPath

Write-Host "Preview release package created:"
Write-Host $releaseDir
