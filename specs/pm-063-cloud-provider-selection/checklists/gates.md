# Gates — pm-063 cloud-provider-selection (T075)

> Estado (2026-11-08): T075 documentada y verificada; pendiente de revisión y aprobación humana.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-063#CA-01` | T075 | Proveedor registrado como Azure for Students; sin AWS ni GCP | `docs/decisions/ADR-003-azure-container-apps.md` y `docs/decisions/README.md` (Azure for Students, decisión del 2026-11-05); no se evalúan otros proveedores | Cumple |
| `pm-063#CA-02` | T075 | ADR por componente con alternativas dentro de Azure | ADR-003 (cómputo y Redis), ADR-005 (balanceo), ADR-006 (almacenamiento), ADR-007 (CDN), ADR-008 (observabilidad), ADR-009 (registro), ADR-010 (secretos); `DecisionRecordsTests.Every_component_adr_lists_alternatives_inside_azure_and_cites_the_cost_study` y `The_compute_and_redis_decision_lists_discarded_alternatives_and_the_index_covers_every_component` | Cumple |
| `pm-063#CA-03` | T075 | Cada servicio cita la clasificación de costo de pm-062 | Cada ADR enlaza el estudio de costos y nombra la clasificación y la fila (`DecisionRecordsTests`) | Cumple |
| `pm-063#CA-04` | T075 | Ningún servicio por nivel gratuito histórico | Cada ADR incluye la regla «nivel gratuito» y la política `azure-cost-policy.md` regla 2; las decisiones diferidas (Blob, CDN) no crean recursos | Cumple |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` |
| Suites | Unit 188/188, Security 177/177, Contract 179/179, Integration 56/56 | Mismo contenedor (SecurityTests ejecutadas tras añadir las pruebas de esta Task) |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Recursos de Azure creados | Ninguno | No se ejecutó `terraform` ni `az` |

- Limitaciones: Los ADR quedan «Propuesto» hasta tu aprobación; Blob y CDN están diferidos y se reevaluarán con precio vigente antes de crearlos.
- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de T075.
