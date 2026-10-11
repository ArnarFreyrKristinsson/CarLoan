# Draws the sample a human checks: a few excused mutants and a few agent-written tests,
# picked at random by this script. The agents do not choose what gets inspected.
#
#   powershell -ExecutionPolicy Bypass -File tools/mutation/Get-Sample.ps1 [-OnlyIfDue]
#
# Appends the sample to docs/mutation/samples.md. With -OnlyIfDue it does nothing unless the
# last sample is older than sampleIntervalDays in settings.json. Reasons: docs/mutation/README.md.
param([switch]$OnlyIfDue)

. (Join-Path $PSScriptRoot 'Common.ps1')

$samplesPath = Join-Path $DocsDir 'samples.md'
$today = Get-Date

if ($OnlyIfDue) {
    $lastHeading = Select-String -Path $samplesPath -Pattern '^## Sample (\d{4}-\d{2}-\d{2})' | Select-Object -Last 1
    if ($lastHeading) {
        $last = [datetime]::ParseExact($lastHeading.Matches[0].Groups[1].Value, 'yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture)
        if (($today - $last).TotalDays -lt $Settings.sampleIntervalDays) { exit 0 }
    }
}

function Get-RandomItems($items) {
    # An empty ledger arrives here as $null, and @($null) would be a list of one.
    $items = @($items | Where-Object { $null -ne $_ })
    if ($items.Count -eq 0) { return @() }
    return @($items | Get-Random -Count ([Math]::Min($Settings.sampleSize, $items.Count)))
}

$excuses = @(Get-RandomItems (Read-State 'excuses'))
$tests = @(Get-RandomItems (Read-State 'agent-tests'))
if ($excuses.Count + $tests.Count -eq 0) {
    if (-not $OnlyIfDue) { Write-Output 'Nothing to sample yet: no excuses and no agent-written tests.' }
    exit 0
}

$section = @("", "## Sample $($today.ToString('yyyy-MM-dd'))", "", "Result: _not checked yet_", "")
$section += "Excused mutants. Is the argument right, that no valid input can tell the two versions apart?"
$section += ""
$section += if ($excuses.Count -eq 0) { "- none yet" } else {
    $excuses | ForEach-Object { "- [ ] $($_.mutant -replace '\s+', ' ')`n      Excuse: $($_.reason)" }
}
$section += ""
$section += "Agent-written tests. Does each expected value trace back to the lender's terms in docs/rules/?"
$section += ""
$section += if ($tests.Count -eq 0) { "- none yet" } else {
    $tests | ForEach-Object { "- [ ] ``$(@($_.files) -join ', ')``: $(@($_.tests) -join ', ')`n      Written to kill: $($_.mutant -replace '\s+', ' ')" }
}

[IO.File]::AppendAllText($samplesPath, ($section -join "`n") + "`n", (New-Object Text.UTF8Encoding($false)))
Write-Output "SAMPLE DUE: $($excuses.Count) excuse(s) and $($tests.Count) test(s) were added to docs/mutation/samples.md for a human to check."
