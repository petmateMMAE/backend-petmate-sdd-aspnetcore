# Checklist de seguridad — OWASP ASVS Nivel 2

> Checklist vivo por Spec (`pm-NNN`); la evidencia histórica conserva el nombre del slice original (ver `specs/_historico/`). Fijar versión específica de ASVS al seleccionar herramientas y baseline. No declarar cumplimiento total sin evaluación de controles aplicables.

## Identidad y autenticación
- [x] Argon2id con configuración calibrada, salt único y hash versionable. _(T015: Security 97/97; benchmark 139–143 ms; ver gates slice 01)_
- [x] Mensajes de login resistentes a enumeración. _(T019: 401 idéntico y hash ficticio para igualar tiempos; excepción documentada: el 409 de Register revela la existencia del correo, mitigado por rate limit)_
- [x] Rate limiting de autenticación y mitigación de abuso. _(T019–T021 y pm-034/T056, 2026-11-08: límite por IP, cuota y bloqueo por correo (5 fallos por 15 minutos, el sexto bloquea 15 minutos; 30 refrescos por usuario) con contadores en Redis de autenticación, hashing Argon2id acotado (núcleos y cola del doble) y 429 con `Retry-After`. Abierto: el bloqueo permite dejar sin acceso temporal a un correo ajeno (H3); el límite por IP es por instancia (H6))_
- [x] JWT valida firma/algoritmo/issuer/audience/exp/nbf/jti. _(T016: HS256 fijo, ClockSkew 0; entropía de la clave solo validada por longitud)_
- [x] Refresh tokens opacos, almacenados como hash, rotatorios y con detección de replay. _(T020)_
- [x] Logout revoca sesión y blacklist JWT en Redis Auth. _(T021; otros access tokens de la misma sesión viven hasta 15 min: `jti` no ligado a la sesión)_
- [x] Redis Auth falla cerrada ante indisponibilidad. _(T017: 503; prueba real de caída de Redis)_

## Autorización y datos
- [ ] Roles únicos Owner/Business; no existe Admin.
- [ ] Denegar por defecto y mínimo privilegio.
- [ ] Ownership/relación de recurso comprobada en cada operación.
- [ ] Pruebas IDOR/BOLA y mass assignment.
- [ ] Datos clínicos y fotografías privados por defecto.

## Secretos y comunicación
- [ ] `.env` ignorado y `.env.example` sin secretos.
- [ ] Producción no despliega `.env`; secretos inyectados por entorno seguro.
- [ ] HTTPS/certificado válido y forwarded headers restringidos a proxies confiables.
- [ ] Tokens, passwords, URIs y PII no aparecen en logs.

## API y operación
- [ ] Validación de inputs y límites de tamaño/paginación.
- [ ] Response Wrapper no filtra stack traces ni secretos.
- [x] Rate limiting y respuestas 429 uniformes. _(pm-036: matriz de límites en `docs/api/endpoints.md`; todas las respuestas 429 usan el wrapper y `Retry-After`; pruebas en pm-034, pm-036 y pm-045)_
- [ ] CORS restringido al origen requerido en despliegue.
- [ ] Health endpoints no exponen configuración sensible.

## Supply chain y DevSecOps
- [ ] SAST ejecutado en CI.
- [ ] Secret scan ejecutado en CI.
- [ ] Vulnerability audit y license audit de NuGet.
- [ ] Container scan y configuración de despliegue revisados.
- [ ] SBOM generado cuando exista herramienta compatible $0.
- [ ] Hallazgos críticos/altos resueltos o excepción aprobada explícitamente.

## Evidencia por slice
| Slice | Controles aplicables | Evidencia | Revisor | Estado |
|---|---|---|---|---|
| `00-foundation` / T004 | `.env` ignorado, plantilla sin secretos, no exposición de valores y no carga en Production | `.env` ignorado y `.env.example` con placeholders; el bootstrap determina el entorno efectivo, omite `.env` con Production por defecto y carga solo desde content root fuera de Production. Pruebas Security 18/18; ver `specs/00-foundation/checklists/gates.md`. | `security-reviewer` (revisión posterior, 2026-11-03) | Verificado; pendiente de aprobación humana |
| `00-foundation` / T007 | Exposición de documentación, dependencias OpenAPI, ausencia de UI alternativa | OpenAPI/Scalar solo en Development; Production 404; Security 26/26, Contract 4/4, NuGet Audit 0 vulnerabilidades. Sin IntegrationTest aplicable: suite vacía y carga bloqueada por WDAC; ver gates. | `security-reviewer`, 2026-11-03 | Verificado; pendiente de aprobación humana |
| `pm-033` / T022 (cerrada el 2026-11-08) | Control de acceso (V4): denegación por defecto, mínimo privilegio, rol solo desde claims de JWT validado, sin rol Admin | `AuthorizationPolicies`, `UnmatchedRouteResultHandler`; pruebas en SecurityTests y ContractTests (ver `specs/pm-033-authorization-policies/checklists/gates.md`) | Revisión independiente de seguridad del módulo y del grupo de autenticación, 2026-11-08 (apto con observaciones) | Cerrado y aprobado (2026-11-08) |
| `pm-034` / T056 y `pm-036` / T057, T054 | Anti-automatización (V2), sesiones (V3) y lógica de negocio (V11): cuota y bloqueo por correo, límites por usuario e IP, hashing acotado, límite de cuerpo de 16 KB | `ThrottlingTests`, `AuthThrottlingEndpointsTests`, `AuthAttemptThrottleTests`, `PetLimitsTests` y las demás pruebas de los gates de pm-034, pm-036 y pm-045; Kestrel y Redis reales: sexto fallo → 429 con `Retry-After: 900` | Revisión independiente de seguridad del grupo de autenticación, 2026-11-08: apto con observaciones (sin críticos ni altos); ver pm-035 gates | T056 y T057 cerradas, T054 aprobada (2026-11-08) |
| `01-initial-authentication` / T016–T021 | JWT, lista negra Redis Auth, Register/Login/Refresh/Logout, Argon2id integrado, rate limiting, anti-enumeración | `scripts/quality-gate.ps1` exit 0 (2026-11-03): Unit 110, Security 135, Contract 46, Integration 39 (0 omitidas; flujos reales Atlas + Redis). NuGet 0 vulnerables, licencias OK. Abiertos: `FallbackPolicy` y roles (T022), `jti` no ligado a sesión (los access tokens de una sesión revocada viven hasta 15 minutos), entropía de la clave de firma y registro de eventos de seguridad. El limitador asíncrono de Argon2id y la cuota por correo se implementaron en pm-034 (T056). Ver gates del slice 01 y plan.md §11.6. | `security-reviewer`, 2026-11-03 (PASS con observaciones) | Verificado; pendiente de aprobación humana |
