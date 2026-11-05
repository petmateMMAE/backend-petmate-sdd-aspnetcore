# Memoria del proyecto — PetMate Backend

> Bitácora de estado y decisiones. No contiene secretos, tokens, credenciales ni datos personales.
> La constitución canónica está en `.specify/memory/constitution.md`.

## Estado actual

- **Proyecto:** PetMate Backend.
- **Estado:** documentación SDD inicial preparada, constitución ratificada y estructura base .NET creada. `T001`–`T011` cerradas; `T012` implementada, abierta; `T013`–`T015` cerradas; `T016`–`T021` verificadas, pendientes de aprobación humana de cierre; `T022` en adelante no iniciadas.
- **Metodología:** SDD basado en specs, planes, tareas, gates, agentes y skills.
- **Primer hito:** completar T001–T041; ver `specs/README.md`.
- **Siguiente acción:** T022 pendiente de autorización humana; no iniciarla sin ella.

## Decisiones confirmadas por el usuario

1. .NET 10 / ASP.NET Core 10.
2. Clean Architecture con proyectos `Api`, `Application`, `Domain` e `Infrastructure`; Domain, Application e Infrastructure son Class Library; Api es Web API MVC.
3. CQRS y patrón Mediator para despachar casos de uso desde Presentation a Application.
4. Inyección de dependencias.
5. MongoDB Atlas como único proveedor MongoDB/persistencia; credenciales desde `.env` local.
6. Dos servicios Redis separados en desarrollo y producción: blacklist JWT y caché.
7. JWT, Argon2id y revocación de access tokens en Redis.
8. Refresh tokens rotatorios para mantener sesiones sin exigir login al expirar el access token.
9. Roles únicos: `Owner` y `Business`; no existe administrador.
10. Respuesta uniforme `statusCode`, `message`, `data`; `data` puede ser `null`.
11. Rutas con versión y acción literal `/api/v1/...`.
12. Desarrollo y herramientas con costo $0; demo Azure sin recursos de pago.
13. Preparación horizontal con varias instancias contenerizadas; la cantidad desplegada debe respetar el límite $0.
14. HTTPS; imágenes en Blob Storage como origen y CDN Azure como distribución.
15. Pruebas unitarias, integración y seguridad obligatorias por funcionalidad; controles DevSecOps y OWASP ASVS L2.
16. Documentación en español, código en inglés, comentarios y mensajes API en español.
17. Test de integración MongoDB contra una base de datos de pruebas independiente en Atlas.
18. OpenAPI como documento generado de la API y Scalar como única UI interactiva; no agregar Swagger UI ni otras UI alternativas. La configuración inicial se incluye en T007 de Foundation.
19. `.env` y `.env.*` deben estar ignorados por Git; `!.env.example` se mantiene como excepción. H-04 se resuelve en T003 por decisión del 2026-11-03.
20. Desarrollo local exclusivamente: no crear/configurar repositorio remoto, GitHub, GitHub Actions, CI remoto, secretos GitHub ni publicar código sin autorización humana posterior.

## Decisiones técnicas iniciales propuestas para revisión

- Access Token con duración inicial de 15 minutos, firmado inicialmente con HS256 y secreto aleatorio de al menos 256 bits, configurable y validado con algoritmo fijo.
- Refresh Token opaco, aleatorio, con rotación, 30 días de inactividad y 90 días de vida absoluta máxima.
- Guardar hash del refresh token y metadatos mínimos de sesión en MongoDB Atlas.
- Logout revoca sesión/refresh token en MongoDB y bloquea el `jti` del access token vigente en Redis de autenticación hasta su expiración.
- Registro público inicial crea Owner; el onboarding y verificación Business se especifican después. El cliente nunca puede autoasignarse Business.
- Eliminación de mascotas propuesta como borrado lógico para preservar futuras referencias médicas; se confirma en `03-pet-management` antes de implementar.
- El primer hito incluye `/auth/refresh` además de Login/Register/Logout porque es necesario para renovar sesiones.
- Framework de pruebas base: xUnit 2.9.3 con `Microsoft.NET.Test.Sdk` 17.14.1, `xunit.runner.visualstudio` 3.1.4 y `coverlet.collector` 6.0.4; licencias permisivas y costo $0. Pendiente de ratificación formal en T003/T012.
- La Api se creó con la plantilla `webapi --use-controllers --no-openapi` para no incorporar el paquete OpenAPI antes de su auditoría; OpenAPI/Scalar pertenece a T007.

## Riesgos y decisiones pendientes

- Selección del paquete Mediator: debe cumplir .NET 10 y uso $0; verificar licencia antes de agregarlo.
- Selección del paquete Argon2id: comprobar licencia, mantenimiento y parámetros adecuados antes de fijarlo.
- Servicio Azure concreto para contenedores, balanceador, HTTPS, Blob y CDN: verificar costo $0 vigente al redactar la spec de despliegue. No crear recursos pagados.
- Capacidad de despliegue de varias réplicas bajo el plan $0: la arquitectura debe soportarlo; si la cuota gratuita no lo permite, documentar el límite y no generar cargos.
- Flujo de alta/verificación de Business y quién valida la afiliación: no definido todavía.
- Campos definitivos de perfil Owner y de mascota: confirmar contra la maqueta/formularios y el spec.
- ~~La constitución sigue en `draft-pending-human-ratification` y el slice `00-foundation` no está marcado `Aprobado`.~~ **Resuelto 2026-11-03:** constitución ratificada como `1.0.1` y slice `00-foundation` aprobado por el usuario.
- ~~**H-04 (bloqueante para T003):** `.env` no está en `.gitignore`.~~ **Resuelto 2026-11-03 en T003:** decisión humana de incluir `.env`, `.env.*` y `!.env.example`; verificado con `git check-ignore`. T004 conserva la creación de `.env.example` y la carga de configuración. No existe `.env` en el repositorio.
- `TreatWarningsAsErrors=true` y `AnalysisLevel=latest-recommended` pueden convertir nuevas advertencias de analyzers en errores de build. Deberán corregirse o justificarse puntualmente; no se silencian reglas de seguridad.
- Con versiones centralizadas (`Directory.Packages.props`), todo paquete nuevo debe declarar su `PackageVersion` y pasar la auditoría de licencia/costo antes de agregarse.
- `CA1707` queda como sugerencia solo en `tests/**/*.cs` por el patrón `Metodo_Escenario`; el código de producción mantiene la regla.
- Las cuatro suites de prueba existen; solo `UnitTests` contiene pruebas (7 de arquitectura). Integration, Security y Contract siguen vacías y se poblarán en las tareas que definan pruebas.
- T004 añade 18 pruebas a SecurityTests (12 del lector `.env`, 4 del validador y 2 de bootstrap); Unit mantiene 7. Integration y Contract siguen sin pruebas porque T004 no modifica contratos ni conecta servicios externos.
- **H-05/H-06 (resueltos 2026-11-03):** `ApplicationBootstrap` determina el entorno efectivo antes de cargar configuración; Production —inclusive el valor por defecto— no lee `.env`. Las pruebas de bootstrap cubren Development y Production por defecto. Las revisiones independientes posteriores no hallaron bloqueantes.
- `T012` debe evitar un “verde silencioso”: hoy `dotnet test` devuelve exit 0 aunque tres suites no tengan pruebas. Hay que distinguir “suite sin pruebas” de “suite con pruebas” en CI.
- La prueba de arquitectura aplica igualdad exacta al grafo de capas fuente y cotas en las suites (solo proyectos fuente, sin referencias entre sí, mínimos requeridos): cualquier cambio futuro de referencias debe venir de un spec aprobado y actualizar la tabla esperada del test.
- `UseHttpsRedirection()` produce un aviso al arrancar solo con perfil HTTP; no bloquea T001 y se resuelve con perfiles/despliegue en tareas posteriores.

## Historial de cambios

### 2026-11-03 — T004 implementada, revisada y bloqueada

- Implementación limitada a T004: `.env.example` con placeholders, lector `.env` propio sin paquetes, validación segura de siete claves y 15 pruebas de seguridad. No se inició T005.
- Evidencia inicial: Build Release 0 advertencias/0 errores; Unit 7/7 y Security 15/15; formato exit 0; `.env` ignorado, `.env.example` incluido y no existe `.env` local.
- Security review y traceability review independientes hallaron H-05/H-06, ambos bloqueantes para CA-005. T004 no está verificada ni cerrada; requiere autorización para corregir exclusivamente esta tarea y nueva revisión.

### 2026-11-03 — T004 corregida y verificada

- Con autorización humana, se corrigió exclusivamente T004: `Program` usa `ApplicationBootstrap`; el host determina el entorno efectivo y, solo fuera de Production, carga `<ContentRootPath>/.env` sin recorrer padres.
- Las variables de entorno ya presentes, inclusive vacías, conservan prioridad. Se añadieron pruebas de bootstrap para carga en Development y omisión con Production efectivo por defecto.
- Evidencia posterior: Build Release 0 advertencias/0 errores; Unit 7/7 y Security 18/18; formato exit 0; `.env` ignorado, `.env.example` incluido, ningún paquete nuevo y 0 vulnerabilidades.
- Security review y traceability review posteriores confirman H-05/H-06 resueltos. T004 queda **Verificada**, pendiente de aprobación humana para pasar a Cerrada. T005 no se inicia.

### 2026-11-03 — T004 cerrada y T005 autorizada

- El usuario autorizó el cierre humano de T004 y el inicio exclusivo de T005.
- T004 pasa a `Cerrado`; T005 queda `En progreso`. No se inicia T006 ni ninguna tarea posterior.
- Inspección previa a T005: Application, Infrastructure y Api no contienen servicios concretos ni módulos DI existentes; solo `AddControllers()` y configuración T004 están registrados. T005 debe crear módulos vacíos/no-op explícitos, sin inventar servicios ni adelantar Mediator, MongoDB o Redis.

### 2026-11-03 — T005 implementada y verificada

- Se crearon módulos `AddApplication` y `AddInfrastructure` no-op, con null guard, y se estableció el composition root en `ApplicationBootstrap`/`Program`.
- No había servicios concretos que registrar; se documentó la limitación y no se inventaron implementaciones. No se inició T006 ni tareas posteriores.
- Se añadió `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.0 (MIT, $0, `net10.0`) como dependencia mínima para `IServiceCollection`; se retiró el acoplamiento `Microsoft.AspNetCore.App` de Application/Infrastructure. Vulnerabilidades: 0.
- Evidencia: build Release 0 advertencias/0 errores; Unit 7/7; Security 24/24; Integration/Contract sin pruebas; formato correcto; composition root construye y resuelve `IServiceProvider`.
- Revisiones independientes posteriores sin bloqueantes. T005 queda **Verificada**, pendiente de aprobación humana para pasar a Cerrada.

### 2026-11-03 — T005 cerrada y T006 autorizada

- El usuario aprobó el cierre humano de T005 y autorizó iniciar exclusivamente T006.
- T005 pasa a `Cerrado`; T006 queda `En progreso`. T007 y tareas posteriores no están autorizadas ni se inician.

### 2026-11-03 — T006 implementada y bloqueada por revisión

- Se implementó exclusivamente T006 con abstracciones CQRS/Mediator propias, registro DI y prueba inicial de despacho; no se agregó paquete Mediator ni se inició T007.
- Evidencia local: build 0 advertencias/0 errores; Unit 7/7; Security 25/25; formato correcto; vulnerabilidades NuGet 0.
- Security review y traceability review detectaron H-07 (Service Locator), H-08 (falta MVC Controller real) y H-09 (evidencia/decisión incompleta). T006 no está verificada ni cerrada.

### 2026-11-03 — Corrección autorizada de T006

- Se resolvieron exclusivamente H-07/H-08/H-09: registro explícito de invocadores sin `IServiceProvider` en el dispatcher, controlador MVC técnico real y ADR-001 con decisión Mediator propio, alternativas, límites y costo $0.
- Evidencia: build Release 0/0; Unit 7/7; Security 25/25; Integration/Contract sin pruebas; formato correcto; auditoría de vulnerabilidades sin hallazgos. T006 permanece pendiente de revisión independiente y aprobación humana; no se inició T007.

### 2026-11-03 — T006 verificada tras revisiones finales

- H-07 resuelto con `MediatorRegistry`/`RequestInvoker` explícitos; el dispatcher no usa `IServiceProvider`, `GetService`, reflexión ni Service Locator.
- H-08 resuelto con `TechnicalMediatorProbeController`, MVC Controller real que depende solo de `ISender`; la prueba verifica despacho y ausencia de acoplamiento al handler.
- H-09 resuelto mediante ADR-001, con decisión, alternativas, límites, pruebas y costo $0.
- Revisiones finales de seguridad y trazabilidad: PASS sin bloqueantes. T006 queda **Verificada**, pendiente de aprobación humana para pasar a Cerrada. T007 no se inicia.

### 2026-11-03 — T006 cerrada y T007 autorizada

- El usuario aprobó el cierre humano de T006 y autorizó iniciar exclusivamente T007.
- T006 pasa a `Cerrado`; T007 queda `En progreso`. T008 y tareas posteriores no están autorizadas ni se inician.

### 2026-11-03 — T007 bloqueada por auditoría de dependencia

- La auditoría de `Microsoft.AspNetCore.OpenApi` 10.0.0 encontró la dependencia transitiva `Microsoft.OpenApi` 2.0.0 con vulnerabilidad alta `GHSA-v5pm-xwqc-g5wc`.
- `Scalar.AspNetCore` 2.11.2 fue evaluado como MIT/$0 y compatible con `net10.0`, pero no se agregó debido al bloqueo de la cadena OpenAPI.
- No se agregó código, paquete, ruta, Swagger UI ni trabajo T008+. Build, tests, formato y auditoría posterior permanecen limpios con las dependencias existentes. T007 queda bloqueada y T008 no se inicia.

### 2026-11-03 — T007 implementada y verificada

- El usuario aprobó candidata C: `Microsoft.AspNetCore.OpenApi` 10.0.12, `Microsoft.OpenApi` 2.12.0 y `Scalar.AspNetCore` 2.11.2. ADR-002 registra compatibilidad, licencia MIT/$0, fuentes y rechazo de la combinación vulnerable anterior.
- OpenAPI y Scalar se habilitan exclusivamente en Development: `/openapi/v1.json`, `/scalar/v1`; Production devuelve 404 por defecto. Scalar sigue como única UI.
- Ruta MVC técnica `GET /api/v1/technical/mediator-probe`: 200 `mediated`; alternativa no versionada: 404. No hay endpoint de negocio, wrapper, autenticación, persistencia ni T008+.
- Evidencia local: build Release 0/0; Unit 7/7; Security 26/26; Contract 4/4; format correcto; auditoría NuGet 0 vulnerabilidades.
- IntegrationTests no aplica a T007: no contiene archivos de prueba ni conexiones Atlas/Redis. La DLL vacía fue bloqueada localmente por WDAC/App Control (`0x800711C7`, Code Integrity 3033/3077); no se declara aprobada. T010 definirá integración real.
- T007 queda **Verificada**, pendiente de revisiones independientes registradas y aprobación humana de cierre. Desarrollo permanece exclusivamente local; no se configuró GitHub, CI remoto ni repositorios remotos.

### 2026-11-03 — T007 cerrada; T008 y T009 autorizadas

- El usuario aprobó el cierre humano de T007 y autorizó ejecutar consecutivamente T008 y T009.
- T010 y tareas posteriores permanecen no autorizadas. Desarrollo continúa exclusivamente local, sin GitHub, CI remoto ni publicaciones.

### 2026-11-03 — T003 aprobado y T004 autorizado

- El usuario autorizó T003 y ordenó continuar con T004.
- T003 queda cerrado por aprobación humana; no se realizó una revisión independiente separada de T003.
- T004 se ejecutará exclusivamente; T005 no se inicia sin autorización.

### 2026-11-03 — T003 ejecutado y verificado

- Se creó `.editorconfig` (UTF-8, LF, 4 espacios en C#, 2 en configuración, reglas de estilo y nombres).
- Se creó `Directory.Build.props` con `LangVersion=latest`, `Nullable`, `ImplicitUsings`, `AnalysisLevel=latest-recommended`, `EnforceCodeStyleInBuild` y `TreatWarningsAsErrors`.
- Se creó `Directory.Packages.props` con versiones centralizadas de xUnit, runner, Test SDK y coverlet; sin paquetes nuevos.
- Se resolvió H-04: `.gitignore` ignora `.env` y `.env.*`, con excepción `!.env.example`.
- Se corrigió `CA1859` en `CleanArchitectureTests` (retorno `string[]`) y se mantuvo `CA1707` solo como sugerencia en pruebas.
- Evidencia: build Release 0 advertencias/0 errores; `dotnet test` 7 superadas; `dotnet format --verify-no-changes` exit 0; 0 paquetes vulnerables; licencias MIT/Apache-2.0 y costo $0 en `plan.md` §6.
- T003 queda pendiente de revisión independiente y aprobación humana. No se inició T004.

### 2026-11-03 — Revisión independiente, cierre de T002 y autorización de T003

- `traceability-reviewer` auditó T001–T002: sin bloqueantes para T002; se corrigió el drift documental; 1 hallazgo bloqueante para T003 (H-04, `.env` fuera de `.gitignore`).
- Reporte: `specs/00-foundation/reviews/2026-11-03-traceability-t001-t002.md`.
- El usuario aprobó el cierre de T002 y autorizó T003, condicionado a resolver H-04 antes de implementar.
- T003 no se inició. T004 no está autorizado.

### 2026-11-03 — T002 ejecutado y verificado

- Se agregaron las referencias de Clean Architecture: `Application` → `Domain`; `Infrastructure` → `Application`, `Domain`; `Api` → `Application`, `Infrastructure`; `Domain` sin referencias.
- Las suites referencian solo proyectos fuente y no se referencian entre sí: Unit → Domain/Application; Integration → Application/Infrastructure; Security y Contract → Application/Api.
- Se creó `tests/PetMate.UnitTests/Architecture/CleanArchitectureTests.cs` (7 casos, `System.Xml.Linq`, sin paquetes nuevos).
- Evidencia: build Release 0 advertencias/0 errores; `dotnet test` → 7 superadas, 0 fallidas, exit 0. CA-002 cumplido.
- Sin paquetes nuevos, sin `.editorconfig`/`Directory.Build.props` (T003), sin `.env` (T004), sin Docker/CI. `T003` queda autorizado; H-04 se resuelve posteriormente en T003.

### 2026-11-03 — Ratificación constitucional y aprobación del slice 00-foundation

- El usuario ratificó formalmente la constitución: versión normalizada a `1.0.1`, `status: ratified`; sin cambios normativos.
- Se aprobó el slice `00-foundation` (spec, plan, tareas y gates) y se autorizó únicamente la ejecución de T002.
- La aprobación de inicio no registra el cierre del slice, que sigue pendiente de gates, revisiones y aprobación humana.

### 2026-11-03 — T001 ejecutado y verificado

- Se creó `PetMate.sln` y 8 proyectos con `net10.0`: `src/PetMate.{Api,Application,Domain,Infrastructure}` y `tests/PetMate.{UnitTests,IntegrationTests,SecurityTests,ContractTests}`.
- `PetMate.Api` es Web API MVC (`Microsoft.NET.Sdk.Web`, `AddControllers`/`MapControllers`); las otras tres capas y las suites usan `Microsoft.NET.Sdk`.
- Evidencia: build Release con 0 advertencias/0 errores; `dotnet test` exit 0 con 0 pruebas; sin `ProjectReference`; 0 paquetes vulnerables; smoke test del host responde 404 en `/`.
- No se agregaron dependencias funcionales, endpoints, secretos, contenedores ni CI. `T002` queda pendiente de autorización humana.
- Autorización recibida: instrucción humana explícita para ejecutar únicamente T001 en esta sesión.

### 2026-11-03 — Ajuste SDD: OpenAPI y Scalar

- Se añadió OpenAPI generado y Scalar como única UI interactiva de la API.
- La configuración se incorporó a T007 de `00-foundation`, sin renumerar tareas ni cambiar el primer hito T001–T041.
- No se agregó Swagger UI ni otra interfaz alternativa.

### 2026-11-03 — Inicialización SDD

- Se generó el paquete SDD inicial desde las decisiones confirmadas por el usuario y el documento descriptivo de PetMate.
- No se ha creado código fuente ni se ha ejecutado ninguna tarea de implementación.
- Constitución pendiente de ratificación humana.

### 2026-11-03 — T008 y T009 implementadas, probadas y revisadas; pendientes de cierre humano

- Recuperado: T008 parcial (wrapper, factory, ApiException, middleware, 4 pruebas Security); T009 no iniciada. Se completó T008 (status pages, `SuppressMapClientErrors`, 400 de binding, log, cancelación, BadHttpRequest) y se implementó T009 sin paquetes (`IRequestValidator`, `RequestInvoker` con validación previa al handler, `AddRequestHandler/AddRequestValidator`). Política: 400 (422 no adoptado), `data.errors[{field,message}]`, máx. 50 errores/200 caracteres.
- Pre-endurecimiento: build 0/0, Unit 12, Security 34, Contract 16, format 0, NuGet 0 vulnerables. Revisiones: trazabilidad PASS; seguridad PASS condicionado (hallazgos 1, 2, 4 corregidos con 6 pruebas nuevas).
- Evidencia 2026-11-03: build Release 0/0; format exit 0; NuGet 0 vulnerables; Unit 12/12 (host Windows); Security 40/40 y Contract 16/16 ejecutadas en contenedor Docker mcr.microsoft.com/dotnet/sdk:10.0 (código en solo lectura, copia sin .env, --cap-drop ALL, sin Atlas/Redis) porque Smart App Control (política {0283ac0f-fff1-49ae-ada1-8a933130cad6}, eventos CI 3033/3077, 0x800711C7) bloquea en host el PetMate.Api.dll sin reputación; SAC no se modificó. Security pasó de 34 a 40 por las pruebas de endurecimiento. Diferido: documentar ApiResponse/errores en esquemas OpenAPI (plan.md:38) a T010-T012 o primer slice de negocio. Pendiente: ratificar política 400 vs 422 y aprobar cierre. T010 no iniciada.

### 2026-11-03 — T008 y T009 cerradas; T010/T011 detenidas por contradicción normativa

- El usuario aprobó el cierre de T008 y T009 y ratificó la política: validación → HTTP 400 con `data.errors[{field,message}]`, sin 422. Brechas menores y el pendiente de documentar el wrapper en OpenAPI (plan.md:38) siguen abiertos.
- Se solicitó T010/T011 con MongoDB local en Docker. Esto contradice la constitución, `AGENTS.md`, la decisión 5/17 de esta memoria y T010/T012 ("MongoDB Atlas único proveedor; no Mongo local ni en Compose"). No se modificó nada de T010/T011; se espera decisión humana.

### 2026-11-03 — T010 y T011 implementadas (T010 parcial por credenciales Atlas)

- Decisión del usuario: Atlas único proveedor, sin MongoDB en Compose, variables aprobadas sin cambios; Redis Auth/Cache en dos contenedores Docker independientes. No se modificaron constitución, AGENTS.md, spec ni criterios de task.md (solo estado/evidencia).
- Implementado: configuración tipada validada al arrancar, conexiones Redis perezosas/reconectables (tipos distintos), cliente Mongo desde `MONGODB_URI`, health checks, `/api/v1/health/live|ready`, Dockerfile multi-stage no root, `docker-compose.yml` (api + 2 Redis) y docs en `docs/infrastructure/docker-redis-atlas.md`.
- Evidencia (contenedores, por bloqueo de Smart App Control en host): Unit 12, Security 53, Contract 18, Integration 7 aprobadas + 2 Skipped (Atlas); stack Docker healthy; `/ready` 503 con mongodb Unhealthy (URI ficticia). Revisión de seguridad PASS con observaciones; trazabilidad PASS condicionado.
- Pendiente: pruebas contra Atlas (credenciales), TLS de Redis Docker en producción, digests de imágenes y decisión de licencia de redis:7.4 (Valkey como alternativa), pendiente OpenAPI del wrapper. T010/T011 no cerradas; T012 no iniciada. Los contenedores `petmate-*` y el volumen `petmate_redis_auth_data` siguen en marcha en tu Docker; no se eliminó nada.

### 2026-11-03 — T010 verificada con MongoDB Atlas real

- El usuario creó `.env` local (ignorado por Git). Se comprobó solo presencia de variables, base de pruebas distinta de la de desarrollo y nombre con `test`; ningún valor se imprimió.
- IntegrationTests en contenedor: 9/9 aprobadas, 0 omitidas. Stack Compose con Atlas: `/ready` 200, mongodb/redis-auth/redis-cache Healthy; sin fugas en respuesta ni logs. Contenedores `petmate-*` siguen activos; no se eliminó nada.
- Pendientes: `REDIS_*_PASSWORD` y `JWT_ISSUER/AUDIENCE` siguen como placeholders en `.env` (esta verificación usó contraseñas Redis aleatorias de un archivo temporal fuera del repo); TLS de Redis en producción; digests/licencia redis:7.4; wrapper OpenAPI. T010 y T011 esperan cierre humano; T012 no iniciada.

### 2026-11-03 — T010 y T011 cerradas; T012–T015 autorizadas

- El usuario aprobó el cierre de T010 y T011 y autorizó T012–T015 consecutivas. T016 y posteriores no autorizadas. Abiertos: TLS de Redis Docker en producción, licencia/digest de redis:7.4, documentación del wrapper en OpenAPI.

### 2026-11-03 — T012 implementada localmente; T013–T015 detenidas

- Implementado `scripts/quality-gate.ps1` (restore, build+SAST, formato, NuGet vulnerabilidades/en desuso/licencias, 4 suites en contenedor) y `SecretScanTests`. Evidencia: Unit 12, Security 62, Contract 18, Integration 4 + 5 Skipped (Redis) sin fallos; el gate solo falla por xunit 2.9.3 «Legacy» (excepción sin aprobar en `scripts/quality-exceptions.json`).
- Decisiones pendientes del usuario: (a) excepción xunit Legacy vs migrar a xunit.v3; (b) workflow CI remoto (contradice decisión 20).
- T013–T015 NO iniciadas (estado de ese momento; ver entradas posteriores): el slice `01-initial-authentication` sigue «Propuesto» con aprobación sin marcar, y `AGENTS.md` prohíbe implementar sin spec/plan/tasks aprobados; además quedan decisiones abiertas (paquete y parámetros Argon2id, modelo de sesión).

### 2026-11-03 — Slice 01 aprobado; T012 sin cerrar; T013 verificada

- El usuario aprobó el slice `01-initial-authentication` (spec, plan y tareas) y autorizó una excepción temporal para xunit 2.9.3 «Legacy» (`scripts/quality-exceptions.json`, revisión 2027-02-03; se retira al migrar a xunit.v3, ante una vulnerabilidad o al vencer). T012 queda implementada, no cerrada: workflow YAML, escaneo de contenedores y SBOM diferidos.
- Decisiones técnicas T013–T015 en `specs/01-initial-authentication/plan.md` §11 (Geralt 4.5.0 para Argon2id; t=3, m=64 MiB, p=1; sesión con documento por refresh token).
- T013: dominio sin dependencias, 71 UnitTests, revisiones PASS; pendiente de cierre humano.

### 2026-11-03 — T014 verificada

- Persistencia Atlas de usuarios y sesiones implementada (repositorios, documentos, índices, hosted service tolerante a fallos). Integration 24/24 con 13 pruebas reales contra la base de pruebas; la base de desarrollo no se tocó (guarda + colecciones con prefijo propio, eliminadas al terminar). Revisiones PASS con correcciones aplicadas. Pendiente de cierre humano.

### 2026-11-03 — T015 verificada; T013–T015 listas para cierre humano

- Hasher Argon2id (Geralt 4.5.0 + libsodium): Security 97/97 en contenedor, benchmark ≈140 ms (t=3, m=64 MiB). Revisiones PASS. Requisitos trasladados: T019 (hash ficticio contra enumeración por tiempo) y T022 (límite de concurrencia de hashing).
- T013, T014 y T015 verificadas; ninguna cerrada. T016 no iniciada. Hallazgos pendientes de decisión humana: ninguno nuevo.

### 2026-11-03 — Regresión acumulada T012–T015

- `scripts/quality-gate.ps1` termina con exit 0: Unit 71, Security 97, Contract 18, Integration 24 (0 omitidas) = 210 aprobadas; SAST, formato, NuGet y licencias OK. T012 sigue sin cerrar (workflow YAML, escaneo de contenedores y SBOM diferidos); T013–T015 verificadas, pendientes de aprobación humana. T016 no iniciada. Los contenedores `petmate-*` siguen activos.

### 2026-11-03 — T013, T014 y T015 cerradas

- El usuario aprobó el cierre de T013, T014 y T015. T012 permanece abierta (workflow YAML, escaneo de contenedores y SBOM diferidos). T016 y posteriores no están autorizadas. Requisitos heredados: T019 (hash ficticio contra enumeración por tiempo) y T022 (límite de concurrencia de hashing).

### 2026-11-03 — T016–T021 verificadas; pendientes de cierre humano

- Implementados Access JWT (HS256, 15 min), lista negra Redis Auth (falla cerrada → 503) y los cuatro endpoints `POST /api/v1/auth/{register,login,refresh,logout}` con refresh token opaco rotatorio y detección de replay. Decisiones en `specs/01-initial-authentication/plan.md` §11.6; contratos en `docs/api/endpoints.md`.
- Paquetes nuevos (MIT, $0): `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.14 y `Microsoft.IdentityModel.JsonWebTokens` 8.23.0. Variables nuevas opcionales: `JWT_ACCESS_TOKEN_MINUTES`, `AUTH_RATE_LIMIT_PER_MINUTE`, `TRUSTED_PROXIES`, `ARGON2_MAX_CONCURRENCY`.
- Evidencia: `scripts/quality-gate.ps1` exit 0 con Atlas de pruebas y Redis de Compose: SAST, formato, NuGet 0 vulnerables, licencias 50 paquetes, Unit 110/110, Security 135/135, Contract 46/46, Integration 39/39 (0 omitidas; flujo real register→login→refresh→logout, carrera de registro, refresh concurrente, replay, aislamiento entre usuarios, rehash y caída de Redis). Pruebas en contenedor del SDK por Smart App Control.
- Revisiones independientes: trazabilidad PASS; seguridad PASS con observaciones (3 medias y 5 bajas; corregidas: carrera UpdateActivity, reloj atrasado, jti inválido → 401, rate limit IPv6/proxies, filtro de logs JwtBearer, descripción OpenAPI).
- Pendientes abiertos (no resueltos): T022 debe definir `FallbackPolicy` y políticas por rol; el semáforo de Argon2id bloquea hilos de forma síncrona (evaluar limitador asíncrono); falta cuota por correo contra credential stuffing distribuido; el `jti` no está ligado a la sesión (otros access tokens de la sesión viven hasta 15 min tras el logout); la entropía de `JWT_SIGNING_KEY` solo se valida por longitud; el 409 de registro revela la existencia del correo (riesgo aceptado, mitigado por rate limit).
- T016–T021 no se cierran sin aprobación humana. T022 no se inicia.

### 2026-11-03 — T016–T021 cerradas

- El usuario aprobó el cierre de T016 a T021 (JWT, blacklist Redis Auth, Register, Login, Refresh y Logout). T022 y posteriores no están autorizadas. Observaciones abiertas (plan.md §11.6): política de autorización por defecto y por rol (T022), limitador asíncrono de hashing, cuota por correo contra credential stuffing, jti no ligado a la sesión, entropía de JWT_SIGNING_KEY validada solo por longitud (la clave de `.env` parece una plantilla: generar una real). T012 sigue abierta (workflow YAML, escaneo de contenedores y SBOM diferidos).

## Migración SDD a `pm-NNN` y Constitución v1.1.0 (2026-11-05)

- **Estado:** migración documental ejecutada en la rama local `sdd/reorg` (sin push, sin etiquetas). Constitución v1.0.1 → **v1.1.0** por aprobación expresa del usuario (la copia previa está en `specs/_historico/constitution-v1.0.1.md`).
- **Decisión de plataforma:** Microsoft Azure con Azure for Students como proveedor exclusivo (sin AWS/GCP); objetivo de $0 MXN de desembolso directo; se mantienen dos instancias detrás de un Load Balancer o equivalente. Derogada la cláusula de réplica única temporal. Ningún servicio se asume gratuito hasta completar `pm-062`.
- **Estructura:** 84 Specs `specs/pm-NNN-slug/` (`spec.md` y `task.md`), registro `specs/README.md`, matriz `specs/traceability.md` y archivo `specs/_historico/` con los 5 slices, el roadmap original y el manifiesto SHA-256 (34 archivos verificados idénticos tras `git mv`).
- **Tasks:** T001–T097. T001–T041 conservan identificador, alcance y estado (T001 `Verificado` pendiente de aprobación humana; T012 implementada y abierta). T042–T054 retrospectivas (sin cerrar) y T055–T097 nuevas.
- **Evidencia y límites:** las reseñas de T008–T021 no existen como archivos; solo las 11 de T001–T007. No se inventó evidencia. La brecha CON-§7.3 de T007 se conserva.
- **Pendientes humanos:** aprobar los Specs migrados (`En revisión`/`Propuesto`), decidir los campos de perfil y de mascota, confirmar la lista `AC-NN`, ejecutar `pm-062` y aprobar el cierre de T001.
- Las referencias anteriores a este apartado a slices (`00-foundation`…`04-first-milestone-closure`) describen la estructura histórica archivada.
