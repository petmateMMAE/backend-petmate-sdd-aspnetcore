# Gates — pm-020 dependency-license-audit (T050)

> Estado (2026-11-08): T050 documentadas/verificadas; pendientes de revisión y aprobación humana. Una retrospectiva nunca se cierra automáticamente.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-020#CA-01` | T050 | La auditoría falla ante vulnerabilidades y paquetes en desuso | `scripts/quality-gate.ps1 -AuditOnly` (2026-11-08): vulnerabilidades (desde High, local y CI), desuso y licencias en verde; el umbral se igualó a High en local y CI (T066) | En verde |
| `pm-020#CA-02` | T050 | Licencias con lista permitida | Auditoría de licencias: 50 paquetes revisados contra la lista permitida | En verde |
| `pm-020#CA-03` | T050 | Excepciones con gobierno | `quality-exceptions.json`: solo cuentan con `approved`, aprobador, fecha y vencimiento vigentes (excepción xunit Legacy hasta 2027-02-03) | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y dos Redis temporales |
| Suites | Unit 188/188, Security 170/170, Contract 179/179, Integration 56/56 | Mismo contenedor |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Paquetes nuevos | Ninguno | — |

- Limitaciones: Evidencia por ejecución del gate, sin prueba automática que verifique que falla ante un hallazgo; el job `3 · Dependencias` de CI no se ha ejecutado con los cambios recientes.
- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de T050.
