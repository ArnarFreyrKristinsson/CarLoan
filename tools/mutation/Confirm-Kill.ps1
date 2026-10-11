# Judges one test-writer attempt at one mutant. The agent's word is not enough: the attempt
# must pass the guard, pass on the unmutated code every time, and make Stryker report the
# mutant as killed. Otherwise it is undone.
#
#   powershell -ExecutionPolicy Bypass -File tools/mutation/Confirm-Kill.ps1 -Key <mutant key>
#
# Exit 0: KILLED, the tests are kept and staged. Exit 1: the attempt failed and was undone.
# Exit 3: no attempts left for this mutant. Process and reasons: docs/mutation/README.md.
param([Parameter(Mandatory)][string]$Key)

. (Join-Path $PSScriptRoot 'Common.ps1')

function Stop-Attempt([string]$message) {
    # Back to the last accepted state: index first (in case it was touched), then the files.
    $acceptedTree = (Get-Content (Join-Path $PassDir 'accepted-tree.txt') -Raw).Trim()
    Invoke-Git read-tree $acceptedTree | Out-Null
    Undo-Pending
    Write-Output $message
    Write-Output "Attempt $($attempt.used) of $allowed for $Key failed and was undone."
    exit 1
}

$mutant = Read-Pass 'queue' | Where-Object { $_.Key -eq $Key } | Select-Object -First 1
if (-not $mutant) { throw "No mutant with key $Key in this pass's queue." }

# --- 1. Attempts are counted here, by the script, so no mutant gets ground away at.
$attempts = @(Read-Pass 'attempts')
$attempt = $attempts | Where-Object { $_.key -eq $Key } | Select-Object -First 1
if (-not $attempt) {
    $attempt = [pscustomobject]@{ key = $Key; used = 0; hinted = $false; killed = $false }
    $attempts += $attempt
}
$allowed = $Settings.attemptsPerMutant + $(if ($attempt.hinted) { $Settings.hintedAttempts } else { 0 })
if ($attempt.killed) { Write-Output "$Key is already killed."; exit 0 }
if ($attempt.used -ge $allowed) {
    Undo-Pending
    Write-Output "NO ATTEMPTS LEFT for $Key ($($attempt.used) used). Any pending change was undone."
    exit 3
}
$attempt.used++
Write-Pass 'attempts' $attempts

# --- 2. The guard: only test files, only additions, nothing that reaches around the public API.
$guardOutput = & (Join-Path $PSScriptRoot 'Test-WriterDiff.ps1')
if ($LASTEXITCODE -ne 0) { Stop-Attempt ($guardOutput -join "`n") }

$pending = @(Get-PendingPaths)
if ($pending.Count -eq 0) { Stop-Attempt 'NOTHING WRITTEN: the working tree has no new test.' }

$addedTests = @(foreach ($path in $pending) {
        $lines = if (@(Invoke-Git ls-files -- $path).Count -eq 0) { Get-Content (Join-Path $RepoRoot $path) }
        else { Invoke-Git diff --unified=0 -- $path | Where-Object { $_ -match '^\+' } }
        $lines | ForEach-Object { if ($_ -match 'public\s+(?:async\s+)?[\w<>]+\s+(\w+)\s*\(') { $Matches[1] } }
    })

# --- 3. The new tests must pass on the real code, every time. A test that fails here is wrong
#        or flaky, and a test that fails on correct code "kills" every mutant for free.
$solution = Join-Path $RepoRoot 'CarLoan.sln'
& dotnet build $solution --nologo --verbosity quiet | Out-File (Join-Path $PassDir 'build.log') -Encoding utf8
if ($LASTEXITCODE -ne 0) { Stop-Attempt 'DOES NOT BUILD. Log: StrykerOutput/mutation/build.log' }

for ($run = 1; $run -le $Settings.testRepeats; $run++) {
    & dotnet test $solution --no-build --nologo --verbosity quiet | Out-File (Join-Path $PassDir 'test.log') -Encoding utf8
    if ($LASTEXITCODE -ne 0) { Stop-Attempt "TESTS FAIL on the unmutated code (run $run of $($Settings.testRepeats)). Log: StrykerOutput/mutation/test.log" }
}

# --- 4. Stryker decides whether the mutant is dead. Only the mutant's own file is re-run.
$reportPath = Invoke-Stryker 'confirm' (Get-MutateArguments @($mutant.File))
$after = if ($reportPath) { Read-Mutants $reportPath | Where-Object { $_.Key -eq $Key } | Select-Object -First 1 } else { $null }
if (-not $after) { Stop-Attempt "Stryker did not report mutant $Key again. Log: StrykerOutput/mutation/confirm.log" }
if ($after.Status -notin 'Killed', 'Timeout') { Stop-Attempt "NOT KILLED: Stryker still reports $Key as $($after.Status)." }

# --- 5. Keep it: record which tests an agent wrote (the monthly sample draws from this) and stage.
$attempt.killed = $true
Write-Pass 'attempts' $attempts

$agentTests = @(Read-State 'agent-tests')
$agentTests += [pscustomobject]@{
    key      = $Key
    date     = (Get-Date).ToString('yyyy-MM-dd')
    project  = $mutant.Project
    mutant   = "$($mutant.File):$($mutant.Line)  $($mutant.Mutator):  $($mutant.Original)  ->  $($mutant.Replacement)"
    files    = $pending
    tests    = $addedTests
}
Write-State 'agent-tests' $agentTests
Save-Accepted

Write-Output "KILLED: $Key. Kept and staged: $($pending -join ', ')"
exit 0
