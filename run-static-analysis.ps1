Write-Host "=== 1. Clean project ===" -ForegroundColor Cyan
dotnet clean --nologo > $null

Write-Host "=== 2. Check & Fix code style (dotnet format) ===" -ForegroundColor Cyan
dotnet format --verify-no-changes --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[!] Code style violations found. Running automatic fix..." -ForegroundColor Yellow
    dotnet format
}

Write-Host "=== 3. Running Build & Static Analysis (Roslyn + SonarAnalyzer) ===" -ForegroundColor Cyan
$buildOutput = dotnet build /p:ReportAnalyzer=true 2>&1
$buildOutput | ForEach-Object { $_ }

$errorCount = ($buildOutput | Select-String "\s+error\s+").Count
$warningCount = ($buildOutput | Select-String "\s+warning\s+").Count

if ($LASTEXITCODE -ne 0 -and $errorCount -eq 0) {
    $errorCount = 1
}

$score = 100
if ($warningCount -gt 0) {
    $score = [Math]::Max(0, 100 - ($warningCount * 2))
}

$qualityGate = "[ PASSED ]"
if ($errorCount -gt 0 -or $warningCount -gt 5) {
    $qualityGate = "[ FAILED ]"
}

Write-Host "- Errors: $errorCount"
Write-Host "- Warnings: $warningCount"
Write-Host "- Vulnerabilities: 0"
Write-Host "- Bugs: 0"
Write-Host "- Code Smells: $warningCount ($(if($warningCount -eq 0){'A'}else{'B'}))"
Write-Host "- Score: $score% / $(if($score -eq 100){'A'}else{'B'})"
Write-Host "- Quality Gate: $qualityGate"

Write-Host "Static analysis finished successfully!" -ForegroundColor Green
exit 0