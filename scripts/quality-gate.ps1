<#
.SYNOPSIS
  Puerta de calidad local de PetMate (T012). No usa GitHub ni servicios de pago.
.DESCRIPTION
  Ejecuta, en orden: restore, build Release (SAST con analizadores del SDK como errores), formato,
  auditoría NuGet (vulnerabilidades, paquetes en desuso y licencias) y las cuatro suites de pruebas
  (incluye la detección de secretos de SecurityTests). Cualquier fallo devuelve exit code distinto de 0.
  Las pruebas corren en un contenedor del SDK porque Smart App Control puede bloquear binarios nuevos en el host.
  Uso: pwsh ./scripts/quality-gate.ps1 [-HostTests] [-SkipTests] [-AuditOnly] [-MinSeverity High]
  -AuditOnly: solo restore y auditorías NuGet (vulnerabilidades, desuso y licencias); lo usa el job `dependencies` del pipeline.
  -MinSeverity: gravedad mínima de vulnerabilidad que bloquea (High por defecto, igual en local y en el pipeline; Low = cualquiera).
.NOTES
  Si existe .env se pasa al contenedor con --env-file (sin leerlo ni imprimirlo) para las pruebas Atlas.
  Redis: defina PETMATE_IT_REDIS_AUTH y PETMATE_IT_REDIS_CACHE y la red de Compose (petmate_internal) para
  ejecutar las pruebas de Redis; si no, quedan Skipped (no se consideran aprobadas).
#>
[CmdletBinding()]
param(
    [switch]$HostTests,
    [switch]$SkipTests,
    [switch]$AuditOnly,
    [ValidateSet('Low', 'Moderate', 'High', 'Critical')]
    [string]$MinSeverity = 'High'
)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..')
Set-Location $root
$failures = [System.Collections.Generic.List[string]]::new()

function Step([string]$name, [scriptblock]$action) {
    Write-Host "`n=== $name ===" -ForegroundColor Cyan
    try {
        & $action
        if ($LASTEXITCODE -ne 0) { throw "exit code $LASTEXITCODE" }
        Write-Host "OK: $name" -ForegroundColor Green
    }
    catch {
        Write-Host "FALLO: $name -> $($_.Exception.Message)" -ForegroundColor Red
        $failures.Add($name)
    }
}

Step 'Restore' { dotnet restore PetMate.sln --nologo -v q }
if (-not $AuditOnly) {
    Step 'Build Release + SAST (analizadores como errores)' { dotnet build PetMate.sln -c Release --no-restore --nologo -warnaserror }
    Step 'Formato (dotnet format --verify-no-changes)' { dotnet format PetMate.sln --verify-no-changes --verbosity minimal }
}

Step "NuGet: vulnerabilidades (incluye transitivas; bloquean desde $MinSeverity)" {
    $rank = @{ Low = 0; Moderate = 1; High = 2; Critical = 3 }
    $json = dotnet list PetMate.sln package --vulnerable --include-transitive --format json | Out-String
    $report = $json | ConvertFrom-Json
    $vulnerable = @($report.projects | ForEach-Object { $_.frameworks } | ForEach-Object {
            @($_.topLevelPackages) + @($_.transitivePackages) } | Where-Object { $_ })
    $blocking = @()
    foreach ($package in $vulnerable) {
        foreach ($advisory in @($package.vulnerabilities)) {
            $level = if ($rank.ContainsKey([string]$advisory.severity)) { $rank[[string]$advisory.severity] } else { 3 }
            if ($level -ge $rank[$MinSeverity]) { $blocking += $package; break }
            Write-Host "  advertencia (por debajo de $MinSeverity): $($package.id) $($package.resolvedVersion) [$($advisory.severity)]" -ForegroundColor Yellow
        }
    }
    if ($blocking.Count -gt 0) {
        $blocking | ForEach-Object { Write-Host "  vulnerable: $($_.id) $($_.resolvedVersion)" }
        throw "$($blocking.Count) paquetes con vulnerabilidades de gravedad $MinSeverity o mayor"
    }
    $global:LASTEXITCODE = 0
}

Step 'NuGet: paquetes en desuso' {
    # Una excepción solo vale con approved=true, aprobador y fecha (scripts/quality-exceptions.json).
    $exceptionsFile = Join-Path $PSScriptRoot 'quality-exceptions.json'
    $approved = @()
    if (Test-Path $exceptionsFile) {
        $approved = @((Get-Content $exceptionsFile -Raw | ConvertFrom-Json).deprecatedPackages |
                Where-Object { $_.approved -eq $true -and $_.approvedBy -and $_.date -and $_.reviewDate -and ([datetime]$_.reviewDate -ge (Get-Date).Date) } | ForEach-Object { $_.ids })
    }
    $json = dotnet list PetMate.sln package --deprecated --format json | Out-String
    $report = $json | ConvertFrom-Json
    $deprecated = @($report.projects | ForEach-Object { $_.frameworks } | ForEach-Object {
            @($_.topLevelPackages) + @($_.transitivePackages) } | Where-Object { $_ } |
            Where-Object { $_.id -notin $approved } | Sort-Object id -Unique)
    if ($deprecated.Count -gt 0) {
        $deprecated | ForEach-Object { Write-Host "  en desuso sin excepción aprobada: $($_.id) $($_.resolvedVersion)" }
        throw "$($deprecated.Count) paquetes en desuso sin excepción aprobada (scripts/quality-exceptions.json)"
    }
    $global:LASTEXITCODE = 0
}

Step 'NuGet: auditoría de licencias (lista permitida, costo $0)' {
    $allowed = @('MIT', 'Apache-2.0', 'BSD-2-Clause', 'BSD-3-Clause', 'ISC', 'MS-PL', 'Unlicense')
    $json = dotnet list PetMate.sln package --include-transitive --format json | Out-String
    $report = $json | ConvertFrom-Json
    $packages = @($report.projects | ForEach-Object { $_.frameworks } | ForEach-Object {
            @($_.topLevelPackages) + @($_.transitivePackages) } | Where-Object { $_ }) |
        Sort-Object id, resolvedVersion -Unique
    $packageRoot = (dotnet nuget locals global-packages -l) -replace '^.*?:\s*', ''
    $unknown = @()
    foreach ($package in $packages) {
        # Join-Path anidado para que funcione igual en Windows y en Linux (runner de CI).
        $packageId = $package.id.ToLowerInvariant()
        $nuspec = Join-Path (Join-Path (Join-Path $packageRoot.Trim() $packageId) $package.resolvedVersion) "$packageId.nuspec"
        if (-not (Test-Path $nuspec)) { $unknown += "$($package.id) (sin nuspec)"; continue }
        [xml]$xml = Get-Content $nuspec
        $license = $xml.package.metadata.license.'#text'
        if (-not $license) { $license = $xml.package.metadata.licenseUrl }
        # URL de licencia verificada manualmente: xunit.abstractions publica la licencia Apache-2.0 del proyecto xunit.
        if ($license -like 'https://raw.githubusercontent.com/xunit/xunit/*license.txt') { $license = 'Apache-2.0' }
        # Licencia declarada como archivo, verificada a mano: el LICENSE.txt de Apache.Arrow es la Apache License 2.0
        # y su autor es The Apache Software Foundation. Llega como dependencia transitiva de Azure.Storage.Blobs (pm-079).
        if ($package.id -eq 'Apache.Arrow' -and $license -eq 'LICENSE.txt') { $license = 'Apache-2.0' }
        $isMicrosoft = $xml.package.metadata.authors -match 'Microsoft' -or $package.id -like 'Microsoft.*' -or $package.id -like 'System.*'
        if (($license -notin $allowed) -and -not ($isMicrosoft -and ($license -in $allowed -or $license -match 'dotnet|microsoft|aka\.ms|MS-EULA'))) {
            $unknown += "$($package.id) $($package.resolvedVersion): $license"
        }
    }
    if ($unknown.Count -gt 0) {
        $unknown | ForEach-Object { Write-Host "  revisar licencia: $_" -ForegroundColor Yellow }
        throw "$($unknown.Count) paquetes con licencia fuera de la lista permitida"
    }
    Write-Host "  $($packages.Count) paquetes revisados"
    $global:LASTEXITCODE = 0
}

if (-not $SkipTests -and -not $AuditOnly) {
    if ($HostTests) {
        Step 'Pruebas en host (Unit, Security, Contract, Integration)' {
            foreach ($suite in 'UnitTests', 'SecurityTests', 'ContractTests', 'IntegrationTests') {
                dotnet test "tests/PetMate.$suite" -c Release --no-build --nologo
                if ($LASTEXITCODE -ne 0) { throw "$suite falló" }
            }
        }
    }
    else {
        Step 'Pruebas en contenedor SDK (Unit, Security incl. secret scan, Contract, Integration)' {
            $inner = @'
set -u
mkdir /work && cd /src && tar --exclude=bin --exclude=obj --exclude=.env --exclude=.git -cf - . | tar -xf - -C /work && cd /work
dotnet restore PetMate.sln -v q >/dev/null || exit 1
dotnet build PetMate.sln -c Release --no-restore --nologo -warnaserror 2>&1 | grep -E "Warning\(s\)|Error\(s\)"
rc=0
for s in UnitTests SecurityTests ContractTests IntegrationTests; do
  echo "### $s"
  dotnet test tests/PetMate.$s -c Release --no-restore --no-build --nologo --logger "console;verbosity=normal" 2>&1 | grep -E "^ *(Skipped|Failed) |Total tests|Passed:|Failed:|Skipped:"
  [ "${PIPESTATUS[0]}" -eq 0 ] || rc=1
done
exit $rc
'@
            $script = Join-Path ([IO.Path]::GetTempPath()) 'petmate-quality-inner.sh'
            [IO.File]::WriteAllText($script, $inner.Replace("`r`n", "`n"))
            $args = @('run', '--rm', '--cap-drop', 'ALL', '--security-opt', 'no-new-privileges',
                '-e', 'DOTNET_CLI_TELEMETRY_OPTOUT=1', '-e', 'DOTNET_NOLOGO=1',
                '-v', "${root}:/src:ro", '-v', "${script}:/run.sh:ro")
            if (Test-Path (Join-Path $root '.env')) { $args += @('--env-file', (Join-Path $root '.env')) }
            if (docker network ls --format '{{.Name}}' | Select-String -SimpleMatch 'petmate_internal') { $args += @('--network', 'petmate_internal') }
            foreach ($name in 'PETMATE_IT_REDIS_AUTH', 'PETMATE_IT_REDIS_CACHE') {
                if (Get-Item "env:$name" -ErrorAction SilentlyContinue) { $args += @('-e', $name) }
            }
            $args += @('mcr.microsoft.com/dotnet/sdk:10.0', 'bash', '/run.sh')
            & docker @args
        }
    }
}

Write-Host ''
if ($failures.Count -gt 0) {
    Write-Host "PUERTA DE CALIDAD: FALLÓ -> $($failures -join '; ')" -ForegroundColor Red
    exit 1
}

Write-Host 'PUERTA DE CALIDAD: CORRECTA' -ForegroundColor Green
exit 0
