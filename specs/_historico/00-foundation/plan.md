# Plan — 00 Foundation y solución inicial

## 1. Contexto

Implementar T001–T012 desde solución vacía. Spec origen: `spec.md`.

## 2. Diseño técnico

- Crear `PetMate.sln`.
- Source projects: `src/PetMate.Api` (Web API MVC), `PetMate.Application`, `PetMate.Domain`, `PetMate.Infrastructure` (Class Library los tres últimos).
- Test projects: `tests/PetMate.UnitTests`, `PetMate.IntegrationTests`, `PetMate.SecurityTests`, `PetMate.ContractTests`.
- Definir referencias permitidas conforme a la constitución.
- Grafo de referencias de T002: `Domain` sin referencias; `Application` → `Domain`; `Infrastructure` → `Application` + `Domain`; `Api` → `Application` + `Infrastructure`.
- Grafo de suites: `UnitTests` → `Domain` + `Application`; `IntegrationTests` → `Application` + `Infrastructure`; `SecurityTests` y `ContractTests` → `Application` + `Api`. Las suites no se referencian entre sí.
- Application organiza Commands, Queries, Handlers, DTOs/Contracts, Validators y abstracciones.
- Domain contiene entidades, value objects y reglas puras.
- Infrastructure implementa Atlas/Mongo y adaptadores Redis.
- API contiene Controllers, middleware, configuración y composición DI.
- Mediator despacha Commands/Queries; paquete concreto pendiente de licencia y compatibilidad.

## 3. Persistencia y servicios

- Configurar conexión a MongoDB Atlas mediante `MONGODB_URI` y `MONGODB_DATABASE`.
- Configurar `MONGODB_TEST_URI` / `MONGODB_TEST_DATABASE` separadas para integración.
- Configurar `REDIS_AUTH_CONNECTION` y `REDIS_CACHE_CONNECTION` independientes.
- Compose local puede contener API, redis-auth y redis-cache. No agregar MongoDB.
- La API debe validar configuración requerida al iniciar, sin incluir valores en errores/logs.
- T004 implementa un lector `.env` propio, sin paquete adicional ni costo/licencia que auditar. El host se crea primero y, solo si su entorno efectivo no es Production, carga exactamente `<ContentRootPath>/.env`; las variables reales del host, incluso vacías, tienen prioridad. `ApplicationBootstrapTests` cubre carga en Development y omisión con Production efectivo por defecto.

## 4. API y contratos

- MVC Controllers.
- Rutas versionadas `/api/v1/...`.
- Acción literal en rutas de negocio.
- `ApiResponse<T>` con `StatusCode`, `Message` y `Data` nullable.
- Generar el documento OpenAPI desde ASP.NET Core y mapear Scalar como única UI interactiva de documentación. Rutas iniciales previstas: `/openapi/v1.json` y `/scalar/v1`.
- Configurar Scalar/OpenAPI habilitados por defecto en Development y deshabilitados en producción salvo habilitación explícita.
- Documentar el Response Wrapper en los esquemas OpenAPI; incorporar el esquema Bearer/JWT al documentar las operaciones de autenticación.
- No agregar Swagger UI, Swashbuckle UI ni NSwag UI. Auditar la licencia/costo del paquete Scalar antes de incorporarlo.
- Middleware centralizado de excepciones y mapeo a wrappers.
- Validación de requests antes de dispatch.

## 5. Seguridad y DevSecOps

- `.env` en `.gitignore`; `.env.example` sin secretos.
- Configuración de headers, HTTPS/forwarded headers documentada para entorno Azure.
- No mostrar connection strings en health checks.
- CI no despliega ni crea infraestructura Azure en esta fase.
- Auditoría de licencia y costo antes de agregar paquetes.

## 6. Paquetes, herramientas y costo

La tabla se completa antes de ejecutar `dotnet add package`. Ningún paquete se considera aprobado por aparecer en tutoriales o ejemplos. El criterio es licencia $0, compatibilidad .NET 10, mantenimiento y vulnerabilidades.

### Dependencias actuales (T003, 2026-11-03)

| Paquete | Versión | Licencia | Costo | Uso |
|---|---|---|---|---|
| `xunit` | 2.9.3 | Apache-2.0 | $0 | Framework de pruebas |
| `xunit.runner.visualstudio` | 3.1.4 | Apache-2.0 | $0 | Runner de xUnit para `dotnet test` |
| `Microsoft.NET.Test.Sdk` | 17.14.1 | MIT | $0 | Infraestructura de pruebas de .NET |
| `coverlet.collector` | 6.0.4 | MIT | $0 | Recolección de cobertura |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 10.0.0 | MIT | $0 | Abstracción mínima `IServiceCollection` para los módulos DI de T005; compatible con `net10.0` |
| `Microsoft.AspNetCore.OpenApi` | 10.0.12 | MIT | $0 | Generación OpenAPI oficial para `net10.0`; depende de `Microsoft.OpenApi` `[2.12.0,3.0.0)` |
| `Microsoft.OpenApi` | 2.12.0 | MIT | $0 | Modelo OpenAPI transitivo fijado explícitamente; versión posterior al parche 2.7.5 de GHSA-v5pm-xwqc-g5wc |
| `Scalar.AspNetCore` | 2.11.2 | MIT | $0 | Única UI interactiva de OpenAPI; compatible con `net10.0`, sin dependencias NuGet |

- Licencias verificadas en los `.nuspec` locales de NuGet (`<license type="expression">`) y en `licenses.nuget.org`.
- Versiones centralizadas en `Directory.Packages.props`; no se agregaron ni actualizaron paquetes en T003.
- `dotnet list PetMate.sln package --vulnerable --include-transitive` → 0 paquetes vulnerables.
- Herramientas: .NET SDK `10.0.302`, `dotnet format`, `dotnet test` y cliente NuGet incluidos en el SDK/flujo oficial; costo $0. No se agregaron herramientas pagas.
- T004 no añade `PackageReference`: `DotEnvFile` es implementación propia y `Microsoft.AspNetCore.App` es un `FrameworkReference` del SDK para las pruebas de configuración.
- T005 añade únicamente `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.0, paquete oficial Microsoft. Evidencia consultada en su `.nuspec`: licencia MIT, grupo `net10.0` sin dependencias adicionales; costo de uso $0. La auditoría `dotnet list PetMate.sln package --vulnerable --include-transitive` no reporta vulnerabilidades.
- T007 se implementa solo con la combinación aprobada en `decisions/ADR-002-openapi-scalar.md`: OpenApi 10.0.12, OpenApi.NET 2.12.0 y Scalar 2.11.2. Las pruebas temporales net10.0 y NuGet Audit no reportaron vulnerabilidades; no se acepta la combinación vulnerable 10.0.0/2.0.0.

## 7. Archivos a crear/modificar

- `PetMate.sln`, proyectos source/test, `Directory.Build.props`, `.editorconfig`, `.gitignore`, `.env.example`.
- `Program.cs`, extensiones DI por capa, wrapper/exception middleware, endpoints de health y configuración OpenAPI/Scalar.
- `Dockerfile`, `compose.yaml` para los dos Redis y API.
- `.github/workflows/ci-pr.yml` y scripts de calidad cuando se hayan seleccionado herramientas $0.

## 8. Secuencia y riesgos

Ejecutar T001–T012 en orden. No iniciar auth hasta verificar la base de composición, persistencia, Redis, API y CI. Riesgo principal: el conjunto de servicios Azure gratuitos puede no permitir dos Redis independientes y varias réplicas; no provisionar servicios pagos.

## 9. Verificación

- `dotnet restore` y `dotnet build -c Release`.
- Listado de proyectos/referencias y comprobación automatizada de dirección de dependencias.
- Tests base de wrapper, DI, configuración, health y disponibilidad/validez del documento OpenAPI y la UI Scalar en Development.
- Confirmar que `.env` está ignorado y no aparece en Git.
- Escaneos de secretos/dependencias sin costo.
- Comprobar que Compose no define servicio MongoDB.

## 10. Aprobación

Aprobado por el usuario el 2026-11-03 tras revisar spec, plan, tareas y gates. La aprobación no autoriza por sí sola tareas no autorizadas ni el cierre del slice.

### Decisiones T010/T011 (2026-11-03)

| Elemento | Selección | Licencia / costo | Motivo |
|---|---|---|---|
| `MongoDB.Driver` | 3.13.0 | Apache-2.0, $0 | Cliente oficial; mismo contrato para cualquier URI Atlas |
| `StackExchange.Redis` | 3.4.31 | MIT, $0 | Cliente Redis; conexión perezosa y reconectable |
| `Microsoft.Extensions.Diagnostics.HealthChecks` / `...Configuration.Abstractions` / `...DependencyInjection.Abstractions` | 10.0.14 | MIT, $0 | Health checks sin paquetes de terceros (versiones alineadas por dependencias transitivas) |
| Imagen `redis:7.4` | 7.4.9 (tag flotante `7.4`; fijar digest antes de producción) | Redis 7.4 usa RSALv2/SSPLv1, no OSI: $0 para uso interno sin ofrecer Redis como servicio | Ya disponible localmente; alternativa $0 abierta: Valkey (BSD) — a evaluar en el spec de despliegue |
| Imágenes `dotnet/sdk:10.0` y `aspnet:10.0` | tags `10.0` | MIT, $0 | Oficiales de Microsoft |

- Redis Auth: `maxmemory 64mb`, `noeviction`, AOF `everysec`, volumen propio. Si se llena, rechaza escrituras y la validación JWT falla cerrada. Redis Cache: `maxmemory 128mb`, `allkeys-lru`, sin persistencia. Tamaños son decisión de implementación, ajustables.
- Variables nuevas (no constitucionales, solo Compose): `REDIS_AUTH_PASSWORD`, `REDIS_CACHE_PASSWORD`, `API_PORT`, `PETMATE_ENVIRONMENT`, `REDIS_AUTH_PORT`, `REDIS_CACHE_PORT`. Variable de pruebas: `PETMATE_IT_REDIS_AUTH`, `PETMATE_IT_REDIS_CACHE`. Los nombres Atlas aprobados no cambian.
- TLS: la API exige `ssl=True` en Redis fuera de Development. Compose local usa texto plano solo en la red interna y en Development. Redis Docker en producción requerirá TLS: pendiente del spec de despliegue. Compose es solo para uso local (por defecto `Development`, expone OpenAPI/Scalar); producción usa secretos del entorno, no Compose.
