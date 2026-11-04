# Plan — 01 Autenticación inicial y sesiones

## 1. Contexto

Depende de la base técnica `00-foundation`. Debe quedar listo para usarse desde endpoints Owner.

## 2. Diseño técnico

- Domain: User, Role, RefreshSession/Session concepts y reglas invariantes.
- Application: Commands/Queries y handlers de Register, Login, Refresh y Logout; abstracciones de repositorio/hasher/token/session/blacklist.
- Infrastructure: Mongo repositories, Argon2id adapter, JWT token issuer/validator, Redis blacklist adapter.
- Api: AuthController MVC delgado; endpoints dispatch por Mediator; middleware authz/authn.
- DI: registrar adaptadores y options desde Infrastructure y composition root.

## 3. Secuencia de flujo

### Register
1. Validar request y normalizar email.
2. Comprobar unicidad.
3. Hashear password con Argon2id.
4. Crear usuario con rol Owner (no aceptar rol libre).
5. Crear sesión y refresh token; persistir solo hash.
6. Emitir Access Token + Refresh Token.
7. Devolver wrapper en español.

### Login
1. Normalizar email.
2. Consultar usuario.
3. Verificar Argon2id; fallo genérico que no enumera cuentas.
4. Aplicar controles de rate/lockout.
5. Crear sesión/refresh token.
6. Emitir tokens.

### Refresh
1. Recibir refresh token por canal protegido de la aplicación.
2. Calcular hash y encontrar sesión vigente.
3. Verificar estado, vencimiento inactivo y máximo absoluto.
4. Rotar de forma atómica o mediante mecanismo seguro equivalente soportado por Atlas.
5. Detectar reuso del token anterior y revocar familia/sesión.
6. Emitir nuevo Access Token y Refresh Token.

### Logout
1. Identificar refresh session.
2. Revocar sesión/refresh token en Atlas.
3. Extraer `jti` y `exp` del Access Token validado.
4. Escribir blacklist Redis Auth con TTL restante.
5. Devolver respuesta wrapper con `data: null`.

## 4. Persistencia y servicios

- User collection con índice único de email normalizado.
- Refresh session/token collection con índices para hash, user/session y expiración.
- Redis Auth con prefijo `jwt:blacklist:` y TTL hasta `exp`.
- Nunca usar Redis Cache para sesiones ni tokens.
- Si la estrategia de rotación requiere atomicidad, usar operaciones atómicas por documento o transacciones compatibles con Atlas; documentar limitación del tier $0 antes de elegir.

## 5. Seguridad y DevSecOps

- Definir algoritmo permitido y validar `iss`, `aud`, `exp`, `nbf`, `sub`, `jti`, `role`.
- No aceptar `alg=none` ni algoritmo no permitido.
- Redis Auth outage: acceso denegado de forma segura y respuesta de indisponibilidad, no falso 401 si el backend no pudo verificar la dependencia.
- Rate limiting y mensajes genéricos.
- Hacer pruebas de carrera en refresh, replay y logout.
- No registrar body de login, Authorization header o tokens.

## 6. Paquetes y costo

Registrar evaluación de paquete JWT, Argon2id y Mediator existente. Todos deben tener licencia/costo $0 y compatibilidad .NET 10. No fijar paquete sin evidenciar la evaluación.

## 7. Archivos a crear/modificar

- Domain: usuario/rol/sesión y reglas.
- Application: abstracciones, DTOs, commands, queries, handlers, validators.
- Infrastructure: repositorios Atlas, hasher Argon2id, JWT, Redis blacklist.
- Api: AuthController, configuración y políticas de autenticación/autorización.
- Tests: Unit, Integration, Security y Contract.
- Docs: `docs/api/endpoints.md` y checklist ASVS.

## 8. Riesgos y decisiones pendientes

- Atomicidad/anti-replay en la política de refresh debe probarse con solicitudes concurrentes.
- Una caída de Redis Auth implica degradación de disponibilidad por fail-closed.
- Business no se autoasigna en Register; onboarding se define en otro slice.
- Login/Register/Logout/Refresh deben ser utilizables desde app móvil sin almacenar tokens en almacenamiento inseguro.

## 9. Verificación

Ejecutar pruebas de hash, token, refresh rotation, replay, blacklist, logout, Redis outage, rate, wrappers, y acceso no autorizado. Ejecutar todas las suites y escaneos previstos en gates.

## 10. Aprobación

Aprobado para implementación el 2026-11-03 (aprobación humana del slice).

## 11. Decisiones técnicas T013–T015 (2026-11-03)

> Propuestas por el implementador dentro del alcance aprobado; compatibles con spec §3–§6 y constitución §6. Cualquier cambio posterior de alcance requiere nueva decisión humana.

### 11.1 Modelo de dominio (T013)

- **Identificadores:** `Guid` versión 7 (`Guid.CreateVersion7()`): ordenables en el tiempo, sin dependencia de MongoDB en Domain. Se persisten como UUID estándar (`GuidRepresentation.Standard`).
- **`Email` (value object):** recorta espacios y pasa a minúsculas invariantes; máx. 254 caracteres; forma mínima `local@dominio` (validación sintáctica simple; la validación de entrada completa pertenece a Register/T018). IDN/alias de proveedor no se normalizan.
- **`UserRole`:** enum `Owner = 1`, `Business = 2`. No existe Admin. La única fábrica pública de usuarios (`User.RegisterOwner`) crea `Owner`; `User.Restore` existe solo para rehidratar desde persistencia. No hay fábrica Business (onboarding en un slice posterior).
- **`PasswordHash` (value object):** envoltorio opaco de una cadena PHC `$argon2id$…`; rechaza vacío, otro algoritmo o longitud > 255. No contiene ni conoce la contraseña.
- **`User`:** `Id`, `Email`, `PasswordHash`, `Role`, `CreatedAtUtc`, `UpdatedAtUtc`. Comportamiento: `ChangePasswordHash` (rehash al autenticar, T019). No se agrega estado/activación hasta que un spec lo requiera (el spec dice «fechas/estado mínimo»; se interpreta como fechas).
- **Sesiones con un documento por refresh token (familia):**
  - `RefreshSession` (la familia/sesión por dispositivo): `Id`, `UserId`, `CreatedAtUtc`, `LastUsedAtUtc`, `IdleExpiresAtUtc` (+30 días), `AbsoluteExpiresAtUtc` (+90 días), `RevokedAtUtc`, `RevocationReason`. Métodos: `Start`, `IsActive(now)`, `RegisterUse(now)` (renueva idle sin superar el límite absoluto), `Revoke`.
  - `RefreshToken`: `Id`, `SessionId`, `TokenHash`, `IssuedAtUtc`, `ConsumedAtUtc`. `Consume` marca el uso; una segunda presentación de un token consumido es la señal de replay.
  - `RefreshTokenHash`: value object de 64 hex (SHA-256 del token opaco); el hashing se implementa en T020.
  - `SessionPolicy`: `IdleTimeout` y `AbsoluteLifetime` configurables (valida >0 e idle ≤ absoluto); por defecto 30/90 días.
- Todas las fechas son UTC (`DateTimeKind.Utc`); Domain no lee el reloj: el tiempo entra como parámetro.

### 11.2 Persistencia Atlas (T014)

- **Colecciones:** `users`, `refresh_sessions`, `refresh_tokens` (prefijo configurable solo para pruebas de integración).
- **Documentos de persistencia** (clases internas de Infrastructure; Domain sin atributos Bson) con mapeo explícito dominio↔documento.
- **Índices:** `users.email` único; `refresh_tokens.tokenHash` único; `refresh_tokens.sessionId`; `refresh_sessions.userId`; TTL (`expireAfter 0`) sobre `absoluteExpiresAtUtc` en `refresh_sessions` y `refresh_tokens` (copia del límite absoluto: pasado ese instante ninguna sesión ni token es válido, por lo que no se conserva evidencia).
- **Creación de índices:** `MongoIndexInitializer` (hosted service con reintentos y logging sin detalles) y método invocable por pruebas; la API no falla al arrancar si Atlas no responde (el readiness lo refleja).
- **Atomicidad sin transacciones (válido en el tier $0):** el consumo de un refresh token es un `findOneAndUpdate` condicional (`consumedAtUtc` nulo → fecha); solo un llamador concurrente gana. Si no gana y el token ya estaba consumido, T020 tratará el replay y revocará la sesión. La unicidad de email usa el índice único y se traduce `DuplicateKey` → `false`, sin lecturas previas (sin condición de carrera).
- **Contratos de Application:** `IUserRepository` (`TryAdd`, `FindByEmail`, `FindById`, `UpdatePasswordHash`) y `ISessionRepository` (`AddSession`, `AddToken`, `FindTokenByHash`, `TryConsumeToken`, `FindSession`, `UpdateActivity`, `Revoke`). Sin tipos de MongoDB.
- **Pruebas de integración:** solo `MONGODB_TEST_URI`/`MONGODB_TEST_DATABASE`, con guarda que falla si la base coincide con `MONGODB_DATABASE` o no contiene «test»; colecciones con prefijo único por ejecución que la prueba elimina al terminar (nunca se tocan otras colecciones).

### 11.3 Argon2id (T015)

| Elemento | Decisión |
|---|---|
| Paquete | `Geralt` 4.5.0 (MIT, $0, publicado 2026-10-31, mantenido): envuelve libsodium (`crypto_pwhash_str`, Argon2id) con salt aleatorio, cadena PHC, verificación en tiempo constante y `needs_rehash`. |
| Alternativas descartadas | `Konscious.Security.Cryptography.Argon2` 1.3.1 (MIT, administrada, sin PHC ni rehash: habría que implementar formato y comparación); `Isopoh.Cryptography.Argon2` 2.0.0 (sin releases desde 2023, licencia no declarada en el paquete); `Sodium.Core` (sin API de rehash equivalente). |
| Parámetros por defecto | `t = 3` iteraciones, `m = 64 MiB` (65 536 KiB), `p = 1` (libsodium fija el paralelismo en 1), salt 16 bytes, hash 32 bytes. Superan el mínimo OWASP (m=19 MiB, t=2, p=1) y el perfil «interactive» de libsodium. |
| Configuración | `ARGON2_MEMORY_KIB`, `ARGON2_ITERATIONS` opcionales; se rechaza todo valor por debajo del mínimo OWASP (no se puede bajar «para acelerar pruebas»). |
| Actualización | `Verify` devuelve `SuccessRehashNeeded` si el hash almacenado usa parámetros inferiores a los configurados; T019 persistirá el hash nuevo tras un login correcto (`User.ChangePasswordHash`). |
| Benchmark | prueba que mide el tiempo del hash con los parámetros por defecto; la medida real se registra en `gates.md`. |
| Riesgos | cada hash usa 64 MiB: un abuso concurrente de Login/Register puede agotar memoria → el rate limiting (T022/T023) es obligatorio y debe acotar la concurrencia de hashing; libsodium nativo en el contenedor Linux y en el host (Smart App Control puede bloquear el binario nativo en este equipo). |

### 11.4 Notas de implementación T014 (2026-11-03)

- Se añade `PersistenceUnavailableException` (Application): los repositorios traducen `MongoException`, `TimeoutException` y documentos ilegibles (`DomainException` al rehidratar) a un error genérico; la API responde 503 y registra solo el tipo de error.
- Para T020: validar `IsActive(now)` antes de `TryConsumeToken`; pasar `session.AbsoluteExpiresAtUtc` a `AddToken`; `UpdateActivity` y `Revoke` son idempotentes (no reviven ni sobrescriben sesiones revocadas).

### 11.5 Notas de implementación T015 (2026-11-03)

- Dependencia nativa: `Geralt` 4.5.0 (MIT) depende del paquete `libsodium` 1.0.22 (ISC); ambas licencias pasan la auditoría de `scripts/quality-gate.ps1`.
- Límites: contraseña ≤ 1024 bytes UTF-8; hash ≤ 255 caracteres; la cabecera PHC del hash almacenado se valida (`v=19`, `p=1`, m en [19 456, 262 144] KiB, t en [2, 10]) antes de invocar libsodium para impedir DoS con hashes manipulados. Tope de memoria configurable 256 MiB.
- Política de parámetros: solo subirlos. Si la configuración los baja, `Verify` devuelve `SuccessRehashNeeded` y el siguiente login degradaría el hash.
- Requisitos para tareas posteriores: **T019** debe ejecutar `Verify` contra un hash ficticio precalculado con los parámetros vigentes cuando el usuario no exista (igualar tiempos, sin enumeración); **T022/T023** deben acotar la concurrencia de hashing (≈64 MiB y ≈140 ms por hash) con rate limiting por IP y cuenta.

### 11.6 Decisiones de implementación T016–T021 (2026-11-03)

> Propuestas por el implementador dentro del alcance aprobado; verificadas, pendientes de aprobación humana de cierre.

**Access JWT (T016) y revocación (T017)**
- HS256 fijo; claims `sub`, `jti`, `role`, `iss`, `aud`, `iat`, `nbf`, `exp`; `ClockSkew` 0; vida 15 min (`JWT_ACCESS_TOKEN_MINUTES` opcional, 1–60).
- Clave ≥ 32 bytes; las plantillas (placeholder/ficticio/no_usar…) se rechazan fuera de Development, donde solo se advierte al arrancar. La entropía solo se valida por longitud.
- Paquetes (MIT, $0): `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.14 y `Microsoft.IdentityModel.JsonWebTokens` 8.23.0.
- Revocación: clave `jwt:blacklist:{jti}` con TTL = vida restante, solo `RedisAuthConnection`; falla cerrada (`AuthDependencyUnavailableException` → 503). Hook `RevocationJwtBearerEvents`: token revocado → 401.
- El log de JwtBearer se filtra a `Critical` porque registraría la causa interna del rechazo.

**Register (T018)**
- Contraseña de 12–128 caracteres sin reglas de composición.
- El 409 por correo duplicado revela la existencia del correo: riesgo aceptado, mitigado por rate limit; la verificación de correo es un slice posterior.
- Los DTO rechazan miembros desconocidos (`JsonUnmappedMemberHandling.Disallow`): enviar `role` devuelve 400.

**Login (T019)**
- Sin comprobación de estado de cuenta: el modelo aprobado (T013) no tiene estado.
- Hash ficticio para igualar tiempos cuando el correo no existe; rehash tras login correcto si los parámetros subieron.

**Refresh (T020)**
- Refresh token opaco de 256 bits, Base64Url (43 caracteres); en Atlas solo se guarda su SHA-256.
- Consumo atómico por update condicional; un token consumido reutilizado revoca la sesión (`ReplayDetected`).
- Ante solicitudes concurrentes idénticas gana como máximo una; un perdedor puede revocar la sesión antes de que el ganador la actualice y entonces ninguna tiene éxito. Un doble envío legítimo cierra la sesión.
- `UpdateActivity` devuelve `false` si la sesión ya estaba revocada y entonces no se emite nada.
- El token se consume antes de insertar el nuevo: si Atlas falla en medio, el cliente debe volver a iniciar sesión.
- Tolerancia a reloj atrasado.

**Logout (T021)**
- Exige access token vigente (si expiró, renovar con refresh y volver a llamar) y el refresh token en el cuerpo.
- Sesión ajena o desconocida → 403; idempotente para sesión propia ya revocada; orden Atlas → Redis.

**Abuso y concurrencia**
- Rate limiting de ventana fija por IP en register/login/refresh: `AUTH_RATE_LIMIT_PER_MINUTE` (por defecto 10, rango 1–10000); IPv6 agrupada por /64; sin cuota por correo todavía.
- Detrás de proxy hay que configurar `TRUSTED_PROXIES` (IPs separadas por coma); sin ella la IP observada es la del proxy.
- Concurrencia de hashing acotada: `ARGON2_MAX_CONCURRENCY` (por defecto max(2, núcleos)); espera de 5 s → 503.

**Observaciones abiertas (no resueltas)**
- **T022:** definir `FallbackPolicy` (autenticación por defecto) y políticas por rol.
- El semáforo de Argon2id bloquea hilos de forma síncrona (riesgo de starvation bajo ráfaga, mitigado por rate limit): evaluar un limitador asíncrono.
- Falta cuota por correo contra credential stuffing distribuido.
- El `jti` no está ligado a la sesión: otros access tokens de la misma sesión viven hasta 15 min tras el logout.
- La entropía de `JWT_SIGNING_KEY` solo se valida por longitud.
