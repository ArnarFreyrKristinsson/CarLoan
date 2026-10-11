# Records the judge's verdict on one surviving mutant. It only writes the verdict down;
# End-Pass.ps1 is what acts on it, so the same placement and checks apply to every verdict.
#
#   powershell -ExecutionPolicy Bypass -File tools/mutation/Set-Verdict.ps1 -Key <mutant key> -Verdict <verdict> -Reason "<text>"
#
#   Killable    the reason is a concrete input that tells the two versions apart. Grants the
#               test writer one more attempt, with that input as its hint.
#   Equivalent  the reason is why no valid input can tell them apart. Becomes an excuse comment.
#   Unsure      the mutant is parked: it stays a survivor in the report and is not retried.
#   Bug         the mutated code matches the lender's terms better than the real code. Goes to
#               docs/mutation/bug-flags.md for a human, and no test pins it.
#
# Process and reasons: docs/mutation/README.md.
param(
    [Parameter(Mandatory)][string]$Key,
    [Parameter(Mandatory)][ValidateSet('Killable', 'Equivalent', 'Unsure', 'Bug')][string]$Verdict,
    [string]$Reason = ''
)

. (Join-Path $PSScriptRoot 'Common.ps1')

if (-not (Read-Pass 'queue' | Where-Object { $_.Key -eq $Key })) { throw "No mutant with key $Key in this pass's queue." }

# One line, because an Equivalent reason ends up inside a // comment in production code.
$Reason = ($Reason -replace '\s+', ' ').Trim()
if ($Verdict -ne 'Unsure' -and $Reason.Length -lt 15) { throw "A $Verdict verdict needs its reason spelled out." }
if ($Reason.Length -gt 300) { throw 'Keep the reason under 300 characters.' }

$verdicts = @(Read-Pass 'verdicts' | Where-Object { $_.key -ne $Key })
$verdicts += [pscustomobject]@{ key = $Key; verdict = $Verdict; reason = $Reason }
Write-Pass 'verdicts' $verdicts

if ($Verdict -eq 'Killable') {
    $attempts = @(Read-Pass 'attempts')
    $attempt = $attempts | Where-Object { $_.key -eq $Key } | Select-Object -First 1
    if (-not $attempt) {
        $attempt = [pscustomobject]@{ key = $Key; used = 0; hinted = $false; killed = $false }
        $attempts += $attempt
    }
    $attempt.hinted = $true
    Write-Pass 'attempts' $attempts
}

Write-Output "Recorded: $Key is $Verdict."
