#Requires -Version 7.0
<#
.SYNOPSIS
    KITOS code quality gate. Same checks run locally, by AI agents and in CI (.github/workflows/pr-quality.yml).

.DESCRIPTION
    1. Format      - dotnet format (whitespace) on changed *.cs files (boy-scout rule; legacy files are not reformatted wholesale)
    2. Build       - KITOS.sln and Kitos_PubSub.sln with -p:KitosQualityGate=true: every compiler/analyzer warning is an error
    3. Conventions - KITOS-specific checks on the diff (debug output, suppressions, relaxed analyzer rules,
                     migrations, database scripts, tests accompanying changes). Deleted files are included.
    4. Tests       - unit test projects of both solutions

    See docs/CODE_QUALITY.md.

.PARAMETER BaseRef
    Git ref the branch is compared to when finding changed files. Default: origin/master.

.PARAMETER Fix
    Apply formatting fixes instead of only verifying.

.PARAMETER SkipBuild
    Skip the build step (useful for a quick format/conventions check).

.PARAMETER SkipTests
    Skip running unit tests.

.EXAMPLE
    pwsh ./scripts/quality-check.ps1
    pwsh ./scripts/quality-check.ps1 -Fix -SkipTests
#>
[CmdletBinding()]
param(
    [string]$BaseRef = "origin/master",
    [switch]$Fix,
    [switch]$SkipBuild,
    [switch]$SkipTests,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

function Stop-Gate([string]$Message) {
    Write-Host "Quality gate ERROR: $Message" -ForegroundColor Red
    exit 1
}

# Native commands do not reliably terminate on non-zero exit codes - check every git call explicitly
function Invoke-Git {
    $output = & git @args
    if ($LASTEXITCODE -ne 0) { Stop-Gate "git $($args -join ' ') failed with exit code $LASTEXITCODE" }
    $output
}

$repoRoot = Invoke-Git rev-parse --show-toplevel
Set-Location $repoRoot.Trim()

$solutions = @("KITOS.sln", "Kitos_PubSub.sln")
$unitTestProjects = @(
    "Tests.Unit.Core.ApplicationServices/Tests.Unit.Core.csproj",
    "Tests.Unit.Presentation.Web/Tests.Unit.Presentation.Web.csproj",
    "PubSub.Test/PubSub.Test.csproj"
)
# Presentation.Web pins a Windows RID unless ContainerBuild=true (same switch the Dockerfile uses)
$platformArgs = if ($IsWindows) { @() } else { @("-p:ContainerBuild=true") }

$nonProductionPath = '^(Tests\.|Tools\.|PubSub\.Test/|DeploymentTools/)|/Migrations/'
$logicPath = '^(Core\.ApplicationServices|Core\.DomainServices|Core\.DomainModel|Core\.BackgroundJobs|Presentation\.Web/Controllers|PubSub\.Application\.Services|PubSub\.Application\.Api/Controllers|PubSub\.Core\.[^/]+)/'
$testPath = '^(Tests\.|PubSub\.Test/)'

$results = [System.Collections.Generic.List[object]]::new()
function Add-Result([string]$Check, [string]$Status, [string[]]$Details = @()) {
    $max = 10
    $text = (@($Details | Select-Object -First $max) -join "; ")
    if ($Details.Count -gt $max) { $text += "; (+$($Details.Count - $max) more)" }
    $results.Add([pscustomobject]@{ Check = $Check; Status = $Status; Details = $text })
    $color = switch ($Status) { "PASS" { "Green" } "WARN" { "Yellow" } "SKIP" { "DarkGray" } default { "Red" } }
    Write-Host ("[{0}] {1} {2}" -f $Status, $Check, $text) -ForegroundColor $color
}
function Write-Section([string]$Title) { Write-Host "`n=== $Title ===" -ForegroundColor Cyan }

#region Changed files
Write-Section "Changed files (vs $BaseRef)"
$mergeBase = git merge-base $BaseRef HEAD 2>$null
if ($LASTEXITCODE -ne 0 -or -not $mergeBase) {
    if ($BaseRef -notmatch '^(?<remote>[^/]+)/(?<branch>.+)$') { Stop-Gate "cannot resolve merge base with '$BaseRef'" }
    Write-Host "Could not resolve '$BaseRef' - fetching it" -ForegroundColor Yellow
    Invoke-Git fetch --no-tags --quiet $Matches.remote $Matches.branch | Out-Null
    $mergeBase = Invoke-Git merge-base $BaseRef HEAD
}
$mergeBase = "$mergeBase".Trim()
if ($mergeBase -notmatch '^[0-9a-f]{40,64}$') { Stop-Gate "invalid merge base '$mergeBase' for '$BaseRef'" }

# Working tree vs merge base: committed + uncommitted changes, including deletions; plus untracked files
$changes = [System.Collections.Generic.List[object]]::new()
foreach ($entry in (Invoke-Git -c core.quotepath=off diff --name-status --no-renames $mergeBase)) {
    $status, $path = $entry -split "`t", 2
    $changes.Add([pscustomobject]@{ Status = $status.Substring(0, 1); Path = $path })
}
$untracked = @(Invoke-Git -c core.quotepath=off ls-files --others --exclude-standard)
foreach ($path in $untracked) { $changes.Add([pscustomobject]@{ Status = "A"; Path = $path }) }

$allChanged = @($changes | ForEach-Object Path | Sort-Object -Unique)          # includes deleted files
$deleted = @($changes | Where-Object Status -eq "D" | ForEach-Object Path)
$existingChanged = @($changes | Where-Object Status -ne "D" | ForEach-Object Path |
    Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Sort-Object -Unique)
$changedCsToFormat = @($existingChanged | Where-Object { $_ -like "*.cs" -and $_ -notmatch "/Migrations/|/(obj|bin)/" })

# Added line numbers per existing C# file (used to inspect complete statements in the current file)
$addedLineNumbers = @{}
$currentFile = $null
foreach ($line in (Invoke-Git -c core.quotepath=off diff --unified=0 --no-renames $mergeBase -- "*.cs")) {
    if ($line -match '^\+\+\+ (?:b/(?<p>.+)|/dev/null)$') {
        $currentFile = $Matches.p
        if ($currentFile -and -not $addedLineNumbers.ContainsKey($currentFile)) { $addedLineNumbers[$currentFile] = [System.Collections.Generic.HashSet[int]]::new() }
        continue
    }
    if ($currentFile -and $line -match '^@@ -\d+(?:,\d+)? \+(?<start>\d+)(?:,(?<count>\d+))? @@') {
        $count = if ($Matches.count) { [int]$Matches.count } else { 1 }
        for ($i = 0; $i -lt $count; $i++) { [void]$addedLineNumbers[$currentFile].Add([int]$Matches.start + $i) }
    }
}
foreach ($path in ($untracked | Where-Object { $_ -like "*.cs" })) {
    $set = [System.Collections.Generic.HashSet[int]]::new()
    $lineCount = @(Get-Content -LiteralPath $path).Count
    for ($i = 1; $i -le $lineCount; $i++) { [void]$set.Add($i) }
    $addedLineNumbers[$path] = $set
}
Write-Host "$($allChanged.Count) changed file(s) ($($deleted.Count) deleted), $($changedCsToFormat.Count) C# file(s) to format"
#endregion

#region 1. Format
Write-Section "1. Formatting (dotnet format whitespace, changed files)"
if ($changedCsToFormat.Count -eq 0) {
    Add-Result "Format" "SKIP" "no changed C# files"
}
else {
    # Folder mode: covers files from every solution and needs no project load
    $formatArgs = @("format", "whitespace", ".", "--folder", "--include") + $changedCsToFormat
    if (-not $Fix) { $formatArgs += "--verify-no-changes" }
    & dotnet @formatArgs
    if ($LASTEXITCODE -eq 0) { Add-Result "Format" "PASS" $(if ($Fix) { "fixes applied" } else { @() }) }
    else { Add-Result "Format" "FAIL" "run: pwsh ./scripts/quality-check.ps1 -Fix -SkipBuild -SkipTests" }
}
#endregion

#region 2. Build
Write-Section "2. Build with warnings as errors"
if ($SkipBuild) {
    Add-Result "Build (0 warnings)" "SKIP"
}
else {
    $failedSolutions = @()
    foreach ($solution in $solutions) {
        & dotnet build $solution -c $Configuration -nologo -p:KitosQualityGate=true @platformArgs
        if ($LASTEXITCODE -ne 0) { $failedSolutions += $solution }
    }
    if ($failedSolutions.Count) { Add-Result "Build (0 warnings)" "FAIL" "$($failedSolutions -join ', ') - fix the reported warnings/errors, do not suppress them" }
    else { Add-Result "Build (0 warnings)" "PASS" ($solutions -join ", ") }
}
#endregion

#region 3. KITOS conventions
Write-Section "3. KITOS conventions"
$fileLines = @{}
function Get-FileLines([string]$Path) {
    if (-not $fileLines.ContainsKey($Path)) { $fileLines[$Path] = @(Get-Content -LiteralPath $Path) }
    , $fileLines[$Path]
}
$prodCsFiles = @($addedLineNumbers.Keys | Where-Object { $_ -notmatch $nonProductionPath -and (Test-Path -LiteralPath $_ -PathType Leaf) } | Sort-Object)

# 3a. Debug leftovers in production code
$debug = foreach ($path in $prodCsFiles) {
    $lines = Get-FileLines $path
    foreach ($n in $addedLineNumbers[$path]) {
        if ($n -le $lines.Count -and $lines[$n - 1] -match 'Console\.Write|Debugger\.Break|Debug\.Write') { "${path}:${n}: $($lines[$n - 1].Trim())" }
    }
}
$debug = @($debug)
if ($debug.Count) { Add-Result "No debug output" "FAIL" $debug } else { Add-Result "No debug output" "PASS" }

# 3b. Warning suppressions must be justified.
#     #pragma warning disable: trailing "// reason" or a "//" comment on the line above.
#     [SuppressMessage(...)]: the complete (possibly multi-line) attribute must contain a non-empty Justification.
$unjustified = foreach ($path in $prodCsFiles) {
    $lines = Get-FileLines $path
    $added = $addedLineNumbers[$path]
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line -match '#pragma\s+warning\s+disable' -and $added.Contains($i + 1)) {
            $previous = if ($i -gt 0) { $lines[$i - 1].Trim() } else { "" }
            if ($line -notmatch '#pragma\s+warning\s+disable[^/]*//\s*\S' -and $previous -notmatch '^//\s*\S') { "${path}:$($i + 1): $($line.Trim())" }
        }
        $start = $line.IndexOf("SuppressMessage(")
        if ($start -ge 0) {
            # Collect the attribute until its parentheses are balanced
            $text = ""; $depth = 0; $end = $i
            for ($j = $i; $j -lt [Math]::Min($lines.Count, $i + 30); $j++) {
                $segment = if ($j -eq $i) { $lines[$j].Substring($start) } else { $lines[$j] }
                $text += " " + $segment
                $depth += ($segment.ToCharArray() | Where-Object { $_ -eq '(' }).Count - ($segment.ToCharArray() | Where-Object { $_ -eq ')' }).Count
                $end = $j
                if ($depth -le 0) { break }
            }
            $touched = $false
            for ($k = $i + 1; $k -le $end + 1; $k++) { if ($added.Contains($k)) { $touched = $true; break } }
            if ($touched -and $text -notmatch 'Justification\s*=\s*(?!"\s*")\S') { "${path}:$($i + 1): $($line.Trim())" }
        }
    }
}
$unjustified = @($unjustified)
if ($unjustified.Count) { Add-Result "Justified suppressions" "FAIL" (@("add a justification (// comment or Justification = ""..."")") + $unjustified) }
else { Add-Result "Justified suppressions" "PASS" }

# 3c. Analyzer rules must not be relaxed. Compares the effective severity settings of every changed
#     (incl. added/deleted) .editorconfig/.globalconfig between merge base and working tree, and flags
#     changes to warning/analyzer settings in MSBuild files and to the gate itself.
$severityRanks = @{ none = 0; silent = 1; refactoring = 1; suggestion = 2; warning = 3; error = 4 }
function Get-SeverityRank([string]$Key, [string]$Value) {
    if ($Key -match '(^|\] )generated_code$') { return $(if ($Value -eq "true") { 0 } else { 4 }) }
    $severity = if ($Value -match ':(?<s>[a-z]+)$') { $Matches.s } else { $Value }
    $severityRanks[$severity]
}
function Get-AnalyzerSettings([string[]]$Lines) {
    $settings = [ordered]@{}
    $section = "(global)"
    foreach ($raw in $Lines) {
        if ($raw -match '^\s*([#;].*)?$') { continue }
        if ($raw -match '^\s*\[(?<s>.+)\]\s*$') { $section = $Matches.s.Trim(); continue }
        if ($raw -notmatch '^\s*(?<k>[^=#;]+?)\s*=\s*(?<v>[^#;]*?)\s*([#;].*)?$') { continue }
        $key = $Matches.k.ToLowerInvariant(); $value = $Matches.v
        $isSeverity = $key -match '^dotnet_(diagnostic|analyzer_diagnostic)\..*severity$' -or
            $key -match '^dotnet_naming_rule\..+\.severity$' -or
            $key -in @("generated_code", "root") -or
            $value -match ':(none|silent|refactoring|suggestion|warning|error|default)$'
        $isNamingDefinition = $key -match '^dotnet_naming_(rule|symbols|style)\.' -and -not $isSeverity
        if ($isSeverity -or $isNamingDefinition) { $settings["[$section] $key"] = $value }
    }
    $settings
}
function Get-BaseFileLines([string]$Path) {
    git cat-file -e "${mergeBase}:$Path" 2>$null
    if ($LASTEXITCODE -ne 0) { return @() }
    @(Invoke-Git show "${mergeBase}:$Path")
}

$relaxations = [System.Collections.Generic.List[string]]::new()
foreach ($path in ($allChanged | Where-Object { $_ -match '(^|/)\.editorconfig$|\.globalconfig$' })) {
    $old = Get-AnalyzerSettings (Get-BaseFileLines $path)
    $new = Get-AnalyzerSettings $(if (Test-Path -LiteralPath $path -PathType Leaf) { Get-Content -LiteralPath $path } else { @() })
    foreach ($key in $old.Keys) {
        $isNaming = $key -match '\] dotnet_naming_(rule|symbols|style)\.' -and $key -notmatch '\.severity$'
        if ($key -match '\] root$') { continue }
        if (-not $new.Contains($key)) {
            $oldRank = Get-SeverityRank $key $old[$key]
            if ($isNaming -or $null -eq $oldRank -or $oldRank -ge 3) { $relaxations.Add("${path}: removed $key = $($old[$key])") }
        }
        elseif ($new[$key] -cne $old[$key]) {
            if ($isNaming) { $relaxations.Add("${path}: $key changed $($old[$key]) -> $($new[$key])"); continue }
            $oldRank = Get-SeverityRank $key $old[$key]; $newRank = Get-SeverityRank $key $new[$key]
            if ($null -eq $newRank -or ($null -ne $oldRank -and $newRank -lt $oldRank)) { $relaxations.Add("${path}: $key lowered $($old[$key]) -> $($new[$key])") }
        }
    }
    foreach ($key in ($new.Keys | Where-Object { -not $old.Contains($_) })) {
        if ($key -match '\] root$') {
            if ($path -ne ".editorconfig" -and $new[$key] -eq "true") { $relaxations.Add("${path}: root = true stops inheritance of the repository .editorconfig") }
            continue
        }
        if ($key -match '\] dotnet_naming_(rule|symbols|style)\.' -and $key -notmatch '\.severity$') { continue }
        $newRank = Get-SeverityRank $key $new[$key]
        if ($null -eq $newRank -or $newRank -lt 3) { $relaxations.Add("${path}: added $key = $($new[$key])") }
    }
}
$warningSettings = '<\s*/?\s*(NoWarn|WarningsNotAsErrors|TreatWarningsAsErrors|MSBuildTreatWarningsAsErrors|WarningLevel|EnableNETAnalyzers|AnalysisLevel|AnalysisMode\w*|EnforceCodeStyleInBuild|RunAnalyzers\w*|CodeAnalysisRuleSet|KitosQualityGate)\b|KitosQualityGate'
foreach ($path in ($allChanged | Where-Object { $_ -match '\.(csproj|props|targets)$' })) {
    if ($path -match '(^|/)Directory\.Build\.(props|targets)$' -and ($changes | Where-Object { $_.Path -eq $path -and $_.Status -in @("A", "D") })) {
        $relaxations.Add("${path}: Directory.Build file added/deleted (affects analyzer settings of all projects below it)")
        continue
    }
    $diffLines = if ($untracked -contains $path) { Get-Content -LiteralPath $path | ForEach-Object { "+$_" } } else { Invoke-Git -c core.quotepath=off diff --unified=0 $mergeBase -- $path }
    $touched = @($diffLines | Where-Object { $_ -match '^[+-](?![+-])' -and $_ -match $warningSettings })
    if ($touched.Count) { $relaxations.Add("${path}: warning/analyzer settings changed: $(($touched | ForEach-Object { $_.Trim() }) -join ' ')") }
}
foreach ($path in ($allChanged | Where-Object { $_ -in @("scripts/quality-check.ps1", ".github/workflows/pr-quality.yml") })) {
    $relaxations.Add("${path}: quality gate definition changed")
}
if ($relaxations.Count) { Add-Result "Analyzer rules not relaxed" "WARN" (@("requires reviewer approval") + $relaxations) }
else { Add-Result "Analyzer rules not relaxed" "PASS" }

# 3d. EF Core migrations: added/deleted migrations must come with their Designer file and an updated model snapshot
$migrationChanges = @($changes | Where-Object { $_.Path -match "Migrations/EfCore/\d+_[^/]+\.cs$" -and $_.Path -notmatch "\.Designer\.cs$" -and $_.Status -in @("A", "D") })
if ($migrationChanges.Count) {
    $snapshotChanged = [bool]($allChanged -match "Migrations/EfCore/KitosContextModelSnapshot\.cs$")
    $inconsistent = @($migrationChanges | Where-Object {
            $designer = $_.Path -replace "\.cs$", ".Designer.cs"
            $designerExists = Test-Path -LiteralPath $designer -PathType Leaf
            ($_.Status -eq "A" -and -not $designerExists) -or ($_.Status -eq "D" -and $designerExists)
        } | ForEach-Object { "$($_.Status) $($_.Path): Designer file not added/deleted with it" })
    if (-not $snapshotChanged) { $inconsistent += "KitosContextModelSnapshot.cs not updated" }
    if ($inconsistent.Count) { Add-Result "Migration consistency" "FAIL" $inconsistent }
    else { Add-Result "Migration consistency" "PASS" ($migrationChanges | ForEach-Object { "$($_.Status) $($_.Path)" }) }
}

# 3e. Manual SQL scripts (added, changed or deleted) - database compatibility (PR checklist)
$sqlScripts = @($changes | Where-Object { $_.Path -match "Migrations/SQLScripts/.+\.sql$" } | ForEach-Object { "$($_.Status) $($_.Path)" })
if ($sqlScripts.Count) { Add-Result "Database compatibility" "WARN" (@("SQL scripts changed - verify PostgreSQL syntax, schema compatibility and data preservation") + $sqlScripts) }

# 3f. Changes (incl. deletions) to business logic should be accompanied by tests
$logicChanged = @($allChanged | Where-Object { $_ -like "*.cs" -and $_ -match $logicPath -and $_ -notmatch "/Migrations/" })
$testsChanged = @($allChanged | Where-Object { $_ -like "*.cs" -and $_ -match $testPath })
if ($logicChanged.Count -and -not $testsChanged.Count) { Add-Result "Tests accompany changes" "WARN" "$($logicChanged.Count) logic file(s) changed without test changes" }
elseif ($logicChanged.Count) { Add-Result "Tests accompany changes" "PASS" }
#endregion

#region 4. Tests
Write-Section "4. Unit tests"
if ($SkipTests) {
    Add-Result "Unit tests" "SKIP"
}
else {
    $failed = @()
    foreach ($project in $unitTestProjects) {
        $testArgs = @("test", $project, "-c", $Configuration, "-nologo", "--logger", "trx", "--results-directory", "TestResults") + $platformArgs
        if (-not $SkipBuild) { $testArgs += "--no-build" }
        & dotnet @testArgs
        if ($LASTEXITCODE -ne 0) { $failed += $project }
    }
    if ($failed.Count) { Add-Result "Unit tests" "FAIL" $failed } else { Add-Result "Unit tests" "PASS" }
}
#endregion

#region Summary
Write-Section "Summary"
$results | Format-Table -AutoSize | Out-String -Width 300 | Write-Host
if ($env:GITHUB_STEP_SUMMARY) {
    $md = @("## KITOS quality gate", "", "| Check | Status | Details |", "|---|---|---|")
    $md += $results | ForEach-Object { "| $($_.Check) | $($_.Status) | $($_.Details -replace '\|', '\|') |" }
    $md | Out-File -FilePath $env:GITHUB_STEP_SUMMARY -Append -Encoding utf8
}
$failures = @($results | Where-Object Status -eq "FAIL")
if ($failures.Count) {
    Write-Host "Quality gate FAILED ($($failures.Count) check(s))" -ForegroundColor Red
    exit 1
}
Write-Host "Quality gate PASSED" -ForegroundColor Green
exit 0
#endregion
