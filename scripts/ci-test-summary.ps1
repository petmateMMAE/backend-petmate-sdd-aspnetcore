<#
.SYNOPSIS
  Resume en $GITHUB_STEP_SUMMARY los resultados TRX de un job de pruebas del pipeline (pm-055).
.DESCRIPTION
  Lee los archivos .trx de -ResultsDir y escribe una tabla Markdown con totales, omitidas (Skipped) y la lista de pruebas
  fallidas. No falla nunca: el resultado del job lo decide `dotnet test`. Las pruebas omitidas (por ejemplo, de integración sin
  secretos de Atlas) se muestran aparte para que no pasen por aprobadas.
  Uso: pwsh ./scripts/ci-test-summary.ps1 -Title "Pruebas unitarias" -ResultsDir TestResults
#>
param(
    [Parameter(Mandatory)][string]$Title,
    [Parameter(Mandatory)][string]$ResultsDir
)

$ErrorActionPreference = 'Continue'
$summaryFile = $env:GITHUB_STEP_SUMMARY
function Write-Summary([string]$text) {
    if ($summaryFile) { Add-Content -Path $summaryFile -Value $text -Encoding utf8 } else { Write-Host $text }
}

$files = @(Get-ChildItem -Path $ResultsDir -Filter '*.trx' -Recurse -ErrorAction SilentlyContinue)
if ($files.Count -eq 0) {
    Write-Summary "### $Title`n`nSin archivos TRX en ``$ResultsDir`` (la compilación o la ejecución no llegó a producir resultados).`n"
    exit 0
}

$total = 0; $passed = 0; $failed = 0; $skipped = 0
$failures = [System.Collections.Generic.List[string]]::new()
foreach ($file in $files) {
    [xml]$trx = Get-Content -Path $file.FullName -Raw
    foreach ($result in @($trx.TestRun.Results.UnitTestResult)) {
        $total++
        switch ($result.outcome) {
            'Passed' { $passed++ }
            'Failed' { $failed++; $failures.Add($result.testName) }
            default { $skipped++ }
        }
    }
}

$status = if ($failed -gt 0) { 'FALLÓ' } elseif ($total -eq 0) { 'SIN PRUEBAS' } else { 'correcto' }
Write-Summary "### $Title — $status`n`n| Total | Aprobadas | Fallidas | Omitidas |`n|---:|---:|---:|---:|`n| $total | $passed | $failed | $skipped |`n"
if ($skipped -gt 0) {
    Write-Summary "> $skipped prueba(s) omitida(s) (Skipped): no cuentan como aprobadas. Revise el motivo en el registro del job.`n"
}
if ($failures.Count -gt 0) {
    Write-Summary "<details open><summary>Pruebas fallidas ($($failures.Count))</summary>`n"
    $failures | Select-Object -First 50 | ForEach-Object { Write-Summary "- ``$_``" }
    Write-Summary "`n</details>`n"
}
exit 0
