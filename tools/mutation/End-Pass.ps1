# Ends a mutation pass: acts on the verdicts, checks every excuse it places, parks what is
# left, runs the CI checks and stages the result.
#
#   powershell -ExecutionPolicy Bypass -File tools/mutation/End-Pass.ps1
#
# Every survivor in the queue ends up in exactly one place: killed (a test was kept),
# excused (a checked Stryker comment), parked (left as a survivor, not retried) or flagged
# as a possible bug. Nothing is asked to raise the score, and it is not printed here.
# Process and reasons: docs/mutation/README.md.

. (Join-Path $PSScriptRoot 'Common.ps1')

function Get-Description($mutant) {
    return "$($mutant.File):$($mutant.Line)  $($mutant.Mutator):  $($mutant.Original)  ->  $($mutant.Replacement)"
}

function Edit-Lines([string]$file, [scriptblock]$edit) {
    # Rewrites a source file line by line, keeping its line endings.
    $path = Join-Path $RepoRoot $file
    $text = [IO.File]::ReadAllText($path)
    $newline = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.AddRange([string[]]($text -split "`r?`n"))
    & $edit $lines
    Write-Text $path ($lines -join $newline)
}

if (-not (Test-Path (Join-Path $PassDir 'queue.json'))) {
    Write-Output 'STOP: no pass is running. Start one with Start-Pass.ps1.'
    exit 2
}
if (@(Get-PendingPaths).Count -gt 0) {
    Write-Output 'STOP: the working tree holds an attempt nobody judged. Run Confirm-Kill.ps1 for it first.'
    exit 2
}

$today = (Get-Date).ToString('yyyy-MM-dd')
$queue = @(Read-Pass 'queue')
$verdicts = @(Read-Pass 'verdicts')
$killedKeys = @(Read-Pass 'attempts' | Where-Object { $_.killed } | ForEach-Object { $_.key })

$toExcuse = @()
$toPark = @()
$bugFlags = @()

# --- 1. Sort every survivor that was not killed.
foreach ($mutant in $queue | Where-Object { $killedKeys -notcontains $_.Key }) {
    $verdict = $verdicts | Where-Object { $_.key -eq $mutant.Key } | Select-Object -First 1
    $kind = if ($verdict) { $verdict.verdict } else { 'None' }
    $reason = if ($verdict) { $verdict.reason } else { '' }
    $mutatorType = Get-MutatorType $mutant.Mutator

    switch ($kind) {
        'Bug' {
            $bugFlags += [pscustomobject]@{ mutant = $mutant; reason = $reason }
            $toPark += [pscustomobject]@{ mutant = $mutant; reason = "flagged as a possible bug: $reason" }
        }
        'Equivalent' {
            if (-not $Settings.autoExcuseEquivalent) {
                $toPark += [pscustomobject]@{ mutant = $mutant; reason = "judged equivalent while automatic excuses are switched off: $reason" }
            }
            elseif (-not $mutatorType) {
                $toPark += [pscustomobject]@{ mutant = $mutant; reason = "judged equivalent, but Common.ps1 has no comment name for this mutator: $reason" }
            }
            else {
                $toExcuse += [pscustomobject]@{
                    mutant  = $mutant
                    reason  = $reason
                    type    = $mutatorType
                    comment = "// Stryker disable once ${mutatorType}: equivalent, because $reason"
                }
            }
        }
        'Killable' { $toPark += [pscustomobject]@{ mutant = $mutant; reason = "judged killable, but not killed with the hint: $reason" } }
        'Unsure' { $toPark += [pscustomobject]@{ mutant = $mutant; reason = "the judge was unsure. $reason".Trim() } }
        default { $toPark += [pscustomobject]@{ mutant = $mutant; reason = 'not killed and not judged' } }
    }
}

# --- 2. Place the excuses. A script does this so the comment always goes in the same spot:
#        directly above the mutant's line, naming one mutator. Bottom-up, so line numbers hold.
foreach ($group in $toExcuse | Group-Object { $_.mutant.File }) {
    $inFile = @($group.Group | Sort-Object { $_.mutant.Line } -Descending)
    Edit-Lines $group.Name {
        param($lines)
        foreach ($excuse in $inFile) {
            $target = $lines[$excuse.mutant.Line - 1]
            $indent = $target.Substring(0, $target.Length - $target.TrimStart().Length)
            $lines.Insert($excuse.mutant.Line - 1, $indent + $excuse.comment)
        }
    }
}

# --- 3. Check each excuse against Stryker itself. An excuse stands only if Stryker now ignores
#        that one mutant for that reason, and no other. Anything else is taken back out and parked,
#        so a comment can never quietly cover a mutant nobody judged.
$excused = @()
if ($toExcuse.Count -gt 0) {
    $reportPath = Invoke-Stryker 'excuses' (Get-MutateArguments @($toExcuse | ForEach-Object { $_.mutant.File }))
    $after = if ($reportPath) { @(Read-Mutants $reportPath) } else { @() }

    foreach ($excuse in $toExcuse) {
        $expectedReason = "equivalent, because $($excuse.reason)"
        $covered = @($after | Where-Object { $_.File -eq $excuse.mutant.File -and $_.Status -eq 'Ignored' -and $_.StatusReason.Trim() -eq $expectedReason })

        if ($covered.Count -eq 1 -and $covered[0].Key -eq $excuse.mutant.Key) { $excused += $excuse; continue }

        Edit-Lines $excuse.mutant.File {
            param($lines)
            for ($i = $lines.Count - 1; $i -ge 0; $i--) { if ($lines[$i].Trim() -eq $excuse.comment) { $lines.RemoveAt($i) } }
        }
        $toPark += [pscustomobject]@{
            mutant = $excuse.mutant
            reason = "judged equivalent, but the comment covered $($covered.Count) mutant(s) instead of this one alone, so it was removed: $($excuse.reason)"
        }
    }
}

# --- 4. Write down what outlives the pass.
if ($excused.Count -gt 0) {
    $ledger = @(Read-State 'excuses')
    foreach ($excuse in $excused) {
        $ledger += [pscustomobject]@{
            key      = $excuse.mutant.Key
            date     = $today
            project  = $excuse.mutant.Project
            file     = $excuse.mutant.File
            comment  = $excuse.comment
            lineHash = Get-Sha1 $excuse.mutant.LineText
            mutant   = Get-Description $excuse.mutant
            reason   = $excuse.reason
        }
    }
    Write-State 'excuses' $ledger
}

if ($toPark.Count -gt 0) {
    $parked = @(Read-State 'parked')
    foreach ($item in $toPark) {
        $parked += [pscustomobject]@{
            key     = $item.mutant.Key
            date    = $today
            project = $item.mutant.Project
            mutant  = Get-Description $item.mutant
            reason  = $item.reason
        }
    }
    Write-State 'parked' $parked
}

if ($bugFlags.Count -gt 0) {
    $entries = foreach ($flag in $bugFlags) {
        "## $today  $($flag.mutant.File):$($flag.mutant.Line)`n`n" +
        "- [ ] Checked by a human`n" +
        "- Real code: ``$($flag.mutant.Original)```n" +
        "- Mutated code: ``$($flag.mutant.Replacement)```n" +
        "- Why the mutated code may be the correct one: $($flag.reason)`n"
    }
    [IO.File]::AppendAllText((Join-Path $DocsDir 'bug-flags.md'), "`n" + ($entries -join "`n"), (New-Object Text.UTF8Encoding($false)))
}

# --- 5. The CI checks from the definition of done, on the result. Formatting is fixed in the
#        test projects only; production code is verified, never rewritten.
$solution = Join-Path $RepoRoot 'CarLoan.sln'
$testFolders = @($TestDirs | ForEach-Object { "$_/" })
$checks = [ordered]@{
    'dotnet format (tests)'  = { & dotnet format $solution --include $testFolders --verbosity quiet }
    'dotnet format --verify' = { & dotnet format $solution --verify-no-changes --verbosity quiet }
    'dotnet build'           = { & dotnet build $solution --configuration Release --nologo --verbosity quiet }
    'dotnet test'            = { & dotnet test $solution --configuration Release --no-build --nologo --verbosity quiet }
}
$failedChecks = @()
foreach ($name in $checks.Keys) {
    & $checks[$name] | Out-File (Join-Path $PassDir 'checks.log') -Encoding utf8 -Append
    if ($LASTEXITCODE -ne 0) { $failedChecks += $name }
}

# --- 6. The monthly sample, when it is due. The script picks the items, not an agent.
$sampleOutput = & (Join-Path $PSScriptRoot 'Get-Sample.ps1') -OnlyIfDue

# --- 7. One line per pass, so a human can watch the excuse count over time.
$allExcuses = @(Read-State 'excuses')
$allParked = @(Read-State 'parked')
$perProject = foreach ($project in @($allExcuses + $allParked | ForEach-Object { $_.project } | Sort-Object -Unique)) {
    '{0}: {1} excused, {2} parked' -f $project,
        @($allExcuses | Where-Object { $_.project -eq $project }).Count,
        @($allParked | Where-Object { $_.project -eq $project }).Count
}
$totals = if ($perProject) { $perProject -join '; ' } else { 'nothing excused or parked' }
$mode = (Read-Pass 'info' | Select-Object -First 1).mode
if ($queue.Count -gt 0) {
    $logLine = "| $today | $mode | $($queue.Count) | $($killedKeys.Count) | $($excused.Count) | $($toPark.Count) | $($bugFlags.Count) | $totals |`n"
    [IO.File]::AppendAllText((Join-Path $DocsDir 'log.md'), $logLine, (New-Object Text.UTF8Encoding($false)))
}

Save-Accepted
$stagedCount = @(Invoke-Git diff --cached --name-only).Count

Write-Output "Pass finished. Survivors in the queue: $($queue.Count)"
Write-Output "  killed by a new test:  $($killedKeys.Count)"
Write-Output "  excused as equivalent: $($excused.Count)"
Write-Output "  parked:                $($toPark.Count)"
Write-Output "  flagged as a possible bug (docs/mutation/bug-flags.md): $($bugFlags.Count)"
Write-Output "Standing totals. $totals"
if ($sampleOutput) { $sampleOutput | ForEach-Object { Write-Output $_ } }
if (Test-Path (Join-Path $PassDir 'run\reports\mutation-report.html')) {
    Write-Output 'Stryker report for a human to read: StrykerOutput/mutation/run/reports/mutation-report.html'
}
if ($failedChecks.Count -gt 0) {
    Write-Output "CHECKS FAILED: $($failedChecks -join ', '). Log: StrykerOutput/mutation/checks.log. Do not commit until they pass."
    exit 1
}
if ($stagedCount -eq 0) { Write-Output 'The CI checks pass. The pass changed nothing, so there is nothing to commit.' }
else { Write-Output 'All changes are staged and the CI checks pass. Commit them; do not push.' }
exit 0
