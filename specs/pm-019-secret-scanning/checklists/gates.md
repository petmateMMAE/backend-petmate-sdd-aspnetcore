# Gates — pm-019 secret-scanning (T049)

> Estado (2026-11-08): T049 documentadas/verificadas; pendientes de revisión y aprobación humana. Una retrospectiva nunca se cierra automáticamente.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-019#CA-01` | T049 | Detecta secretos en archivos versionables | `SecretScanTests.Repository_files_contain_no_secrets` (dentro de Security 170/170) | En verde |
| `pm-019#CA-02` | T049 | Controles positivo y negativo | `SecretScanTests.Scanner_detects_planted_secrets` (positivo) y `Scanner_ignores_documented_placeholders` (negativo) | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y dos Redis temporales |
| Suites | Unit 188/188, Security 170/170, Contract 179/179, Integration 56/56 | Mismo contenedor |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Paquetes nuevos | Ninguno | — |

- Limitaciones: El job gitleaks del pipeline no se ha ejecutado con los cambios recientes.
- [ ] Revisión independiente.
- [x] Aprobación humana de T049 (2026-11-08).
