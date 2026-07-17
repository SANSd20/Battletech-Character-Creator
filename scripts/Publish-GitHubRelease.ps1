param(
    [string]$Version = "0.1.25-preview",
    [string]$Repository = "SANSd20/Battletech-Character-Creator",
    [string]$TagName = "",
    [string]$Title = "",
    [switch]$DryRun,
    [switch]$UpdateExisting,
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
$currentCommit = git rev-parse --short HEAD
$installerName = "atow-character-creator-$Version-$currentCommit-setup.exe"
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
if (!$notesText.Contains($Version)) {
    throw "GitHub release draft does not mention requested version ($Version). Rebuild the package."
}
if (!$notesText.Contains($actualHash)) {
    throw "GitHub release draft does not include the current installer hash. Rebuild the package."
}
if ($notesText.Contains("{{INSTALLER_SHA256}}")) {
    throw "GitHub release draft still contains the installer hash placeholder. Rebuild the package."
}

$releaseNotesText = Get-Content -LiteralPath $releaseNotesPath -Raw
if (!$releaseNotesText.Contains($Version)) {
    throw "Preview release notes do not mention requested version ($Version). Rebuild the package."
}

$manifestText = Get-Content -LiteralPath $manifestPath -Raw
if (!$AllowDirtyManifest -and $manifestText -match "Dirty working tree:\s*True") {
    throw "Release manifest was built from a dirty working tree. Rebuild the package from a clean commit."
}

if ($manifestText -notmatch "Version:\s*$([regex]::Escape($Version))\b") {
    throw "Release manifest version does not match requested version ($Version). Rebuild the package."
}

if ($manifestText -notmatch "Commit:\s*$currentCommit\b") {
    throw "Release manifest commit does not match HEAD ($currentCommit). Rebuild the package after committing."
}
if ($manifestText -notmatch "Installer:\s*$([regex]::Escape($installerName))\b") {
    throw "Release manifest installer name does not match expected build artifact ($installerName). Rebuild the package."
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

$editArguments = @(
    "release",
    "edit",
    $TagName,
    "--repo",
    $Repository,
    "--title",
    $Title,
    "--notes-file",
    $notesPath,
    "--prerelease"
)

$uploadArguments = @(
    "release",
    "upload",
    $TagName
) + $assetPaths + @(
    "--repo",
    $Repository,
    "--clobber"
)

Write-Host "Release package validated:"
Write-Host $releaseDir
Write-Host "Tag: $TagName"
Write-Host "Repository: $Repository"

if ($DryRun) {
    Write-Host "Dry run only. GitHub release command:"
    if ($UpdateExisting) {
        Write-Host "gh $($editArguments -join ' ')"
        Write-Host "gh $($uploadArguments -join ' ')"
    }
    else {
        Write-Host "gh $($arguments -join ' ')"
    }
    exit 0
}

$ghCommand = Get-Command gh -ErrorAction SilentlyContinue
if ($null -eq $ghCommand) {
    throw "GitHub CLI was not found. Install gh or publish the release manually from the files in $releaseDir."
}

$previousErrorActionPreference = $ErrorActionPreference
$ErrorActionPreference = "Continue"
try {
    $authOutput = & gh auth status --hostname github.com 2>&1
    $authExitCode = $LASTEXITCODE
}
finally {
    $ErrorActionPreference = $previousErrorActionPreference
}
if ($authOutput) {
    $authOutput | ForEach-Object { Write-Host $_ }
}
if ($authExitCode -ne 0) {
    throw "GitHub CLI is not authenticated for github.com. Run 'gh auth login -h github.com', then retry this script."
}

$previousErrorActionPreference = $ErrorActionPreference
$ErrorActionPreference = "Continue"
try {
    $existingReleaseOutput = & gh release view $TagName --repo $Repository --json url 2>&1
    $existingReleaseExitCode = $LASTEXITCODE
}
finally {
    $ErrorActionPreference = $previousErrorActionPreference
}
if ($existingReleaseExitCode -eq 0) {
    if ($existingReleaseOutput) {
        $existingReleaseOutput | ForEach-Object { Write-Host $_ }
    }
    if (!$UpdateExisting) {
        throw "GitHub release $TagName already exists in $Repository. Rerun with -UpdateExisting to refresh notes and assets, delete it, or choose a new -Version/-TagName."
    }
}
elseif ($UpdateExisting) {
    Write-Host "GitHub release $TagName does not exist yet. Creating it instead of updating."
}

if ($existingReleaseExitCode -eq 0 -and $UpdateExisting) {
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $editOutput = & gh @editArguments 2>&1
        $editExitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }
    if ($editOutput) {
        $editOutput | ForEach-Object { Write-Host $_ }
    }
    if ($editExitCode -ne 0) {
        $editOutputText = ($editOutput | Out-String).Trim()
        if ([string]::IsNullOrWhiteSpace($editOutputText)) {
            $editOutputText = "No output was returned by GitHub CLI."
        }
        throw "GitHub release update failed with exit code $editExitCode. GitHub CLI output: $editOutputText"
    }

    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        $uploadOutput = & gh @uploadArguments 2>&1
        $uploadExitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }
    if ($uploadOutput) {
        $uploadOutput | ForEach-Object { Write-Host $_ }
    }
    if ($uploadExitCode -ne 0) {
        $uploadOutputText = ($uploadOutput | Out-String).Trim()
        if ([string]::IsNullOrWhiteSpace($uploadOutputText)) {
            $uploadOutputText = "No output was returned by GitHub CLI."
        }
        throw "GitHub release asset upload failed with exit code $uploadExitCode. GitHub CLI output: $uploadOutputText"
    }

    exit 0
}

$previousErrorActionPreference = $ErrorActionPreference
$ErrorActionPreference = "Continue"
try {
    $releaseOutput = & gh @arguments 2>&1
    $releaseExitCode = $LASTEXITCODE
}
finally {
    $ErrorActionPreference = $previousErrorActionPreference
}
if ($releaseOutput) {
    $releaseOutput | ForEach-Object { Write-Host $_ }
}
if ($releaseExitCode -ne 0) {
    $releaseOutputText = ($releaseOutput | Out-String).Trim()
    if ([string]::IsNullOrWhiteSpace($releaseOutputText)) {
        $releaseOutputText = "No output was returned by GitHub CLI."
    }
    throw "GitHub release creation failed with exit code $releaseExitCode. GitHub CLI output: $releaseOutputText"
}
