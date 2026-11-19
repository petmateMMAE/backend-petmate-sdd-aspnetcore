# Política de clasificación de datos y caché (pm-051 / T062)

> Fecha: 2026-11-08. La infraestructura (`ICacheStore`, `RedisCacheStore`, `CacheKey`) existe desde pm-050; esta política decide **qué** puede guardar.

## 1. Clasificación de datos (CA-01)
| Clase | Definición | Ejemplos en PetMate | ¿Cacheable? |
|---|---|---|---|
| Público | Igual para todos y sin dato personal | Catálogos futuros (razas, especies) | Sí, con TTL |
| Privado | Pertenece a un usuario pero no identifica a una persona por sí solo | Listado de mascotas de un propietario | No por defecto |
| Personal | Identifica o describe a una persona | Correo, nombre visible, perfil, mascotas con nombre | No |
| Sensible | Credenciales, tokens, material de sesión, datos clínicos | Contraseña, hash, access/refresh token, `jti`, datos clínicos futuros | **Nunca** |

## 2. Reglas (CA-02)
1. Los datos personales, clínicos y de autenticación **no se cachean** salvo decisión documentada y aprobada por Miguel.
2. Un dato público solo se cachea con TTL (1 s a 1 h, obligatorio) y clave opaca `petmate:cache:v1:{área}:{id}`.
3. La caché es una optimización: ante falla se continúa sin ella (`ICacheStore` no propaga excepciones). Nunca es fuente de verdad ni se usa para autenticación o autorización (eso vive en Redis Auth).
4. Se invalida (`Remove`) al cambiar el dato de origen.

## 3. Criterio por endpoint (CA-03)
| Endpoint | Clase | ¿Cacheable? | TTL | Motivo |
|---|---|---|---|---|
| `POST /auth/register`, `/auth/login`, `/auth/refresh`, `/auth/logout` | Sensible | No | — | Tokens y credenciales |
| `GET/PUT /users/me/*` | Personal | No | — | Perfil de una persona |
| `POST/PUT/DELETE /pets/*` | Privado/personal | No | — | Escrituras y datos del propietario |
| `GET /pets/get-all`, `/pets/get-by-id/{id}` | Privado/personal | No | — | Datos por propietario con nombre; el índice por propietario ya los sirve rápido (pm-052) |
| `GET /health/*`, `GET /technical/mediator-probe` | Público técnico | No | — | Deben reflejar el estado real |
| `GET /businesses/get-nearby`, `GET /businesses/get-by-id/{id}` | Público | **Sí** (pm-121) | 5 minutos | Datos públicos de negocios (nombre, tipo, horario, ubicación, servicios y contacto público); nunca la cédula ni datos de personas. Cada cambio de un negocio, sus servicios o su foto invalida la caché (cambio de versión) y el TTL solo acota un dato viejo si la invalidación falla. «Abierto ahora» no se guarda: se calcula en cada respuesta. Las coordenadas de la consulta se redondean a 3 decimales. Un fallo de la caché no afecta la respuesta. |
| `POST /access-grants/create`, `POST /scans/resolve` | Privado/sensible | No | — | Autorizaciones y fichas de mascotas: datos del propietario y de la mascota |
| `/consultations/*`, `/history/*`, `/followups/*`, `/messages/*`, `/conversations/*`, `GET /scans/get-attended` | Sensible/personal | **Nunca** | — | Datos clínicos y conversaciones privadas |

**Resultado actual: solo las lecturas públicas de negocios usan la caché** (pm-121, ampliación de pm-051); ningún dato personal, clínico ni de autenticación se cachea. Un endpoint nuevo cacheable (p. ej. catálogos) necesita una fila aquí, su clase, TTL justificado y la prueba `CachePolicyTests` actualizada.

## 4. Verificación
`CachePolicyTests` (SecurityTests) comprueba que el código de producción no consume `ICacheStore` fuera de su definición y registro, y que este documento clasifica cada ruta del catálogo.
