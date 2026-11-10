# Gates — pm-052 database-indexing-efficiency (T063)

> Estado (2026-11-08): implementado y verificado; pendiente de revisión independiente y de aprobación humana.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-052#CA-01` | T063 | Inventario de índices por colección y motivo | `docs/quality/database-indexes.md` (8 índices, 4 colecciones) | Cumple |
| `pm-052#CA-02` | T063 | Evidencia `explain` de uso de índice | `QueryPlanIntegrationTests.Critical_queries_use_an_index_and_never_scan_the_collection`: 8 consultas críticas con IXSCAN del índice esperado y sin COLLSCAN (Atlas de pruebas, 2026-11-08) | Cumple |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (`-warnaserror`) |
| Suites | Unit 206/206, Security 180/180, Contract 198/198, Integration 37 pasan y 23 se omiten de 60 (las omitidas son de Redis sin contraseñas reales y la de caché con Redis real); medido el 2026-11-08 tras las revisiones | Mismo contenedor, repositorio en solo lectura; Integration con Atlas de pruebas vía `--env-file` (no impreso) |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |

- Limitaciones: Observación abierta: el desempate por `_id` del listado no está en el índice (costo despreciable con el máximo de 100 mascotas); cambiar el índice es decisión pendiente.
- [x] Revisión independiente (2026-11-08): subagentes de solo lectura, seguridad (apto con observaciones) y trazabilidad (consistente con observaciones); ver `Memoria.md`. Hallazgos corregidos o registrados.
- [x] Aprobación humana de T063 (2026-11-08).
