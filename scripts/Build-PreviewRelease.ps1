param(
    [string]$Version = "0.1.19-preview",
    [switch]$SkipReleaseChecks,
    [switch]$AllowDirty,
    [switch]$AllowStaleInstaller
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

$commit = git rev-parse --short HEAD
$installerName = "atow-character-creator-$Version-setup.exe"
$releaseInstallerName = "atow-character-creator-$Version-$commit-setup.exe"
$installerPath = Join-Path $repoRoot "niss\$installerName"
$releaseDir = Join-Path $repoRoot "artifacts\release\$Version"
$releaseInstaller = Join-Path $releaseDir $releaseInstallerName
$checksumPath = Join-Path $releaseDir "$releaseInstallerName.sha256"
$manifestPath = Join-Path $releaseDir "release-manifest.txt"
$notesSourcePath = Join-Path $repoRoot "docs\PREVIEW_RELEASE_NOTES.md"
$notesOutputPath = Join-Path $releaseDir "PREVIEW_RELEASE_NOTES.md"
$releaseDraftSourcePath = Join-Path $repoRoot "docs\GITHUB_RELEASE_$Version.md"
$releaseDraftOutputPath = Join-Path $releaseDir "GITHUB_RELEASE.md"

if (!$SkipReleaseChecks) {
    powershell -NoProfile -ExecutionPolicy Bypass `
        -File scripts\Run-ReleaseChecks.ps1 `
        -Version $Version
    if ($LASTEXITCODE -ne 0) {
        throw "Release checks failed with exit code $LASTEXITCODE."
    }
}

$gitStatus = git status --porcelain
$isDirty = $gitStatus.Length -gt 0
if ($isDirty -and !$AllowDirty) {
    throw "Working tree has uncommitted changes. Commit them or rerun with -AllowDirty."
}

if (!(Test-Path -LiteralPath $installerPath)) {
    throw "Installer not found: $installerPath"
}

$headCommitTime = [DateTimeOffset]::FromUnixTimeSeconds(
    [int64](git log -1 --format=%ct)).UtcDateTime
$installerInfo = Get-Item -LiteralPath $installerPath
if ($SkipReleaseChecks -and !$AllowStaleInstaller -and
    $installerInfo.LastWriteTimeUtc -lt $headCommitTime) {
    throw @"
Installer predates the current commit: $installerPath
Run scripts\Run-ReleaseChecks.ps1 or rerun without -SkipReleaseChecks to rebuild it.
Use -AllowStaleInstaller only when intentionally repackaging an older installer.
"@
}

if (Test-Path -LiteralPath $releaseDir) {
    Remove-Item -LiteralPath $releaseDir -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $releaseDir | Out-Null

Copy-Item -LiteralPath $installerPath -Destination $releaseInstaller
Copy-Item -LiteralPath $notesSourcePath -Destination $notesOutputPath

$hash = Get-FileHash -LiteralPath $releaseInstaller -Algorithm SHA256
"$($hash.Hash)  $releaseInstallerName" | Set-Content -LiteralPath $checksumPath
$releaseDraft = Get-Content -LiteralPath $releaseDraftSourcePath -Raw
$releaseDraft.Replace("{{INSTALLER_SHA256}}", $hash.Hash) |
    Set-Content -LiteralPath $releaseDraftOutputPath

$created = Get-Date -Format "yyyy-MM-dd HH:mm:ss K"
$installerSize = (Get-Item -LiteralPath $releaseInstaller).Length
$installerBuilt = $installerInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss K")

@(
    "A Time of War Character Creator Preview Release",
    "Version: $Version",
    "Commit: $commit",
    "Dirty working tree: $isDirty",
    "Created: $created",
    "Source installer: $installerName",
    "Installer: $releaseInstallerName",
    "Installer bytes: $installerSize",
    "Installer built: $installerBuilt",
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
