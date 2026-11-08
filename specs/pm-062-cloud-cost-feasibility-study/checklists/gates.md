# Gates — pm-062 cloud-cost-feasibility-study (T074)

> Estado (2026-11-08): T074 documentada y verificada; pendiente de revisión y aprobación humana.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-062#CA-01` | T074 | Cada componente tiene fuente oficial con URL y fecha de consulta | `docs/infrastructure/azure-cost-study.md` §2: 15 componentes con URL oficial y fecha 2026-11-08, y el estado real de cada fuente (verificada, parcial o no consultada); `DecisionRecordsTests.The_cost_study_covers_the_required_components_with_official_sources_and_a_consultation_date` | Cumple, con fuentes parciales señaladas (precios dinámicos que no cargaron) |
| `pm-062#CA-02` | T074 | Clasificación (incluido, crédito o posible cargo) con importe en MXN | Mismo §2: clasificación y USD/MXN por componente; tipo de cambio 18.652374 con su fuente y fecha | Cumple (importes de lista pendientes de confirmar en la calculadora) |
| `pm-062#CA-03` | T074 | Costo total de operación y consumo de créditos | §3: completo ~26–28 USD (~485–522 MXN) al mes, crédito para ~3.7 meses; modo ahorro ~13–15 USD (~242–280 MXN), ~7 meses; ventana de evaluación ~23 % del crédito | Cumple |
| `pm-062#CA-04` | T074 | Conflicto académico documentado sin reducir requisitos | §4: 12 meses completos (~324 USD) exceden el crédito (100 USD); se mantiene la arquitectura de dos instancias con Load Balancer y se acota el tiempo de operación; `Conflicto académico` y `no se reduce ningún requisito` verificados por prueba | Cumple |
| `pm-062#CA-05` | T074 | No crea recursos ni habilita servicios con cargo | Solo se leyeron páginas públicas y se calculó; no se ejecutó `terraform`, `az` ni ninguna llamada a Azure; `infra/` sin cambios en este trabajo | Cumple |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` |
| Suites | Unit 188/188, Security 177/177, Contract 179/179, Integration 56/56 | Mismo contenedor (SecurityTests ejecutadas tras añadir las pruebas de esta Task) |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Recursos de Azure creados | Ninguno | No se ejecutó `terraform` ni `az` |

- Limitaciones: Precios no cargados (ACR Basic, Azure Files, Blob, Container Apps por segundo) y fuentes no consultadas (secretos, GitHub Actions, Atlas) marcados en el estudio; consumo real pendiente de leer en Cost Management; comportamiento al agotarse el crédito pendiente de confirmar (la FAQ devolvió 404).
- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de T074.
