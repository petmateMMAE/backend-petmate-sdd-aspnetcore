# Spec — 01 Autenticación inicial y sesiones

> Estado: Aprobado para implementación (2026-11-03, aprobación humana del slice). Este slice implementa Register, Login, Logout y Refresh, y deja preparadas las bases de autorización Owner/Business.

## 1. Objetivo y contexto

Implementar el flujo de autenticación inicial de PetMate con contraseñas protegidas por Argon2id, JWT Access Tokens, Refresh Tokens rotatorios y revocación inmediata de access tokens mediante una lista negra en Redis de autenticación.

## 2. Alcance y fuera de alcance

### Incluye
- `POST /api/v1/auth/register`.
- `POST /api/v1/auth/login`.
- `POST /api/v1/auth/refresh`.
- `POST /api/v1/auth/logout`.
- Rol Owner y rol Business en el modelo de autorización, sin rol Admin.
- Registro público inicial de Owner.
- Hash Argon2id.
- JWT Access Token y Refresh Token opaco.
- Persistencia de sesión/refresh token en MongoDB Atlas.
- Rotación de refresh tokens y detección de reuso.
- Redis Auth blacklist por `jti`.
- Rate limiting y respuestas de error uniformes.

### No incluye en este hito
- Verificación de correo.
- Recuperación/cambio de contraseña.
- MFA/2FA.
- Onboarding y verificación de negocios Business.
- Eliminación de cuenta.
- Administración de sesiones de otros usuarios.

Estas funciones se planificarán como slices posteriores. No se deben simular como terminadas.

## 3. Actores, permisos y reglas de negocio

- Usuario no autenticado: puede registrar Owner e iniciar sesión.
- Owner: obtiene tokens para endpoints Owner autorizados.
- Business: rol reconocido por el sistema, pero el onboarding/activación se define después.
- No existe Admin.
- El cliente no puede seleccionar un rol arbitrario en el registro público inicial.
- Correo electrónico normalizado y único.
- Mensajes de fallo de login no deben revelar si el correo existe.
- Refresh tokens son específicos de una sesión/dispositivo; cada uso rota el token.

## 4. Requisitos funcionales y flujos

- RF-001 Register crea cuenta Owner si los datos son válidos y no existe email duplicado.
- RF-002 Login verifica hash Argon2id y emite Access + Refresh Token.
- RF-003 Refresh verifica vigencia/revocación, rota refresh token e intercambia por nuevos tokens.
- RF-004 Reutilizar un refresh token consumido debe detectar replay y revocar su familia/sesión.
- RF-005 Logout revoca refresh session en MongoDB y añade el `jti` actual a Redis Auth.
- RF-006 Un access JWT incluido en blacklist se rechaza aunque no haya expirado.
- RF-007 La expiración del Access Token no cierra una sesión cuyo Refresh Token sigue válido.
- RF-008 Los endpoints protegidos consultan Redis Auth de manera fail-closed.

## 5. Contrato de API y datos

| Método | Endpoint | Acceso | Resultado |
|---|---|---|---|
| POST | `/api/v1/auth/register` | Público | Crea Owner y emite tokens |
| POST | `/api/v1/auth/login` | Público | Valida credenciales y emite tokens |
| POST | `/api/v1/auth/refresh` | Refresh Token | Rota sesión y emite tokens nuevos |
| POST | `/api/v1/auth/logout` | Sesión autenticada | Revoca sesión y JWT actual |

Todos devuelven Response Wrapper. Los nombres exactos de DTOs y campos se cierran en el plan antes de implementación.

### Token policy inicial

- Access Token: 15 minutos, configurable.
- Refresh Token: opaco, aleatorio, hash almacenado en MongoDB Atlas.
- Expiración por inactividad: 30 días.
- Expiración absoluta: 90 días.
- Rotación en cada refresh.
- Claim `jti` obligatorio y único por Access Token.
- La firma debe usar algoritmo fijo y clave fuerte mediante configuración externa.

### Persistencia

- User: identificador, email normalizado, hash Argon2id, rol, fechas/estado mínimo.
- Refresh session/token: hash, usuario, identificador de sesión/familia, estado, expiración, fechas de rotación/revocación. No almacenar refresh token en texto claro.
- Índice único para email normalizado y los índices necesarios para consultas/revocación de sesiones.
- Redis Auth: `jwt:blacklist:{jti}` con TTL restante.

## 6. Requisitos no funcionales y seguridad

- Argon2id con parámetros calibrados para el entorno; hash versionable y rehash al autenticar si se elevan parámetros.
- JWT validado con firma, algoritmo permitido, issuer, audience, `exp`, `nbf` y `jti`.
- Clave JWT nunca en código, logs o `.env.example` real.
- Redis Auth falla cerrado: si no se puede verificar blacklist, no conceder acceso.
- Rate limit en Register/Login/Refresh para mitigar abuso; valores deben justificarse con pruebas y costo $0.
- Errores de login indistinguibles para usuario inexistente y contraseña incorrecta.
- No escribir credenciales, tokens ni información sensible en logs.
- OWASP ASVS L2 y pruebas unitarias, integración, seguridad y contrato obligatorias.

## 7. Criterios de aceptación

- [ ] CA-001 Register crea Owner y rechaza correo duplicado.
- [ ] CA-002 contraseña se persiste como hash Argon2id, nunca texto claro.
- [ ] CA-003 Login emite Access Token y Refresh Token.
- [ ] CA-004 Access Token expirado puede renovarse si la sesión refresh sigue vigente.
- [ ] CA-005 Refresh rota token; el anterior no puede volver a usarse.
- [ ] CA-006 replay de refresh token se detecta y revoca la sesión/familia según política aprobada.
- [ ] CA-007 Logout revoca refresh session y bloquea `jti` vigente en Redis Auth.
- [ ] CA-008 JWT en blacklist recibe rechazo de autenticación.
- [ ] CA-009 Redis Auth inaccesible no provoca autorización accidental.
- [ ] CA-010 no existe rol Admin y el cliente no puede autoasignarse Business.
- [ ] CA-011 los cuatro endpoints usan Response Wrapper y mensajes en español.
- [ ] CA-012 pruebas unitarias, integración, seguridad y contrato pasan.

## 8. Estrategia de pruebas y gates

- Unit: hashing, validación de tokens, rotación, expiraciones, políticas.
- Integration: Atlas test DB para users/sessions y Redis Auth no productivo.
- Security: brute force/rate limit, enumeration, replay, token revoked, wrong role, bad signature, expired token, Redis outage, token in logs.
- Contract: rutas, wrappers, status codes y token DTOs.
- DevSecOps: ASVS L2, SAST, secret scan, NuGet vuln/license audit.

## 9. Dependencias, riesgos y decisiones pendientes

- Depende de `00-foundation`.
- Seleccionar algoritmo JWT, paquete Argon2id y librería JWT conforme a licencia $0 y seguridad.
- Cerrar formato del body de Logout (refresh token) y Refresh.
- Cerrar política de almacenamiento seguro de tokens en la app Android como contrato con el equipo móvil.
- El alta Business queda pendiente de un spec posterior.

## 10. Aprobación

- [x] Contrato de auth y política de sesión revisados. _(aprobación humana 2026-11-03)_
- [x] Plan y tareas aprobados. _(2026-11-03; las decisiones técnicas de T013–T015 constan en plan.md §11)_
