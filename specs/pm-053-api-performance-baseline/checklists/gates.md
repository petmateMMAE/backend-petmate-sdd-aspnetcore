# Gates — pm-053 api-performance-baseline (T064)

> Estado (2026-11-08): implementado y verificado; pendiente de revisión independiente y de aprobación humana.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-053#CA-01` | T064 | Objetivos de latencia definidos y medibles | `docs/quality/performance-baseline.md` (p95 ≤ 250 ms en proceso; ≤ 500 ms / 1500 ms objetivo en el despliegue) y topes en `PerformanceBaselineTests` | Cumple |
| `pm-053#CA-02` | T064 | Prueba de carga de costo $0 reproducible | `PerformanceBaselineTests` (concurrencia 16, 600 solicitudes): health p95 10.1 ms, get-all p95 19.9 ms y 13.2 ms, 0 errores | Cumple |
| `pm-053#CA-03` | T064 | Resultados sin PII | Datos sintéticos; solo percentiles y conteos | Cumple |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (`-warnaserror`) |
| Suites | Unit, Security, Contract e Integration en verde (ver Memoria.md para los conteos de esta fecha) | Mismo contenedor, repositorio en solo lectura; Integration con Atlas de pruebas vía `--env-file` (no impreso) |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |

- Limitaciones: Mide la aplicación en proceso, sin red, TLS, Atlas ni Redis reales; la carga contra el despliegue queda para pm-084.
- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de T064.
