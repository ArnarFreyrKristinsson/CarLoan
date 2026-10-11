# Shared helpers for the mutation-pass scripts. Dot-sourced by the others, never run on its own.
# Process and reasons: docs/mutation/README.md.

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$PassDir = Join-Path $RepoRoot 'StrykerOutput\mutation'   # gitignored: the working state of one pass
$StateDir = Join-Path $PSScriptRoot 'state'               # tracked: what outlives a pass
$DocsDir = Join-Path $RepoRoot 'docs\mutation'            # tracked: what a human reads
$Settings = Get-Content (Join-Path $PSScriptRoot 'settings.json') -Raw | ConvertFrom-Json

# The only place an agent may write. Everything else in the repo is off limits to it.
$TestPathPattern = '^CarLoan\.\w+\.Tests/.+\.cs$'
$TestDirs = @(Get-ChildItem $RepoRoot -Directory -Filter 'CarLoan.*.Tests' | ForEach-Object { $_.Name })

# Stryker's report names a mutator in prose; its disable comment wants the type name.
$MutatorTypes = [ordered]@{
    'Block removal'      = 'Block'
    'Statement'          = 'Statement'
    'Equality'           = 'Equality'
    'Arithmetic'         = 'Arithmetic'
    'String'             = 'String'
    'Linq method'        = 'Linq'
    'Conditional'        = 'Conditional'
    'Logical'            = 'Logical'
    'Boolean'            = 'Boolean'
    'Negate expression'  = 'Boolean'
    'Object initializer' = 'Initializer'
    'Array initializer'  = 'Initializer'
    'Assignment'         = 'Assignment'
    'Unary'              = 'Unary'
    'Update'             = 'Update'
    'Null coalescing'    = 'NullCoalescing'
    'Math'               = 'Math'
}

function Invoke-Git {
    # Runs git from the repo root and fails loudly, so a script never continues on a half-done step.
    $output = & git -C $RepoRoot @args
    if ($LASTEXITCODE -ne 0) { throw "git $($args -join ' ') failed with exit code $LASTEXITCODE" }
    if ($null -eq $output) { return }
    return $output
}

function Get-Sha1([string]$text) {
    $sha = [System.Security.Cryptography.SHA1]::Create()
    try { return -join ($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($text)) | ForEach-Object { $_.ToString('x2') }) }
    finally { $sha.Dispose() }
}

function Read-State([string]$name) {
    # State files are { "items": [...] } so a single entry stays a list through PowerShell's JSON round trip.
    $path = Join-Path $StateDir "$name.json"
    if (-not (Test-Path $path)) { return @() }
    return @((Get-Content $path -Raw | ConvertFrom-Json).items)
}

function Write-State([string]$name, $items) {
    Write-Json (Join-Path $StateDir "$name.json") ([pscustomobject]@{ items = @($items) })
}

function Read-Pass([string]$name) {
    $path = Join-Path $PassDir "$name.json"
    if (-not (Test-Path $path)) { return @() }
    return @((Get-Content $path -Raw | ConvertFrom-Json).items)
}

function Write-Pass([string]$name, $items) {
    Write-Json (Join-Path $PassDir "$name.json") ([pscustomobject]@{ items = @($items) })
}

function Write-Json([string]$path, $value) {
    Write-Text $path ((ConvertTo-Json -InputObject $value -Depth 8) + "`n")
}

function Write-Text([string]$path, [string]$text) {
    # UTF-8 without a BOM, like every other file in the repo. Windows PowerShell's own cmdlets add one.
    $directory = Split-Path $path -Parent
    if (-not (Test-Path $directory)) { New-Item -ItemType Directory -Force $directory | Out-Null }
    [IO.File]::WriteAllText($path, $text, (New-Object Text.UTF8Encoding($false)))
}

function Get-MutatorType([string]$mutatorName) {
    foreach ($prefix in $MutatorTypes.Keys) {
        if ($mutatorName.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) { return $MutatorTypes[$prefix] }
    }
    return $null
}

function Get-ProjectName([string]$relativePath) {
    return $relativePath.Split('/')[0]
}

function Read-Mutants([string]$reportPath) {
    # Flattens Stryker's JSON report into one object per mutant, each with a key that survives
    # line shifts: it is built from the code under the mutant, not from the line number. When
    # that code changes the key changes with it, so an old verdict never covers new code.
    $report = Get-Content $reportPath -Raw | ConvertFrom-Json
    $testNames = @{}
    if ($report.PSObject.Properties.Name -contains 'testFiles' -and $report.testFiles) {
        foreach ($testFile in $report.testFiles.PSObject.Properties) {
            foreach ($test in $testFile.Value.tests) { $testNames[$test.id] = $test.name }
        }
    }

    $mutants = foreach ($file in $report.files.PSObject.Properties) {
        $relativePath = $file.Name.Substring($RepoRoot.Length).TrimStart('\', '/').Replace('\', '/')
        $lines = $file.Value.source -split "`r?`n"
        $seen = @{}
        $ordered = $file.Value.mutants | Sort-Object { $_.location.start.line }, { $_.location.start.column }, { [int]$_.id }

        foreach ($mutant in $ordered) {
            $start = $mutant.location.start
            $end = $mutant.location.end
            $original = Get-SourceSpan $lines $start $end
            $lineText = $lines[$start.line - 1].Trim()
            $identity = "$relativePath|$($mutant.mutatorName)|$original|$($mutant.replacement)|$lineText"
            if ($seen.ContainsKey($identity)) { $seen[$identity]++ } else { $seen[$identity] = 0 }

            $reason = if ($mutant.PSObject.Properties.Name -contains 'statusReason') { $mutant.statusReason } else { '' }
            $coveredBy = if ($mutant.PSObject.Properties.Name -contains 'coveredBy') { @($mutant.coveredBy) } else { @() }

            [pscustomobject]@{
                Key          = (Get-Sha1 "$identity|$($seen[$identity])").Substring(0, 10)
                File         = $relativePath
                Project      = Get-ProjectName $relativePath
                Line         = [int]$start.line
                Column       = [int]$start.column
                Mutator      = $mutant.mutatorName
                Original     = $original
                Replacement  = $mutant.replacement
                LineText     = $lineText
                Status       = $mutant.status
                StatusReason = $reason
                CoveredBy    = @($coveredBy | ForEach-Object { $testNames[$_] } | Where-Object { $_ })
            }
        }
    }
    return @($mutants)
}

function Get-SourceSpan($lines, $start, $end) {
    # Report positions are 1-based and the end column is exclusive.
    if ($start.line -eq $end.line) {
        return $lines[$start.line - 1].Substring($start.column - 1, $end.column - $start.column)
    }
    $parts = @($lines[$start.line - 1].Substring($start.column - 1))
    for ($i = $start.line; $i -lt $end.line - 1; $i++) { $parts += $lines[$i] }
    $parts += $lines[$end.line - 1].Substring(0, $end.column - 1)
    return ($parts -join "`n")
}

function Invoke-Stryker([string]$outputName, [string[]]$extraArguments) {
    # One fixed way to run Stryker, so no caller can add a flag that changes what counts.
    # Returns the path of the JSON report, or $null when Stryker found nothing to mutate.
    $outputDir = Join-Path $PassDir $outputName
    if (Test-Path $outputDir) { Remove-Item $outputDir -Recurse -Force }
    New-Item -ItemType Directory -Force $outputDir | Out-Null

    Push-Location $RepoRoot
    try {
        & dotnet stryker --reporter json --reporter html --output $outputDir @extraArguments |
            Out-File (Join-Path $PassDir "$outputName.log") -Encoding utf8
        $exitCode = $LASTEXITCODE
    }
    finally { Pop-Location }

    $reportPath = Join-Path $outputDir 'reports\mutation-report.json'
    if (Test-Path $reportPath) { return $reportPath }
    if ($exitCode -ne 0) { throw "Stryker failed with exit code $exitCode. Log: $(Join-Path $PassDir "$outputName.log")" }
    return $null
}

function Get-MutateArguments([string[]]$files) {
    # Limits a Stryker run to these source files. Stryker reads each glob relative to the
    # file's own project, so the project folder is dropped from the path.
    return @($files | Sort-Object -Unique | ForEach-Object { '--mutate'; '**/' + ($_ -split '/', 2)[1] })
}

function Get-PendingPaths {
    # What changed since the scripts last accepted something: unstaged edits and untracked files.
    # The index holds accepted work, so this is exactly the attempt under judgement.
    $modified = @(Invoke-Git diff --name-only)
    $untracked = @(Invoke-Git ls-files --others --exclude-standard)
    return @($modified + $untracked | Where-Object { $_ } | Sort-Object -Unique)
}

function Undo-Pending {
    # Puts the working tree back to the last accepted state.
    $modified = @(Invoke-Git diff --name-only | Where-Object { $_ })
    if ($modified.Count -gt 0) { Invoke-Git restore --worktree -- $modified | Out-Null }
    foreach ($path in @(Invoke-Git ls-files --others --exclude-standard | Where-Object { $_ })) {
        Remove-Item (Join-Path $RepoRoot $path) -Force
    }
}

function Save-Accepted {
    # Stages everything and remembers the resulting tree, so a later change to the index that
    # did not come through a script is noticed by Test-WriterDiff.
    Invoke-Git add -A | Out-Null
    $tree = (Invoke-Git write-tree | Select-Object -First 1).Trim()
    Write-Text (Join-Path $PassDir 'accepted-tree.txt') $tree
}
