# Starts a mutation pass: checks the tree is clean, drops excuses whose code has changed,
# runs Stryker and writes the queue of survivors to work through.
#
#   powershell -ExecutionPolicy Bypass -File tools/mutation/Start-Pass.ps1 [-Full]
#
# Runs Stryker on what changed since the main branch. Runs it on everything when -Full is
# given, or when the last full run is older than fullRunMaxAgeDays in settings.json.
# Prints the queue, never the score. Process and reasons: docs/mutation/README.md.
param([switch]$Full)

. (Join-Path $PSScriptRoot 'Common.ps1')

$lastFullPath = Join-Path $RepoRoot 'StrykerOutput\last-full-run.txt'

function Remove-StaleExcuses {
    # An excuse covers the exact line it was written for. If that line has changed, or the
    # comment is not one a script placed, the comment goes and the mutant is judged afresh.
    $ledger = @(Read-State 'excuses')
    $kept = @()
    $removed = @()
    $sources = @(Invoke-Git ls-files '*.cs' | Where-Object { $_ -notmatch $TestPathPattern })

    foreach ($file in $sources) {
        $path = Join-Path $RepoRoot $file
        $text = [IO.File]::ReadAllText($path)
        if ($text -notmatch 'Stryker') { continue }

        $newline = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }
        $lines = $text -split "`r?`n"
        $output = New-Object System.Collections.Generic.List[string]

        for ($i = 0; $i -lt $lines.Count; $i++) {
            if ($lines[$i] -notmatch '^\s*//\s*Stryker\b') { $output.Add($lines[$i]); continue }

            $codeIndex = $i + 1
            while ($codeIndex -lt $lines.Count -and $lines[$codeIndex] -match '^\s*//\s*Stryker\b') { $codeIndex++ }
            $codeHash = if ($codeIndex -lt $lines.Count) { Get-Sha1 $lines[$codeIndex].Trim() } else { '' }

            $entry = $ledger | Where-Object { $_.file -eq $file -and $_.comment -eq $lines[$i].Trim() -and $_.lineHash -eq $codeHash } | Select-Object -First 1
            if ($entry) { $output.Add($lines[$i]); $kept += $entry }
            else { $removed += "${file}:$($i + 1)  $($lines[$i].Trim())" }
        }

        if ($output.Count -ne $lines.Count) { Write-Text $path ($output -join $newline) }
    }

    if ($removed.Count -gt 0 -or $kept.Count -ne $ledger.Count) { Write-State 'excuses' $kept }
    return $removed
}

function Write-Brief($mutant) {
    # What an agent is told about one mutant. No score, no counts, nothing about the rest of the queue.
    $lines = [IO.File]::ReadAllText((Join-Path $RepoRoot $mutant.File)) -split "`r?`n"
    $from = [Math]::Max(1, $mutant.Line - 8)
    $to = [Math]::Min($lines.Count, $mutant.Line + 8)
    $context = for ($n = $from; $n -le $to; $n++) {
        $marker = if ($n -eq $mutant.Line) { '>' } else { ' ' }
        '{0} {1,4}: {2}' -f $marker, $n, $lines[$n - 1]
    }
    $covered = if ($mutant.CoveredBy.Count -gt 0) { ($mutant.CoveredBy | Select-Object -First 10) -join "`n  " } else { 'Stryker reported none' }

    $brief = @"
# Mutant $($mutant.Key)

File: $($mutant.File), line $($mutant.Line), column $($mutant.Column)
Mutation: $($mutant.Mutator)

Original code:
  $($mutant.Original)
Mutated code:
  $($mutant.Replacement)

Tests Stryker saw running this code:
  $covered

Source around the mutant:
$($context -join "`n")
"@
    Write-Text (Join-Path $PassDir "briefs\$($mutant.Key).md") ($brief + "`n")
}

# --- 1. A pass starts from a clean tree, so everything that changes during it is the pass's own.
if (@(Invoke-Git status --porcelain).Count -gt 0) {
    Write-Output 'STOP: the working tree is not clean. Commit or stash first; a mutation pass starts from a clean tree.'
    exit 2
}

if (Test-Path $PassDir) { Remove-Item $PassDir -Recurse -Force }
New-Item -ItemType Directory -Force $PassDir | Out-Null

# --- 2. Excuses that no longer match their code are removed before Stryker looks.
$removedExcuses = @(Remove-StaleExcuses)
Save-Accepted

# --- 3. Run Stryker: on the changes, or on everything when a full run is due.
$fullIsDue = -not (Test-Path $lastFullPath) -or
    ((Get-Date) - [datetime]::Parse((Get-Content $lastFullPath -Raw).Trim())).TotalDays -gt $Settings.fullRunMaxAgeDays
$runFull = $Full -or $fullIsDue

# The changed files come from git here, not from Stryker's own "since" option: in a
# solution with several projects that option skipped the changed file's mutants, and it
# re-tested every mutant whenever any non-C# file had changed.
$changedSources = @()
if (-not $runFull) {
    $forkPoint = (Invoke-Git merge-base $Settings.sinceTarget HEAD | Select-Object -First 1).Trim()
    $changedSources = @(Invoke-Git diff --cached --name-only --diff-filter=d $forkPoint -- '*.cs' |
            Where-Object { $_ -match '^CarLoan\.\w+/' -and $_ -notmatch $TestPathPattern })
}

$reportPath = if ($runFull) { Invoke-Stryker 'run' @() }
elseif ($changedSources.Count -gt 0) { Invoke-Stryker 'run' (Get-MutateArguments $changedSources) }
else { $null }
$mutants = if ($reportPath) { @(Read-Mutants $reportPath) } else { @() }

# --- 4. The queue: survivors nobody has parked. A full run also retires parked entries whose code is gone.
$parked = @(Read-State 'parked')
if ($runFull -and $reportPath) {
    Write-Text $lastFullPath ((Get-Date).ToString('s') + "`n")
    $liveKeys = @($mutants | ForEach-Object { $_.Key })
    $stillParked = @($parked | Where-Object { $liveKeys -contains $_.key })
    if ($stillParked.Count -ne $parked.Count) {
        Write-State 'parked' $stillParked
        Save-Accepted
        $parked = $stillParked
    }
}

$parkedKeys = @($parked | ForEach-Object { $_.key })
$queue = @($mutants | Where-Object { $_.Status -in 'Survived', 'NoCoverage' -and $parkedKeys -notcontains $_.Key })

Write-Pass 'queue' $queue
Write-Pass 'info' @([pscustomobject]@{ mode = $(if ($runFull) { 'full' } else { 'since' }) })
foreach ($mutant in $queue) { Write-Brief $mutant }

# --- 5. Report what there is to do.
Write-Output "Mode: $(if ($runFull) { 'full run' } else { "$($changedSources.Count) engine file(s) changed since $($Settings.sinceTarget)" })"
if ($removedExcuses.Count -gt 0) {
    Write-Output "Removed $($removedExcuses.Count) excuse comment(s) whose code changed or that no script placed:"
    $removedExcuses | ForEach-Object { Write-Output "  $_" }
}
Write-Output "Queue: $($queue.Count) mutant(s) to work through. Briefs: StrykerOutput/mutation/briefs/<key>.md"
foreach ($mutant in $queue) {
    Write-Output ("  {0}  {1}:{2}  {3}" -f $mutant.Key, $mutant.File, $mutant.Line, $mutant.Mutator)
}
