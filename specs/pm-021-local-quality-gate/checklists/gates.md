# Gates — pm-021 local-quality-gate (T051)

> Estado (2026-11-08): T051 documentadas/verificadas; pendientes de revisión y aprobación humana. Una retrospectiva nunca se cierra automáticamente.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-021#CA-01` | T051 | Un comando ejecuta restore, build, formato, auditorías y suites | `scripts/quality-gate.ps1` (todos los pasos; las suites corren en contenedor del SDK por Smart App Control); la parte de auditorías se ejecutó el 2026-11-08 con `-AuditOnly` | En verde |
| `pm-021#CA-02` | T051 | No omite escáneres en silencio | Cada paso termina con `OK` o `FALLO` y el comando devuelve exit code distinto de 0 si alguno falla; no hay pasos opcionales ocultos | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y dos Redis temporales |
| Suites | Unit 188/188, Security 170/170, Contract 179/179, Integration 56/56 | Mismo contenedor |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Paquetes nuevos | Ninguno | — |

- Limitaciones: El gate completo no corre en el host por Smart App Control (las suites se ejecutan en contenedor); en CI el trabajo se reparte en jobs y la puerta final los agrega.
- [ ] Revisión independiente.
- [x] Aprobación humana de T051 (2026-11-08).
