# Gates — pm-167-integration-environment-flow

> Evidencia reproducible del 2026-11-17. El cierre queda pendiente de la revisión independiente y de la aprobación humana.

## Comandos ejecutados
- `pwsh scripts/quality-gate.ps1` (host Windows sin `dotnet`; Docker 29.9.3). Pruebas en contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, con `.env` si existe.
- Unitarias: 348/348. Seguridad (incluye escaneo de secretos): 206/206. Contrato: 431/431.
- Integración: 12 pasan, 69 omitidas (sin Atlas ni Redis de prueba configurados); **no cuentan como aprobadas**.
- `scripts/check-sdd.ps1`: OK (167 Specs, 185 Tasks).

## Limitaciones
- Restore, build Release con analizadores como error, formato y auditorías NuGet **no se ejecutaron**: el script las corre con `dotnet` en el host y no está instalado. La compilación solo está evidenciada por las pruebas del contenedor.
- Documento `docs/ci-cd/mobile-development-flow.md`; sin código. El estado de la demo de Azure es una observación fechada (2026-11-17).
- No se ejecutó la revisión independiente de seguridad ni de trazabilidad. Aprobación humana de cierre: pendiente.
