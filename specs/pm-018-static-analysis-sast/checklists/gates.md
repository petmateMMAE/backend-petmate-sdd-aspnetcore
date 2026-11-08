# Gates — pm-018 static-analysis-sast (T048)

> Estado (2026-11-08): T048 documentadas/verificadas; pendientes de revisión y aprobación humana. Una retrospectiva nunca se cierra automáticamente.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-018#CA-01` | T048 | Reglas de seguridad como errores de compilación | `Directory.Build.props` (analizadores y advertencias como error); el build con `-warnaserror` falla ante una regla incumplida (así lo hicieron CA1827, CA1861 y CA1862 durante esta sesión) | En verde |
| `pm-018#CA-02` | T048 | Build Release con 0 advertencias | 0 advertencias y 0 errores en todas las compilaciones del 2026-11-08 (Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y dos Redis temporales); job `2 · Compilación y analizadores (SAST)` del pipeline | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y dos Redis temporales |
| Suites | Unit 188/188, Security 170/170, Contract 179/179, Integration 56/56 | Mismo contenedor |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |
| Paquetes nuevos | Ninguno | — |

- Limitaciones: CodeQL es informativo y el job de CI aún no se ha ejecutado con los cambios recientes.
- [ ] Revisión independiente.
- [x] Aprobación humana de T048 (2026-11-08).
