# Gates — pm-048 response-compression (T059)

> Estado (2026-11-08): implementado y verificado; pendiente de revisión independiente y de aprobación humana.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-048#CA-01` | T059 | Respuestas comprimidas si el cliente lo solicita | `CompressionAndJsonTests.A_large_list_is_compressed_when_the_client_asks_and_the_effect_is_measured` (gzip y br, `Vary`, contenido idéntico) | Cumple |
| `pm-048#CA-02` | T059 | Evaluación de riesgos y exclusión de material sensible | `docs/security/compression-risk.md`; `Authentication_responses_with_tokens_are_never_compressed` | Cumple |
| `pm-048#CA-03` | T059 | Efecto medido antes y después | 50 mascotas: 12 777 B → 1 909 B gzip (14.9 %) y 1 901 B Brotli | Cumple |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (`-warnaserror`) |
| Suites | Unit 206/206, Security 180/180, Contract 198/198, Integration 37 pasan y 23 se omiten de 60 (las omitidas son de Redis sin contraseñas reales y la de caché con Redis real); medido el 2026-11-08 tras las revisiones | Mismo contenedor, repositorio en solo lectura; Integration con Atlas de pruebas vía `--env-file` (no impreso) |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |

- Limitaciones: Medición sobre datos sintéticos; si una ruta fuera de `/auth/*` llegara a devolver un secreto debe excluirse.
- [x] Revisión independiente (2026-11-08): subagentes de solo lectura, seguridad (apto con observaciones) y trazabilidad (consistente con observaciones); ver `Memoria.md`. Hallazgos corregidos o registrados.
- [x] Aprobación humana de T059 (2026-11-08).
