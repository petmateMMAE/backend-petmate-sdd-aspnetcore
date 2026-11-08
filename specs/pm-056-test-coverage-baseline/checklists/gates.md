# Gates — pm-056 test-coverage-baseline (T068)

> Estado (2026-11-08): implementado y verificado; pendiente de revisión independiente y de aprobación humana.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-056#CA-01` | T068 | Línea base medida y registrada | `docs/quality/coverage-baseline.md`: 93.7 % de líneas (1973/2105) con las cuatro suites; 78.6 % sin Integration/Atlas; por proyecto y por archivo; comandos reproducibles; `scripts/coverage-merge.py` | Cumple |
| `pm-056#CA-02` | T068 | Umbral inicial registrado mediante enmienda cuando exista evidencia | Propuesta de umbral y texto de enmienda en el documento; **no aplicada**: requiere la aprobación de Miguel | Pendiente de enmienda (la evidencia ya existe) |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (`-warnaserror`) |
| Suites | Unit 188/188, Security 177/177, Contract 188/188, Integration 56 (34 pasan y 22 se omiten sin contraseñas reales de Redis; las 56 pasaron con los Redis desechables en la corrida previa) | Mismo contenedor, repositorio en solo lectura, sin `.env` salvo la corrida de Atlas |
| Cobertura medida | 93.7 % total de líneas | `python scripts/coverage-merge.py <dir>` sobre los informes de las suites |
- Limitaciones: Solo cobertura de líneas (coverlet no usa ramas aquí); las pruebas de Redis se omiten sin contraseñas reales (RedisAuthAttemptThrottle 2.4 % en esta corrida, cubierto con Redis desechable el 2026-11-08); el 100 % no es objetivo; el umbral no está en el gate de CI.
- [ ] Revisión independiente.
- [ ] Aprobación humana del cierre de las Tasks de este Spec.
