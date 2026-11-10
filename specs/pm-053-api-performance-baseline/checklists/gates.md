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
| Suites | Unit 206/206, Security 180/180, Contract 198/198, Integration 37 pasan y 23 se omiten de 60 (las omitidas son de Redis sin contraseñas reales y la de caché con Redis real); medido el 2026-11-08 tras las revisiones | Mismo contenedor, repositorio en solo lectura; Integration con Atlas de pruebas vía `--env-file` (no impreso) |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |

- Limitaciones: Mide la aplicación en proceso, sin red, TLS, Atlas ni Redis reales; la carga contra el despliegue queda para pm-084.
- [x] Revisión independiente (2026-11-08): subagentes de solo lectura, seguridad (apto con observaciones) y trazabilidad (consistente con observaciones); ver `Memoria.md`. Hallazgos corregidos o registrados.
- [x] Aprobación humana de T064 (2026-11-08).
