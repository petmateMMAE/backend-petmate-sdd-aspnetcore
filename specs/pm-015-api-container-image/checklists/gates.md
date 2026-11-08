# Gates — pm-015 api-container-image (T046)

> Estado (2026-11-08): T046 documentadas/verificadas; pendientes de revisión y aprobación humana. Una retrospectiva nunca se cierra automáticamente.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-015#CA-01` | T046 | Dockerfile compila y publica multi-stage | Build de la imagen `petmate-api:local` con `docker compose up --build` (2026-11-08) y pipeline `docker-build` | En verde |
| `pm-015#CA-02` | T046 | Ejecuta con usuario no root | `docker image inspect`: usuario `1654` (UID de `$APP_UID`), sin shell en la imagen | En verde |
| `pm-015#CA-03` | T046 | Sin secretos en la imagen | `.dockerignore` excluye `.env`; Trivy `--scanners secret` sobre `petmate-api:local` sin hallazgos; ningún archivo `.env*` en `/app`; `SecretScanTests` cubre `Dockerfile` | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y dos Redis temporales |
| Suites | Unit 188/188, Security 170/170, Contract 179/179, Integration 56/56 | Mismo contenedor |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Paquetes nuevos | Ninguno | — |

- Limitaciones: Sin prueba automática del Dockerfile en la suite; la evidencia es la construcción, los escaneos y la inspección de la imagen. El escaneo de vulnerabilidades es pm-057 (cerrado).
- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de T046.
