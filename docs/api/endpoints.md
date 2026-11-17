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
- Toda respuesta incluye la cabecera **`X-Correlation-Id`** (pm-054): se conserva la que envíe el cliente si cumple `[A-Za-z0-9-]{8,64}`; en otro caso el servidor genera un valor de 32 caracteres hexadecimales. Se registra como ámbito de todos los logs de la solicitud. Los intentos de inicio de sesión fallidos, los bloqueos por exceso de intentos y los refresh rechazados se registran como eventos de seguridad (nivel Warning) con un seudónimo de la cuenta (12 caracteres hexadecimales del SHA-256 del correo), nunca el correo, la contraseña ni los tokens. Los logs son JSON y por solicitud contienen solo método, plantilla de ruta (nunca valores), código HTTP y duración: sin cuerpos, cabeceras, consulta, IP, correos, nombres ni tokens.

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
| POST | `/api/v1/auth/forgot-password` | Público | `pm-089` / T107 | Implementado (pendiente de cierre) |
| POST | `/api/v1/auth/reset-password` | Público | `pm-090` / T108 | Implementado (pendiente de cierre) |

### Contratos de autenticación

- Los cuerpos de entrada rechazan miembros desconocidos: enviar p. ej. `role` devuelve 400. El rol siempre es `User` en el registro público.
- Login y Refresh tienen además cuota por cuenta (pm-034, T101/H3): 5 intentos fallidos de login por **par (correo, IP)** en 15 minutos y el sexto bloquea ese par 15 minutos (también con la contraseña correcta, sin revelar si el correo existe), de modo que quien falla desde su IP no deja fuera al propietario que entra desde otra. Un **techo por correo**, independiente de la IP, de 50 intentos en 15 minutos impone una espera de 1 minuto a todas las IPs ante ataques distribuidos. 30 refrescos por usuario en 15 minutos. Las respuestas son 429 con el wrapper y `Retry-After`. El hashing de contraseñas está acotado (tantos a la vez como núcleos y el doble en cola): lo que no cabe se rechaza con 429 y `Retry-After: 1`.
- Register, Login y Refresh están limitados por IP (`AUTH_RATE_LIMIT_PER_MINUTE`, por defecto 10 por minuto): al exceder el límite devuelven 429. El límite se cuenta **en Redis de autenticación**, compartido entre todas las réplicas (T100/H6), además de la barrera en proceso; si Redis no responde las rutas devuelven 503 (falla cerrada). Detrás de un proxy debe configurarse `TRUSTED_PROXIES` (con `REQUIRE_TRUSTED_PROXIES=true` Production no arranca sin ella; sin esa variable solo se advierte al arrancar). Con el ingress de Azure Container Apps hay **un salto** de proxy, por lo que `ForwardLimit = 1` es correcto; si se añade otro (por ejemplo un balanceador delante), hay que revisar la lista y el límite de saltos.
- **Access tokens ligados a la sesión (T098/H7):** el JWT incluye el claim `sid` y deja de valer en cuanto su sesión se revoca (logout o replay), no al expirar. Los tokens emitidos antes del cambio, sin `sid`, valen hasta expirar.
- **Ventana de gracia del refresh (T099/H8):** presentar un refresh token ya consumido dentro de los 10 segundos siguientes devuelve 401 **sin revocar la sesión** (dos pestañas o un reintento); pasada la ventana es un replay: se revoca la sesión y todos sus access tokens.
- **Registro (T102/H4):** el 409 de un correo duplicado sigue revelando que el correo existe. Riesgo **aceptado**: el hash se calcula siempre antes de comprobar la unicidad (tiempos iguales) y el límite por IP acota la enumeración; solo se elimina con verificación por correo, fuera de alcance por ahora.
- `AuthResponse` (en `data` de Register, Login y Refresh): `accessToken`, `tokenType` (`Bearer`), `accessTokenExpiresAt`, `refreshToken`, `refreshTokenExpiresAt`, `user { id, email, role }`. El refresh token se entrega solo ahí.
- Los ejemplos usan valores ficticios; nunca documentar tokens reales.

#### POST `/api/v1/auth/register`

Cuerpo: `{ "displayName": "Ana", "email": "usuario@ejemplo.com", "password": "contraseña-de-12-a-128-caracteres" }`. El nombre es obligatorio (1 a 80 caracteres, pm-085); el celular no se pide en el registro.

| Código | Cuándo | `data` |
|---|---|---|
| 201 | Cuenta creada | `AuthResponse` |
| 400 | Validación (nombre ausente o fuera de 1–80, correo inválido, contraseña fuera de 12–128 caracteres, miembro desconocido) | `{ "errors": [{ "field", "message" }] }` |
| 409 | Correo ya registrado; mensaje genérico «No se pudo completar el registro con los datos proporcionados.» | `null` |
| 429 | Límite de solicitudes | `null` |
| 503 | Atlas o Redis Auth no disponible | `null` |

#### Respuesta de sesión (Register, Login y Refresh)

`data.user` incluye `id`, `email`, `role` (siempre `User`, pm-086), `displayName` y `business` (pm-087: `{ "type": ... }` con el tipo del negocio asociado o `null`; mientras no exista el módulo de negocios es siempre `null`; lo calcula el servidor). El nombre es obligatorio en el registro (pm-085): `displayName` de 1 a 80 caracteres.

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

#### POST `/api/v1/auth/forgot-password` (pm-089)

Cuerpo: `{ "email": "usuario@ejemplo.com" }`. La respuesta es **la misma** exista o no el correo.

| Código | Cuándo | `data` |
|---|---|---|
| 200 | «Si el correo está registrado, recibirás un enlace…»; para una cuenta existente se envía un enlace con un token aleatorio de 256 bits, de un solo uso, que vence en 1 hora (solo se guarda su hash) | `null` |
| 400 | Correo con formato inválido o miembro desconocido | `{ "errors": [...] }` |
| 429 | Más de 3 solicitudes por correo en 15 minutos (cuentan existan o no las cuentas) o límite por IP; `Retry-After` | `null` |
| 503 | Atlas o Redis Auth no disponible | `null` |

El envío usa el puerto `IEmailSender`. Un fallo o espera agotada (5 s) del proveedor no cambia la respuesta ni se revela. Limitación conocida: la solicitud de una cuenta existente tarda un poco más que la de una inexistente (llamada al proveedor), lo que un atacante paciente podría medir.

#### POST `/api/v1/auth/reset-password` (pm-090)

Cuerpo: `{ "token": "<token del enlace>", "newPassword": "<12 a 128 caracteres>" }`

| Código | Cuándo | `data` |
|---|---|---|
| 200 | Contraseña cambiada; el token queda usado y **todas las sesiones y access tokens** de la cuenta quedan revocados | `null` |
| 400 | Validación (contraseña fuera de 12–128), o token inexistente, usado o vencido (mismo mensaje: «El enlace no es válido o ya venció. Solicita uno nuevo.») | `{ "errors": [...] }` o `null` |
| 429 | Límite por IP o hashing saturado | `null` |
| 503 | Atlas o Redis Auth no disponible | `null` |

Correo transaccional (pm-088): proveedor **Resend** (plan gratuito: 3 000 correos al mes y 100 al día, verificado el 2026-11-09 en resend.com/pricing), sin paquetes nuevos. Variables de entorno: `EMAIL_API_KEY` (secreta), `EMAIL_FROM` (remitente de un dominio verificado con SPF y DKIM) y `PASSWORD_RESET_URL_BASE` (URL HTTPS a la que se añade `?token=`). Si falta alguna, no se envía ningún correo y el endpoint responde igual. Hoy no hay dominio verificado, por lo que el envío real no está operativo.

## Autorización (pm-033 / T022)

- **Denegación por defecto.** Todo endpoint exige un usuario autenticado con rol `User` salvo que se marque `[AllowAnonymous]` de forma explícita. Una prueba (`PublicSurfaceTests`) fija la lista cerrada de endpoints públicos.
- **Roles.** Solo existe `User` (pm-086; los tokens anteriores con `Owner` se aceptan como `User` hasta que expiran); no hay rol de administración. El rol sale únicamente del claim `role` de un JWT con firma, vigencia y lista negra ya validadas; un token con rol ausente, desconocido o con distinto uso de mayúsculas se rechaza con 401.
- **Políticas.** `RequireUser` (convención `Require<Capacidad>`), aplicada a mascotas, carnet y perfil; cada recurso se limita además a su propietario.
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
- `UserProfileResponse` (en `data`): `id`, `email`, `role` (`User`), `displayName`, `phone` (`null` si no se agregó, pm-091), `createdAt`. Nunca incluye el hash ni tokens.

#### GET `/api/v1/users/me/get`

| Código | Cuándo | `data` |
|---|---|---|
| 200 | Perfil propio | `UserProfileResponse` |
| 401 | Sin token, token inválido, expirado o revocado | `null` |
| 404 | La cuenta del token ya no existe | `null` |

#### PUT `/api/v1/users/me/update`

Cuerpo: `{ "displayName": "Ana Pérez", "phone": "55 1234 5678" }`. `phone` es opcional (pm-091): 10 dígitos, se admiten espacios, guiones, paréntesis y el prefijo +52, y se guarda como 10 dígitos; ausente, nulo o vacío lo quita (la actualización reemplaza el perfil completo); sin verificación por SMS. El nombre se recorta; de 1 a 80 caracteres, sin caracteres de control. Correo y rol no son editables: enviar `email`, `role` u otro miembro desconocido devuelve 400.

| Código | Cuándo | `data` |
|---|---|---|
| 200 | Nombre actualizado | `UserProfileResponse` |
| 400 | Nombre vacío, demasiado largo, con caracteres de control o miembro desconocido | `{ "errors": [...] }` |
| 401 | Sin autenticación válida | `null` |
| 404 | La cuenta del token ya no existe | `null` |

## Mascotas

| Método | Endpoint | Acceso | Spec | Estado |
|---|---|---|---|---|
| POST | `/api/v1/pets/create` | User | `pm-040` | Implementado (cerrado) |
| GET | `/api/v1/pets/get-all` | User | `pm-041` / `pm-047` | Implementado (cerrado) |
| GET | `/api/v1/pets/get-by-id/{id}` | User + propietario | `pm-042` | Implementado (cerrado) |
| PUT | `/api/v1/pets/update/{id}` | User + propietario | `pm-043` | Implementado (cerrado) |
| DELETE | `/api/v1/pets/delete/{id}` | User + propietario | `pm-044` | Implementado (cerrado) |
| GET | `/api/v1/pets/get-qr/{id}` | User + propietario | `pm-094` | Implementado (cerrado) |
| POST | `/api/v1/pets/regenerate-qr/{id}` | User + propietario | `pm-095` | Implementado (cerrado) |
| POST | `/api/v1/vaccines/create` | User + propietario | `pm-097` | Implementado (pendiente de cierre) |
| GET | `/api/v1/vaccines/get-all` | User + propietario | `pm-097` | Implementado (pendiente de cierre) |
| DELETE | `/api/v1/vaccines/delete/{id}` | User + propietario | `pm-098` | Implementado (pendiente de cierre) |
| POST | `/api/v1/dewormings/create` | User + propietario | `pm-099` | Implementado (pendiente de cierre) |
| GET | `/api/v1/dewormings/get-all` | User + propietario | `pm-099` | Implementado (pendiente de cierre) |
| DELETE | `/api/v1/dewormings/delete/{id}` | User + propietario | `pm-100` | Implementado (pendiente de cierre) |
| POST | `/api/v1/weights/create` | User + propietario | `pm-101` | Implementado (pendiente de cierre) |
| GET | `/api/v1/weights/get-all` | User + propietario | `pm-101` | Implementado (pendiente de cierre) |
| POST | `/api/v1/allergies/create` | User + propietario | `pm-102` | Implementado (pendiente de cierre) |
| GET | `/api/v1/allergies/get-all` | User + propietario | `pm-102` | Implementado (pendiente de cierre) |
| PUT | `/api/v1/allergies/update/{id}` | User + propietario | `pm-103` | Implementado (pendiente de cierre) |
| POST | `/api/v1/businesses/create` | User (negocio de la cuenta) | `pm-107` | Implementado (pendiente de cierre) |
| PUT | `/api/v1/businesses/update-location` | User (negocio de la cuenta) | `pm-108` | Implementado (pendiente de cierre) |
| PUT | `/api/v1/businesses/update` | User (negocio de la cuenta) | `pm-109` | Implementado (pendiente de cierre) |
| DELETE | `/api/v1/businesses/delete` | User (negocio de la cuenta) | `pm-110` | Implementado (pendiente de cierre) |
| POST | `/api/v1/services/create` | User (negocio de la cuenta) | `pm-112` | Implementado (pendiente de cierre) |
| GET | `/api/v1/services/get-all` | User (negocio de la cuenta) | `pm-113` | Implementado (pendiente de cierre) |
| PUT | `/api/v1/services/update/{id}` | User (negocio de la cuenta) | `pm-114` | Implementado (pendiente de cierre) |
| DELETE | `/api/v1/services/delete/{id}` | User (negocio de la cuenta) | `pm-115` | Implementado (pendiente de cierre) |
| GET | `/api/v1/businesses/get-nearby` | Público | `pm-116` | Implementado (pendiente de cierre) |
| GET | `/api/v1/businesses/get-by-id/{id}` | Público | `pm-119` | Implementado (pendiente de cierre) |
| POST | `/api/v1/access-grants/create` | User + propietario | `pm-123` | Implementado (pendiente de cierre) |
| POST | `/api/v1/scans/resolve` | User con negocio | `pm-125` | Implementado (pendiente de cierre) |
| POST | `/api/v1/consultations/create` | User con veterinaria autorizada | `pm-131` | Implementado (pendiente de cierre) |
| POST | `/api/v1/consultations/add-medication` | Veterinaria autora | `pm-132` | Implementado (pendiente de cierre) |
| GET | `/api/v1/consultations/get-all` | User + propietario | `pm-133` | Implementado (pendiente de cierre) |
| GET | `/api/v1/consultations/get-by-id/{id}` | User + propietario | `pm-134` | Implementado (pendiente de cierre) |
| POST | `/api/v1/vaccines/create-by-business` | User con veterinaria autorizada | `pm-135` | Implementado (pendiente de cierre) |
| POST | `/api/v1/dewormings/create-by-business` | User con veterinaria autorizada | `pm-136` | Implementado (pendiente de cierre) |
| GET | `/api/v1/history/get-all` | User + propietario | `pm-137` | Implementado (pendiente de cierre) |
| GET | `/api/v1/scans/get-attended` | User con negocio | `pm-130` | Implementado (pendiente de cierre) |
| POST | `/api/v1/followups/create` | Veterinaria autora | `pm-138` | Implementado (pendiente de cierre) |
| PUT | `/api/v1/followups/update/{id}` | Veterinaria autora | `pm-139` | Implementado (pendiente de cierre) |
| POST | `/api/v1/followups/add-note` | Veterinaria autora | `pm-140` | Implementado (pendiente de cierre) |
| GET | `/api/v1/followups/get-by-consultation/{id}` | User + propietario | `pm-141` | Implementado (pendiente de cierre) |
| GET | `/api/v1/followups/get-all` | Veterinaria | `pm-142` | Implementado (pendiente de cierre) |
| POST | `/api/v1/messages/send` | Participante | `pm-144` | Implementado (pendiente de cierre) |
| GET | `/api/v1/messages/get-all` | Participante | `pm-145` | Implementado (pendiente de cierre) |
| GET | `/api/v1/conversations/get-all` | Participante | `pm-148` | Implementado (pendiente de cierre) |
| POST | `/api/v1/conversations/mark-read` | Participante | `pm-149` | Implementado (pendiente de cierre) |
| POST | `/api/v1/conversations/reopen` | Veterinaria | `pm-147` | Implementado (pendiente de cierre) |
| POST | `/api/v1/pets/upload-photo/{id}` | User + propietario | `pm-080` | Implementado (pendiente de cierre) |
| GET | `/api/v1/pets/get-photo-url/{id}` | User + propietario | `pm-081` | Implementado (pendiente de cierre) |
| DELETE | `/api/v1/pets/delete-photo/{id}` | User + propietario | `pm-080` | Implementado (pendiente de cierre) |
| POST | `/api/v1/businesses/upload-photo` | User (negocio de la cuenta) | `pm-111` | Implementado (pendiente de cierre) |
| DELETE | `/api/v1/businesses/delete-photo` | User (negocio de la cuenta) | `pm-111` | Implementado (pendiente de cierre) |

### Contrato de mascotas

- Acceso: cualquier `User` autenticado (política `RequireUser`). Sin token → 401. El propietario sale del token, nunca del cuerpo.
- Cuerpo de `create` y `update` (se rechazan miembros desconocidos como `ownerId`, `id` o `createdAt` con 400):
  `{ "name": "Firulais", "species": "Perro", "breed": "Mestizo", "birthDate": "2020-05-01", "sex": "Male", "photoUrl": "https://..." }`
  - `name` 1–50 obligatorio; `species` obligatoria y solo `Perro` o `Gato` (lista cerrada); `sex` opcional (`Male`, `Female` o `Unknown`; si falta se guarda `Unknown`); `breed` hasta 60, opcional.
  - `birthDate` opcional, `yyyy-MM-dd`, no futura ni de hace más de 50 años.
  - `photoUrl` opcional: URL HTTPS sin credenciales. Es la forma antigua de poner foto; desde pm-080 se puede **subir** el archivo y entonces `photoUrl` deja de usarse (ver «Contrato de imágenes»).
- Respuesta (`data`): `id`, los campos anteriores, `createdAt` y `updatedAt`. Nunca incluye el propietario.
- Listado `GET /api/v1/pets/get-all?page=1&pageSize=20` (convención de paginación de pm-047, aplicable a todos los listados): solo las mascotas del propietario del token, de la más reciente a la más antigua; cualquier `ownerId` en la consulta se ignora.
  - `page` desde 1 (por defecto 1); `pageSize` por defecto 20 y máximo 50. Un valor menor que 1, mayor que 50 o no numérico devuelve **400** con el nombre del parámetro; no se recorta en silencio.
  - `data`: `{ "items": [...], "page": 1, "pageSize": 20, "totalCount": 3, "totalPages": 1 }`. Una página fuera de rango devuelve `items` vacío con el total real; sin mascotas, `totalCount` y `totalPages` valen 0.
- Códigos: crear **201** (o **409** en el máximo); listar, detalle, actualizar y eliminar **200** (eliminar con `data: null`); datos inválidos **400** con `data.errors`; mascota ajena, inexistente o `{id}` que no es GUID **404**.
- La URL de la foto (`photoUrl`) solo se guarda y se devuelve: el servidor nunca la resuelve ni la descarga. Los textos (`name`, `species`, `breed`) no se sanean ni se codifican (se rechazan solo los caracteres de control): el cliente debe tratarlos como texto al mostrarlos. Una foto **subida** (pm-080) no pasa por `photoUrl`: la guarda el servidor en su propio almacenamiento y manda su origen.
- Límites contra el abuso (pm-045 CA-04): máximo **100 mascotas por propietario** (crear una más devuelve **409** con el wrapper); **60 peticiones por minuto por usuario** en `/api/v1/pets/*` (`PETS_RATE_LIMIT_PER_MINUTE`; al excederlo, **429** con `Retry-After`); y **16 KB** como tamaño máximo del cuerpo de cualquier petición de la API (**413** con el wrapper, también si el cuerpo llega fragmentado). El tope de 100 se comprueba al crear y no es atómico: peticiones simultáneas pueden superarlo por muy poco.
- Eliminar es **físico** (decisión del usuario, 2026-11-08): la mascota deja de existir.

### Contrato de imágenes (`pm-079` a `pm-081`, `pm-111`)

- **Único punto de la API que no es JSON**: el cuerpo es `multipart/form-data` con un solo campo, `file`. Cualquier otro campo se ignora. Sin archivo → **400**.
- **Tipos permitidos**: `image/jpeg`, `image/png` y `image/webp`. El tipo se deduce de los **bytes** del archivo: el nombre y el `Content-Type` que manda el cliente se ignoran, así que un `.jpg` que en realidad es otra cosa responde **400**. La extensión con la que se guarda la decide el servidor.
- **Tamaño**: hasta **2 MB** la foto de una mascota y **1 MB** la de un negocio (`pm-111#CA-02`). Pasarse responde **413**. El límite general de 16 KB del cuerpo no aplica a estas rutas.
- **Dónde se guarda**: Azure Blob Storage, nunca el disco del contenedor. Dos contenedores separados: `pet-photos` es **privado** y `business-photos` es **público de solo lectura**.
- Sin almacenamiento configurado en el entorno (`IMAGES_STORAGE_ACCOUNT` o `IMAGES_STORAGE_CONNECTION`), las rutas de carga responden **503**; el resto de la API funciona igual.

**Foto de una mascota (privada).** Una mascota tiene una sola foto: subir otra la reemplaza.

- `POST pets/upload-photo/{id}` → **200** con `{ "petId", "url", "expiresAt" }`. Mascota ajena o inexistente → **404**, el mismo código para los dos casos.
- `GET pets/get-photo-url/{id}` → **200** con la misma forma. La propiedad se comprueba **antes** de firmar: una mascota ajena responde 404 y no se emite ninguna URL. Una mascota sin foto subida también responde 404.
- `DELETE pets/delete-photo/{id}` → **200** con `data: null`. Quita la foto y borra el archivo.
- `url` es temporal y dura **15 minutos** (`expiresAt` dice cuándo caduca). No existe ninguna URL permanente de la foto de una mascota: sin firma vigente el archivo no se puede leer. Una URL ya emitida no se puede revocar antes de que caduque; la ventana corta es la mitigación.
- La ficha del escaneo (`scans/resolve`) manda en `photoUrl` una URL temporal igual, y solo cuando hay una autorización vigente del dueño.
- Eliminar la mascota borra su foto.

**Foto de un negocio (pública).**

- `POST businesses/upload-photo` → **200** con `{ "photoUrl" }`. Cuenta sin negocio → **403**, como el resto del módulo.
- `DELETE businesses/delete-photo` → **200** con `data: null`.
- `photoUrl` es permanente y la sirve el Blob directo: aparece en `businesses/get-by-id` y en `businesses/get-nearby`, que antes mandaban `null`. Lleva un parámetro `?v=` que cambia al reemplazar la foto, para que nadie siga viendo la anterior desde su caché.
- Dar de baja el negocio borra su foto.

### Negocio afiliado y catálogo (`pm-105` a `pm-115`)

- Acceso: cualquier `User` autenticado. El negocio es **siempre el de la cuenta del token** (no hay identificador de negocio en el cliente): solo el dueño puede ver o cambiar su negocio y su catálogo. Una cuenta tiene como máximo un negocio y no hay aprobación manual: al crearlo aparece de inmediato.
- `POST businesses/create` `{type, name, phone, whatsApp?, address, description?, latitude, longitude, licenseNumber?, schedule?}` → **201** con el negocio. `type`: `Veterinary`, `Store` o `Grooming` (uno solo). `licenseNumber` es obligatoria solo para `Veterinary`, se captura sin validarse y es **privada** (solo la ve su dueño). Teléfonos de 10 dígitos (se admite +52). Una cuenta que ya tiene negocio → **409**. El tipo de negocio de la sesión (`user.business`) se calcula en el servidor al iniciar sesión, registrarse o renovar la sesión (login y refresh dependen de la colección de negocios); no cambia en un access token ya emitido.
- `schedule`: `{ "monday": [{"open":"09:00","close":"18:00"}], ... }` con los días `monday` a `sunday`; cierre posterior a la apertura, hasta 6 franjas por día y sin solaparse; un día ausente o sin franjas es un día cerrado. Las horas son locales de America/Mexico_City; el cierre es exclusivo y no se puede expresar medianoche (`24:00` no es válido): para cerrar al final del día se usa `23:59`.
- `PUT businesses/update-location` `{latitude, longitude}` (latitud −90 a 90, longitud −180 a 180; la geocodificación ocurre en la app). `PUT businesses/update` `{name, description?, phone, whatsApp?, schedule?}` **reemplaza el perfil completo**: omitir `schedule` deja el negocio sin horario (todos los días cerrado) y omitir `description` o `whatsApp` los quita, por lo que el cliente debe reenviar lo que quiera conservar; el tipo **no se puede cambiar** (enviar `type` devuelve **400**: hay que dar de baja el negocio y afiliarlo de nuevo).
- `DELETE businesses/delete` da de baja el negocio y su catálogo (primero los servicios y al final el negocio, idempotente). La cuenta, su rol y sus mascotas siguen funcionando.
- Cuenta sin negocio en estas rutas o en el catálogo → **403** «La cuenta no tiene un negocio asociado.», con `data: null`.
- Catálogo: `POST services/create` `{name, description?, price?}` (**201**; `price` ≥ 0 con hasta 2 decimales y máximo 1 000 000; sin precio se devuelve `null`, nunca 0; sin existencias ni ventas), `GET services/get-all` (paginado con la convención de `pm-047`, más antiguo primero y por identificador), `PUT services/update/{id}`, `DELETE services/delete/{id}`. Servicio de otro negocio o inexistente → **404** idéntico.
- Límites: **60 peticiones por minuto por usuario** (`BUSINESS_RATE_LIMIT_PER_MINUTE`) y 16 KB de cuerpo. Persistencia: un índice único por cuenta y un índice geoespacial `2dsphere` para las consultas por cercanía de los Specs siguientes. La foto del negocio (`pm-111`) se sube aparte; ver «Contrato de imágenes».

### Mapa público, autorizaciones y escaneo (`pm-116` a `pm-129`)

- **Mapa público** (sin cuenta, `AllowAnonymous`): `GET businesses/get-nearby?latitude=&longitude=&radiusKm=5&types=Veterinary,Store&search=&page=&pageSize=` devuelve los negocios activos dentro del radio (5 km por defecto, máximo 50), de más cerca a más lejos (a igualdad, por identificador), paginados con la convención de `pm-047`. Cada elemento: `id`, `name`, `type`, `photoUrl` (la foto pública del local, o nulo si no subió ninguna), `isOpenNow`, `todayHours` y `distanceMeters`. `latitude` y `longitude` son obligatorias; `types` acepta uno o varios valores separados por comas (uno inválido → **400**; sin parámetro, todos); `search` filtra por nombre sin distinguir mayúsculas ni acentos (2 a 50 caracteres, se trata como dato y nunca como expresión) y se combina con tipo y cercanía.
- `GET businesses/get-by-id/{id}`: perfil público con descripción, foto, horario, ubicación, servicios y contacto (`phone`, `whatsApp`; ausente → `null`). **Nunca incluye la cédula profesional.** Negocio inexistente o dado de baja → **404**.
- Límites: **120 solicitudes por minuto por IP** (`PUBLIC_BUSINESS_RATE_LIMIT_PER_MINUTE`), contadas en Redis Auth y compartidas entre réplicas; al excederlo **429** con `Retry-After`. Estas lecturas se guardan en la caché **5 minutos**; cada cambio del negocio o su catálogo la invalida (política en `docs/security/cache-policy.md`). El objetivo de 2 segundos en 4G se mide en la línea base de rendimiento.
- **Autorización de acceso** (dueño): `POST access-grants/create` `{petId, businessId?, data[], durationMinutes}`. `businessId` ausente = cualquier negocio que escanee; `data`: `Vaccines`, `Dewormings`, `Weights`, `Consultations` (las alergias siempre se incluyen); `durationMinutes`: **30, 1440 o 10080**, otro valor → **400**. Mascota ajena o inexistente → **404**. **No existe endpoint para listar ni revocar autorizaciones**: el dueño no las revoca ni ve quién entró; la única forma de cortar el acceso es `pets/regenerate-qr`, que las cancela todas antes de cambiar el QR. Un negocio destinatario inexistente → 400 (`businessId`).
- **Escaneo** (cuenta con negocio): `POST scans/resolve` `{code}` con el token del QR (43 caracteres) o el código corto de 8 (sin distinguir mayúsculas). Cuenta sin negocio → **403**; código desconocido o QR regenerado → **404** con el mismo mensaje; sin una autorización vigente que incluya al negocio → **403 sin datos de la mascota** (conocer el QR no basta); más de **20 intentos por negocio por minuto** o más de 120 por IP → **429** (5 minutos de espera por negocio; impide enumerar códigos). Limitación conocida: un código existente no autorizado responde 403 y uno inexistente 404, lo que permite saber que existe; lo exige el Spec y lo acota el código de 40 bits con el límite por negocio e IP. Con autorización vigente responde **200** con la ficha básica: `photoUrl`, `name`, `species`, `breed`, `ageYears`, `ageMonths` y las alergias activas, y ningún otro campo; `petId` se añade solo para una veterinaria. Una **veterinaria** recibe además, solo de los grupos que alguna autorización vigente permite, `vaccines`, `dewormings`, `weights` y `consultations` (con su receta; hasta 50 por grupo, de solo lectura); un grupo no autorizado no aparece en la respuesta y una tienda o estética nunca los recibe (`pm-127`). Cada acceso concedido deja un registro interno de escaneo (negocio, mascota y fecha) que ni el dueño ni la API pueden consultar; se borra al eliminar la mascota.

### Consultas, seguimiento y chat (`pm-130` a `pm-149`)

- **Operaciones de una veterinaria sobre una mascota ajena** (`consultations/create`, `vaccines/create-by-business`, `dewormings/create-by-business`): solo una cuenta con una **veterinaria** (tienda y estética → **403**) y con una autorización vigente que la incluya (si no, **403**); mascota inexistente → 404. El registro queda en el carnet con origen de negocio (nombre del negocio en «aplicada por», fijado por el servidor) y es de **solo lectura** para el dueño. Fechas: aplicación no futura; próxima dosis posterior; próxima fecha de desparasitación futura.
- **Consultas:** `POST consultations/create` `{petId, reason, diagnosis, weightKg?, medications[]}` (motivo y diagnóstico obligatorios de hasta 500 caracteres, peso de 0.1 a 200 kg con un decimal, receta de hasta **10** medicamentos con `name`, `dose`, `frequency`, `duration` e `instructions?`). El peso de la consulta se queda en la consulta: no se suma al historial de peso. `POST consultations/add-medication` agrega un medicamento (solo la veterinaria autora; el dueño no modifica la receta). El dueño usa `GET consultations/get-all?petId=` (de la más reciente a la más antigua: fecha, veterinaria, motivo y diagnóstico) y `GET consultations/get-by-id/{id}` (con la receta completa); ajeno o inexistente → **404**.
- **Historial:** `GET history/get-all?petId=&types=Consultation,Vaccine,Deworming,Weight` mezcla consultas, vacunas, desparasitaciones y pesos de la más reciente a la más antigua (hasta 200 registros por tipo), con paginación de `pm-047`.
- **Mascotas atendidas:** `GET scans/get-attended?period=day|week` lista, para el negocio, las mascotas escaneadas con foto, nombre y fecha (sin datos clínicos ni acceso a la ficha). «Hoy» y «semana» (últimos 7 días) se cuentan en America/Mexico_City. El nombre y la foto son los del momento del escaneo.
- **Seguimiento:** `POST followups/create` `{consultationId, nextReviewInDays?}` (3, 7 o 15; solo la veterinaria autora de la consulta; una consulta tiene un solo seguimiento → **409**); abre el seguimiento en `Treating` y **crea su conversación** con el dueño y la veterinaria. `PUT followups/update/{id}` `{status, nextReviewInDays?}` (`Treating`, `Improving`, `Discharged`; dar de alta elimina la revisión y **cierra la conversación**). `POST followups/add-note` `{followupId, text}` (hasta 500 caracteres, en orden cronológico). El dueño lee con `GET followups/get-by-consultation/{id}`. La veterinaria lista con `GET followups/get-all?reviewDate=&onlyActive=` (marca `isOverdue` si la revisión ya pasó y no está dado de alta; solo los de su negocio).
- **Chat:** conversación de exactamente dos participantes (el dueño y la cuenta de la veterinaria); cualquier otro usuario recibe **404**. `POST messages/send` `{conversationId, text}` (hasta 1000 caracteres; conversación cerrada → **409**), `GET messages/get-all?conversationId=&after=<messageId>&pageSize=` (en orden; `after` pide solo los posteriores; incluye los avisos del sistema del seguimiento; `hasMore`), `GET conversations/get-all` (con último mensaje y mensajes sin leer, paginado), `POST conversations/mark-read` `{conversationId, upToMessageId?}` (cada participante lleva su propio estado de lectura) y `POST conversations/reopen` `{conversationId}` (solo la veterinaria; el seguimiento vuelve a `Treating`; el dueño → **403**).
- **Pendientes de este bloque:** las fotos del chat (`pm-146`) dependen de Blob y el tiempo real (`pm-150`) está propuesto para sustituirse por la consulta al abrir (`pm-150#CA-04`); ver `specs/pm-150-*/plan.md`.

### Carnet de salud (`pm-096` a `pm-103`)

- Acceso: cualquier `User` autenticado; el dueño sale del token. Mascota o registro ajenos o inexistentes → **404** idéntico. Los listados exigen `petId` (400 si falta) y usan la convención de paginación de `pm-047` (`page`, `pageSize` 20 por defecto y 50 máximo), con la forma `PagedResponse`.
- Cada registro devuelve `origin` (`type`: `Owner` o `Business`; `businessId` y `businessName` solo si lo creó un negocio). Lo creado por un negocio es de solo lectura: borrar o editar responde **403** «No se puede modificar ni borrar: lo registró el negocio.».
- Tope de **200** registros por mascota y tipo (crear uno más → **409**). Fechas `yyyy-MM-dd`; ninguna fecha de aplicación o medición puede ser futura. Se rechazan miembros desconocidos (`ownerId`, `origin`, …).
- Vacunas: `POST vaccines/create` `{petId, appliedOn, name, batch?, appliedBy?, nextDoseOn?}` (`nextDoseOn` posterior a `appliedOn`); `GET vaccines/get-all?petId=` (más reciente primero); `DELETE vaccines/delete/{id}`.
- Desparasitaciones: `POST dewormings/create` `{petId, appliedOn, product, type?, appliedBy?, nextOn?}` (`type` `Internal`/`External`; `nextOn` futura); `GET dewormings/get-all?petId=`; `DELETE dewormings/delete/{id}`.
- Pesos: `POST weights/create` `{petId, measuredOn, kilograms}` (mayor que 0, hasta 200, un decimal); `GET weights/get-all?petId=` (más reciente primero).
- Alergias y condiciones: `POST allergies/create` `{petId, name, kind, notes?}` (`kind` `Allergy`/`Condition`; nace `Active`); `GET allergies/get-all?petId=` (activas primero); `PUT allergies/update/{id}` `{name, kind, notes?, status}` (`status` `Active`/`Resolved`, obligatorio).
- Límites: **60 peticiones por minuto por usuario** en estas rutas (`CARNET_RATE_LIMIT_PER_MINUTE`) y 16 KB de cuerpo.
- `PUT pets/update/{id}` sin `sex` deja el sexo en `Unknown` (reemplazo completo). Una mascota guardada antes con otra especie se sigue leyendo (no rompe el listado) pero no se puede guardar de nuevo sin cambiarla a Perro o Gato.
- QR permanente (`pm-093` a `pm-095`): cada mascota recibe al crearse un `qrToken` (256 bits aleatorios, base64url de 43 caracteres) y un `qrCode` corto de 8 caracteres sin símbolos ambiguos; ambos son únicos y no aparecen en `PetResponse`. `GET /api/v1/pets/get-qr/{id}` los devuelve (`data`: `petId`, `qrToken`, `qrCode`) y `POST /api/v1/pets/regenerate-qr/{id}` genera otros y deja inválidos los anteriores. Ambos responden con `Cache-Control: no-store`, **404** si la mascota es ajena o inexistente, y el token nunca se escribe en los logs. Las mascotas anteriores al QR reciben uno al leerse por primera vez.

## Matriz de límites de tasa

Todas las respuestas de límite son **429** con el Response Wrapper y la cabecera `Retry-After` (pm-036, pm-034, pm-045). Los cuerpos de toda la API están limitados a 16 KB (**413**).

| Endpoints | Clave | Límite | Configuración |
|---|---|---|---|
| `POST auth/register`, `auth/login`, `auth/refresh` | IP (IPv6 agrupada por /64), compartido entre réplicas en Redis Auth; espera de 1 minuto al excederlo | 10 por minuto | `AUTH_RATE_LIMIT_PER_MINUTE` |
| `POST auth/login` | par (correo, IP), solo su hash | 5 fallos por 15 minutos; el sexto bloquea 15 minutos | Fijo (`AuthThrottleRules`) |
| `POST auth/login` | correo (solo su hash), todas las IPs | techo de 50 intentos por 15 minutos; al superarlo espera de 1 minuto | Fijo (`AuthThrottleRules`) |
| `POST auth/refresh` | usuario | 30 por 15 minutos; el 31.º bloquea 15 minutos | Fijo (`AuthThrottleRules`) |
| `POST auth/register`, `auth/login` | instancia (hashing Argon2id) | núcleos de CPU simultáneos y el doble en cola (429 con `Retry-After: 1`); una segunda compuerta interna del hasher responde 503 si la espera supera 5 s | Fijo |
| `POST auth/logout` | usuario | 60 por minuto (política `users`) | `USERS_RATE_LIMIT_PER_MINUTE` |
| `GET users/me/get`, `PUT users/me/update` | usuario | 60 por minuto | `USERS_RATE_LIMIT_PER_MINUTE` |
| `POST pets/create`, `GET pets/get-all`, `GET pets/get-by-id/{id}`, `PUT pets/update/{id}`, `DELETE pets/delete/{id}`, `GET pets/get-qr/{id}`, `POST pets/regenerate-qr/{id}` | usuario | 60 por minuto; máximo 100 mascotas por propietario | `PETS_RATE_LIMIT_PER_MINUTE` |
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
