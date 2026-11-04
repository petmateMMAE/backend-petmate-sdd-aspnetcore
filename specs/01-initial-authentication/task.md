# Task — 01 Autenticación inicial

> Tareas globales T013–T025. Depende de T001–T012.

## T013 — Modelar usuario, roles y sesión
- **Estado:** Cerrado y aprobado (2026-11-03, aprobación humana). Decisiones: plan.md §11.1.
- **Crear:** conceptos Domain para User, roles `Owner`/`Business` y sesión refresh.
- **Verificar:** invariantes unitarias; no existe Admin; roles no pueden ampliarse desde request público.
- **Evidencia:** Domain sin paquetes ni referencias (Common, Users, Sessions); UnitTests 71/71 en contenedor (44 pruebas de dominio + 15 de endurecimiento); build Release 0/0 con SAST. Revisión de seguridad PASS (hallazgo medio: `Restore` público → ahora `internal` con `InternalsVisibleTo` solo para Infrastructure y UnitTests; bajos: coherencia temporal en `Restore`, reloj anterior en `RegisterUse` y tope de 366 días en `SessionPolicy` corregidos; `Email.ToString` devuelve PII: política de logs en T018). Revisión de trazabilidad PASS. Cerrada por aprobación humana (2026-11-03).

## T014 — Persistencia Atlas de usuarios y sesiones
- **Estado:** Cerrado y aprobado (2026-11-03, aprobación humana). Decisiones: plan.md §11.2.
- **Crear:** documentos/repositorios MongoDB Atlas, índice único de email normalizado e índices de sesión.
- **Verificar:** integration tests contra DB Atlas de test independiente; duplicados y concurrencia; no secretos en logs.
- **Evidencia:** `IUserRepository`/`ISessionRepository` en Application (sin tipos Mongo); repositorios, documentos, mapeo e índices en Infrastructure; `MongoIndexHostedService` tolerante a fallos. En contenedor: Unit 71/71, Security 65/65, Contract 18/18, Integration 24/24 (13 pruebas reales contra la base Atlas de pruebas, ninguna omitida: round-trip, email único, carrera de 20 altas → 1, consumo concurrente → 1, índices únicos/TTL idempotentes, sin cambios sobre sesión revocada, documento corrupto → error de persistencia). Revisiones: seguridad PASS (2 medias y 4 bajas; corregidas: documentos corruptos ahora `PersistenceUnavailableException`, limpieza ante fallo de índices, pruebas de revocación y de filtro) y trazabilidad PASS. Notas para T020: `TryConsumeToken` no valida revocación/expiración (hacerlo antes con `IsActive`); pasar `session.AbsoluteExpiresAtUtc` a `AddToken`; `UpdateActivity`/`Revoke` son idempotentes y no informan filas afectadas. Cerrada por aprobación humana (2026-11-03).

## T015 — Hasher Argon2id
- **Estado:** Cerrado y aprobado (2026-11-03, aprobación humana). Decisiones: plan.md §11.3.
- **Crear:** abstracción Application e implementación Infrastructure con dependencia $0 verificada.
- **Verificar:** salt distinto por password, verificación correcta/incorrecta, formato versionable, rehash policy y benchmark de parámetros.
- **No hacer:** almacenar password en texto claro o bajar parámetros para producción.
- **Evidencia:** `IPasswordHasher` (Application) y `Argon2idPasswordHasher` + `Argon2idOptions` (Infrastructure, Geralt 4.5.0 MIT + libsodium 1.0.22 ISC); registro en DI. En contenedor: Security 97/97 (28 pruebas de hashing: PHC versionado, salt distinto, correcta/incorrecta, hashes mal formados u hostiles rechazados sin trabajo costoso, límites 1024 bytes, rehash, parámetros mínimos OWASP/máximos, benchmark mediana 139–143 ms con t=3, m=64 MiB). Revisiones: seguridad PASS (medias: DoS por concurrencia → T022; enumeración por tiempo → T019; bajas corregidas: tope de memoria 256 MiB, validación de parámetros PHC antes de verificar) y trazabilidad PASS. Requisitos heredados: T019 debe verificar contra un hash ficticio precalculado cuando el usuario no existe; T022 debe limitar la concurrencia de hashing. Los parámetros solo deben subirse: bajarlos degrada hashes al siguiente login. Cerrada por aprobación humana (2026-11-03).

## T016 — Servicio de Access JWT
- **Estado:** Cerrado y aprobado (2026-11-03, aprobación humana). Decisiones: plan.md §11.6.
- **Crear:** emisión y validación JWT; claims mínimos; issuer/audience/exp/nbf/jti/role; options desde entorno.
- **Verificar:** firma válida, firma alterada, algoritmo incorrecto, issuer/audience incorrectos, expiración y claims faltantes.
- **Evidencia:** `IAccessTokenService` (Application/Security), `JwtOptions` y `JwtAccessTokenService` (Infrastructure/Security). HS256 fijo; claims `sub`/`jti`/`role`/`iss`/`aud`/`iat`/`nbf`/`exp`; `ClockSkew` 0; vida 15 min (`JWT_ACCESS_TOKEN_MINUTES` opcional, 1–60); clave ≥ 32 bytes y plantillas (placeholder/ficticio/no_usar…) rechazadas fuera de Development (en Development se advierte al arrancar). Paquetes: `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.14 (MIT) y `Microsoft.IdentityModel.JsonWebTokens` 8.23.0 (MIT). Regresión acumulada: ver gates.md.

## T017 — Blacklist Redis Auth
- **Estado:** Cerrado y aprobado (2026-11-03, aprobación humana). Decisiones: plan.md §11.6.
- **Crear:** abstracción de revocación y adaptador Redis Auth; hook de validación JWT.
- **Verificar:** token en blacklist se rechaza; TTL coincide con vida restante; caída de Redis Auth falla cerrada; Redis Cache no participa.
- **Evidencia:** `ITokenRevocationStore` (Application) y `RedisTokenRevocationStore` (Infrastructure/Redis): clave `jwt:blacklist:{jti}`, TTL = vida restante, solo `RedisAuthConnection`; falla cerrada (`AuthDependencyUnavailableException` → 503). Hook `RevocationJwtBearerEvents` (Api/Authentication): token revocado → 401. Prueba de integración real con caída de Redis incluida en la regresión (gates.md).

## T018 — Endpoint Register
- **Estado:** Cerrado y aprobado (2026-11-03, aprobación humana). Decisiones: plan.md §11.6.
- **Crear:** `POST /api/v1/auth/register`, validadores, Command/Handler y persistencia.
- **Regla:** asigna Owner; ignora/rechaza cualquier rol arbitrario; email normalizado único.
- **Verificar:** creación exitosa, duplicado, validación, hash Argon2id y respuesta wrapper.
- **Evidencia:** `RegisterHandler` y validadores (Application/Authentication), `AuthController` y `AuthContracts` (Api). 201 con tokens; 400 con `data.errors`; 409 con mensaje genérico; 429. Los DTO rechazan miembros desconocidos (`JsonUnmappedMemberHandling.Disallow`): enviar `role` devuelve 400. Carrera de registro (Atlas real) → una sola cuenta. Detalle de contrato: `docs/api/endpoints.md`.

## T019 — Endpoint Login
- **Estado:** Cerrado y aprobado (2026-11-03, aprobación humana). Decisiones: plan.md §11.6.
- **Crear:** `POST /api/v1/auth/login`, Query/Command apropiado, verificación Argon2id, rate limit y mensajes anti-enumeración.
- **Verificar:** credenciales correctas/incorrectas, usuario no existente, throttling y emisión de tokens.
- **Evidencia:** `LoginHandler`: 401 «Credenciales inválidas.» idéntico para correo inexistente y contraseña incorrecta, hash ficticio para igualar tiempos (requisito heredado de T015 cumplido) y rehash si los parámetros subieron (prueba real de rehash). Rate limiting por IP (`AUTH_RATE_LIMIT_PER_MINUTE`) y concurrencia de hashing acotada (`ARGON2_MAX_CONCURRENCY`, requisito heredado de T015/T022 cubierto).

## T020 — Refresh Token persistente y rotatorio
- **Estado:** Cerrado y aprobado (2026-11-03, aprobación humana). Decisiones: plan.md §11.6.
- **Crear:** token opaco, almacenamiento exclusivo de hash en Atlas, rotación, expiración idle/absoluta y detección de reuso.
- **Verificar:** access expirado con refresh válido, refresh expirado, refresh consumido, solicitudes concurrentes/replay y revocación de familia.
- **Evidencia:** `RefreshHandler` y `AuthTokenIssuer`; token opaco de 256 bits (Base64Url, 43 caracteres), solo SHA-256 en Atlas; consumo atómico por update condicional; token consumido reutilizado revoca la sesión (`ReplayDetected`). Pruebas reales Atlas: refresh concurrente, replay y aislamiento entre usuarios. Limitaciones documentadas en plan.md §11.6 (doble envío legítimo puede cerrar la sesión).

## T021 — Endpoint Logout
- **Estado:** Cerrado y aprobado (2026-11-03, aprobación humana). Decisiones: plan.md §11.6.
- **Crear:** `POST /api/v1/auth/logout`.
- **Implementar:** revocar refresh session Atlas y escribir `jti` access vigente en Redis Auth con TTL restante.
- **Verificar:** token reutilizado recibe rechazo; refresh session no puede renovar; no se registran tokens; respuesta wrapper con `data:null`.
- **Evidencia:** `LogoutHandler`; `[Authorize]` con access token vigente más `refreshToken` en el cuerpo; sesión ajena o desconocida → 403; idempotente para sesión propia ya revocada; orden Atlas → Redis; 503 si Redis Auth cae. Flujo real register→login→refresh→logout verificado contra Atlas + Redis.

> Regresión T016–T021 (2026-11-03, `scripts/quality-gate.ps1`, exit 0): Unit 110/110, Security 135/135, Contract 46/46, Integration 39/39 (0 omitidas). Revisiones independientes: trazabilidad PASS; seguridad PASS con observaciones (ver gates.md). Ninguna tarea está cerrada hasta la aprobación humana.

## T022 — Políticas de autenticación y autorización
- **Crear:** configuración de roles Owner/Business y políticas. Aplicar la política Owner a los endpoints iniciales de mascotas.
- **Verificar:** anónimo 401; rol incorrecto 403; no hay rol Admin; endpoints no conceden acceso por claims manipulados.

## T023 — Pruebas unitarias, integración y seguridad de auth
- **Crear:** cobertura de reglas, repositorios Atlas, Redis blacklist, handlers y endpoints.
- **Verificar:** unit/integration/security suites; replay, blacklist, rate, lockout si aplica, expiración, Redis outage e IDOR básico.

## T024 — Contratos y documentación de auth
- **Modificar:** `docs/api/endpoints.md`, DTOs documentados, reglas y checklist ASVS.
- **Verificar:** tests de contrato comparan método, ruta, status y wrapper; docs coinciden con rutas implementadas.

## T025 — Gates y revisión independiente de auth
- **Ejecutar:** security-reviewer y traceability-reviewer.
- **Verificar:** todos los CA cubiertos; issues corregidos o excepciones aprobadas; evidencia en gates; usuario aprueba el slice.

## Definition of Done
- [ ] Los cuatro endpoints de auth están probados.
- [ ] Access JWT expirado puede renovarse sin login mientras refresh siga vigente.
- [ ] Logout revoca sesión y bloquea el access JWT vigente.
- [ ] T013–T025 tienen evidencia reproducible.
- [ ] Slice aprobado por el usuario.
