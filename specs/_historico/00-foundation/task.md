# Task — 00 Foundation

> Tareas globales T001–T012. Slice aprobado por el usuario el 2026-11-03. Ejecutar solo las tareas autorizadas explícitamente.

## Dependencias

- Spec: `spec.md`
- Plan: `plan.md`
- Constitución ratificada (2026-11-03).
- Slice aprobado por el usuario (2026-11-03).

## T001 — Crear solución y proyectos
- **Estado:** Verificado (2026-11-03) — revisado por `traceability-reviewer` sin bloqueantes.
- **Crear:** solución `PetMate.sln`; `src/PetMate.Api`, `PetMate.Application`, `PetMate.Domain`, `PetMate.Infrastructure`; proyectos `PetMate.UnitTests`, `PetMate.IntegrationTests`, `PetMate.SecurityTests`, `PetMate.ContractTests` bajo `tests/`.
- **Verificar:** `dotnet sln list`; cada proyecto apunta a `net10.0`; Api es Web API MVC; las otras tres capas son Class Library.
- **No hacer:** crear entidades/casos de uso de producto; crear MongoDB local; añadir Admin.
- **Evidencia (2026-11-03):**
  - `dotnet sln PetMate.sln list` → 8 proyectos: 4 en `src/` y 4 en `tests/`.
  - Los 8 `.csproj` declaran `<TargetFramework>net10.0</TargetFramework>`.
  - `src/PetMate.Api` usa `Microsoft.NET.Sdk.Web` con MVC (`AddControllers()`/`MapControllers()` en `Program.cs`); `Application`, `Domain`, `Infrastructure` y las 4 suites usan `Microsoft.NET.Sdk` (Class Library / test).
  - `dotnet build PetMate.sln -c Release` → compilación correcta, 0 advertencias, 0 errores.
  - `dotnet test PetMate.sln -c Release --no-build` → exit code 0; 0 pruebas ejecutadas porque las suites están vacías por diseño en T001 (no se dejaron pruebas ficticias).
  - Sin `ProjectReference` en ningún `.csproj` (estado al cierre de T001); las referencias se agregaron en T002.
  - `dotnet list PetMate.sln package --vulnerable --include-transitive` → 0 paquetes vulnerables.
  - Smoke test del host publicado: arranca y responde `HTTP 404` en `/` (no existen endpoints en T001).
  - No se creó `.env`, `.env.example`, `Dockerfile`, `compose*`, `.github/`, `Directory.Build.props` ni `.editorconfig`.
  - Scaffold de plantilla eliminado: `WeatherForecast*`, `Class1.cs`, `UnitTest1.cs`, `PetMate.Api.http`.
- **Dependencias introducidas:** solo las de las plantillas de prueba: `xunit` 2.9.3, `xunit.runner.visualstudio` 3.1.4, `Microsoft.NET.Test.Sdk` 17.14.1 y `coverlet.collector` 6.0.4 (licencias permisivas, costo $0). La Api no referencia paquetes NuGet; OpenAPI/Scalar se difiere a T007 con su auditoría.

## T002 — Configurar referencias de Clean Architecture
- **Estado:** Cerrado (2026-11-03) — revisión independiente sin bloqueantes y aprobación humana registrada.
- **Modificar:** referencias de proyecto según la constitución.
- **Verificar:** build; test/script de arquitectura confirma que Domain no referencia otras capas, Application no referencia Infrastructure/Api.
- **Evidencia (2026-11-03):**
  - Grafo final: `Domain` sin referencias; `Application` → `Domain`; `Infrastructure` → `Application`, `Domain`; `Api` → `Application`, `Infrastructure`.
  - Pruebas: `UnitTests` → `Domain`, `Application`; `IntegrationTests` → `Application`, `Infrastructure`; `SecurityTests` → `Application`, `Api`; `ContractTests` → `Application`, `Api`. Sin referencias entre proyectos de prueba.
  - `dotnet list <proyecto> reference` sobre los 8 proyectos confirma el grafo anterior y la ausencia de referencias entre suites.
  - `dotnet build PetMate.sln -c Release` → 0 advertencias, 0 errores.
  - `dotnet test PetMate.sln -c Release --no-build` → **7 superadas, 0 fallidas, 0 omitidas** (exit 0); las otras tres suites siguen sin pruebas.
  - `tests/PetMate.UnitTests/Architecture/CleanArchitectureTests.cs`: 7 casos que localizan la raíz por `PetMate.sln`, leen los `.csproj` con `System.Xml.Linq` y aplican igualdad exacta al grafo de capas fuente; en las suites verifican cotas (solo proyectos fuente, sin referencias entre sí y referencias mínimas requeridas). Sin paquetes nuevos.
  - Sin `PackageReference` nuevos; no se creó `.editorconfig`, `Directory.Build.props`, `.env`, Docker ni CI.
  - CA-002 cumplido. Revisión independiente: `specs/00-foundation/reviews/2026-11-03-traceability-t001-t002.md` (sin bloqueantes para T002).

## T003 — Estándares y administración de dependencias
- **Estado:** Cerrado (2026-11-03) — aprobación humana registrada.
- **Crear:** `.editorconfig`, `Directory.Build.props` y `Directory.Packages.props` (versiones centralizadas).
- **Modificar:** `.gitignore` para ignorar `.env` y `.env.*`, manteniendo `!.env.example` como excepción (H-04).
- **Verificar:** warnings/analyzers, nullable y convenciones; registrar licencia/costo de cada paquete en `plan.md` §6; build y pruebas sin regresiones.
- **No hacer:** añadir paquetes sin auditoría de licencia.
- **Evidencia (2026-11-03):**
  - `.editorconfig`: UTF-8, LF, indentación de 4 espacios para C# y 2 para configuración; reglas de estilo, nombres y formato para C#.
  - `Directory.Build.props`: `LangVersion=latest`, `Nullable=enable`, `ImplicitUsings=enable`, `AnalysisLevel=latest-recommended`, `EnforceCodeStyleInBuild=true` y `TreatWarningsAsErrors=true`.
  - `Directory.Packages.props`: `ManagePackageVersionsCentrally=true` con las 4 versiones existentes; se retiró `Version` de los `PackageReference` de las suites. Sin paquetes nuevos ni actualizados.
  - `.gitignore`: añadidas las reglas `.env`, `.env.*` y `!.env.example`. `git check-ignore -v .env` → `.gitignore:31:.env`; `git check-ignore -q .env.example` → exit 1 (no ignorado).
  - Analyzers: `CA1859` corregido en `CleanArchitectureTests` (retorno `string[]`, uso de `Length`); `CA1707` queda como sugerencia solo en `tests/**/*.cs`. No se silenció ninguna regla de seguridad ni se agregaron supresiones.
  - `dotnet build PetMate.sln -c Release` → 0 advertencias, 0 errores.
  - `dotnet test PetMate.sln -c Release --no-build` → 7 superadas, 0 fallidas, 0 omitidas (exit 0); Integration/Security/Contract siguen vacías.
  - `dotnet format PetMate.sln --verify-no-changes` → exit 0, sin cambios pendientes.
  - `dotnet list PetMate.sln package --vulnerable --include-transitive` → 0 paquetes vulnerables.
  - Licencias/costos registrados en `plan.md` §6: MIT y Apache-2.0, costo $0.
  - No se creó `.env` ni `.env.example` (T004); no se implementaron tareas posteriores.

## T004 — Configuración `.env` local y secretos
- **Estado:** Cerrado (2026-11-03) — aprobación humana registrada; H-05/H-06 corregidos, verificaciones repetidas y revisiones independientes satisfactorias.
- **Crear:** `.env.example`; configurar carga local de `.env` con solución de costo $0 verificada; verificar que `.env` sigue ignorado (resuelto en T003).
- **Incluir variables:** Atlas dev/test, Redis Auth, Redis Cache, JWT y futuras variables Azure/Blob/CDN como placeholders.
- **Verificar:** test confirma carga sin exponer valores; Git no rastrea `.env`; startup falla con mensaje seguro si falta configuración crítica.
- **Implementación y evidencia (2026-11-03):**
  - Se crearon `.env.example`, `src/PetMate.Api/Configuration/DotEnvFile.cs`, `StartupConfigurationValidator.cs` y `ApplicationBootstrap.cs`; se actualizó `Program.cs`; se añadieron 18 pruebas de seguridad de configuración (12 del lector, 4 del validador y 2 de bootstrap) y el `FrameworkReference` `Microsoft.AspNetCore.App` en `PetMate.SecurityTests`.
  - El lector no añade paquetes NuGet: analiza comentarios, `export`, comillas, `=` y comentarios inline controlados; no registra valores. Toda variable presente en el proceso, incluso vacía, tiene precedencia.
  - El bootstrap crea primero el host, determina el entorno efectivo y solo fuera de Production carga exactamente `<ContentRootPath>/.env`, sin recorrer padres. Las pruebas cubren carga en Development y omisión con Production efectivo por defecto.
  - `dotnet build PetMate.sln -c Release --nologo` → 0 advertencias, 0 errores. `dotnet test PetMate.sln -c Release --no-build --nologo` → Unit 7/7 y Security 18/18; Integration y Contract sin pruebas. `dotnet format PetMate.sln --verify-no-changes --verbosity minimal` → exit 0.
  - `git check-ignore -v .env` → `.gitignore:31:.env`; `git check-ignore -q .env.example` → exit 1; `Test-Path .env` → `False`.
  - Arranque Release sin configuración crítica → exit 1 con nombres de las 7 variables requeridas y sin valores. El stack trace de desarrollo sigue presente; el mensaje de validación no contiene secretos.
- **Revisión y resolución de hallazgos (2026-11-03):** H-05 y H-06 fueron corregidos con autorización humana. Las revisiones posteriores confirman que CA-005 queda satisfecha; la observación baja sobre las APIs públicas heredadas de búsqueda ascendente no bloquea el flujo de arranque, que ya usa ruta determinista.
- **Reportes:** `reviews/2026-11-03-security-t004.md`, `reviews/2026-11-03-traceability-t004.md`, `reviews/2026-11-03-security-t004-correction.md` y `reviews/2026-11-03-traceability-t004-correction.md`.

- **Aprobación humana:** el usuario autorizó el cierre de T004 y el inicio exclusivo de T005 el 2026-11-03.

## T005 — Módulos de inyección de dependencias
- **Estado:** Cerrado (2026-11-03) — aprobación humana registrada; implementación, pruebas y revisiones independientes satisfactorias. T006 autorizado exclusivamente.
- **Crear:** métodos de registro para Application e Infrastructure, con composition root en Api.
- **Verificar:** prueba de arranque/DI resuelve servicios y no hay `new` manual de servicios de infraestructura en controllers/handlers.
- **Implementación y evidencia (2026-11-03):**
  - Se crearon `ApplicationServiceCollectionExtensions.AddApplication` e `InfrastructureServiceCollectionExtensions.AddInfrastructure` como módulos no-op con null guard; no se inventaron servicios porque ambas capas no tenían servicios concretos.
  - `ApplicationBootstrap.ConfigureCompositionRoot` y `Program.cs` registran ambos módulos mediante `IServiceCollection`; se conserva `AddControllers()` y no se adelantan Mediator, MongoDB, Redis, endpoints, wrapper, health, OpenAPI, Docker o CI.
  - Se sustituyó el `FrameworkReference` ASP.NET Core de Application/Infrastructure por `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.0, centralizado en `Directory.Packages.props`; licencia MIT, costo $0, compatible con `net10.0`, sin vulnerabilidades reportadas.
  - Las pruebas DI verifican identidad, encadenamiento, colección vacía, null guards y composition root. La prueba de host construye el `ServiceProvider` y resuelve `IServiceProvider`; no inicia servicios externos ni registra servicios ficticios. No existen controllers/handlers ni instanciaciones manuales de infraestructura en el alcance actual; esa inspección queda documentada como limitación estructural.
  - `dotnet build PetMate.sln -c Release --nologo` → 0 advertencias, 0 errores. `dotnet test PetMate.sln -c Release --no-build --nologo` → Unit 7/7 y Security 24/24; Integration/Contract sin pruebas. `dotnet format PetMate.sln --verify-no-changes --verbosity minimal` → exit 0.
  - Referencias: Domain ∅; Application → Domain; Infrastructure → Application/Domain; Api → Application/Infrastructure. No se agregaron referencias entre suites.
  - `dotnet list PetMate.sln package --vulnerable --include-transitive` → 0 vulnerabilidades. No se implementaron tareas T006+.
- **Revisiones independientes:** security review y traceability review detectaron inicialmente la referencia web amplia, falta de resolución explícita y drift documental; tras la corrección, la evidencia quedó conciliada y los hallazgos fueron resueltos sin bloqueantes.

## T006 — CQRS y Mediator
- **Estado:** Cerrado (2026-11-03) — aprobación humana registrada; H-07/H-08/H-09 resueltos, validaciones repetidas y revisiones independientes PASS. T007 autorizado exclusivamente.
- **Crear:** convención base de Command, Query, Handler y despacho Mediator en Application/Api.
- **Verificar:** un caso de uso de prueba se despacha desde controller a handler por Mediator.
- **Restricción:** la dependencia elegida debe tener licencia/costo $0 verificados. No asumir que un paquete popular es gratuito.
- **Implementación y evidencia inicial (2026-11-03):**
  - Se crearon `IRequest`, `ICommand`, `IQuery`, `IRequestHandler`, `ISender`, `IMediator` y `MediatorDispatcher` en Application; `AddApplication` registra `IMediator`/`ISender` mediante DI.
  - Se eligió una implementación propia sin paquete Mediator: costo/licencia de dependencia adicional $0; no se agregó `PackageReference` nuevo.
  - La prueba ficticia actual verifica `ISender → ProbeHandler` y que el controlador no referencia el handler concreto, pero usa una clase privada de test y no un MVC Controller real.
  - `dotnet build PetMate.sln -c Release --nologo` → 0 advertencias, 0 errores. `dotnet test PetMate.sln -c Release --no-build --nologo` → Unit 7/7 y Security 25/25; Integration/Contract sin pruebas. `dotnet format PetMate.sln --verify-no-changes --verbosity minimal` → exit 0. Vulnerabilidades NuGet: 0.
  - No se implementaron T007+: sin OpenAPI/Scalar, versionado, wrapper, MongoDB, Redis, health, Docker, CI ni endpoints funcionales.
- **Hallazgos bloqueantes (2026-11-03):**
  - H-07 (media): `MediatorDispatcher` recibe `IServiceProvider` y resuelve handlers dinámicamente; las revisiones lo consideran Service Locator contrario a la constitución §2.4.
  - H-08 (media): CA-003 exige evidencia desde un controller de Presentation; la prueba usa `ProbeController` privado dentro de SecurityTests y no un MVC Controller real.
  - H-09 (media): falta evidencia documental inicial de T006 y la auditoría formal de la decisión de implementación propia.
- **Reportes:** revisiones independientes `security-reviewer` y `traceability-reviewer` del 2026-11-03; T006 no verificada ni cerrada.
- **Corrección y evidencia final (2026-11-03):**
  - `MediatorDispatcher` recibe `MediatorRegistry` explícito e inmutable; cada `RequestInvoker<TRequest,TResponse>` recibe directamente su handler. No usa `IServiceProvider`, `GetService`, reflexión ni Service Locator.
  - `TechnicalMediatorProbeController` es un MVC Controller real en `src/PetMate.Api/Controllers`, deriva de `ControllerBase`, usa `[ApiController]` y depende únicamente de `ISender`. La prueba compone explícitamente request, handler e invocador y verifica el despacho.
  - `ADR-001-mediator-propio.md` documenta decisión, alternativas, funcionamiento, responsabilidades, límites, pruebas y costo $0.
  - Build Release 0 advertencias/0 errores; Unit 7/7; Security 25/25; Integration/Contract sin pruebas; format exit 0; vulnerabilidades NuGet 0. No se implementó T007+.
  - Revisiones independientes finales de seguridad y trazabilidad: PASS, sin bloqueantes.
- **Reportes:** `reviews/2026-11-03-security-t006.md`, `reviews/2026-11-03-traceability-t006.md`, `reviews/2026-11-03-security-t006-correction.md` y `reviews/2026-11-03-traceability-t006-correction.md`.

- **Aprobación humana:** el usuario aprobó el cierre de T006 y autorizó iniciar T007 el 2026-11-03.

## T007 — Versionado, convenciones MVC, OpenAPI y Scalar
- **Estado:** Cerrado (2026-11-03) — aprobación humana registrada. T008 y T009 autorizadas consecutivamente.
- **Crear:** configuración de rutas versionadas `/api/v1/...` y convención de acción explícita.
- **Configurar:** generación del documento OpenAPI desde ASP.NET Core y Scalar como única UI interactiva de documentación de la API. Rutas iniciales previstas: `/openapi/v1.json` y `/scalar/v1`.
- **Configurar:** título y versión del documento; esquema HTTP Bearer/JWT para que las operaciones protegidas puedan documentarse cuando se implemente autenticación.
- **Disponibilidad:** OpenAPI y Scalar habilitados por defecto en Development; deshabilitados en producción salvo habilitación explícita por configuración.
- **Dependencia:** antes de agregar el paquete Scalar, comprobar compatibilidad con .NET 10, licencia y costo $0, mantenimiento y vulnerabilidades; registrar evidencia.
- **Restricción:** no agregar Swagger UI, Swashbuckle UI, NSwag UI ni otra UI alternativa.
- **Verificar:** pruebas de ruta validan versión y acción; el JSON OpenAPI es válido; Scalar carga el documento y presenta la API; no se filtran secretos ni configuración interna. Rutas sin versión no deben caer silenciosamente en v1.
- **Implementación y evidencia (2026-11-03):**
  - La candidata C autorizada se incorporó de forma centralizada: `Microsoft.AspNetCore.OpenApi` 10.0.12, `Microsoft.OpenApi` 2.12.0 y `Scalar.AspNetCore` 2.11.2. ADR: `decisions/ADR-002-openapi-scalar.md`. La combinación anterior 10.0.0/2.0.0 permanece rechazada por `GHSA-v5pm-xwqc-g5wc`.
  - En Development: `/openapi/v1.json` y `/scalar/v1`; Scalar es la única UI. En Production ambas rutas responden 404 por defecto. El documento contiene título `PetMate API`, versión `v1`, esquema Bearer/JWT documental y la ruta MVC técnica `GET /api/v1/technical/mediator-probe`.
  - La ruta técnica versionada responde 200 con `mediated`; `/technical/mediator-probe` responde 404. No se agregaron endpoints de negocio, wrapper, autenticación, persistencia ni T008+.
  - Verificación local reproducible: `dotnet build PetMate.sln -c Release --nologo` → exit 0, 0 advertencias, 0 errores; Unit 7/7, Security 26/26 y Contract 4/4, todos exit 0; `dotnet format PetMate.sln --verify-no-changes --verbosity minimal` → exit 0; `dotnet list PetMate.sln package --vulnerable --include-transitive` → exit 0, 0 vulnerabilidades.
  - IntegrationTests no son requisito aplicable a T007: `tests/PetMate.IntegrationTests` no contiene archivos `.cs`, no conecta Atlas ni Redis y T007 no modifica infraestructura. Su ejecución local intenta cargar una DLL de suite vacía y WDAC/App Control la bloquea (`0x800711C7`, eventos Code Integrity 3033/3077, proceso `testhost.exe`). T010 definirá infraestructura y pruebas de integración contra Atlas test/Redis no productivos. No se declara prueba Integration aprobada.
  - Revisiones independientes: `security-reviewer` y `traceability-reviewer`, sin hallazgos bloqueantes; ver reportes `reviews/2026-11-03-security-t007.md` y `reviews/2026-11-03-traceability-t007.md`.

## T008 — Response Wrapper y middleware de errores
- **Estado:** Cerrado y aprobado (2026-11-03, aprobación humana). Implementada y probada. Evidencia 2026-11-03: build Release 0/0; format exit 0; NuGet 0 vulnerables; Unit 12/12 (host Windows); Security 40/40 y Contract 16/16 ejecutadas en contenedor Docker mcr.microsoft.com/dotnet/sdk:10.0 (código en solo lectura, copia sin .env, --cap-drop ALL, sin Atlas/Redis) porque Smart App Control (política {0283ac0f-fff1-49ae-ada1-8a933130cad6}, eventos CI 3033/3077, 0x800711C7) bloquea en host el PetMate.Api.dll sin reputación; SAC no se modificó. Security pasó de 34 a 40 por las pruebas de endurecimiento. Diferido: documentar ApiResponse/errores en esquemas OpenAPI (plan.md:38) a T010-T012 o primer slice de negocio. Revisiones independientes: seguridad PASS, trazabilidad PASS condicionado (docs resueltas). Brechas menores abiertas: sin pruebas de límites 400/599 de ApiException ni del tope de errores en binding MVC. Trabajo NO resuelto: documentar ApiResponse/errores en esquemas OpenAPI (plan.md:38), diferido.
- **Crear:** wrapper genérico, factory/helper de respuestas y middleware global de excepciones.
- **Verificar:** respuesta exitosa/error contiene solo `statusCode`, `message`, `data`; `data:null` funciona; `statusCode` coincide con HTTP; error interno no filtra detalles.
- **Estado previo recuperado:** existían `ApiResponse<T>`, `ApiResponseFactory` (Ok/500), `ApiException`, `ExceptionHandlingMiddleware` y 4 pruebas en SecurityTests (sin pruebas HTTP, sin envoltura de 4xx sin cuerpo ni del 400 automático de MVC, sin log). Se reutilizaron y se completaron.
- **Implementación:** `ApiResponseMessages` (mensajes en español), `ApiResponseFactory.WriteAsync/BadRequest` como punto único de escritura, middleware con log del servidor (sin detalle al cliente), manejo de cancelación y respuesta ya iniciada, `UseStatusCodePages` para 401/403/404/405 sin cuerpo, `SuppressMapClientErrors` (evita ProblemDetails de `[ApiController]`; hallazgo detectado por las pruebas) y `InvalidModelStateResponseFactory` con mensajes genéricos.
- **Evidencia:** ver `checklists/gates.md` (T008). ContractTests HTTP cubren éxito, `data:null`, 401/403/404/405, error controlado, 500 sin fuga, binding inválido.

## T009 — Validación de requests y contratos básicos
- **Estado:** Cerrado y aprobado (2026-11-03, aprobación humana; política 400 ratificada). Implementada y probada. Evidencia 2026-11-03: build Release 0/0; format exit 0; NuGet 0 vulnerables; Unit 12/12 (host Windows); Security 40/40 y Contract 16/16 ejecutadas en contenedor Docker mcr.microsoft.com/dotnet/sdk:10.0 (código en solo lectura, copia sin .env, --cap-drop ALL, sin Atlas/Redis) porque Smart App Control (política {0283ac0f-fff1-49ae-ada1-8a933130cad6}, eventos CI 3033/3077, 0x800711C7) bloquea en host el PetMate.Api.dll sin reputación; SAC no se modificó. Security pasó de 34 a 40 por las pruebas de endurecimiento. Diferido: documentar ApiResponse/errores en esquemas OpenAPI (plan.md:38) a T010-T012 o primer slice de negocio. Revisiones independientes: seguridad PASS, trazabilidad PASS condicionado (docs resueltas). Política 400 ratificada (no se usa 422). Brechas menores y pendiente OpenAPI del wrapper (plan.md:38) siguen abiertos, no declarados cumplidos.
- **Crear:** pipeline de validación integrado con Mediator o frontera MVC, errores consistentes en español.
- **Verificar:** request inválido devuelve wrapper y 400/422 según política aprobada; no ejecuta handler.
- **Estado previo recuperado:** no iniciado (no existía código de validación).
- **Decisión de política (a ratificar al cierre):** se usa **400** para toda validación de forma; 422 no se adopta (`docs/api/endpoints.md` lo dejaba condicionado). `data` = `{ "errors": [ { "field", "message" } ] }`. Sin paquete nuevo (FluentValidation no se agregó; coherente con ADR-001, costo $0).
- **Implementación:** `IRequestValidator<T>`, `ValidationFailure`, `RequestValidationException` en Application; `RequestInvoker` ejecuta todos los validadores y acumula errores antes del handler; `AddRequestHandler`/`AddRequestValidator` (registro explícito, sin Service Locator en el dispatcher); middleware mapea a 400 wrapper; binding MVC inválido produce el mismo formato.
- **Evidencia:** ver `checklists/gates.md` (T009).

## T010 — Configuración MongoDB Atlas y Redis separados
- **Estado:** Cerrado y aprobado (2026-11-03, aprobación humana). Verificada con Atlas real.
- **Crear:** configuración/health checks para Atlas, Redis Auth y Redis Cache.
- **Verificar:** conexiones son independientes; IntegrationTest apunta a base Atlas test distinta; startup no usa fallback en memoria ni Mongo local.
- **No hacer:** agregar servicio Mongo a Compose.
- **Implementación:** `InfrastructureSettings.Parse` (claves aprobadas, sin valores en errores; Redis exige contraseña y `ssl=True` fuera de Development; Auth y Cache deben ser instancias distintas); `RedisAuthConnection`/`RedisCacheConnection` (tipos distintos, conexión perezosa y reconectable); `IMongoClient`/`IMongoDatabase` singleton desde `MONGODB_URI`/`MONGODB_DATABASE` (sin ramas por proveedor); `MongoHealthCheck` y `RedisHealthCheck<T>` sin detalles. Paquetes: MongoDB.Driver 3.13.0 (Apache-2.0), StackExchange.Redis 3.4.31 (MIT), Diagnostics.HealthChecks 10.0.14 (MIT); 0 vulnerables.
- **Verificación Atlas (2026-11-03):** con `.env` local (ignorado por Git) se ejecutaron en contenedor las 9 pruebas de IntegrationTests: 9/9 aprobadas, 0 omitidas (persistencia básica en la base de pruebas distinta de la de desarrollo, health Mongo Healthy, Redis Auth/Cache). Stack Compose con Atlas real: `/api/v1/health/ready` → 200 con mongodb, redis-auth y redis-cache Healthy; sin URI ni contraseñas en la respuesta ni en los logs de la API.
- **Evidencia:** ver `checklists/gates.md` (T010).

## T011 — Health, contenedor y ejecución local
- **Estado:** Cerrado y aprobado (2026-11-03, aprobación humana).
- **Crear:** Dockerfile multi-stage para Api y Compose con `api`, `redis-auth`, `redis-cache`; health liveness/readiness.
- **Verificar:** imagen se ejecuta; readiness detecta dependencias configuradas sin exponer URI/secrets; dos Redis tienen puertos/volúmenes/config separados.

- **Implementación:** `Dockerfile` multi-stage (sdk:10.0 → aspnet:10.0, UID 1654, sin `.env`), `.dockerignore`, `docker-compose.yml` (api, redis-auth, redis-cache; sin MongoDB) y `docker-compose.debug-ports.yml` opcional (loopback). `HealthController`: `/api/v1/health/live` y `/ready` con wrapper (503 si falla una dependencia). Documentación: `docs/infrastructure/docker-redis-atlas.md`.
- **Evidencia:** ver `checklists/gates.md` (T011).

## T012 — CI base DevSecOps
- **Estado:** Implementada; NO cerrada (2026-11-03). Verificados: SAST local, formato, NuGet vulnerabilidades/licencias, secret scan, suites. Pendientes y diferidos (decisión: trabajo local, sin herramienta $0 verificada): workflow PR YAML, escaneo de contenedores, SBOM. Excepción xunit 2.9.3 Legacy aprobada por el responsable del proyecto el 2026-11-03 (revisión 2027-02-03; se retira al migrar a xunit.v3, ante vulnerabilidad o al vencer sin renovación); la migración a xunit.v3 sigue pendiente de evaluación.
- **Crear:** workflow PR con restore, build, unit, integración (cuando esté configurada la base Atlas test), seguridad, contrato y análisis $0 elegidos.
- **Verificar:** workflow YAML válido; fallos de escaneo bloquean; no existe deploy Azure ni herramienta que genere cargos.
- **Implementación local (sin CI remoto):** `scripts/quality-gate.ps1` ejecuta restore, build Release con SAST (analizadores del SDK, `AnalysisModeSecurity=All`, advertencias como errores), `dotnet format --verify-no-changes`, auditoría NuGet de vulnerabilidades, paquetes en desuso (con `scripts/quality-exceptions.json`: una excepción sin approved/aprobador/fecha no vale) y licencias (lista permitida), más las 4 suites en contenedor SDK (SAC bloquea binarios nuevos en el host). Detección de secretos: `SecretScanTests` (SecurityTests) con controles positivos y negativos; `.env` real nunca se lee.
- **Evidencia 2026-11-03:** SAST verificado con control positivo (SHA1 → CA5350 falla el build); secretos: escáner con 5 casos positivos y 3 negativos; gate: restore/build/format/vulnerabilidades/licencias (39 paquetes) OK; suites: Unit 12/12, Security 62/62, Contract 18/18, Integration 4 aprobadas + 5 Skipped (Redis, sin red/variables de Compose en esa corrida); única falla: xunit 2.9.3 Legacy sin excepción aprobada. Hallazgos corregidos por el gate: el escáner marcaba fixtures de prueba y un test de health dependía de variables de entorno ambientales (ahora aísla su entorno).
- **Pendiente:** aprobar o rechazar la excepción xunit (migrar a xunit.v3 o aceptar Legacy); decidir si se crea workflow de CI remoto; contenedor/SBOM/Docker scan quedan fuera (sin herramienta $0 verificada).

## Definition of Done
- [ ] T001–T012 verificados con evidencia.
- [ ] CA-001 a CA-010 cubiertos.
- [ ] Gates de `checklists/gates.md` actualizados.
- [ ] Security/traceability reviewers emitieron reporte.
- [ ] Aprobación humana pendiente.
