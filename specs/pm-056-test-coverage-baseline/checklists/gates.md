# Gates — pm-056 test-coverage-baseline (T068)

> Estado (2026-11-19): **cerrado**. La línea base estaba medida desde el
> 2026-11-08; lo que faltaba era aprobar el umbral, y se aprobó.

| CA | Task | Criterio | Evidencia | Resultado |
|---|---|---|---|---|
| `pm-056#CA-01` | T068 | Línea base medida y registrada | `docs/quality/coverage-baseline.md`: 93.7 % de líneas (1973/2105) con las cuatro suites; 78.6 % sin Integration/Atlas; por proyecto y por archivo; comandos reproducibles; `scripts/coverage-merge.py` | Cumple |
| `pm-056#CA-02` | T068 | Umbral inicial registrado mediante enmienda cuando exista evidencia | **Umbral aprobado el 2026-11-19** y registrado en `docs/quality/coverage-baseline.md`: ≥ 90 % en la corrida completa, ≥ 75 % en el pipeline de PR (sin Redis real) y no bajar más de 2 puntos entre PR | **Cumple** |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (`-warnaserror`) |
| Suites | Unit 188/188, Security 177/177, Contract 188/188, Integration 56 (34 pasan y 22 se omiten sin contraseñas reales de Redis; las 56 pasaron con los Redis desechables en la corrida previa) | Mismo contenedor, repositorio en solo lectura, sin `.env` salvo la corrida de Atlas |
| Cobertura medida | 93.7 % total de líneas | `python scripts/coverage-merge.py <dir>` sobre los informes de las suites |
- Limitaciones: Solo cobertura de líneas (coverlet no usa ramas aquí); las pruebas de Redis se omiten sin contraseñas reales (RedisAuthAttemptThrottle 2.4 % en esta corrida, cubierto con Redis desechable el 2026-11-08); el 100 % no es objetivo; el umbral no está en el gate de CI.
- [x] Revisión independiente (2026-11-08): subagentes de solo lectura, seguridad (apto con observaciones) y trazabilidad (consistente con observaciones); ver `Memoria.md`. Hallazgos corregidos o registrados.
- [x] Aprobación humana de las Tasks de este Spec (2026-11-08).


## Enmienda del umbral (2026-11-19)

CA-02 pedía registrar el umbral «mediante enmienda cuando exista evidencia». La
evidencia existía desde el 2026-11-08: 93.7 % de líneas (1973/2105) con las
cuatro suites, 78.6 % sin Integration/Atlas, medido con `coverage-merge.py` y
reproducible.

El umbral aprobado:

| Dónde | Umbral | Razón |
|---|---|---|
| Corrida completa | **≥ 90 %** | 3.7 puntos de margen sobre la línea base |
| Pipeline de PR | **≥ 75 %** | Sin los Redis de prueba la medición baja a 78.6 % |
| Entre PR | No bajar **2 puntos** | Detecta la erosión gradual |

Dos umbrales en vez de uno porque el CI no tiene los Redis de prueba: exigir
90 % ahí rompería todos los PR por una limitación del entorno, no por falta de
pruebas. Sube a 90 % cuando esos Redis existan.

## Cierre

- [x] CA-01 — línea base medida, registrada y reproducible.
- [x] CA-02 — umbral aprobado y registrado.
- [x] Revisión independiente — la medición se hizo en contenedor limpio, sin `.env`.
- [x] **Aprobación humana registrada:** Adrián Adonai García Jiménez, 2026-11-19.

## Nota

El documento original asignaba esta aprobación a Miguel. La tomó Adrián, que
administra el proyecto y la suscripción, con la evidencia ya medida delante. Si
Miguel considera que el umbral debe ser otro, cambiarlo es editar el documento y
el gate del pipeline: no hay nada construido encima que lo impida.
