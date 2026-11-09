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
20. Desarrollo local exclusivamente: no crear/configurar repositorio remoto, GitHub, GitHub Actions, CI remoto, secretos GitHub ni publicar código sin autorización humana posterior. **Actualizada 2026-11-05:** el repositorio vive en la organización `petmateMMAE` y el equipo autorizó GitHub Actions para CI, CD e infraestructura (ver «Primer despliegue en Azure»).

## Decisiones técnicas iniciales propuestas para revisión

- Access Token con duración inicial de 15 minutos, firmado inicialmente con HS256 y secreto aleatorio de al menos 256 bits, configurable y validado con algoritmo fijo.
- Refresh Token opaco, aleatorio, con rotación, 30 días de inactividad y 90 días de vida absoluta máxima.
- Guardar hash del refresh token y metadatos mínimos de sesión en MongoDB Atlas.
- Logout revoca sesión/refresh token en MongoDB y bloquea el `jti` del access token vigente en Redis de autenticación hasta su expiración.
- Registro público inicial crea Owner; el onboarding y verificación Business se especifican después. El cliente nunca puede autoasignarse Business.
- Eliminación de mascotas propuesta como borrado lógico para preservar futuras referencias médicas; se confirma en `03-pet-management` antes de implementar. **(Superado el 2026-11-08: borrado físico por decisión del usuario.)**
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

## Primer despliegue en Azure (2026-11-05)

- **Alcance (acordado con el equipo):** desplegar el backend tal como está (7 endpoints: `auth/register`, `auth/login`, `auth/refresh`, `auth/logout`, `health/live`, `health/ready`, `technical/mediator-probe`) sin funciones nuevas. Coincide con la evaluación de la unidad 2 de los lineamientos v3 (backend en la nube con registro, login, logout y 4 endpoints más).
- **Responsable de nube y despliegue:** Adrián Adonai García Jiménez.
- **Arquitectura (ADR-003, constitución §8.16 propuesta):** Azure Container Apps (workload profiles, Consumption, VNet propia) con la API en 2 réplicas fijas detrás del ingress (balanceo + HTTPS válido); `redis-auth` y `redis-cache` como contenedores con TCP interno y TLS de CA propia; Atlas con lista de IPs de salida sincronizada; ACR Basic; secretos de Container Apps; Log Analytics + alertas + presupuesto USD 40. Costo estimado ~USD 26–28/mes (pm-062, en `docs/infrastructure/azure-deployment.md`).
- **Artefactos nuevos:** `infra/` (bootstrap, azure, atlas), `.github/workflows/` (`ci.yml`, `deploy.yml`, `infra.yml`, `atlas-ip-sync.yml`), `scripts/smoke-test.sh`, `scripts/mongo/petmate-db.mongodb.js` (script de BD para el .rar), `docs/versioning/version-matrix.md` (pm-059), `docs/decisions/ADR-003` y `ADR-004`, `docs/infrastructure/azure-deployment.md`.
- **Cambios mínimos de soporte (sin funciones nuevas):** `Dockerfile` admite `REDIS_CA_CERT_B64` para confiar en la CA de Redis; `quality-gate.ps1` usa rutas portables para correr en Linux (CI); `.gitignore`/`.dockerignore` excluyen Terraform y CI.
- **Constitución 1.2.0 propuesta:** §8.9 (Redis en contenedores) y §8.16 (arquitectura vigente). Se ratifica al aprobar el PR; Miguel debe revisarla.
- **Specs aprobados para este alcance** por Adrián (pendientes de ratificación de Miguel en el PR): pm-055, pm-057…pm-060, pm-062…pm-069, pm-072…pm-078, pm-083, pm-084. `pm-070` y `pm-071` siguen `Propuesto` hasta aprobar la enmienda §8.9.
- **Ejecutado (2026-11-06):** v0.1.0 desplegada en **Chile Central** (`https://petmate-api.purplefield-b84659a6.chilecentral.azurecontainerapps.io`): 2 réplicas, `ready` Healthy con Mongo y los dos Redis, prueba de humo de los 7 endpoints aprobada y etiqueta `api-v0.1.0`. México Central no pudo crear el ambiente de Container Apps en 9 intentos (~7 h, siempre `Waiting`); detalle en `docs/infrastructure/azure-deployment.md` §2.1.
- **Ajustes durante el despliegue (PR #4 a #8 y docs):** Terraform sin registro de proveedores; tiempos de espera de 90 min; `trivy-action@v0.35.0`; `apt-get upgrade` en la imagen final (CVE HIGH de apt); el CD reutiliza la imagen si su etiqueta inmutable ya existe; credencial OIDC con sujeto con IDs; despliegue automático en cada merge a `main` con la versión del archivo `VERSION`; `scripts/watch-deploy.ps1` para diagnosticar despliegues atorados.
- **Riesgos abiertos:** `TRUSTED_PROXIES` solo acepta IPs (rate limit agrupado por proxy hasta medirlas); HSTS no implementado; validar que la región permitida por Azure for Students admita Container Apps; montaje de Azure Files con la red restringida; registro de la app OIDC en el tenant escolar.
- **Pendiente para la entrega final (fuera de este alcance):** caché en backend, paginación, compresión, eliminar cuenta, imágenes + CDN + moderación, notificaciones push.
## pm-033 / T022 — políticas de autorización implementadas (2026-11-05)

- **Estado:** implementado y probado en el contenedor SDK (sin Atlas ni Redis reales); **T022 no está cerrada** y el Spec pm-033 está `En progreso`. Pendientes: revisión independiente de seguridad y trazabilidad, aprobación humana y la aplicación de `RequireOwner` a los endpoints de mascotas (pm-040 a pm-044, trazabilidad en pm-045 CA-02).
- **Decisiones:** política por defecto y de respaldo (denegación por defecto), `RequireOwner` y `RequireBusiness`, rol solo desde claims del JWT validado, sin rol Admin. La política de respaldo también se aplica a rutas inexistentes y endpoints sintéticos (405/415); `UnmatchedRouteResultHandler` conserva sus códigos habituales (decisión D-09, aprobada por el usuario con la condición de que el manejador nunca omita la autorización de endpoints reales de la aplicación; las pruebas de regresión lo verifican).
- **DAG:** pm-033 y pm-034 son dependencias directas de pm-035 (pm-035 L10, pm-046 L11, pm-053 L12).
- **Riesgo registrado:** `mediator-probe` sigue público de forma temporal; hay que revisar su exposición en producción en un trabajo de seguridad posterior.
- **Evidencia:** `specs/pm-033-authorization-policies/checklists/gates.md`.

## Versión 0.2.0 — perfil propio (2026-11-06)

- Implementados `GET /api/v1/users/me/get` y `PUT /api/v1/users/me/update` (pm-037, pm-038; aprobados por Miguel, en progreso hasta su cierre humano): nombre visible opcional (1-80), identidad solo desde el token, correo y rol no editables. 9 pruebas unitarias y 12 de contrato nuevas; la prueba de humo cubre el perfil.
- Cuenta demo creada en producción: `demo@petmate.mx` / `PetMateDemo2026` (nombre «Usuario Demo»), sin datos reales, para la revisión.
- La app móvil ya muestra el perfil del servidor y guarda el nombre del registro en el servidor.

## Pipeline CI/CD DevSecOps único (2026-11-06)

- **Qué cambió:** `.github/workflows/pipeline.yml` («CI/CD DevSecOps») reemplaza a `ci.yml` y `deploy.yml`: un solo workflow con jobs encadenados con `needs:` (secretos con gitleaks, compilación con analizadores, SAST CodeQL informativo, dependencias NuGet, IaC con Trivy, integridad SDD, pruebas unitarias y de seguridad, **un job de contrato por endpoint** en una matriz de 9, contrato transversal, integración, puerta de calidad, imagen con Trivy y SBOM, despliegue, prueba de humo y DAST con ZAP). En PR no despliega y esas etapas aparecen omitidas. Documentación en `docs/ci-cd/pipeline.md` y `docs/testing/endpoints.md`.
- **Despliegue sin cambios de lógica:** OIDC, versión desde `VERSION`, imagen inmutable reutilizable, despertar la API, prueba de humo con reversa y etiqueta. Sin `environment:` (subject de OIDC ligado a `refs/heads/main`; el plan gratuito no los permite).
- **Pruebas:** cada método de ContractTests lleva `[Trait("Endpoint", ...)]` y `EndpointTraitTests` lo exige; se añadieron pruebas de validación (400 con wrapper) de login, refresh y logout, límites de contraseña de register, 404 de perfil, readiness sano y 405 de salud y del probe técnico. `SecretScanTests` volvió a pasar (contraseña de prueba de menos de 24 caracteres).
- **Scripts:** `quality-gate.ps1` acepta `-AuditOnly` y `-MinSeverity` (el pipeline bloquea High y Critical; localmente sigue bloqueando cualquier vulnerabilidad); `ci-test-summary.ps1` resume los TRX en el resumen del job; `.gitleaks.toml` con 5 falsos positivos revisados.
- **Primer run en GitHub (2026-11-06):** todas las etapas en verde salvo integración: 21 pruebas contra Atlas fallaron porque Atlas no admite las IP de los runners (las 18 de Redis y de configuración pasaron). Se añadió un *ping* previo: si Atlas no responde, esas pruebas quedan Skipped con aviso. Acciones actualizadas a Node 24 (checkout, setup-dotnet y upload-artifact v5, CodeQL v4) y runner fijado a `ubuntu-24.04`.
- **Riesgos abiertos:** CodeQL en repositorio privado requiere GitHub Advanced Security (decisión de Miguel; interruptor `CODEQL_DISABLED`); el pipeline nuevo no se ha podido ejecutar en GitHub desde este equipo, se validó con actionlint, gitleaks, Trivy y las suites en contenedor; la etiqueta de `deploy` y el DAST solo se ven en un run real de `main`.

## pm-039 / T029 — modelo de mascota y persistencia (2026-11-08)

- **Estado:** implementado y probado en la rama `feature/pm-039-pet-model`; T029 y pm-039 en `En progreso` (falta revisión independiente y aprobación humana). Sin endpoints todavía (llegan con pm-040 a pm-044).
- **Decisiones del usuario:** campos de la maqueta móvil (nombre, especie, raza, fecha de nacimiento, sexo y fotografía) y **borrado físico** en la base de datos. Se reescribieron con sus mismos identificadores pm-039 CA-01/CA-03, pm-044 CA-01 a CA-03 y pm-041 CA-01.
- **Diseño:** `Pet` en Domain con validaciones; `PetValidation` en Application (errores por campo en español); `IPetRepository` con el propietario en cada filtro (una mascota ajena se comporta como inexistente); `MongoPetRepository` e índice `ix_pets_ownerId_createdAtUtc`. La fotografía es una URL HTTPS hasta que existan pm-079/pm-080.
- **Evidencia:** 19 pruebas unitarias nuevas y 6 de integración contra Atlas de pruebas, todas en verde (ver `specs/pm-039-pet-domain-persistence/checklists/gates.md`).
- **Cierre:** el usuario aprobó el cierre de T029 el 2026-11-08 (sin revisión independiente). pm-039 sigue en progreso por T036 y T037.

## pm-040, pm-042, pm-043, pm-044 — endpoints de mascotas (2026-11-08)

- **Estado:** implementados y probados en `feature/pm-039-pet-model` (sin subir). T030, T032, T033 y T034 en `En progreso`; T031 (listar) espera la paginación de pm-047.
- **Contrato:** solo rol Owner; propietario del token; 201 al crear, 200 en detalle, actualizar y eliminar (borrado físico); 400 con el campo; 404 para mascota ajena o inexistente. Detalle en `docs/api/endpoints.md`.
- **Pipeline:** cuatro jobs nuevos en la matriz de endpoints (`pets-create`, `pets-get-by-id`, `pets-update`, `pets-delete`) y la prueba de humo cubre crear, ver, actualizar y eliminar.
- **Evidencia:** Unit 142, Security 155, Contract 142 (26 nuevas), Integration sin cambios; `gates.md` de cada Spec.
- **Cierre:** el usuario aprobó el cierre de T030, T032, T033 y T034 el 2026-11-08 (sin revisión independiente).

## pm-039 / T036 y T037 — pruebas de mascotas (2026-11-08)

- **Estado:** cerradas con aprobación humana el 2026-11-08 (sin revisión independiente); pm-039 queda `Cerrado`.
- **Cambio:** dos pruebas nuevas de integración en `PetPersistenceIntegrationTests`: la consulta de listado usa el índice `ix_pets_ownerId_createdAtUtc` (`explain`, IXSCAN, sin COLLSCAN) y el borrado concurrente de la misma mascota tiene un solo ganador.
- **Evidencia:** Unit 142/142; Integration 32 aprobadas + 15 omitidas por Redis, con las 8 de mascotas en verde contra Atlas; build Release 0/0. Detalle en `specs/pm-039-pet-domain-persistence/checklists/gates.md`.
- **Riesgo menor:** el desempate por `_id` en `ListByOwner` no está en el índice (orden en memoria); revisar en pm-052.

## Grupo de CI — T066, T069, T070, T071, T072 (2026-11-08)

- **Estado:** verificadas y en `En progreso`; falta revisión independiente y aprobación humana. T012 y T067 sin cambios.
- **Cambios:** `check-sdd.ps1` valida CA duplicados, Tasks con más de un Spec principal y trazabilidad completa por criterio; nuevo `check-sdd.tests.ps1` (8 casos) y su paso en el job `sdd-integrity`; resumen de la puerta de calidad sin el conteo fijo de endpoints; `version-matrix.md` corregida (la etiqueta la crea el CD).
- **Decisiones pendientes del usuario:** (1) aceptar el merge/push a `main` como autorización expresa de la etiqueta (T071). _[Resuelta el 2026-11-08: el usuario eligió la opción 2, la etiqueta solo con lanzamiento manual y `create_tag`.]_ El umbral NuGet quedó igualado a High en local y CI (T066). La de Trivy (T069) quedó resuelta el mismo día (ver abajo).
- **Limitación:** los cambios de `pipeline.yml` no se han ejecutado en GitHub porque no se ha hecho push.

## Cierres y endurecimiento de la imagen (2026-11-08)

- **T001, T070, T072:** cerradas con aprobación humana (sin revisión independiente); pm-058 y pm-060 `Cerrado`; pm-001 sigue `En revisión` (Spec sin aprobar).
- **T069 (pm-057):** Trivy ya no ignora vulnerabilidades sin parche. La imagen final es `mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled` (sin shell, sin gestor de paquetes, no root): 0 HIGH/CRITICAL y 2 MEDIUM de `libc6` frente a 19 hallazgos de la imagen anterior; 225 MB frente a 392 MB. La CA de Redis del CD se arma en una etapa `certs`. Se quitó el healthcheck `bash` de la API en Compose. Verificado en local con Atlas real (flujo completo) y Security 155/155. T069 cerrada con aprobación humana el 2026-11-08 (sin revisión independiente); pm-057 `Cerrado`. Pendiente: primera ejecución en GitHub/Azure tras el próximo push.

## pm-033 / T022 — alcance completo (2026-11-08)

- **Estado:** `RequireOwner` ya está aplicado a los endpoints de mascotas (pm-040 a pm-044); NuGet audit en verde; Unit 152, Security 155, Contract 142 e Integration 48/48 con Atlas real y Redis temporales. T022 sigue `En progreso`: faltan la revisión independiente y la aprobación humana.
- **Nota técnica:** las pruebas de Redis se omiten si la contraseña contiene «ficticio» (las de `.env` lo hacen); para ejecutarlas se usaron dos Redis temporales con contraseñas aleatorias.

## Cierres del 2026-11-08: T012, T026, T027, T028; T066 igualada

- **T012** cerrada con aprobación humana (sin revisión independiente). **T066**: `quality-gate.ps1` bloquea desde High por defecto, igual que CI; sigue pendiente de aprobación.
- **T026, T027, T028** cerradas con aprobación humana (sin revisión independiente); pm-037 y pm-038 `Cerrado`. Al escribir la evidencia se detectó que faltaban pruebas unitarias de los casos de uso del perfil y una prueba de Atlas para `UpdateProfile`: se añadieron `ProfileHandlersTests` (10) y `Profile_update_persists_only_the_name_and_the_update_date_of_that_user`.
- **Totales actuales:** Unit 152, Security 155, Contract 142, Integration 48 (Atlas real y Redis temporales).

## Cierres de T022 y T066 (2026-11-08)

- **T022** (pm-033) y **T066** (pm-055) cerradas con aprobación humana, sin revisión independiente; pm-033 `Cerrado`. pm-055 sigue `En progreso` por T067 (ramas protegidas, requiere autorización expresa).
- **Pendiente:** T071 (interpretación de la autorización de etiquetas; resuelta el 2026-11-08, ver la sección de T071) y la primera ejecución en GitHub de los cambios de `pipeline.yml` (imagen chiseled, pruebas negativas del gate SDD).

## Specs aprobados: pm-047, pm-041, pm-045 (2026-11-08)

- **Aprobados por el usuario** (estado `Aprobado`). Paginación: `page` desde 1, `pageSize` 20 por defecto y 50 máximo, 400 si es inválido, respuesta `items`/`page`/`pageSize`/`totalCount`/`totalPages` (pm-047 §6).
- **Orden de trabajo:** T058, T031, T035, T038, T040. T041 (pm-046) sigue bloqueada por pm-035, pm-034 y pm-036. No se ha implementado nada todavía.

## Paginación y listado de mascotas — T058, T031, T035, T038, T040 (2026-11-08)

- **Implementado:** `PaginationConvention`/`PagedResult<T>` (pm-047), `GET /api/v1/pets/get-all` con `ListMyPetsQuery`/`CountByOwner` (pm-041) y pruebas de propiedad (pm-045): `PetOwnershipSecurityTests` (7), `PetListTests` (20), `PetListEndpointsTests` (15) y una prueba de Atlas de `CountByOwner`. Endpoint nuevo `pets-get-all` en la matriz del pipeline, en `EndpointTraitTests`, en `docs/` y en la prueba de humo (14 endpoints).
- **Totales:** Unit 172, Security 162, Contract 157, Integration 49 (Atlas real y Redis temporales); extremo a extremo con la imagen chiseled y Atlas.
- **Estado:** T058, T031, T035 y T038 verificadas, pendientes de aprobación humana. **T040 no está lista:** falta la revisión independiente de seguridad y trazabilidad del módulo de mascotas (CA-03).

## Revisión independiente del módulo de mascotas (2026-11-08)

- **Seguridad:** apto con observaciones, sin críticos ni altos. **Dos riesgos medios abiertos que necesitan decisión del usuario:** S1 (sin tope de mascotas por Owner ni rate limiting en `/pets/*`) y S2 (sin límite explícito del tamaño del cuerpo). Documentados en `specs/pm-045-pet-ownership-bola/checklists/gates.md`.
- **Trazabilidad:** consistente con observaciones; se corrigieron todas (gates reescritos de pm-040/042/043/044, trazabilidad, casillas, planes retrospectivos y documentación). Los planes de pm-040/042/043/044 son retrospectivos: desviación del flujo registrada.

## Límites contra el abuso en mascotas (2026-11-08)

- **Implementado por decisión del usuario** (riesgos S1 y S2 de la revisión de seguridad; pm-045 CA-04): máximo 100 mascotas por propietario (409), política `pets` de 60 peticiones por minuto por usuario (`PETS_RATE_LIMIT_PER_MINUTE`, 429 con `Retry-After`) y 16 KB de cuerpo para toda la API (`RequestBodyLimitMiddleware`, 413, incluso fragmentado).
- **Verificación:** Unit 174/174, Security 166/166, Contract 163/163, Integration 49/49; en Kestrel real con la imagen chiseled (413 con y sin `Content-Length`, 429 tras 60).
- **Estado:** T035, T038 y T040 siguen pendientes de aprobación humana; T031 y T058 también.

## Cierre del módulo de mascotas (2026-11-08)

- **T031, T035, T038, T040 y T058** cerradas con aprobación humana; pm-041, pm-045 y pm-047 `Cerrado`. Con ellas, pm-039 a pm-045 y pm-047 están cerrados: solo queda abierta T041 (pm-046), bloqueada por pm-035, pm-034 y pm-036.
- **Observación abierta menor:** el desempate por `_id` del orden de `ListByOwner` no está en el índice (revisar en pm-052).

## Protección contra fuerza bruta y matriz de límites — T056, T057, T054 (2026-11-08)

- **Aprobados por el usuario:** pm-034, pm-035 y pm-036. Valores de T056: login 5 fallos por correo/15 min (el sexto bloquea 15 min, también con la contraseña correcta), refresh 30 por usuario/15 min, hashing acotado a los núcleos con cola del doble; contadores en Redis de autenticación (solo hashes) y falla cerrada (503).
- **T057:** matriz en `docs/api/endpoints.md`; nueva política `users` (60 por minuto, `USERS_RATE_LIMIT_PER_MINUTE`). **T054:** retrospectiva documentada con la evidencia existente.
- **Totales:** Unit 186/186, Security 169/169, Contract 168/168, Integration 53/53; verificado con Kestrel y Redis reales (401×5, 429 con `Retry-After: 900`).
- **Pendiente:** aprobación de T056, T057 y T054; T023, T025 (con revisión independiente del grupo de autenticación) y T041 siguen abiertas. pm-028 a pm-032 siguen «En revisión» y son dependencias formales de pm-035.
- **Riesgo conocido:** el bloqueo por correo permite bloquear temporalmente la cuenta de otra persona; lo acota el límite por IP.

## Cierre de T054, T056 y T057 (2026-11-08)

- T056 y T057 cerradas con aprobación humana; T054 (retrospectiva) aprobada y documentada, sin estado «Cerrado» porque `check-sdd.ps1` lo prohíbe para retrospectivas; pm-036 `Verificado`; pm-034 `En progreso` (falta T023). Pendientes del camino a T041: T023, T025 (con la revisión independiente del grupo de autenticación) y T041.

## T023 y T025 — grupo de autenticación (2026-11-08)

- **Revisión independiente** (dos agentes de solo lectura): seguridad apto con observaciones, trazabilidad consistente con observaciones. **Corregido:** conteo del intento antes de verificar (H1), cupo del limitador liberado al terminar el hash (H2), límite por usuario en logout (H5) y el script de Redis ya no reinicia el contador durante un bloqueo (hallado con Redis real). Documentación al día (ASVS, endpoints, registro).
- **Totales:** Unit 188/188, Security 170/170, Contract 170/170, Integration 56/56.
- **Abiertos para decisión del usuario:** H3 (bloqueo dirigido), H4 (registro enumerable), H6 (`TRUSTED_PROXIES` y límite por instancia), H7 (access tokens vivos hasta 15 minutos tras replay), H8 (sin ventana de gracia del refresh) e I2 (sin log de eventos de seguridad ni `no-store`).
- **Bloqueo de T025 y T041:** pm-028 a pm-032 siguen «En revisión» y son dependencias formales de pm-035; T023 y T025 esperan aprobación humana.

## Cierre del grupo de autenticación (2026-11-08)

- **Aprobados:** specs pm-028 a pm-032; **cerradas:** T023 y T025; **pm-034 y pm-035 `Cerrado`**. Las dependencias de pm-046 (pm-035, pm-038 y pm-045) están cerradas.
- **Abiertos para decisión:** H3, H4, H6, H7, H8 e I2 de la revisión de seguridad (ver `specs/pm-034-auth-brute-force-protection/checklists/gates.md`). Siguen «En revisión» otros specs base de los que dependen los anteriores (por ejemplo pm-011, pm-013 y pm-024 a pm-027).
- **Siguiente:** T041 (pm-046): requiere aprobar el spec pm-046 (hoy `Propuesto`) y después la aceptación de extremo a extremo de los 11 endpoints con tu aprobación explícita del hito.

## Aceptación del primer hito — T041 (2026-11-08)

- **pm-046 aprobado.** Validación de extremo a extremo con `scripts/acceptance-milestone.ps1` (nuevo): **39/39** comprobaciones sobre los 11 endpoints (más sondas), imagen `petmate-api:local` chiseled, Atlas y Redis reales. Suites Unit 188/188, Security 170/170, Contract 170/170, Integration 56/56; NuGet y Trivy (0 HIGH/CRITICAL) en verde.
- **Pendiente:** aprobación humana explícita del hito (pm-046 CA-02). Abiertos: H3, H4, H6, H7, H8 e I2; primera ejecución del pipeline en GitHub y despliegue con esta versión; specs base «En revisión».

## Primer hito aceptado (2026-11-08)

- **El usuario aprobó explícitamente el primer hito** (11 endpoints: 4 de autenticación y 7 funcionales); T041 y pm-046 `Cerrado`. Evidencia: `specs/pm-046-first-milestone-acceptance/checklists/gates.md` y `scripts/acceptance-milestone.ps1` (39/39).
- **Estado del código:** local en `feature/pm-039-pet-model`; lo último subido a `origin/developer` es `b6efa1e`. Quedan cambios sin confirmar (límites de autenticación, cuota por correo, tests, documentación SDD y el script de aceptación).
- **Pendiente:** hallazgos H3, H4, H6, H7, H8 e I2 (decisión), primera ejecución del pipeline en GitHub (imagen chiseled, límites, gate SDD) y despliegue a Azure con esta versión, specs base «En revisión» (pm-011, pm-013, pm-024 a pm-027, etc.) y la observación de pm-052.

## Specs base aprobados (2026-11-08)

- **Aprobados los 27 specs de pm-001 a pm-027** que seguían «En revisión»: 15 `Cerrado` (Task histórica cerrada) y 12 `Aprobado` (pm-007, pm-010, pm-013, pm-014, pm-015, pm-017 a pm-021, pm-023 y pm-025). Ya no queda ningún spec «En revisión».
- **Pendiente:** Tasks retrospectivas T042 a T053 (documentar la evidencia existente), T055 (verificación automática de rutas y reglas de nombrado, pm-007) y T097 (evidencia contra la falsificación de `X-Forwarded-For`, pm-017). Aprobar un spec no las cierra.

## Retrospectivas T042–T053 y tareas nuevas T055 y T097 (2026-11-08)

- **Documentadas** las 12 retrospectivas (T042 a T053) con tabla criterio → prueba y sus limitaciones; **construidas** T055 (comprobación automática catálogo ↔ rutas y reglas de nombrado, `RouteCatalogTests`) y T097 (`ForwardedHeadersSpoofingTests`: un cliente no confiable no evade el límite por IP con `X-Forwarded-For`; control positivo con proxy listado).
- **Totales:** Unit 188/188, Security 170/170, Contract 179/179, Integration 56/56. Evidencia directa de la imagen: usuario 1654, sin shell, Trivy de secretos y de vulnerabilidades sin hallazgos.
- **Pendiente de aprobación humana:** T042 a T053, T055 y T097 (una retrospectiva no puede marcarse «Cerrado» por regla del gate SDD). Cobertura parcial: pm-010 CA-03 (cancelación del cliente) sin prueba directa.

## Cadena documental de Azure — T074, T075, T076 (2026-11-08)

- **T074 (pm-062):** `docs/infrastructure/azure-cost-study.md` con 15 componentes, fuentes oficiales consultadas el 2026-11-08 (varias páginas de precios son dinámicas y no cargaron: marcadas), tipo de cambio 18.652374 MXN/USD, consumo del crédito (completo ~3.7 meses, modo ahorro ~7 meses) y conflicto académico documentado (12 meses completos exceden el crédito; no se reducen requisitos).
- **T075 (pm-063):** ADR-005 a ADR-010 (balanceo, almacenamiento, CDN, observabilidad, registro, secretos) con alternativas dentro de Azure, e índice `docs/decisions/README.md`; ADR-003 sigue cubriendo cómputo y Redis. Siguen «Propuesto».
- **T076 (pm-064):** `docs/infrastructure/environments.md` (desarrollo, pruebas, demo; bases `petmate_dev`, `petmate_test`, `petmate_prod`; nombres y etiquetas) con `EnvironmentStrategyTests` y `DecisionRecordsTests` (Security 177). No se creó ni se consultó ningún recurso de Azure.
- **Pendiente:** aprobación humana de T074, T075 y T076; confirmar en la calculadora los precios que no cargaron y leer el consumo real en Cost Management.

## Aprobaciones del 2026-11-08 (T042–T053, T055, T097)

- Aprobadas por el usuario: retrospectivas T042–T053 («Aprobada y documentada») y T055 y T097 («Cerrado y aprobado»). Sin revisión independiente de T055/T097.
- **No aprobadas (decisión del usuario):** T071, T074, T075 y T076 siguen «En progreso».

## Logging, cobertura y contratos — T065, T068, T073 (2026-11-08)

- **T065 (pm-054):** `RequestLoggingMiddleware` + consola JSON con ámbitos. Hallazgo propio: el ámbito `RequestPath` de `Hosting.Diagnostics` filtraba ids y texto del cliente; se desactivó (filtro a `None`). `X-Correlation-Id` se escribe con `OnStarting` porque el Response Wrapper limpia las cabeceras. `AuthFlowFactory` dejó de ser `sealed` para poder capturar logs en pruebas.
- **T068 (pm-056):** cobertura de líneas 93.7 % con las cuatro suites (78.6 % sin Atlas/Redis). `docs/quality/coverage-baseline.md` y `scripts/coverage-merge.py`. El umbral propuesto (90 % / 75 % en PR) **no está aplicado**: requiere enmienda aprobada por Miguel.
- **T073 / T024 / T039 (pm-061):** `OpenApiWrapperTests` comprueba que el OpenAPI usa el wrapper; T024 y T039 se verificaron con pruebas existentes.
- Suites: Unit 188, Security 177, Contract 188, Integration 56 (34 con Atlas; 22 de Redis se omiten sin contraseñas reales).
- **Pendiente:** revisión independiente y aprobación humana de T065, T068, T073, T024 y T039; decidir la enmienda del umbral; eventos de seguridad en logs (I2).

## Rendimiento, caché e índices — T059 a T064 (2026-11-08)

- **T059:** compresión gzip/Brotli del JSON (también HTTPS) excepto `/api/v1/auth/*` (BREACH, tokens): 12 777 B → 1 909 B. `docs/security/compression-risk.md`.
- **T060:** `ApiJsonContext` (generación en compilación), JSON idéntico al de reflexión; 321.6 → 196.1 ms por 2000 serializaciones de 50 mascotas.
- **T061:** `ICacheStore` (`Read`/`Write`/`Remove`; `Get`/`Set` los prohíbe CA1716), `CacheKey` `petmate:cache:v1:{área}:{id}`, TTL 1 s a 1 h, `RedisCacheStore` solo con `RedisCacheConnection`, 250 ms y fallo seguro. Sin consumidores.
- **T062:** `docs/security/cache-policy.md` + `CachePolicyTests`: ningún endpoint es cacheable; nada consume `ICacheStore` sin decisión documentada.
- **T063:** `docs/quality/database-indexes.md` + `QueryPlanIntegrationTests` (8 consultas con IXSCAN en Atlas de pruebas). Abierto: desempate por `_id` fuera del índice de mascotas.
- **T064:** `PerformanceBaselineTests` en proceso (p95: health 10.1 ms, get-all 19.9/13.2 ms); `docs/quality/performance-baseline.md`. La carga real queda para pm-084.
- El ámbito de correlación del logging ahora usa `LoggerMessage.DefineScope`.
- **Pendiente:** revisión independiente y aprobación humana de T059 a T064 (y de las del turno anterior); corregir la prueba de concurrencia de refresh, que falló una vez.

## Revisiones independientes, I2 y flaky (2026-11-08)

- **Prueba de concurrencia de refresh:** la causa era el doble en memoria, que devolvía la misma instancia de `RefreshSession` a todas las solicitudes; un perdedor revocaba el objeto del ganador y `RegisterUse` lanzaba. Ahora `InMemorySessionRepository` devuelve copias (como Atlas); se añadió `InternalsVisibleTo` de Domain a ContractTests e IntegrationTests (el doble se comparte). 3 corridas seguidas verdes.
- **I2 cerrado:** `AuthController` registra (Warning) login fallido, bloqueo y refresh rechazado con un seudónimo de la cuenta (SHA-256 del correo, 12 hex), sin correo, contraseña, tokens ni IP.
- **Revisión de seguridad (subagente, solo lectura): apto con observaciones, sin hallazgos altos.** Corregido: regex del correlation id con `\A…\z` y pruebas con CR/LF; método HTTP normalizado en el log; pruebas de exclusión de compresión con mayúsculas; riesgo residual de PII comprimida documentado. Registrado sin cambio: exclusión por ruta (no por metadato), `Hosting.Diagnostics = None` y `JwtBearer = Critical` reducen señal de logs, `ForwardLimit = 1` con más de un salto en Azure, valores de caché sin control de contenido (revisar al añadir el primer consumidor), `xml.etree` con XML local.
- **Revisión de trazabilidad (subagente): consistente con observaciones.** Corregido: conteos reales en los gates (Unit 206/206, Security 180/180, Contract 198/198, Integration 37 pasan y 23 se omiten de 60 (las omitidas son de Redis sin contraseñas reales y la de caché con Redis real); medido el 2026-11-08 tras las revisiones), cabeceras de los gates de pm-007 y pm-017, T068 «CA-01 verificado, CA-02 pendiente», pm-062 «cumple parcialmente», revisión independiente anotada. Pendiente: casillas de la Definition of Done de los `task.md` cerrados.
- **Aprobación de T074, T075 y T076** por el usuario: «Cerrado y aprobado»; ADR-005 a ADR-010 siguen «Propuesto».
- **Pendiente:** commit y push de este lote; umbral de cobertura (enmienda); H3, H4, H6, H7, H8; T067, T071.

## Aprobaciones del 2026-11-08 (T059–T065, T068, T073, T024, T039)

- Aprobadas por el usuario: T059, T060, T061, T062, T063, T064, T065, T073, T024 y T039 («Cerrado y aprobado») y T068 (con **CA-02 pendiente de enmienda**: umbral de cobertura). Specs pm-048 a pm-054 y pm-061 «Verificado»; pm-056 sigue «En progreso».
- **No aprobada:** T071 (versionado de releases).

## Hallazgos de seguridad H3, H4, H6, H7 y H8 — T098 a T102 (2026-11-08)

Aprobados por el usuario (H7, H3, H8 opción A; H4 opción B; H6 A+B+C) y registrados como T098 a T102 bajo pm-028, pm-031, pm-036, pm-034 y pm-029 (el gate `check-sdd` amplió su rango a T001–T102).

- **H7 (T098):** claim `sid` en el access token; `RevokeSession`/`IsSessionRevoked` en Redis Auth (`jwt:session-revoked:{sid}`, TTL = vida del token); se revoca en logout, replay y usuario inexistente. Tokens sin `sid` valen hasta expirar.
- **H8 (T099):** `RefreshHandler.ReplayGrace` = 10 s: reutilizar un token consumido dentro de la ventana da 401 sin revocar; después, replay. La prueba de concurrencia ahora exige 1 éxito y sesión viva.
- **H6 (T100):** `AuthIpThrottleFilter` (Redis Auth, 10/min por IP, espera de 1 min) en register/login/refresh, además de la barrera en proceso; 503 si Redis no responde. `REQUIRE_TRUSTED_PROXIES=true` hace fatal la falta de `TRUSTED_PROXIES` en Production; por defecto solo advierte, porque el primer despliegue la deja vacía. Saltos (1 con el ingress de Container Apps, `ForwardLimit = 1`) documentados. El scope de `RedisAuthAttemptThrottle` ahora admite guion.
- **H3 (T101):** bloqueo duro por par (correo, IP) y techo por correo (50/15 min, espera de 1 min); `LoginCommand` recibe `ClientKey`.
- **H4 (T102):** el hash ya se calculaba antes de `TryAdd` (prueba nueva); el 409 queda como riesgo aceptado y documentado.
- **Entorno:** Docker Desktop estaba apagado al reanudar (el equipo se apagó/reinició); se volvió a iniciar.
- **Pendiente:** revisión independiente y aprobación humana de T098 a T102; commit y push (el usuario pidió no subir todavía).

## Aprobación de T098 a T102 (2026-11-08)

- El usuario aprobó las cinco Tasks (H7, H8, H6, H3, H4). **No hubo revisión independiente** de estos cambios; es la brecha a recordar. Specs pm-028, pm-029, pm-031, pm-034 y pm-036 «Verificado».

## T071 — autorización de la etiqueta de versión (2026-11-08)

- El usuario eligió la **opción 2**: el merge a `main` despliega pero no etiqueta; la etiqueta `api-vX.Y.Z` solo se crea al lanzar el pipeline a mano con la casilla `create_tag`. Cambio en `.github/workflows/pipeline.yml` (entrada `create_tag`, condición del paso) y `ReleaseTagAuthorizationTests`.
- **Límite:** no se ha ejecutado en GitHub; la primera ejecución real del paso queda pendiente. Falta revisión independiente y aprobación humana de T071.
- **Revisión independiente de T071 (2026-11-08): apto con observaciones.** Corregido: documentos que decían que el merge etiqueta, entrada `version` por `env`, paso de etiqueta sin ocultar errores, versión = `VERSION` con `create_tag` y pruebas más estrictas (Security 185). Pendiente: aprobación humana de T071; el riesgo de `terraform apply` sobre `PETMATE_VERSION` queda documentado.

## T071 cerrada (2026-11-08)

- Aprobada y cerrada por el usuario tras la revisión independiente (apto con observaciones, corregidas). Spec pm-059 «Verificado». **Límite:** el pipeline no ha corrido en GitHub con estos cambios; la primera ejecución real del paso de etiqueta (y de la entrada `create_tag`) está pendiente. Riesgo documentado: `terraform apply` puede revertir `PETMATE_VERSION`.
