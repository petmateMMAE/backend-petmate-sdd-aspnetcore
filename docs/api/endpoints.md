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
- Toda respuesta incluye la cabecera **`X-Correlation-Id`** (pm-054): se conserva la que envíe el cliente si cumple `[A-Za-z0-9-]{8,64}`; en otro caso el servidor genera un valor de 32 caracteres hexadecimales. Se registra como ámbito de todos los logs de la solicitud. Los logs son JSON y por solicitud contienen solo método, plantilla de ruta (nunca valores), código HTTP y duración: sin cuerpos, cabeceras, consulta, IP, correos, nombres ni tokens.

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
| POST | `/api/v1/auth/register` | Público | `pm-029` / T018 | Implementado (cerrado; spec aprobado el 2026-11-08) |
| POST | `/api/v1/auth/login` | Público | `pm-030` / T019 | Implementado (cerrado; spec aprobado el 2026-11-08) |
| POST | `/api/v1/auth/refresh` | Refresh Token | `pm-031` / T020 | Implementado (cerrado; spec aprobado el 2026-11-08) |
| POST | `/api/v1/auth/logout` | Sesión autenticada | `pm-032` / T021 | Implementado (cerrado; spec aprobado el 2026-11-08) |

### Contratos de autenticación

- Los cuerpos de entrada rechazan miembros desconocidos: enviar p. ej. `role` devuelve 400. El rol siempre es `Owner` en el registro público.
- Login y Refresh tienen además cuota por cuenta (pm-034): 5 intentos fallidos de login por correo en 15 minutos y el sexto bloquea 15 minutos (también con la contraseña correcta, sin revelar si el correo existe); 30 refrescos por usuario en 15 minutos. Las respuestas son 429 con el wrapper y `Retry-After`. El hashing de contraseñas está acotado (tantos a la vez como núcleos y el doble en cola): lo que no cabe se rechaza con 429 y `Retry-After: 1`.
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
- **Políticas.** `RequireOwner` y `RequireBusiness` (convención `Require<Capacidad>`). `RequireOwner` está aplicado a los endpoints de mascotas (`pm-040` a `pm-044`).
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
| GET | `/api/v1/users/me/get` | Autenticado | `pm-037` / T026 | Implementado (cerrado el 2026-11-08) |
| PUT | `/api/v1/users/me/update` | Autenticado | `pm-038` / T027 | Implementado (cerrado el 2026-11-08) |

### Contratos de perfil

- La identidad sale solo del claim `sub` del Access Token validado: nunca de la ruta, el cuerpo ni la query. No hay forma de leer ni editar el perfil de otra cuenta.
- `UserProfileResponse` (en `data`): `id`, `email`, `role` (`Owner`/`Business`), `displayName` (`null` hasta que se define), `createdAt`. Nunca incluye el hash ni tokens.

#### GET `/api/v1/users/me/get`

| Código | Cuándo | `data` |
|---|---|---|
| 200 | Perfil propio | `UserProfileResponse` |
| 401 | Sin token, token inválido, expirado o revocado | `null` |
| 404 | La cuenta del token ya no existe | `null` |

#### PUT `/api/v1/users/me/update`

Cuerpo: `{ "displayName": "Ana Pérez" }`. Se recorta; de 1 a 80 caracteres, sin caracteres de control. Correo y rol no son editables: enviar `email`, `role` u otro miembro desconocido devuelve 400.

| Código | Cuándo | `data` |
|---|---|---|
| 200 | Nombre actualizado | `UserProfileResponse` |
| 400 | Nombre vacío, demasiado largo, con caracteres de control o miembro desconocido | `{ "errors": [...] }` |
| 401 | Sin autenticación válida | `null` |
| 404 | La cuenta del token ya no existe | `null` |

## Mascotas

| Método | Endpoint | Acceso | Spec | Estado |
|---|---|---|---|---|
| POST | `/api/v1/pets/create` | Owner | `pm-040` | Implementado (cerrado) |
| GET | `/api/v1/pets/get-all` | Owner | `pm-041` / `pm-047` | Implementado (cerrado) |
| GET | `/api/v1/pets/get-by-id/{id}` | Owner + propietario | `pm-042` | Implementado (cerrado) |
| PUT | `/api/v1/pets/update/{id}` | Owner + propietario | `pm-043` | Implementado (cerrado) |
| DELETE | `/api/v1/pets/delete/{id}` | Owner + propietario | `pm-044` | Implementado (cerrado) |

### Contrato de mascotas

- Acceso: solo rol `Owner` (política `RequireOwner`). Sin token → 401; rol Business → 403. El propietario sale del token, nunca del cuerpo.
- Cuerpo de `create` y `update` (se rechazan miembros desconocidos como `ownerId`, `id` o `createdAt` con 400):
  `{ "name": "Firulais", "species": "Perro", "breed": "Mestizo", "birthDate": "2020-05-01", "sex": "Male", "photoUrl": "https://..." }`
  - `name` 1–50 y `species` 1–40, obligatorios; `breed` hasta 60, opcional.
  - `birthDate` opcional, `yyyy-MM-dd`, no futura ni de hace más de 50 años.
  - `sex` obligatorio: `Male`, `Female` o `Unknown`. `photoUrl` opcional: URL HTTPS sin credenciales (la carga de archivos llega con pm-079/pm-080).
- Respuesta (`data`): `id`, los campos anteriores, `createdAt` y `updatedAt`. Nunca incluye el propietario.
- Listado `GET /api/v1/pets/get-all?page=1&pageSize=20` (convención de paginación de pm-047, aplicable a todos los listados): solo las mascotas del propietario del token, de la más reciente a la más antigua; cualquier `ownerId` en la consulta se ignora.
  - `page` desde 1 (por defecto 1); `pageSize` por defecto 20 y máximo 50. Un valor menor que 1, mayor que 50 o no numérico devuelve **400** con el nombre del parámetro; no se recorta en silencio.
  - `data`: `{ "items": [...], "page": 1, "pageSize": 20, "totalCount": 3, "totalPages": 1 }`. Una página fuera de rango devuelve `items` vacío con el total real; sin mascotas, `totalCount` y `totalPages` valen 0.
- Códigos: crear **201** (o **409** en el máximo); listar, detalle, actualizar y eliminar **200** (eliminar con `data: null`); datos inválidos **400** con `data.errors`; mascota ajena, inexistente o `{id}` que no es GUID **404**.
- La URL de la foto (`photoUrl`) solo se guarda y se devuelve: el servidor nunca la resuelve ni la descarga. Los textos (`name`, `species`, `breed`) no se sanean ni se codifican (se rechazan solo los caracteres de control): el cliente debe tratarlos como texto al mostrarlos. Cuando exista la carga de imágenes (pm-079/pm-080) se restringirá el origen permitido.
- Límites contra el abuso (pm-045 CA-04): máximo **100 mascotas por propietario** (crear una más devuelve **409** con el wrapper); **60 peticiones por minuto por usuario** en `/api/v1/pets/*` (`PETS_RATE_LIMIT_PER_MINUTE`; al excederlo, **429** con `Retry-After`); y **16 KB** como tamaño máximo del cuerpo de cualquier petición de la API (**413** con el wrapper, también si el cuerpo llega fragmentado). El tope de 100 se comprueba al crear y no es atómico: peticiones simultáneas pueden superarlo por muy poco.
- Eliminar es **físico** (decisión del usuario, 2026-11-08): la mascota deja de existir.

## Matriz de límites de tasa

Todas las respuestas de límite son **429** con el Response Wrapper y la cabecera `Retry-After` (pm-036, pm-034, pm-045). Los cuerpos de toda la API están limitados a 16 KB (**413**).

| Endpoints | Clave | Límite | Configuración |
|---|---|---|---|
| `POST auth/register`, `auth/login`, `auth/refresh` | IP (IPv6 agrupada por /64) | 10 por minuto | `AUTH_RATE_LIMIT_PER_MINUTE` |
| `POST auth/login` | correo (solo su hash) | 5 fallos por 15 minutos; el sexto bloquea 15 minutos | Fijo (`AuthThrottleRules`) |
| `POST auth/refresh` | usuario | 30 por 15 minutos; el 31.º bloquea 15 minutos | Fijo (`AuthThrottleRules`) |
| `POST auth/register`, `auth/login` | instancia (hashing Argon2id) | núcleos de CPU simultáneos y el doble en cola (429 con `Retry-After: 1`); una segunda compuerta interna del hasher responde 503 si la espera supera 5 s | Fijo |
| `POST auth/logout` | usuario | 60 por minuto (política `users`) | `USERS_RATE_LIMIT_PER_MINUTE` |
| `GET users/me/get`, `PUT users/me/update` | usuario | 60 por minuto | `USERS_RATE_LIMIT_PER_MINUTE` |
| `POST pets/create`, `GET pets/get-all`, `GET pets/get-by-id/{id}`, `PUT pets/update/{id}`, `DELETE pets/delete/{id}` | usuario | 60 por minuto; máximo 100 mascotas por propietario | `PETS_RATE_LIMIT_PER_MINUTE` |
| `GET health/live`, `GET health/ready` | — | sin límite: sondas del balanceador (`ready` consulta dependencias; riesgo aceptado y revisable) | — |
| `GET technical/mediator-probe` | — | sin límite: endpoint técnico público temporal (pm-033, P-04) | — |
| OpenAPI y Scalar | — | solo existen en Development | — |

## Reglas de nombrado de rutas

Reglas para todo recurso actual y futuro (pm-007 CA-05). `RouteCatalogTests` (ContractTests) las verifica sobre los controladores reales y comprueba que el catálogo y el código tengan exactamente las mismas rutas:

- Toda ruta de negocio empieza por `/api/v1`; la alternativa no versionada no se mapea. Un cambio incompatible del contrato abre `/api/v2` con su propia decisión.
- Forma: `/api/v1/{recurso}/{acción}` o `/api/v1/{recurso}/{acción}/{id}`. La acción aparece de forma **literal** en la ruta (`create`, `get-all`, `get-by-id`, `update`, `delete`, `login`, ...) y el verbo HTTP la acompaña.
- Recurso y acción en **minúsculas con guiones** (`pets`, `get-by-id`); un recurso es un sustantivo en plural cuando es una colección (`pets`, `users`) o un nombre de capacidad (`auth`, `health`, `technical`).
- Un parámetro de ruta solo puede ser `{id}` (en el código `{id:guid}`), va al final y nunca sustituye a la acción.
- Subrecurso intermedio permitido: solo `me` (`/api/v1/users/me/get`, el perfil del usuario autenticado). Añadir otro exige actualizar esta sección y la prueba.
- Sin barra final, sin dobles barras y sin mayúsculas. Los endpoints técnicos no funcionales (`/api/v1/technical/...`) se marcan como tales en el catálogo.
- Todo endpoint nuevo exige una fila en este catálogo, su valor en la matriz del pipeline (`EndpointTraitTests`) y su fila en `docs/testing/endpoints.md`.

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
