# The referee for one test-writer attempt. It looks at the result in the working tree, not at
# how the agent got there, so it holds whichever tool made the edit.
#
#   powershell -ExecutionPolicy Bypass -File tools/mutation/Test-WriterDiff.ps1
#
# Exit 0: the attempt keeps to the rules and may be tested. Exit 1: it broke one; each is listed.
# Process and reasons: docs/mutation/README.md.

. (Join-Path $PSScriptRoot 'Common.ps1')

# What an added test line may not contain, and what to call it when it does.
$banned = [ordered]@{
    'a Stryker comment (only a script may excuse a mutant)' = 'Stryker'
    'reflection (tests go through the public API)'          = 'System\.Reflection|BindingFlags|\.Get(Field|Method|Property|Member|Constructor|Runtime\w+)s?\(|\.InvokeMember\(|Activator\.CreateInstance|UnsafeAccessor|\bdynamic\b'
    'a mock (tests use the real engine components)'         = '\bMock<|\bMock\.Of\b|\bMoq\b|\.Setup\(|\.Verify\('
    'a skipped test'                                        = '\bSkip\s*='
}

$violations = New-Object System.Collections.Generic.List[string]

# The index is what the scripts last accepted. If it moved, something was staged behind their back.
$acceptedTreePath = Join-Path $PassDir 'accepted-tree.txt'
if (-not (Test-Path $acceptedTreePath)) {
    Write-Output 'STOP: no pass is running. Start one with Start-Pass.ps1.'
    exit 2
}
$acceptedTree = (Get-Content $acceptedTreePath -Raw).Trim()
$currentTree = (Invoke-Git write-tree | Select-Object -First 1).Trim()
if ($acceptedTree -ne $currentTree) { $violations.Add('The git index was changed outside the scripts.') }

$untracked = @(Invoke-Git ls-files --others --exclude-standard)

foreach ($path in @(Get-PendingPaths)) {
    if ($path -notmatch $TestPathPattern) {
        $violations.Add("${path}: only test files may change. Production code, config, scripts and docs are off limits.")
        continue
    }

    if ($untracked -contains $path) {
        $added = @(Get-Content (Join-Path $RepoRoot $path))
    }
    else {
        $diff = @(Invoke-Git diff --unified=0 -- $path)
        $added = @($diff | Where-Object { $_ -match '^\+' -and $_ -notmatch '^\+\+\+' } | ForEach-Object { $_.Substring(1) })
        $removedCount = @($diff | Where-Object { $_ -match '^-' -and $_ -notmatch '^---' }).Count
        if ($removedCount -gt 0) {
            $violations.Add("${path}: $removedCount existing line(s) removed or rewritten. A mutation test is added; existing tests stay as they are.")
        }
    }

    foreach ($rule in $banned.Keys) {
        $hit = $added | Where-Object { $_ -match $banned[$rule] } | Select-Object -First 1
        if ($hit) { $violations.Add("${path}: adds $rule -> $($hit.Trim())") }
    }
}

if ($violations.Count -gt 0) {
    Write-Output 'REJECTED by the guard:'
    $violations | ForEach-Object { Write-Output "  $_" }
    exit 1
}

Write-Output 'Guard passed: only test files changed, and only by adding.'
exit 0
