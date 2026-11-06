# Catálogo vivo de endpoints — PetMate API

> Fuente canónica de rutas. Mantener sincronizado con los specs, controllers, tests de contrato y OpenAPI.

## Documentación interactiva

- El contrato se publica como documento OpenAPI generado desde la API: `/openapi/v1.json`.
- Scalar es la única UI interactiva y consume ese documento: `/scalar/v1`.
- La UI y el documento se habilitan por defecto en Development; en producción permanecen deshabilitados salvo habilitación explícita y revisión de seguridad.
- No agregar Swagger UI ni otra interfaz alternativa.

## Convenciones

- Base de ruta versionada `/api/v1`.
- La acción aparece explícitamente en la ruta; no se infiere solo del verbo HTTP.
- Todas las respuestas usan `statusCode`, `message`, `data`; `data` es nullable.
- `statusCode` dentro del wrapper debe coincidir con HTTP.
- Mensajes en español.

## Ruta técnica Foundation

| Método | Endpoint | Acceso | Spec | Estado |
|---|---|---|---|---|
| GET | `/api/v1/technical/mediator-probe` | Técnico | `pm-007` / T007, T042 | Implementado; no funcional de negocio |
| GET | `/api/v1/health/live` | Público (técnico) | `pm-014` / T011 | Liveness: 200, sin consultar dependencias |
| GET | `/api/v1/health/ready` | Público (técnico) | `pm-014` / T011 | Readiness: 200 o 503; `data.dependencies[{name,status}]` para `mongodb`, `redis-auth`, `redis-cache`; sin hosts ni secretos |

La ruta existe solo para comprobar MVC, Mediator, versionado y OpenAPI. La alternativa no versionada no se mapea.

## Autenticación inicial

| Método | Endpoint | Acceso | Spec | Estado |
|---|---|---|---|---|
| POST | `/api/v1/auth/register` | Público | `pm-029` / T018 | Implementado (verificado; pendiente de cierre humano) |
| POST | `/api/v1/auth/login` | Público | `pm-030` / T019 | Implementado (verificado; pendiente de cierre humano) |
| POST | `/api/v1/auth/refresh` | Refresh Token | `pm-031` / T020 | Implementado (verificado; pendiente de cierre humano) |
| POST | `/api/v1/auth/logout` | Sesión autenticada | `pm-032` / T021 | Implementado (verificado; pendiente de cierre humano) |

### Contratos de autenticación

- Los cuerpos de entrada rechazan miembros desconocidos: enviar p. ej. `role` devuelve 400. El rol siempre es `Owner` en el registro público.
- Register, Login y Refresh están limitados por IP (`AUTH_RATE_LIMIT_PER_MINUTE`, por defecto 10 por minuto): al exceder el límite devuelven 429. Detrás de un proxy debe configurarse `TRUSTED_PROXIES`.
- `AuthResponse` (en `data` de Register, Login y Refresh): `accessToken`, `tokenType` (`Bearer`), `accessTokenExpiresAt`, `refreshToken`, `refreshTokenExpiresAt`, `user { id, email, role }`. El refresh token se entrega solo ahí.
- Los ejemplos usan valores ficticios; nunca documentar tokens reales.

#### POST `/api/v1/auth/register`

Cuerpo: `{ "email": "usuario@ejemplo.com", "password": "contraseña-de-12-a-128-caracteres" }`

| Código | Cuándo | `data` |
|---|---|---|
| 201 | Cuenta creada | `AuthResponse` |
| 400 | Validación (correo inválido, contraseña fuera de 12–128 caracteres, miembro desconocido) | `{ "errors": [{ "field", "message" }] }` |
| 409 | Correo ya registrado; mensaje genérico «No se pudo completar el registro con los datos proporcionados.» | `null` |
| 429 | Límite de solicitudes | `null` |
| 503 | Atlas o Redis Auth no disponible | `null` |

#### POST `/api/v1/auth/login`

Cuerpo: `{ "email": "usuario@ejemplo.com", "password": "..." }`

| Código | Cuándo | `data` |
|---|---|---|
| 200 | Credenciales correctas | `AuthResponse` |
| 400 | Validación | `{ "errors": [...] }` |
| 401 | «Credenciales inválidas.»: idéntico para correo inexistente y contraseña incorrecta | `null` |
| 429 | Límite de solicitudes | `null` |
| 503 | Dependencia crítica no disponible (incluye espera de hashing agotada, 5 s) | `null` |

#### POST `/api/v1/auth/refresh`

Cuerpo: `{ "refreshToken": "<token opaco de 43 caracteres>" }`

| Código | Cuándo | `data` |
|---|---|---|
| 200 | Rotación correcta; devuelve tokens nuevos y el anterior queda consumido | `AuthResponse` |
| 400 | Validación | `{ "errors": [...] }` |
| 401 | Token desconocido, expirado, consumido (replay: revoca la sesión) o sesión revocada | `null` |
| 429 | Límite de solicitudes | `null` |
| 503 | Dependencia crítica no disponible | `null` |

#### POST `/api/v1/auth/logout`

Requiere `Authorization: Bearer <access token vigente>`. Si el access token expiró, renovar con `/refresh` y volver a llamar. Cuerpo: `{ "refreshToken": "<token opaco>" }`

| Código | Cuándo | `data` |
|---|---|---|
| 200 | Sesión revocada en Atlas y `jti` en lista negra de Redis Auth; idempotente para una sesión propia ya revocada | `null` |
| 400 | Validación | `{ "errors": [...] }` |
| 401 | Access token ausente, inválido, expirado o revocado | `null` |
| 403 | El refresh token pertenece a otro usuario o es desconocido | `null` |
| 503 | Redis Auth o Atlas no disponible (falla cerrada) | `null` |

## Autorización (pm-033 / T022)

- **Denegación por defecto.** Todo endpoint exige un usuario autenticado con rol `Owner` o `Business` salvo que se marque `[AllowAnonymous]` de forma explícita. Una prueba (`PublicSurfaceTests`) fija la lista cerrada de endpoints públicos.
- **Roles.** Solo existen `Owner` y `Business`; no hay rol de administración. El rol sale únicamente del claim `role` de un JWT con firma, vigencia y lista negra ya validadas; un token con rol ausente, desconocido o con distinto uso de mayúsculas se rechaza con 401.
- **Políticas.** `RequireOwner` y `RequireBusiness` (convención `Require<Capacidad>`). `RequireOwner` se aplicará a los endpoints de mascotas en `pm-040` a `pm-044`.
- **Códigos.** 401 si no hay autenticación válida; 403 si hay autenticación válida pero el rol no tiene permiso; ambos con el wrapper estándar (`data: null`). Las rutas inexistentes siguen devolviendo 404 a cualquier llamador.

| Método | Endpoint | Motivo |
|---|---|---|
| POST | `/api/v1/auth/register`, `/login`, `/refresh` | Flujo de autenticación inicial |
| GET | `/api/v1/health/live`, `/ready` | Sondas del balanceador y de Compose |
| GET | `/api/v1/technical/mediator-probe` | **Temporal:** público para no ampliar el alcance de pm-033. Pendiente revisar su exposición en producción en un trabajo de seguridad posterior |
| GET | `/openapi/v1.json`, `/scalar/v1` | Solo en Development |

## Perfil

| Método | Endpoint | Acceso | Spec | Estado |
|---|---|---|---|---|
| GET | `/api/v1/users/me/get` | Autenticado | `pm-037` | Planeado |
| PUT | `/api/v1/users/me/update` | Autenticado | `pm-038` | Planeado |

## Mascotas

| Método | Endpoint | Acceso | Spec | Estado |
|---|---|---|---|---|
| POST | `/api/v1/pets/create` | Owner | `pm-040` | Planeado |
| GET | `/api/v1/pets/get-all` | Owner | `pm-041` | Planeado |
| GET | `/api/v1/pets/get-by-id/{id}` | Owner + propietario | `pm-042` | Planeado |
| PUT | `/api/v1/pets/update/{id}` | Owner + propietario | `pm-043` | Planeado |
| DELETE | `/api/v1/pets/delete/{id}` | Owner + propietario | `pm-044` | Planeado |

## Códigos HTTP iniciales

200 éxito de consulta/actualización/eliminación; 201 creación; 400 solicitud inválida; 401 no autenticado; 403 sin permiso; 404 recurso no encontrado/no accesible; 409 conflicto; 422 validación semántica si se adopta; 429 rate limit; 500 error interno; 503 dependencia crítica indisponible.

No devolver 204 porque la API debe responder con el wrapper.

## Errores de validación (T009)

- Política: toda validación de forma devuelve **400**; 422 no se adopta (ratificado 2026-11-03).
- `data` = `{ "errors": [ { "field": "...", "message": "..." } ] }`, máximo 50 errores y 200 caracteres por texto. Los mensajes no deben incluir el valor recibido.
- Errores de binding/JSON de MVC usan el mismo formato con el mensaje genérico `El valor del campo no es válido.`
- 401/403/404/405 y demás 4xx/5xx sin cuerpo se envuelven con `data: null`.

## Cambios

Cada nuevo endpoint o modificación requiere Spec `pm-NNN` y actualización de este catálogo, más pruebas de contrato.
