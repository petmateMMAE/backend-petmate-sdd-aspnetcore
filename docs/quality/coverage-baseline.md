# Línea base de cobertura de pruebas (pm-056 / T068)

> Medida el 2026-11-08 con `coverlet.collector` 6.0.4 (MIT, $0) sobre el código de producción (`src/`), excluyendo pruebas y código generado. Cobertura de **líneas**.

## Resultado

| Proyecto | Cobertura (4 suites) |
|---|---|
| PetMate.Api | 94.6 % (595/629) |
| PetMate.Application | 96.3 % (441/458) |
| PetMate.Domain | 99.6 % (263/264) |
| PetMate.Infrastructure | 89.4 % (674/754) |
| **Total** | **93.7 % (1973/2105)** |

Sin la suite de integración (sin Atlas ni Redis): **78.6 %** (1655/2105); Infrastructure baja a 47.5 % porque los repositorios de MongoDB y los adaptadores de Redis solo se ejercitan contra los servicios reales.

Por suite (líneas de producción ejecutadas): Unit 639, Security 772, Contract 1436 e Integration 630, de 2105.

### Archivos con menor cobertura (≥ 15 líneas)
`RedisAuthAttemptThrottle` 2.4 % en esta corrida (sus pruebas de integración se omiten sin contraseñas reales de Redis y se ejecutaron con Redis desechables el 2026-11-08); `AuthContracts` 70.6 %; `UsersController` 76.5 %; `RedisTokenRevocationStore` 79.3 %; `MongoIndexInitializer` 83.3 %; `ApiResponseFactory` 85.7 %; `MongoPetRepository` 85.7 %.

## Cómo reproducirlo
1. Ejecutar cada suite con `dotnet test --collect:"XPlat Code Coverage" --results-directory <dir>/<Suite>` en el contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (repositorio en solo lectura; Integration con `--env-file .env`, sin imprimirlo).
2. `python scripts/coverage-merge.py <dir>` une los informes (una línea cuenta si alguna suite la ejecutó).

## Umbral propuesto (CA-02) — pendiente de enmienda
- **Propuesta:** cobertura total de líneas ≥ **90 %** (margen de 3.7 puntos sobre la línea base) en la corrida con las cuatro suites, y no permitir que baje más de 2 puntos entre PR. Para el pipeline de PR (sin Redis real) medir y exigir solo ≥ **75 %** hasta que los Redis de pruebas existan en CI.
- No se aplicó: cambiar el gate o la Constitución requiere la aprobación de Miguel (CA-02: «mediante enmienda cuando exista evidencia»). Esta evidencia queda disponible para esa enmienda.
- Limitaciones: no mide ramas ni calidad de las aserciones; 100 % no es objetivo.
