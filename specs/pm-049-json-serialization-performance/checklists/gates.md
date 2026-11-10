# Gates — pm-049 json-serialization-performance (T060)

> Estado (2026-11-08): implementado y verificado; pendiente de revisión independiente y de aprobación humana.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-049#CA-01` | T060 | Serialización eficiente sin alterar el contrato | `ApiJsonContext`, `ApiJson`; `Source_generated_json_is_identical_to_reflection_json_and_is_not_slower` (8 tipos del wrapper, JSON idéntico) y `OpenApiWrapperTests` | Cumple |
| `pm-049#CA-02` | T060 | Efecto medido antes y después | 50 mascotas x 2000: reflexión 321.6 ms; generado 196.1 ms (≈ 39 % menos) | Cumple |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (`-warnaserror`) |
| Suites | Unit 206/206, Security 180/180, Contract 198/198, Integration 37 pasan y 23 se omiten de 60 (las omitidas son de Redis sin contraseñas reales y la de caché con Redis real); medido el 2026-11-08 tras las revisiones | Mismo contenedor, repositorio en solo lectura; Integration con Atlas de pruebas vía `--env-file` (no impreso) |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |

- Limitaciones: Microbenchmark en proceso, informativo; el aserto solo impide una regresión grande.
- [x] Revisión independiente (2026-11-08): subagentes de solo lectura, seguridad (apto con observaciones) y trazabilidad (consistente con observaciones); ver `Memoria.md`. Hallazgos corregidos o registrados.
- [x] Aprobación humana de T060 (2026-11-08).
