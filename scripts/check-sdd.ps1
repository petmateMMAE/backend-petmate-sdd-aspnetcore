# Validacion de integridad SDD (inicial). Solo lectura. Uso: powershell -File scripts/check-sdd.ps1
# Comprueba IDs, dependencias, ciclos, Tasks, criterios de aceptacion, trazabilidad y preservacion historica.
# Se ejecuta en CI (job "3 · Integridad SDD" de pipeline.yml, pm-060 / T072). Pruebas negativas: scripts/check-sdd.tests.ps1.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$specsDir = Join-Path $root 'specs'
$utf8 = New-Object System.Text.UTF8Encoding($false)
function Read-Text($p) { [System.IO.File]::ReadAllText($p, $utf8) }
$errors = New-Object System.Collections.Generic.List[string]
function Fail($m) { $script:errors.Add($m) }

$dirs = Get-ChildItem $specsDir -Directory | Where-Object { $_.Name -match '^pm-\d{3}-[a-z0-9-]+$' } | Sort-Object Name
$ids = @($dirs | ForEach-Object { [int]$_.Name.Substring(3, 3) })
if ($ids.Count -eq 0) { Fail 'No hay Specs pm-NNN.' }
for ($i = 0; $i -lt $ids.Count; $i++) { if ($ids[$i] -ne ($i + 1)) { Fail "IDs de Spec no consecutivos cerca de pm-$('{0:D3}' -f $ids[$i])"; break } }

$deps = @{}; $caCount = @{}; $caOwner = @{}; $specState = @{}
$allowed = 'Propuesto','En revisi','Aprobado','En progreso','Bloqueado','Verificado','Cerrado','Reemplazado'
foreach ($d in $dirs) {
  $id = $d.Name.Substring(3, 3)
  $spec = Join-Path $d.FullName 'spec.md'; $task = Join-Path $d.FullName 'task.md'
  if (-not (Test-Path $spec)) { Fail "$($d.Name): falta spec.md"; continue }
  if (-not (Test-Path $task)) { Fail "$($d.Name): falta task.md"; continue }
  $st = Read-Text $spec
  if ((Read-Text $task).Length -lt 100) { Fail "$($d.Name): task.md vacio" }
  if ($st -notmatch '## 1\. Objetivo\s+\S') { Fail "$($d.Name): sin objetivo" }
  $m = [regex]::Match($st, '> Estado: ([^|]+)\|')
  if (-not $m.Success) { Fail "$($d.Name): sin estado" } else { $e = $m.Groups[1].Value.Trim(); if (-not ($allowed | Where-Object { $e.StartsWith($_) })) { Fail "$($d.Name): estado no permitido '$e'" } }
  $sec = [regex]::Match($st, '## 2\. Dependencias(.*?)## 3\.', 'Singleline').Groups[1].Value
  $deps[$id] = @([regex]::Matches($sec, '\[pm-(\d{3})\]') | ForEach-Object { $_.Groups[1].Value })
  $cas = @([regex]::Matches($st, '\| `CA-(\d{2})` \|'))
  $caCount[$id] = $cas.Count
  if ($cas.Count -lt 1) { Fail "$($d.Name): sin criterios de aceptacion" }
  # CA sin duplicados y numerados de forma consecutiva desde CA-01 (pm-060 CA-01).
  $caNums = @($cas | ForEach-Object { [int]$_.Groups[1].Value })
  for ($n = 0; $n -lt $caNums.Count; $n++) { if ($caNums[$n] -ne ($n + 1)) { Fail "$($d.Name): CA duplicado o no consecutivo cerca de CA-$('{0:D2}' -f $caNums[$n])"; break } }
}
foreach ($k in $deps.Keys) { foreach ($x in $deps[$k]) { if (-not $deps.ContainsKey($x)) { Fail "pm-$k depende de pm-$x inexistente" } elseif ($x -eq $k) { Fail "pm-$k depende de si mismo" } } }
# ciclos (Kahn)
$left = New-Object System.Collections.ArrayList; foreach ($k in $deps.Keys) { [void]$left.Add($k) }
$progress = $true
while ($left.Count -gt 0 -and $progress) {
  $progress = $false
  foreach ($k in @($left)) { $pending = @($deps[$k] | Where-Object { $left -contains $_ }); if ($pending.Count -eq 0) { $left.Remove($k); $progress = $true } }
}
if ($left.Count -gt 0) { Fail ('Ciclo de dependencias entre: ' + ($left -join ',')) }

# Tasks
$taskIds = @{}; $principal = @{}
foreach ($d in $dirs) {
  $id = $d.Name.Substring(3, 3)
  $tt = Read-Text (Join-Path $d.FullName 'task.md')
  $blocks = [regex]::Split($tt, '(?m)^### ') | Select-Object -Skip 1
  foreach ($b in $blocks) {
    $hm = [regex]::Match($b, '^(T\d{3}) ')
    if (-not $hm.Success) { continue }
    $t = $hm.Groups[1].Value
    if ($taskIds.ContainsKey($t)) { Fail "$t duplicada" }
    $taskIds[$t] = $true
    $pm = [regex]::Match($b, '\*\*Spec principal:\*\* pm-(\d{3})')
    if ([regex]::Matches($b, '\*\*Spec principal:\*\*').Count -gt 1 -or [regex]::Matches($b, '\*\*Spec principal:\*\*[^\r\n]*pm-\d{3}[^\r\n]*pm-\d{3}').Count -gt 0) { Fail "$t con mas de un Spec principal" }
    if (-not $pm.Success) { Fail "$t sin Spec principal" } elseif ($pm.Groups[1].Value -ne $id) { Fail "${t}: Spec principal pm-$($pm.Groups[1].Value) no coincide con la carpeta pm-$id" }
    $principal[$t] = $id
    foreach ($c in [regex]::Matches($b, 'pm-(\d{3})#CA-(\d{2})')) {
      if ($c.Groups[1].Value -ne $id) { Fail "$t cita criterios de otro Spec en 'criterios propios'" }
      else { $key = "$id#$($c.Groups[2].Value)"; if (-not $caOwner.ContainsKey($key)) { $caOwner[$key] = @() }; $caOwner[$key] += $t }
    }
    $stm = [regex]::Match($b, '\*\*Estado:\*\* ([^\r\n]+)')
    if (-not $stm.Success) { Fail "$t sin estado" }
    $tm = [regex]::Match($b, '\*\*Tipo:\*\* (\S+)')
    if ($tm.Success -and $tm.Groups[1].Value -eq 'Retrospectiva' -and $stm.Value -match 'Cerrado') { Fail "$t retrospectiva marcada como cerrada" }
  }
}
$expectedT = 1..97 | ForEach-Object { 'T{0:D3}' -f $_ }
foreach ($t in $expectedT) { if (-not $taskIds.ContainsKey($t)) { Fail "$t no existe" } }
foreach ($t in $taskIds.Keys) { if ($expectedT -notcontains $t) { Fail "$t fuera del rango T001-T097" } }
foreach ($k in $caCount.Keys) { for ($n = 1; $n -le $caCount[$k]; $n++) { $key = "$k#$('{0:D2}' -f $n)"; if (-not $caOwner.ContainsKey($key)) { Fail "pm-$k#CA-$('{0:D2}' -f $n) sin Task propietaria" } elseif ($caOwner[$key].Count -gt 1) { Fail "pm-$k#CA-$('{0:D2}' -f $n) con mas de una Task propietaria: $($caOwner[$key] -join ',')" } } }
foreach ($d in $dirs) { $id = $d.Name.Substring(3, 3); if (-not ($principal.Values | Where-Object { $_ -eq $id })) { Fail "pm-$id sin Tasks" } }

# estados historicos preservados
$t001 = [regex]::Match((Read-Text (Join-Path $specsDir 'pm-001-solution-structure\task.md')), '### T001.*?\*\*Estado:\*\* ([^\r\n]+)', 'Singleline').Groups[1].Value
if ($t001 -notmatch '^(Verificado|Cerrado y aprobado)') { Fail 'T001 no conserva el estado Verificado ni tiene cierre aprobado' }
$t012 = [regex]::Match((Read-Text (Join-Path $specsDir 'pm-055-ci-pull-request-pipeline\task.md')), '### T012.*?\*\*Estado:\*\* ([^\r\n]+)', 'Singleline').Groups[1].Value
if ($t012 -notmatch '^(Implementada; NO cerrada|Cerrado y aprobado)') { Fail 'T012 no conserva el estado Implementada ni tiene cierre aprobado' }

# registro y matriz
$readme = Read-Text (Join-Path $specsDir 'README.md'); $trace = Read-Text (Join-Path $specsDir 'traceability.md')
foreach ($d in $dirs) { if ($readme -notmatch [regex]::Escape($d.Name)) { Fail "README no lista $($d.Name)" } }
foreach ($t in $expectedT) { if ($readme -notmatch "\| $t \|") { Fail "README no lista $t" } }
if ($trace -notmatch 'AC-01') { Fail 'traceability.md sin requisitos academicos' }
# Trazabilidad completa (pm-060 CA-02): cada criterio de cada Spec tiene fila en la matriz con su Task propietaria,
# y la matriz no cita criterios que no existen.
$traceRows = @{}
foreach ($r in [regex]::Matches($trace, '(?m)^\| pm-(\d{3})#CA-(\d{2}) \| (T\d{3}) \|')) { $traceRows["$($r.Groups[1].Value)#$($r.Groups[2].Value)"] = $r.Groups[3].Value }
foreach ($k in $caCount.Keys) {
  for ($n = 1; $n -le $caCount[$k]; $n++) {
    $key = "$k#$('{0:D2}' -f $n)"
    if (-not $traceRows.ContainsKey($key)) { Fail "traceability.md sin pm-$key" }
    elseif ($caOwner.ContainsKey($key) -and $caOwner[$key].Count -eq 1 -and $traceRows[$key] -ne $caOwner[$key][0]) { Fail "traceability.md: pm-$key asignado a $($traceRows[$key]) pero su Task propietaria es $($caOwner[$key][0])" }
  }
}
foreach ($key in $traceRows.Keys) { $p = $key.Split('#'); if (-not $caCount.ContainsKey($p[0]) -or [int]$p[1] -gt $caCount[$p[0]]) { Fail "traceability.md cita pm-$key, que no existe" } }

# enlaces relativos de Markdown en specs/pm-*
foreach ($d in $dirs) {
  foreach ($f in 'spec.md','task.md') {
    $txt = Read-Text (Join-Path $d.FullName $f)
    foreach ($l in [regex]::Matches($txt, '\]\((\.\./[^)#]+|[a-z][^)#:]*\.md)\)')) {
      $target = Join-Path $d.FullName $l.Groups[1].Value
      if (-not (Test-Path ([System.IO.Path]::GetFullPath($target)))) { Fail "$($d.Name)/$f enlace roto: $($l.Groups[1].Value)" }
    }
  }
}

# preservacion historica
$manifest = Join-Path $specsDir '_historico\MANIFIESTO-SHA256.txt'
$checked = 0
foreach ($line in [System.IO.File]::ReadAllLines($manifest, $utf8)) {
  if ($line.StartsWith('#') -or [string]::IsNullOrWhiteSpace($line)) { continue }
  $parts = $line -split '\s+', 3
  $path = $parts[2]
  if ($path -eq 'specs/README.md') { $path = 'specs/_historico/roadmap-README-original.md' }
  elseif ($path -match '^specs/0\d-') { $path = 'specs/_historico/' + $path.Substring(6) }
  elseif ($path -notmatch '^specs/') { continue }
  $full = Join-Path $root ($path -replace '/', '\')
  if (-not (Test-Path $full)) { Fail "Historico ausente: $path"; continue }
  # Se normaliza CRLF a LF byte a byte (Latin1) antes del SHA-256: el manifiesto usa contenido LF y Git puede convertir finales de linea (core.autocrlf).
  $latin1 = [System.Text.Encoding]::GetEncoding(28591)
  $norm = $latin1.GetString([System.IO.File]::ReadAllBytes($full)).Replace("`r`n", "`n")
  $sha = [System.Security.Cryptography.SHA256]::Create()
  $h = ([System.BitConverter]::ToString($sha.ComputeHash($latin1.GetBytes($norm))) -replace '-', '').ToLowerInvariant()
  $checked++
  if ($h -ne $parts[0]) { Fail "Historico alterado: $path" }
}

if ($errors.Count -gt 0) { $errors | ForEach-Object { Write-Host "ERROR: $_" }; Write-Host "check-sdd: FALLO ($($errors.Count) errores)"; exit 1 }
Write-Host "check-sdd: OK - $($dirs.Count) Specs, $($taskIds.Count) Tasks, $checked archivos historicos verificados por SHA-256."
exit 0
