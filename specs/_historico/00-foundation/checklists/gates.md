# Gates — 00 Foundation

- [ ] T001–T012 completados y con evidencia.
- [x] Build Release correcto en .NET 10. _(T001–T002; re-verificar al cierre)_
- [x] Referencias de Clean Architecture verificadas. _(T002; re-verificar al cierre)_
- [ ] Mediator seleccionado con licencia/costo $0 documentados. _(T006 usa implementación propia sin paquete; decisión y bloqueo H-09 pendientes de resolución.)_
- [x] `.env` ignorado; `.env.example` no contiene secretos; carga local no se ejecuta en Production. _(T004 verificado con pruebas de bootstrap.)_
- [ ] No existe servicio MongoDB local en Docker Compose.
- [ ] Mongo Atlas, Redis Auth y Redis Cache son conexiones separadas.
- [x] Response Wrapper uniforme y `data:null` probado. _(T008; pendiente de cierre humano.)_
- [x] API versionada y con acción literal. _(T007: ruta técnica versionada 200; ruta equivalente no versionada 404.)_
- [x] Documento OpenAPI válido y UI Scalar funcional en Development; Scalar es la única UI; exposición en producción deshabilitada por defecto. _(T007 ContractTests 4/4.)_
- [x] Compatibilidad, licencia/costo $0 y vulnerabilidades del paquete Scalar documentadas. _(ADR-002; auditoría NuGet sin vulnerabilidades.)_
- [x] Unit/Integration/Security/Contract base ejecutados donde aplique. _(T004: Unit/Security ejecutados; Integration/Contract continúan sin pruebas porque T004 no cambia contrato ni usa dependencias externas.)_
- [ ] SAST, secret scan y dependency/license audit ejecutados.
- [ ] No se crearon recursos Azure ni herramientas pagas.
- [x] Security review y traceability review realizados. _(T004: corrección revisada sin bloqueantes; las revisiones no sustituyen aprobación humana.)_
- [x] Aprobación humana registrada para T004. _(El usuario autorizó el cierre de T004 y el inicio exclusivo de T005 el 2026-11-03.)_

## Evidencia
| Gate | Resultado | Evidencia | Fecha |
|---|---|---|---|
| T001 — Solución y proyectos | Cumple | `dotnet sln PetMate.sln list` → 8 proyectos; `net10.0` en 8/8; Api `Microsoft.NET.Sdk.Web` con MVC; 3 capas y 4 suites con `Microsoft.NET.Sdk` | 2026-11-03 |
| T001 — Build Release | Correcto | `dotnet build PetMate.sln -c Release` → 0 advertencias, 0 errores | 2026-11-03 |
| T001 — Suites base | Ejecutables, 0 pruebas | `dotnet test PetMate.sln -c Release --no-build` → exit 0; suites sin pruebas todavía (T001 no define pruebas) | 2026-11-03 |
| T001 — Dirección de dependencias | Cumple para T001 | Ningún `ProjectReference` en los 8 `.csproj` (estado al cierre de T001); el grafo se configura en T002 | 2026-11-03 |
| T001 — Dependencias y costo | Cumple | `dotnet list PetMate.sln package --vulnerable --include-transitive` → 0 vulnerables; solo paquetes de prueba xUnit/Microsoft.NET.Test.Sdk/coverlet con licencia permisiva y costo $0 | 2026-11-03 |
| T001 — Secretos e infraestructura | Cumple | Sin `.env`, `Dockerfile`, `compose*`, `.github/`; `appsettings*.json` y `launchSettings.json` sin credenciales | 2026-11-03 |
| Aprobación de inicio del slice (no cierre) | Aprobada | Ratificación constitucional `1.0.1` y aprobación de spec/plan/task/gates por el usuario | 2026-11-03 |
| T002 — Grafo de referencias | Cumple | `Domain` ∅; `Application` → `Domain`; `Infrastructure` → `Application`, `Domain`; `Api` → `Application`, `Infrastructure`; suites solo a proyectos fuente y sin referencias entre sí | 2026-11-03 |
| T002 — Prueba de arquitectura | 7/7 | `dotnet test PetMate.sln -c Release --no-build` → 7 superadas, 0 fallidas, 0 omitidas, exit 0; `CleanArchitectureTests` aplica igualdad exacta al grafo de capas fuente y cotas en las suites | 2026-11-03 |
| T002 — CA-002 | Cumplido | Build Release 0 advertencias/0 errores + `dotnet list reference` por proyecto confirma la dirección constitucional | 2026-11-03 |
| T001–T002 — Traceability review | Sin bloqueantes para T002 | `specs/00-foundation/reviews/2026-11-03-traceability-t001-t002.md`; 1 hallazgo bloqueante para T003 (H-04) | 2026-11-03 |
| T002 — Aprobación humana | Aprobada | El usuario registró el cierre de T002 tras la revisión independiente | 2026-11-03 |
| H-04 — `.env` en `.gitignore` | Resuelto: se incluye en T003 | Decisión humana 2026-11-03: T003 modifica `.gitignore` con `.env`, `.env.*` y `!.env.example`; T004 conserva la creación de `.env.example` y la carga de configuración | 2026-11-03 |
| T003 — Estándares y dependencias | Cumple | `.editorconfig`, `Directory.Build.props` (`Nullable`, `ImplicitUsings`, `AnalysisLevel=latest-recommended`, `EnforceCodeStyleInBuild`, `TreatWarningsAsErrors`) y `Directory.Packages.props` con versiones centralizadas de los 4 paquetes existentes | 2026-11-03 |
| T003 — Build, tests y formato | Cumple | `dotnet build -c Release` → 0 advertencias/0 errores; `dotnet test --no-build` → 7 superadas, 0 fallidas; `dotnet format --verify-no-changes` → exit 0 | 2026-11-03 |
| T003 — `.env` ignorado | Cumple | `git check-ignore -v .env` → `.gitignore:31:.env`; `git check-ignore -q .env.example` → exit 1 (no ignorado), por la excepción `!.env.example` | 2026-11-03 |
| T003 — Licencias y vulnerabilidades | Cumple | `plan.md` §6: xUnit/Apache-2.0, Test SDK/MIT, coverlet/MIT, runner/Apache-2.0, costo $0; `dotnet list package --vulnerable --include-transitive` → 0 | 2026-11-03 |
| T003 — Aprobación humana | Aprobada | El usuario autorizó T003 y ordenó continuar con T004; no se realizó revisión independiente separada de T003 | 2026-11-03 |
| T004 — Implementación de configuración | Cumple | `.env.example` con 12 placeholders; `DotEnvFile` sin dependencias; `StartupConfigurationValidator` con 7 claves; `ApplicationBootstrap` integrado en `Program.cs`; 18 pruebas de seguridad de configuración | 2026-11-03 |
| T004 — Build, pruebas y formato | Correcto | `dotnet build PetMate.sln -c Release --nologo` → 0 advertencias/0 errores; `dotnet test PetMate.sln -c Release --no-build --nologo` → Unit 7/7 y Security 18/18, 0 fallidas; Integration/Contract sin pruebas; `dotnet format ... --verify-no-changes` → exit 0 | 2026-11-03 |
| T004 — Secretos y Git | Cumple | `.env` no existe; `git check-ignore -v .env` → `.gitignore:31:.env`; `.env.example` no ignorado (exit 1) y contiene placeholders ficticios. La carga de arranque se omite con Production efectivo. | 2026-11-03 |
| T004 — Arranque y configuración | Cumple | `ApplicationBootstrapTests`: Development carga `<ContentRootPath>/.env` y lo incorpora a configuración; Production efectivo por defecto omite el archivo. Sin configuración crítica, `dotnet run --project src/PetMate.Api -c Release --no-build` → exit 1 y mensaje solo con 7 nombres requeridos, sin valores. | 2026-11-03 |
| T004 — Dependencias | Cumple | Sin `PackageReference` nuevo; `Microsoft.AspNetCore.App` es FrameworkReference. El lector `.env` es propio. No se añadieron servicios, herramientas o paquetes de costo | 2026-11-03 |
| T004 — Security review | Cumple | Revisión posterior: H-05/H-06 resueltos; sin hallazgos críticos/altos. Observación baja: las APIs públicas heredadas de búsqueda ascendente no son usadas por el arranque. Ver `reviews/2026-11-03-security-t004-correction.md` | 2026-11-03 |
| T004 — Traceability review | Cumple | Revisión posterior: H-05/H-06 resueltos, CA-005 trazada hasta pruebas de bootstrap, sin alcance T005+. Ver `reviews/2026-11-03-traceability-t004-correction.md` | 2026-11-03 |
| T004 — Cierre humano | Cerrado | El usuario autorizó explícitamente el cierre de T004 y el inicio exclusivo de T005 | 2026-11-03 |
| T005 — Módulos DI | Cumple | `AddApplication`/`AddInfrastructure` no-op con null guard; composition root en `ApplicationBootstrap`/`Program`; no servicios ficticios ni tareas posteriores | 2026-11-03 |
| T005 — Pruebas DI | 31/31 | `dotnet test PetMate.sln -c Release --no-build --nologo` → Unit 7/7, Security 24/24, Integration/Contract sin pruebas; incluye resolución de `IServiceProvider` en composition root | 2026-11-03 |
| T005 — Paquete DI | Cumple | `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.0, MIT/$0, compatible `net10.0`, centralizado; `dotnet list ... --vulnerable --include-transitive` → 0 | 2026-11-03 |
| T005 — Referencias y alcance | Cumple | Grafo Clean Architecture sin cambios indebidos; sin Mediator, MongoDB, Redis, endpoints, wrapper, health, OpenAPI/Scalar, Docker, CI o funcionalidades de negocio | 2026-11-03 |
| T005 — Security/traceability review | Cumple | Revisiones posteriores sin bloqueantes tras resolver referencia web amplia, resolución del provider y drift documental | 2026-11-03 |
| T005 — Cierre humano | Cerrado | El usuario aprobó explícitamente el cierre de T005 y autorizó iniciar T006; T007 no autorizado | 2026-11-03 |
| T006 — Implementación inicial | Parcial/Bloqueado | Interfaces CQRS/Mediator, dispatcher propio, registros DI y prueba de despacho inicial; build 0/0, Unit 7/7, Security 25/25, formato correcto, vulnerabilidades 0 | 2026-11-03 |
| T006 — Security review | Bloqueado | H-07: uso directo de `IServiceProvider` como Service Locator; H-08: prueba no usa MVC Controller real. Revisión independiente, sin modificaciones | 2026-11-03 |
| T006 — Traceability review | Bloqueado | H-08/H-09: CA-003 no demostrada con controller MVC real; falta evidencia documental y decisión formal del Mediator propio. Revisión independiente, sin modificaciones | 2026-11-03 |
| T006 — Corrección H-07/H-08/H-09 | Cumple | Dispatcher recibe registro explícito inmutable de invocadores y no usa `IServiceProvider`; controlador MVC técnico en `src/PetMate.Api/Controllers`; ADR-001 documenta decisión, alternativas, límites, pruebas y costo $0. Build 0/0; Unit 7/7; Security 25/25; Integration/Contract sin pruebas; formato correcto; vulnerabilidades 0 | 2026-11-03 |
| T006 — Security review final | Cumple | PASS: sin Service Locator, reflexión ni resolución implícita; dependencias explícitas, controller MVC real y sin alcance T007+ | 2026-11-03 |
| T006 — Traceability review final | Cumple | PASS: RF-002/CA-003 trazadas a MediatorRegistry, controller, DI, ADR y pruebas; sin drift bloqueante | 2026-11-03 |
| T006 — Cierre humano | Cerrado | El usuario aprobó explícitamente T006 y autorizó iniciar exclusivamente T007; T008 no autorizado | 2026-11-03 |
| T007 — Auditoría de dependencias | Bloqueado | `Microsoft.AspNetCore.OpenApi` 10.0.0 requiere `Microsoft.OpenApi` 2.0.0, reportado con vulnerabilidad alta `GHSA-v5pm-xwqc-g5wc`; `Scalar.AspNetCore` 2.11.2 no se incorpora. Ver `decisions/ADR-002-openapi-scalar.md` | 2026-11-03 |
| T007 — Alcance preservado | Cumple | No se agregaron paquetes, rutas, OpenAPI/Scalar, Swagger UI, endpoints de negocio ni T008+; build/tests/formato posteriores permanecen correctos y vulnerabilidades actuales 0 | 2026-11-03 |
| T007 — Paquetes y auditoría | Cumple | `Microsoft.AspNetCore.OpenApi` 10.0.12, `Microsoft.OpenApi` 2.12.0 y `Scalar.AspNetCore` 2.11.2 centralizados; ADR-002 registra licencia MIT/$0, compatibilidad `net10.0`, fuentes y rechazo de `GHSA-v5pm-xwqc-g5wc`; `dotnet list PetMate.sln package --vulnerable --include-transitive` → exit 0, 0 vulnerabilidades | 2026-11-03 |
| T007 — OpenAPI, Scalar y rutas | Cumple | ContractTests 4/4: Development `/openapi/v1.json` 200 y documento con `PetMate API`/`v1`/ruta MVC; `/scalar/v1` 200 y referencia OpenAPI; Production 404 para ambas; `GET /api/v1/technical/mediator-probe` 200 `mediated`; ruta no versionada 404 | 2026-11-03 |
| T007 — Seguridad y regresión local | Cumple con limitación Integration separada | Build Release exit 0, 0 advertencias/errores; Unit 7/7, Security 26/26, Contract 4/4, todos exit 0; formato exit 0. SecurityTest prohíbe referencias Swashbuckle/NSwag | 2026-11-03 |
| T007 — IntegrationTests | No aplicable; no aprobada | La suite no contiene archivos `.cs`, no configura Atlas/Redis ni pruebas para T007. La carga local de su DLL vacía queda bloqueada por WDAC/App Control: `0x800711C7`, Code Integrity 3033/3077, `testhost.exe` y `PetMate.IntegrationTests.dll`. T010 define la infraestructura y validación de integración; no se cuenta como prueba aprobada | 2026-11-03 |
| T007 — Revisiones independientes | Cumple | `security-reviewer` y `traceability-reviewer`: sin hallazgos bloqueantes; reportes `reviews/2026-11-03-security-t007.md` y `reviews/2026-11-03-traceability-t007.md` | 2026-11-03 |
| T007 — Cierre humano | Cerrado | El usuario aprobó explícitamente T007 y autorizó T008/T009 consecutivamente | 2026-11-03 |

| T008 — Estado previo | Recuperado | Existían wrapper, factory, ApiException, middleware y 4 pruebas Security; reutilizados y completados (status pages, ProblemDetails suprimido, binding 400, log, cancelación, BadHttpRequest) | 2026-11-03 |
| T008/T009 — Pruebas | Cumple | Evidencia 2026-11-03: build Release 0/0; format exit 0; NuGet 0 vulnerables; Unit 12/12 (host Windows); Security 40/40 y Contract 16/16 ejecutadas en contenedor Docker mcr.microsoft.com/dotnet/sdk:10.0 (código en solo lectura, copia sin .env, --cap-drop ALL, sin Atlas/Redis) porque Smart App Control (política {0283ac0f-fff1-49ae-ada1-8a933130cad6}, eventos CI 3033/3077, 0x800711C7) bloquea en host el PetMate.Api.dll sin reputación; SAC no se modificó. Security pasó de 34 a 40 por las pruebas de endurecimiento. Diferido: documentar ApiResponse/errores en esquemas OpenAPI (plan.md:38) a T010-T012 o primer slice de negocio. | 2026-11-03 |
| T008/T009 — Revisión de seguridad | PASS condicionado | Hallazgos medios 1 (BadHttpRequest→500) y 2 (errores sin límite) corregidos; bajo 4 (ApiException rango) corregido; 3/5/6 aceptados y documentados | 2026-11-03 |
| T008/T009 — Revisión de trazabilidad | PASS | Sin bloqueantes de código; docs actualizadas en esta entrega | 2026-11-03 |
| T008/T009 — Reverificación tras endurecimiento | Cumple (vía contenedor) | Security 40/40 y Contract 16/16 en Docker; revisión final de seguridad PASS (sin hallazgos medios/altos) y de trazabilidad PASS condicionado (docs, resueltas aquí) | 2026-11-03 |
| IntegrationTests | Sin pruebas | Suite vacía; no se declara aprobada (T010 define integración real) | 2026-11-03 |
| T008/T009 — Cierre humano | Cerrado | El usuario aprobó el cierre de T008 y T009 y ratificó la política de validación HTTP 400 con data.errors[{field,message}] (sin 422). Abiertos: brechas menores de pruebas y documentación del wrapper en OpenAPI (plan.md:38) | 2026-11-03 |

| T010 — Pruebas | Cumple | Contenedores Docker: Unit 12/12, Security 53/53, Contract 18/18, Integration 9/9 (0 omitidas, incluidas las 2 de Atlas con base de pruebas distinta); stack con Atlas real: /ready 200 con las 3 dependencias Healthy, sin fuga de URI/contraseñas en respuesta ni logs. Ejecución en contenedor por bloqueo de Smart App Control en host | 2026-11-03 |
| T010 — Dependencias | Cumple | MongoDB.Driver 3.13.0 Apache-2.0; StackExchange.Redis 3.4.31 MIT; HealthChecks/DI/Configuration 10.0.14 MIT; costo $0 | 2026-11-03 |
| T011 — Docker/Compose | Cumple | build de imagen correcto; api/redis-auth/redis-cache healthy; /live 200; /ready 503 con mongodb Unhealthy (URI ficticia) y ambos Redis Healthy; persistencia AOF de Redis Auth tras stop/start, caché vacía; Redis sin puertos publicados; API solo 127.0.0.1; no root, read_only, cap_drop ALL; sin MongoDB en Compose | 2026-11-03 |
| T010/T011 — Revisión seguridad | PASS con observaciones | Sin bloqueantes. Observaciones documentadas (Development por defecto en Compose, secretos visibles en docker inspect local, licencia redis 7.4, tags sin digest) | 2026-11-03 |
| T010/T011 — Revisión trazabilidad | PASS condicionado | Condición (pruebas Atlas) cumplida el 2026-11-03; documentación actualizada | 2026-11-03 |
| T010/T011 — Cierre humano | Cerrado | El usuario aprobó el cierre de T010 y T011 y autorizó T012–T015; T016 no autorizada. Abiertos: TLS Redis Docker en producción, licencia/digest redis:7.4, wrapper OpenAPI | 2026-11-03 |

| T012 — Puerta de calidad local | Parcial | scripts/quality-gate.ps1: SAST (CA5350 probado), formato, NuGet vulnerabilidades 0, licencias 39 paquetes OK, secret scan 62/62 Security; falla únicamente por xunit 2.9.3 Legacy sin excepción aprobada | 2026-11-03 |
| T012 — CI remoto | No cumplido (diferido) | Sin workflow por decisión 20 (sin GitHub/CI remoto); requiere decisión humana | 2026-11-03 |

| T012–T015 — Regresión acumulada | Cumple | scripts/quality-gate.ps1 exit 0: restore, build Release + SAST, formato, NuGet 0 vulnerables / 0 en desuso (excepción xunit aprobada y vigente hasta 2027-02-03), licencias 41 paquetes; Unit 71, Security 97, Contract 18, Integration 24 (0 omitidas, con Atlas test y Redis de Compose); 210 pruebas aprobadas | 2026-11-03 |

Nota CA-001: se valida con comandos reproducibles (`dotnet build`/`dotnet test`) y no con una prueba automatizada de estructura; la justificación y su reevaluación en T012 quedan registradas. La prueba de arquitectura cubre CA-002 de forma automatizada.

Estado: En progreso — T001–T007 cerrados; T008/T009 autorizadas; T010–T012 pendientes y no autorizadas. Cierre de slice pendiente de gates completos y aprobación humana.
