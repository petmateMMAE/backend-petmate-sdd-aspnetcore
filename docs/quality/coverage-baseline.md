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

## Umbral (CA-02) — aprobado el 2026-11-19

**Aprobado por Adrián Adonai García Jiménez el 2026-11-19**, sobre la evidencia
medida en este documento (93.7 % de líneas con las cuatro suites).

| Dónde | Umbral | Por qué |
|---|---|---|
| Corrida completa, las cuatro suites | **≥ 90 %** | Deja 3.7 puntos de margen sobre la línea base: exige mantener el nivel sin romper el build por una variación menor |
| Pipeline de PR, sin Redis real | **≥ 75 %** | Sin los Redis de prueba en CI la medición baja a 78.6 %; exigir 90 % ahí rompería todos los PR |
| Entre PR | No bajar más de **2 puntos** | Detecta la erosión gradual, que es como la cobertura se pierde de verdad |

El umbral del pipeline sube a 90 % cuando existan los Redis de prueba en CI.

- Limitaciones: no mide ramas ni calidad de las aserciones; 100 % no es objetivo.
- Un número alto no garantiza buenas pruebas: sirve para detectar código que
  nadie ejecuta, no para medir si lo ejecutado se verifica bien.
