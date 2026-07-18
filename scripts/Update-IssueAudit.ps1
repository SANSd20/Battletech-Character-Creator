param(
    [string]$IssuesPath = "issues\issues.json",
    [string]$OutputPath = "docs\ISSUE_AUDIT.md"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

$issuesFullPath = if ([System.IO.Path]::IsPathRooted($IssuesPath)) {
    $IssuesPath
} else {
    Join-Path $repoRoot $IssuesPath
}
$outputFullPath = if ([System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath
} else {
    Join-Path $repoRoot $OutputPath
}

if (!(Test-Path -LiteralPath $issuesFullPath)) {
    throw "Issues file not found: $issuesFullPath"
}

$issues = Get-Content -LiteralPath $issuesFullPath -Raw |
    ConvertFrom-Json

$localStatus = @{
    "7" = @{
        Status = "Implemented locally; ready for manual confirmation and GitHub closure"
        Evidence = @(
            "The wizard has a dedicated Free XP step before final review.",
            "The Free XP step can spend remaining XP from one grouped target dropdown.",
            "Free XP allocation rows can be removed individually without resetting all spending.",
            "Rule-check fixes moved to the Free XP step; Attribute, Trait, and Skill gaps can spend Free XP directly.",
            "Non-XP rule-check issues navigate back to the relevant wizard stage, including Education issues returning to Stage 3."
        )
        NextStep = "Confirm the workflow in the packaged app, then close the GitHub issue."
    }
}

function Escape-MarkdownTableCell([string]$Value) {
    if ($null -eq $Value) { return "" }
    return $Value.Replace("|", "\|").Replace("`r", "").Replace("`n", "<br>")
}

$created = Get-Date -Format "yyyy-MM-dd HH:mm:ss K"
$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# Imported GitHub Issue Audit")
$lines.Add("")
$lines.Add("Generated: $created")
$lines.Add("")
$lines.Add("Source: ``$IssuesPath``")
$lines.Add("")
$lines.Add("This file summarizes the locally imported GitHub issue export without")
$lines.Add("modifying the exported ``issues.json`` snapshot. Use it to decide what")
$lines.Add("still needs manual testing or GitHub-side closure.")
$lines.Add("")
$lines.Add("## Summary")
$lines.Add("")
$lines.Add("| Issue | GitHub state | Local status | Next step |")
$lines.Add("| --- | --- | --- | --- |")

foreach ($issue in @($issues)) {
    $number = [string]$issue.number
    $status = if ($localStatus.ContainsKey($number)) {
        $localStatus[$number].Status
    } else {
        "Not audited locally"
    }
    $nextStep = if ($localStatus.ContainsKey($number)) {
        $localStatus[$number].NextStep
    } else {
        "Review issue details and decide whether implementation is still needed."
    }
    $issueLabel = "#$($issue.number): $($issue.title)"
    $lines.Add("| $($issueLabel | ForEach-Object { Escape-MarkdownTableCell $_ }) | $($issue.state) | $(Escape-MarkdownTableCell $status) | $(Escape-MarkdownTableCell $nextStep) |")
}

$lines.Add("")
$lines.Add("## Details")
$lines.Add("")

foreach ($issue in @($issues)) {
    $number = [string]$issue.number
    $lines.Add("### #$($issue.number) $($issue.title)")
    $lines.Add("")
    $lines.Add("- GitHub state: $($issue.state)")
    $lines.Add("- URL: $($issue.url)")
    if ($localStatus.ContainsKey($number)) {
        $lines.Add("- Local status: $($localStatus[$number].Status)")
        $lines.Add("- Evidence:")
        foreach ($item in $localStatus[$number].Evidence) {
            $lines.Add("  - $item")
        }
        $lines.Add("- Next step: $($localStatus[$number].NextStep)")
    } else {
        $lines.Add("- Local status: Not audited locally")
        $lines.Add("- Next step: Review and map implementation evidence.")
    }
    $lines.Add("")
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputFullPath) |
    Out-Null
$lines -join "`n" | Set-Content -LiteralPath $outputFullPath

Write-Host "Issue audit written:"
Write-Host $outputFullPath
