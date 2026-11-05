# Plan — pm-033 authorization-policies

> Derivado de [spec.md](spec.md). Estado del plan: **Aprobado por el usuario para implementar (2026-11-05)**; implementación ejecutada y pendiente de revisiones y de la aprobación de cierre. No cambia el alcance histórico de T022 ni reasigna la propiedad de ningún criterio de aceptación (ver §15).

## 1. Contexto y requisitos vinculados

- Spec: [pm-033](spec.md). Task: T022 (histórica, `Propuesto`), propietaria de `pm-033#CA-01` a `CA-04`. Origen histórico: [`_historico/01-initial-authentication/task.md`](../_historico/01-initial-authentication/task.md) (sección T022).
- Dependencias previas: [pm-027](../pm-027-access-token-jwt/spec.md) y [pm-028](../pm-028-token-revocation-redis/spec.md), ambas implementadas, probadas y cerradas (T016, T017).
- Constitución v1.1.0: §2 (principios), §6 (autenticación, sesiones y autorización; sin rol Admin), §7 (Response Wrapper y códigos), §9 y §10 (pruebas y DevSecOps).
- Requisitos académicos relacionados: ninguno específico (`traceability.md` no vincula `pm-033` con un `AC-NN`).
- Habilita: [pm-037](../pm-037-owner-profile-read/spec.md) y [pm-039](../pm-039-pet-domain-persistence/spec.md), y a través de ellos el resto del primer hito.

## 2. Estado actual contrastado con el código

| CA | Criterio (resumen) | Estado | Evidencia existente | Falta |
|---|---|---|---|---|
| CA-01 | Política por defecto que exige usuario autenticado | **Parcial** | `services.AddAuthorization()` crea la política por defecto de ASP.NET (usuario autenticado) y se usa con `[Authorize]` en `AuthController.Logout`. `JwtPipelineTests.Request_without_token_returns_wrapped_401` | No hay política de **respaldo** (*fallback*): un endpoint sin atributos es público. No hay declaración explícita ni prueba de que el respaldo deniegue |
| CA-02 | Políticas por rol Owner y Business | **Pendiente** | El JWT lleva el claim `role` (`Owner` o `Business`) y `RoleClaimType = "role"` (`JwtAccessTokenService`). `TryRead` solo acepta nombres exactos del enum | No existe ninguna política con nombre ni constantes de rol en la capa Api |
| CA-03 | Anónimo 401 y rol insuficiente 403 | **Parcial** | 401 con wrapper para ausente, malformado, expirado, firma/audiencia ajena y revocado (`JwtPipelineTests`); 503 fail-closed con Redis caído | No hay endpoint que pueda responder 403 por rol; el 403 con wrapper solo existe en `Logout` por regla de dominio, no por política |
| CA-04 | Business sin acceso por defecto a la API de propietario | **Pendiente** | Rol Business reconocido y emitido (`Business_role_round_trips`) | Política `RequireOwner` y prueba de que un token Business recibe 403 |

Evidencia ya existente que se **conserva y no se reimplementa**:

- Validación criptográfica del JWT (HS256 fijo, emisor, audiencia, vigencia, firma): `JwtAccessTokenService.CreateValidationParameters`, compartida con el middleware JwtBearer.
- Rechazo de tokens firmados válidamente pero con rol ausente, desconocido o con mayúsculas distintas: `RevocationJwtBearerEvents.TokenValidated` llama a `JwtAccessTokenService.TryRead` (comparación exacta contra el enum `UserRole`). Cobertura unitaria en `JwtAccessTokenServiceTests` (claims `role` ausente, `Admin`, `1`, `owner`). **Falta cobertura a nivel HTTP**: ninguna prueba de contrato comprueba que esos tokens produzcan 401 con wrapper.
- Ausencia de rol Admin: `UserRole` solo define `Owner` y `Business`; `RestoreAndHardeningTests` verifica que no exista un nombre con "Admin".
- Wrapper para 401/403 sin cuerpo: `app.UseStatusCodePages(ApiBehaviorConfiguration.WriteStatusCodeAsync)` y `ApiResponseMessages.ForStatusCode` (401 y 403 ya tienen mensaje en español).
- Patrón de prueba HTTP sin tocar la API de producción: `AuthApiFactory` (hereda de `IsolatedApiFactory`) añade `ProtectedProbeController` con `AddApplicationPart` **solo en el ensamblado de pruebas**; incluye reloj y almacén de revocación falsos.

## 3. Diseño técnico

### 3.1 Capas y responsabilidades

La autorización es una responsabilidad de **Presentation/composition root** (`PetMate.Api`). No cambia Domain, Application ni Infrastructure y no añade dependencias entre proyectos. No es lógica de negocio: los controladores solo declaran la política; las reglas de propiedad de recursos (BOLA) pertenecen a `pm-045` y se resolverán en los handlers.

### 3.2 Política por defecto y denegación por defecto

- **Política por defecto** (`[Authorize]` sin argumentos): usuario autenticado **y** claim `role` con valor `Owner` o `Business`. La condición de rol es defensa en profundidad: hoy `TokenValidated` ya rechaza otros valores.
- **Política de respaldo** (*fallback*, endpoints sin metadatos de autorización): la misma. Con ella cualquier endpoint nuevo queda **denegado por defecto** para anónimos hasta que se marque explícitamente `[AllowAnonymous]`.
- Un endpoint que referencie una política inexistente falla de forma ruidosa (ASP.NET lanza excepción al evaluar): no se concede acceso en ningún caso.

### 3.3 Políticas por rol y convención de nombres

| Política | Requisito |
|---|---|
| `RequireOwner` | Autenticado y claim `role` == `Owner` |
| `RequireBusiness` | Autenticado y claim `role` == `Business` |

Convención propuesta: **`Require<Capacidad>`** en PascalCase, constante en `AuthorizationPolicies`, sin cadenas literales en los controladores (`[Authorize(Policy = AuthorizationPolicies.RequireOwner)]`). Las políticas por rol son `Require<Rol>`; las futuras de capacidad o recurso seguirán `Require<Capacidad>` (por ejemplo `RequirePetOwnership` en `pm-045`, evaluada mediante `IAuthorizationService` porque depende del recurso). No hay política `RequireAdmin` ni equivalente.

Alternativas descartadas: `OwnerOnly`/`BusinessOnly` (ambiguas cuando una capacidad admita varios roles) y nombres cortos `Owner`/`Business` (colisionan con los valores del claim y con `RequireRole`).

### 3.4 Validación de rol mediante claims confiables

- Los claims solo se consideran si provienen de un JWT que ya superó firma, algoritmo fijo, emisor, audiencia, vigencia y lista negra de Redis.
- Se usa `RequireClaim("role", "Owner")` sobre el claim del token (comparación ordinal, sensible a mayúsculas) en lugar de `RequireRole`, para no depender de `ClaimsIdentity.RoleClaimType` ni de la asignación de claims de Microsoft (`MapInboundClaims = false` ya evita `ClaimTypes.Role`).
- Los claims de otros orígenes (cabeceras, cuerpo, query) **nunca** participan en una decisión de autorización.
- Los nombres de rol en `AuthorizationPolicies` son constantes de la capa Api; una prueba los compara con `Enum.GetNames<UserRole>()` para que no puedan divergir sin que falle el build (la capa Api no referencia Domain directamente).

### 3.5 Respuestas 401 y 403

- **401** (sin autenticar o token inválido): lo emite JwtBearer sin cuerpo y `UseStatusCodePages` lo envuelve en `statusCode/message/data` con `data: null`. Incluye `WWW-Authenticate: Bearer`. Sin cambios.
- **403** (autenticado sin permiso): lo emite `UseAuthorization` mediante `ForbidAsync` del esquema JwtBearer; `UseStatusCodePages` lo envuelve igual. No se escribe un `IAuthorizationMiddlewareResultHandler` propio salvo que las pruebas demuestren que el wrapper no se aplica (decisión de contingencia D-07).
- Los tokens con rol ausente o desconocido son **401**, no 403: fallan en la autenticación (claims obligatorios), no en la autorización. El 403 queda reservado a principales válidos sin permiso.

### 3.6 Superficie pública explícita

Con el respaldo activo hay que marcar `[AllowAnonymous]` donde hoy el acceso es público. Inventario actual (única fuente: `docs/api/endpoints.md` y `Program.cs`):

| Endpoint | Hoy | Acción |
|---|---|---|
| `POST /api/v1/auth/register`, `login`, `refresh` | `[AllowAnonymous]` explícito | Sin cambio |
| `GET /api/v1/health/live`, `ready` | Público por ausencia de atributo | Añadir `[AllowAnonymous]` a `HealthController` |
| `GET /api/v1/technical/mediator-probe` | Público por ausencia de atributo | Añadir `[AllowAnonymous]` a `TechnicalMediatorProbeController` (se conserva el comportamiento) |
| `/openapi/v1.json` y `/scalar/v1` (solo Development) | Público por ausencia de atributo | `.AllowAnonymous()` en `MapOpenApi` y `MapScalarApiReference` |
| `POST /api/v1/auth/logout` | `[Authorize]` | Sin cambio (usa la política por defecto) |

Una prueba compara el conjunto de endpoints con `IAllowAnonymous` contra esta lista exacta: cualquier endpoint público nuevo exige actualizar la prueba y, por tanto, una decisión explícita.

### 3.7 Aplicación a mascotas (alcance real de T022)

La Task histórica T022 incluye «aplicar la política Owner a los endpoints iniciales de mascotas». Esos endpoints no existen (`pm-040` a `pm-044`). Este plan **no** inventa endpoints en producción: demuestra la política con controladores de prueba y deja la aplicación efectiva de `RequireOwner` a cada endpoint de mascotas cuando se implemente. Ver decisión pendiente P-01.

## 4. Persistencia y servicios

- MongoDB Atlas: sin cambios (la autorización no consulta datos).
- Redis autenticación: sin cambios; la consulta de lista negra sigue ocurriendo en `TokenValidated` antes de evaluar políticas, de modo que un token revocado nunca llega a autorizarse y Redis caído sigue produciendo 503.
- Redis caché: no interviene.
- Secretos y variables: ninguna variable nueva; las pruebas reutilizan las claves de prueba de `IsolatedApiFactory`.

## 5. API y contratos

- Rutas nuevas en producción: **ninguna**. Rutas de prueba (solo en el ensamblado de pruebas): `/api/v1/test-authz/*`.
- Códigos: 401 y 403 con wrapper (`statusCode`, `message`, `data: null`). Sin cambios de versionado.
- `docs/api/endpoints.md`: añadir una sección «Autorización» con la política por defecto, la tabla de endpoints públicos y la semántica 401/403. No se modifica ningún contrato de endpoint existente.
- OpenAPI: fuera de alcance marcar requisitos de seguridad por operación; queda anotado como riesgo para `pm-061` (ver §8).

## 6. Seguridad y DevSecOps

- OWASP ASVS L2, capítulo V4 (control de acceso): aplicación en capa de servicio de confianza, mínimo privilegio y fallo seguro (denegación por defecto). La versión exacta de ASVS la fija `docs/security/asvs-l2-checklist.md`; se añadirá una fila de evidencia al cerrar.
- Pruebas (todas con servicios simulados, sin Atlas ni Redis reales):

| Suite | Archivo propuesto | Qué comprueba |
|---|---|---|
| Security | `tests/PetMate.SecurityTests/Authorization/AuthorizationPoliciesTests.cs` | Respaldo y política por defecto definidos; conjunto exacto de políticas con nombre (`RequireOwner`, `RequireBusiness`); constantes de rol iguales a `UserRole`; no existe política para un rol inexistente; evaluación de cada política con principales sintéticos (Owner, Business, sin claim, rol `Admin`, mayúsculas distintas) |
| Contract | `tests/PetMate.ContractTests/Authorization/AuthorizationPolicyTests.cs` | Con `AuthApiFactory` y un controlador de prueba por política: anónimo, Owner, Business, rol desconocido, token sin claim de rol, claims manipulados, 401/403 con wrapper y fallback sin atributos |
| Contract | `tests/PetMate.ContractTests/Authorization/PublicSurfaceTests.cs` | Conjunto exacto de endpoints públicos de la API real, en Development y en Production |
| Security | `tests/PetMate.SecurityTests/Authorization/ForgedClaimsTests.cs` (o dentro de Contract, ver §7) | Tokens forjados a mano: payload alterado sin firmar, `alg: none`, firma de otra clave con rol Owner, rol como arreglo, `ClaimTypes.Role` largo ignorado |

- SAST, escaneo de secretos y auditoría NuGet: sin paquetes nuevos; se ejecutan con `scripts/quality-gate.ps1` como en las Tasks anteriores.

## 7. Escenarios de prueba y criterios cubiertos

Controladores de prueba (solo ensamblado de pruebas, ruta `/api/v1/test-authz/…`): `default` (`[Authorize]`), `owner` (`RequireOwner`), `business` (`RequireBusiness`), `unmarked` (sin atributos: ejerce el respaldo), `public` (`[AllowAnonymous]`).

| # | Escenario | Resultado esperado | CA |
|---|---|---|---|
| 1 | Anónimo en `default`, `owner`, `business`, `unmarked` | 401 con wrapper | CA-01, CA-03 |
| 2 | Anónimo en `public` | 200 | CA-01 |
| 3 | Owner en `owner` | 200 con identidad | CA-02 |
| 4 | Business en `business` | 200 | CA-02 |
| 5 | Business en `owner` | 403 con wrapper | CA-03, CA-04 |
| 6 | Owner en `business` | 403 con wrapper | CA-03 |
| 7 | Owner y Business en `default` y `unmarked` | 200 | CA-01 |
| 8 | Token válido con rol `Admin`, `owner`, `1` o vacío | 401 con wrapper | CA-01, CA-03 |
| 9 | Token válido sin claim `role` | 401 con wrapper | CA-01, CA-03 |
| 10 | Token con `role` como arreglo (`["Owner","Business"]`) | 401 con wrapper | CA-03 |
| 11 | Payload alterado (Business→Owner) sin volver a firmar | 401 con wrapper | CA-03 |
| 12 | `alg: none` con rol Owner | 401 con wrapper | CA-03 |
| 13 | Rol Owner firmado con otra clave | 401 con wrapper | CA-03 |
| 14 | Token con claim largo `…/ClaimTypes/role` = Owner pero sin `role` | 401 (el claim extra no concede acceso) | CA-03 |
| 15 | Token Owner revocado | 401; con Redis caído, 503 (sin regresión) | CA-01 |
| 16 | Políticas registradas = {`RequireOwner`, `RequireBusiness`}; no existe `RequireAdmin` | Aserción de conjunto | CA-02 |
| 17 | Nombres de rol de `AuthorizationPolicies` == `Enum.GetNames<UserRole>()` | Aserción | CA-02 |
| 18 | Superficie pública real == lista exacta de §3.6, en Development y Production | Aserción de conjunto | CA-01 |
| 19 | Regresión: Health, OpenAPI/Scalar, Register/Login/Refresh siguen accesibles sin token; Logout sigue exigiendo token | Suites existentes sin cambios en sus aserciones | CA-01 |

Cobertura: los escenarios mapean los cuatro criterios; los tests no deben depender entre sí ni del orden, y se ejecutan serializados en la colección `ApiEnvironment` como los existentes.

## 8. Archivos a crear o modificar

| Archivo | Acción | Motivo |
|---|---|---|
| `src/PetMate.Api/Authorization/AuthorizationPolicies.cs` | Crear | Constantes de políticas y de roles, y método de extensión `AddPetMateAuthorization` que registra política por defecto, respaldo y políticas por rol |
| `src/PetMate.Api/Configuration/ApplicationBootstrap.cs` | Modificar | `ConfigureAuthentication` llama a `AddPetMateAuthorization()` en lugar de `AddAuthorization()` |
| `src/PetMate.Api/Controllers/HealthController.cs` | Modificar | `[AllowAnonymous]` explícito |
| `src/PetMate.Api/Controllers/TechnicalMediatorProbeController.cs` | Modificar | `[AllowAnonymous]` explícito |
| `src/PetMate.Api/Program.cs` | Modificar | `.AllowAnonymous()` en `MapOpenApi` y `MapScalarApiReference` |
| Tres archivos de prueba bajo `tests/` (ver §6) y, si hace falta, un controlador de prueba en `ContractTests` | Crear | Escenarios del §7 |
| `docs/api/endpoints.md` | Modificar | Sección «Autorización» |
| `docs/security/asvs-l2-checklist.md` | Modificar | Fila de evidencia al cerrar |
| `specs/pm-033-authorization-policies/checklists/gates.md` | Crear (este plan) | Evidencia reproducible al cerrar |
| `Memoria.md`, `specs/README.md`, `specs/traceability.md` | Modificar **solo al cerrar**, con tu autorización | Conciliación documental |

No se modifican la Constitución, otros Specs, otras Tasks ni el DAG.

## 9. Paquetes, herramientas y costo

| Dependencia/recurso | Uso | Licencia/costo | Decisión |
|---|---|---|---|
| `Microsoft.AspNetCore.Authorization` (parte del framework `Microsoft.AspNetCore.App`) | Políticas, respaldo y requisitos de claims | Incluido con .NET 10, $0 | Reutilizar; sin paquetes nuevos |
| `Microsoft.AspNetCore.Mvc.Testing`, xunit | Pruebas HTTP con la fábrica existente | Ya aprobados en el proyecto | Reutilizar |
| `Microsoft.IdentityModel.JsonWebTokens` | Forjar tokens de prueba | Ya referenciado | Reutilizar |

No se añade ninguna dependencia ni servicio, por lo que no aplica verificación adicional de licencia o costo.

## 10. Secuencia, riesgos y decisiones

Secuencia de ejecución propuesta (pasos internos de T022; **no son Tasks nuevas** y no consumen IDs):

1. Pruebas primero: escenarios 1 a 15 y 18 con las políticas aún inexistentes (deben fallar por la razón correcta).
2. Crear `AuthorizationPolicies` y registrar el respaldo, la política por defecto y las dos políticas por rol.
3. Marcar `[AllowAnonymous]` en Health, probe técnico, OpenAPI y Scalar.
4. Ejecutar las pruebas y la regresión completa; corregir hasta que pasen.
5. Documentar en `endpoints.md` y en el checklist ASVS.
6. Ejecutar `scripts/quality-gate.ps1` (o las suites en el contenedor SDK si Smart App Control bloquea el host) y registrar la evidencia en `checklists/gates.md`.
7. Revisiones independientes de seguridad y de trazabilidad; aprobación humana.

Riesgos y mitigaciones:

| Riesgo | Mitigación |
|---|---|
| El respaldo vuelve privados endpoints hoy públicos (Health, OpenAPI, Scalar, probe) y rompe sondas o pruebas | Marcado explícito (§3.6) y prueba de superficie pública; las suites existentes actúan como regresión |
| `UseStatusCodePages` no envuelve el 403 | Prueba 5 y 6 lo verifican; contingencia D-07 con un `IAuthorizationMiddlewareResultHandler` mínimo, sin tocar el 401 |
| Orden del middleware: `UseAuthorization` antes de `UseRateLimiter` | Sin cambios; el límite por política de `AuthController` no se ve afectado. Se confirma con las suites actuales |
| Un rol futuro (p. ej. un tercero) obliga a tocar varias partes | Roles centralizados en `AuthorizationPolicies`; la prueba 17 detecta la divergencia con `UserRole` |
| Los documentos OpenAPI no declaran requisitos de seguridad por operación | Fuera de alcance; anotado para `pm-061` (T073) |

Decisiones técnicas propuestas (D-xx) y pendientes (P-xx) están resumidas en la sección 11.

## 11. Decisiones y pendientes de aprobación

| ID | Decisión propuesta | Alternativa |
|---|---|---|
| D-01 | Política por defecto y de respaldo = autenticado con rol Owner o Business | Solo autenticado (más débil) |
| D-02 | Nombres `RequireOwner`, `RequireBusiness`; convención `Require<Capacidad>` | `OwnerOnly`, `Owner` |
| D-03 | `RequireClaim("role", …)` sobre el claim del JWT | `RequireRole` |
| D-04 | Rol ausente o desconocido = 401 (falla la autenticación); 403 solo para principal válido sin permiso | Convertir en 403 (requeriría relajar `TokenValidated`) |
| D-05 | Constantes de rol en Api, sincronizadas con `UserRole` por prueba; sin referencia nueva de Api a Domain | Referenciar Domain desde Api |
| D-06 | Controladores de prueba en el ensamblado de pruebas mediante `AddApplicationPart` (patrón existente), sin endpoints artificiales en producción | Endpoints de prueba en Api (descartado) |
| D-07 | Contingencia: handler de resultado de autorización solo si el wrapper del 403 no funciona | Handler propio desde el principio |
| D-08 | `[AllowAnonymous]` explícito en Health, probe técnico, OpenAPI y Scalar, con prueba de lista exacta | Dejar públicos por omisión (incompatible con denegar por defecto) |

| ID | Pendiente que requiere tu aprobación |
|---|---|
| P-01 | **Alcance de T022 sobre mascotas.** Recomendación: T022 cierra con las políticas demostradas por pruebas y la aplicación de `RequireOwner` a cada endpoint se cumple en `pm-040`…`pm-044`; la trazabilidad queda en `pm-045#CA-02` (Business sin acceso). Alternativa: mantener T022 abierta hasta que existan los endpoints (bloquearía `pm-037`/`pm-039`). |
| P-02 | **Redacción de criterios.** Los CA no se han modificado. Opcional: precisar CA-01 («incluye denegación por defecto de endpoints sin atributos») y CA-04 («se verifica con la política `RequireOwner`»). Son aclaraciones, no cambios de propiedad: los cuatro siguen siendo de T022. |
| P-03 | **Dependencias de `pm-035`.** Recomendación en el anexo; no se ha modificado el DAG. |
| P-04 | **Probe técnico público.** Se conserva público por compatibilidad; ¿quieres restringirlo (por ejemplo solo Development) en otro Spec? |

## 12. Verificación reproducible

- Comandos (en el equipo con Smart App Control, usar el contenedor SDK con el código en solo lectura y sin `.env`, como en las Tasks anteriores):
  - `dotnet build PetMate.sln -c Release`
  - `dotnet test PetMate.sln -c Release`
  - `powershell -File scripts/quality-gate.ps1`
  - `powershell -File scripts/check-sdd.ps1`
- Evidencia esperada: las suites actuales (Unit 110, Security 135, Contract 46, Integration 10 + 29 omitidas sin Atlas/Redis) siguen en verde y se suman las pruebas nuevas; `check-sdd.ps1` en OK.
- Condiciones de fallo: cualquier endpoint público no listado, un 200 para un rol sin permiso, un token con rol no válido que no devuelva 401, un 403 sin wrapper, o una regresión en las suites existentes.

## 13. Anexo — recomendación sobre las dependencias de `pm-035`

**No modificado; para revisión posterior.** `pm-035` (T025, cierre del grupo de autenticación) depende hoy solo de `pm-028`…`pm-032`.

- Motivo para ampliar: T025 pide «todos los CA cubiertos» y la revisión independiente del grupo. Si `pm-033` y `pm-034` no son dependencias, el grupo podría cerrarse y, vía `pm-046`, el primer hito, sin autorización por política ni protección frente a fuerza bruta.
- Recomendación: **añadir `pm-033` y `pm-034` como dependencias directas de `pm-035`.** `pm-036` ya entra de forma transitiva a través de `pm-034`, por lo que añadirla explícitamente es opcional.
- Efectos: `pm-035` pasaría del nivel L9 a L10 o superior y `pm-046` conserva su dependencia de `pm-035`. No cambia la propiedad de ningún CA.
- Riesgo de la ampliación: `pm-034` tiene parámetros sin decidir (umbrales y ventanas), lo que puede retrasar el cierre del grupo. Alternativa: limitar el CA-02 de `pm-035` a `pm-028`…`pm-033` y cerrar `pm-034` después, sin bloquear el hito.
- Condición: se aplicaría en una revisión posterior del registro, con tu aprobación.

## 14. Aprobación

- [x] Plan consistente con spec y constitución (aprobado por el usuario para implementar, 2026-11-05).
- Usuario/revisor: usuario del proyecto (aprobación de implementación). Aprobación de cierre: pendiente.
- Fecha: 2026-11-05.

## 15. Implementación y decisiones aprobadas (2026-11-05)

**Aprobación.** El usuario aprobó este plan y las decisiones D-01 a D-08 y autorizó la implementación. Resolución de los pendientes:

| ID | Resolución |
|---|---|
| P-01 | Aprobada la estrategia propuesta: políticas verificadas ahora con controladores de prueba, sin bloquear pm-037 ni pm-039; `RequireOwner` se aplicará en pm-040 a pm-044 con trazabilidad en pm-045 CA-02. **T022 no se declara cerrada** hasta cumplir su alcance histórico completo. |
| P-02 | Se precisó la redacción de CA-01 y CA-04 en el Spec (identificadores, intención y propiedad intactos); ningún criterio se marca como cumplido. |
| P-03 | Aprobado añadir pm-033 y pm-034 como dependencias directas de pm-035 (pm-036 entra de forma transitiva). Registrado en `specs/README.md`, en pm-035 y en `specs/traceability.md`; efecto: pm-035 L10, pm-046 L11, pm-053 L12. No se modificó ninguna Task histórica. |
| P-04 | `mediator-probe` se conserva con `[AllowAnonymous]` temporal y comentado en el código. Queda registrada la necesidad de revisar su exposición en producción en un trabajo de seguridad posterior (restringirlo a Development o protegerlo sin rol Admin de negocio). |

**Hallazgo de implementación y decisión D-09 (aprobada por el usuario el 2026-11-05, con la condición de que el manejador nunca omita la autorización de endpoints reales de la aplicación; las pruebas de regresión que lo verifican se conservan: anónimo 401 y rol insuficiente 403 en endpoints de aplicación, endpoint sin atributos denegado, superficie pública cerrada y Logout protegido).** El plan asumía que la política de respaldo solo se aplica a rutas con endpoint. No es así: ASP.NET Core la evalúa también cuando ninguna ruta coincide y para los endpoints sintéticos del enrutador (405 y 415). Con la política de respaldo la primera ejecución convirtió en 401 los 404, 405 y 415 que las suites existentes esperan (documentación oculta en Production, ruta técnica no versionada, método no permitido). Se resolvió de forma conservadora sin abandonar `FallbackPolicy`:

- Se añadió `UnmatchedRouteResultHandler` (`IAuthorizationMiddlewareResultHandler`): si la solicitud no tiene un endpoint de aplicación (`RouteEndpoint`) y la política no se cumple, continúa la canalización y termina en su 404, 405 o 415 habitual; con endpoint de aplicación delega en el manejador estándar (401 y 403 con el wrapper actual).
- Se descartó envolver `UseAuthorization` en `UseWhen`: el host mínimo añadía una segunda autorización antes de la autenticación y todo respondía 401.
- Esto se aparta de D-07 solo en el motivo: el manejador no corrige el wrapper del 403 (que funcionó sin cambios), sino que preserva los códigos existentes de rutas inexistentes.
- Consecuencia: las rutas inexistentes siguen devolviendo 404 a anónimos (no se filtra qué rutas existen ni cambia el contrato).

**Cambio en una prueba existente.** `WrapperTestController` (controlador de prueba del wrapper de `ResponseWrapperHttpTests`) recibió `[AllowAnonymous]` porque con la denegación por defecto dejó de ser público. No cambia ninguna aserción.

**Archivos creados:** `src/PetMate.Api/Authorization/AuthorizationPolicies.cs`, `src/PetMate.Api/Authorization/UnmatchedRouteResultHandler.cs`, `tests/PetMate.SecurityTests/Authorization/AuthorizationPoliciesTests.cs`, `tests/PetMate.ContractTests/Authorization/{AuthorizationPolicyTests,ForgedClaimsTests,PublicSurfaceTests}.cs`.

**Archivos modificados:** `ApplicationBootstrap.cs` (registro), `HealthController.cs` y `TechnicalMediatorProbeController.cs` (`[AllowAnonymous]`), `Program.cs` (`.AllowAnonymous()` en OpenAPI y Scalar), `ResponseWrapperHttpTests.cs` (ver arriba), `docs/api/endpoints.md`, `docs/security/asvs-l2-checklist.md`, `Memoria.md` y los documentos SDD de pm-033 y pm-035.

**Superficie pública resultante** (verificada por `PublicSurfaceTests`): `POST` register, login y refresh; `GET` health live y ready; `GET technical/mediator-probe` (temporal); y en Development la documentación OpenAPI y Scalar. Logout exige token.

**Desviación respecto al paso 1 de la secuencia (§10).** Las pruebas se escribieron junto con el código; no se ejecutó una fase roja previa. Las primeras ejecuciones fallaron por regresiones (ver D-09) y no por ausencia de la política.
