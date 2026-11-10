# Pruebas negativas de check-sdd.ps1 (pm-060 CA-03). Solo lectura sobre el repositorio: cada caso copia specs/ y el
# script a un directorio temporal, introduce una inconsistencia y comprueba que la validacion falla con el mensaje esperado.
# Uso: powershell -File scripts/check-sdd.tests.ps1   (o pwsh en CI). Exit 0 si todos los casos se comportan como se espera.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$engine = (Get-Process -Id $PID).Path
$utf8 = New-Object System.Text.UTF8Encoding($false)
$failures = 0

function Edit-File($path, [scriptblock]$change) {
  $text = [System.IO.File]::ReadAllText($path, $utf8)
  $new = & $change $text
  if ($new -eq $text) { throw "La mutacion no cambio $path" }
  [System.IO.File]::WriteAllText($path, $new, $utf8)
}

function Invoke-Case([string]$name, [string]$expected, [scriptblock]$mutate) {
  $work = Join-Path ([System.IO.Path]::GetTempPath()) ("check-sdd-" + [guid]::NewGuid().ToString('N').Substring(0, 8))
  try {
    New-Item -ItemType Directory -Path (Join-Path $work 'scripts') | Out-Null
    Copy-Item (Join-Path $root 'scripts/check-sdd.ps1') (Join-Path $work 'scripts')
    Copy-Item (Join-Path $root 'specs') $work -Recurse
    Copy-Item (Join-Path $root 'docs') $work -Recurse
    if ($mutate) { & $mutate (Join-Path $work 'specs') }
    $output = & $engine -NoProfile -ExecutionPolicy Bypass -File (Join-Path $work 'scripts/check-sdd.ps1') 2>&1 | Out-String
    $code = $LASTEXITCODE
    $ok = if ($expected) { $code -ne 0 -and $output -match [regex]::Escape($expected) } else { $code -eq 0 }
    if ($ok) { Write-Host "OK    $name" } else { Write-Host "FALLO $name (exit $code)`n$output"; $script:failures++ }
  }
  finally { Remove-Item $work -Recurse -Force -ErrorAction SilentlyContinue }
}

Invoke-Case 'Copia sin cambios pasa' $null $null

Invoke-Case 'CA duplicado en un Spec' 'pm-047-pagination-convention: CA duplicado' {
  param($s) Edit-File (Join-Path $s 'pm-047-pagination-convention/spec.md') { param($t) $t -replace '(\| `CA-02` \|)', ('| `CA-01` | Duplicado. | T058 |' + "`n" + '$1') }
}

Invoke-Case 'Task con dos Spec principales' 'T058 con mas de un Spec principal' {
  param($s) Edit-File (Join-Path $s 'pm-047-pagination-convention/task.md') { param($t) $t -replace '(- \*\*Spec principal:\*\* pm-047)', "`$1`n- **Spec principal:** pm-048" }
}

Invoke-Case 'Criterio sin fila en la matriz de trazabilidad' 'traceability.md sin pm-047#02' {
  param($s) Edit-File (Join-Path $s 'traceability.md') { param($t) $t -replace '(?m)^\| pm-047#CA-02 \|[^\n]*\n', '' }
}

Invoke-Case 'Matriz que cita un criterio inexistente' 'traceability.md cita pm-047#09' {
  param($s) Edit-File (Join-Path $s 'traceability.md') { param($t) $t -replace '(?m)^(\| pm-047#CA-01 \|)', "| pm-047#CA-09 | T058 | Nueva | Propuesto | prueba |`n`$1" }
}

Invoke-Case 'Matriz con Task propietaria equivocada' 'pm-047#01 asignado a T059' {
  param($s) Edit-File (Join-Path $s 'traceability.md') { param($t) $t -replace '(?m)^\| pm-047#CA-01 \| T058 \|', '| pm-047#CA-01 | T059 |' }
}

Invoke-Case 'Ciclo de dependencias' 'Ciclo de dependencias' {
  param($s) Edit-File (Join-Path $s 'pm-009-response-wrapper-contract/spec.md') { param($t) $t -replace '(## 2\. Dependencias\s*\n)', "`$1`n- [pm-047](../pm-047-pagination-convention/spec.md) - ciclo de prueba`n" }
}

Invoke-Case 'Documento historico alterado' 'Historico alterado' {
  param($s) Edit-File (Join-Path $s '_historico/03-pet-management/task.md') { param($t) $t + "`nalterado`n" }
}

if ($failures -gt 0) { Write-Host "check-sdd.tests: FALLO ($failures casos)"; exit 1 }
Write-Host 'check-sdd.tests: OK - la validacion falla ante cada inconsistencia probada.'
exit 0
