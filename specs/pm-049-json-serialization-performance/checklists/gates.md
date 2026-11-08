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
| Suites | Unit, Security, Contract e Integration en verde (ver Memoria.md para los conteos de esta fecha) | Mismo contenedor, repositorio en solo lectura; Integration con Atlas de pruebas vía `--env-file` (no impreso) |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |

- Limitaciones: Microbenchmark en proceso, informativo; el aserto solo impide una regresión grande.
- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de T060.
