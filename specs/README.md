# Índice y roadmap SDD — PetMate Backend

> Estado inicial del roadmap: solución vacía. El avance real de cada slice se registra en la tabla inferior; ningún slice se cierra sin evidencia y aprobación humana.

## Primer hito — Autenticación + perfil Owner + siete endpoints funcionales

El objetivo del primer hito es entregar una API ejecutable y verificable con:

**Autenticación adicional a los siete endpoints funcionales:**

1. `POST /api/v1/auth/register`
2. `POST /api/v1/auth/login`
3. `POST /api/v1/auth/logout`
4. `POST /api/v1/auth/refresh` — adicional, necesaria para mantener sesiones al expirar el Access Token.

**Siete endpoints funcionales:**

| # | Método | Endpoint | Descripción |
|---:|---|---|---|
| 1 | GET | `/api/v1/users/me/get` | Obtener perfil del usuario autenticado. |
| 2 | PUT | `/api/v1/users/me/update` | Actualizar perfil del usuario autenticado. |
| 3 | POST | `/api/v1/pets/create` | Crear mascota. |
| 4 | GET | `/api/v1/pets/get-all` | Listar mascotas del Owner autenticado. |
| 5 | GET | `/api/v1/pets/get-by-id/{id}` | Obtener detalle de una mascota propia. |
| 6 | PUT | `/api/v1/pets/update/{id}` | Actualizar mascota propia. |
| 7 | DELETE | `/api/v1/pets/delete/{id}` | Eliminar lógicamente una mascota propia. |

Todas las rutas mantienen versión y acción literal. Las rutas son una especificación inicial acordada y deben permanecer sincronizadas con `docs/api/endpoints.md`.

## Slices iniciales y tareas globales

| Slice | Alcance | Tareas | Estado |
|---|---|---:|---|
| `00-foundation` | Solución, capas, DI, Mediator/CQRS, configuración, wrapper, OpenAPI + Scalar, Atlas, Redis, contenedores y CI base | T001–T012 | Aprobado y en progreso (T001–T007 cerrados; T008–T009 cerradas; T010–T011 cerradas; T012 implementada localmente (pendiente de decisiones)) |
| `01-initial-authentication` | Register, Login, Refresh, Logout, Argon2id, JWT, blacklist, sesiones y roles | T013–T025 | Aprobado; T013–T021 cerradas; T022–T025 pendientes |
| `02-owner-profile` | GET/PUT de perfil autenticado | T026–T028 | Propuesto |
| `03-pet-management` | Crear, listar, consultar, actualizar y eliminar mascota con autorización de propiedad | T029–T040 | Propuesto |
| `04-first-milestone-closure` | Validación end-to-end de los 11 endpoints, gates integrados y aprobación del hito | T041 | Propuesto |

**Hito inicial:** El primer hito se considera cumplido al terminar T041**, y solo si los cuatro endpoints de autenticación y los siete endpoints funcionales están operativos, integrados, probados y pasan los gates. No basta con compilar ni con terminar la última tarea de código.

`T041` está definido en `04-first-milestone-closure/task.md` y no significa que PetMate Backend esté terminado. Quedan módulos posteriores.

## Funcionalidades posteriores identificadas en el documento del producto

- Verificación de correo, recuperación y cambio de contraseña, eliminación de cuenta.
- Onboarding/verificación de negocios Business y administración de perfiles.
- Vacunas, desparasitaciones, peso, alergias e historial médico.
- QR, autorizaciones, vigencia y revocación de acceso.
- Búsqueda geográfica y catálogo de negocios/servicios.
- Atenciones veterinarias, diagnósticos, recetas, tratamientos y seguimiento.
- Conversaciones y fotografías de seguimiento.
- Servicios, conceptos de cobro e importes.
- Recordatorios y notificaciones push.
- Almacenamiento de imágenes en Blob Storage y distribución CDN.
- Observabilidad, optimización, despliegue Azure y escalamiento horizontal.

Cada módulo necesita un spec específico. Esta lista es un backlog derivado del documento funcional, no una afirmación de que los requisitos estén completamente definidos.

## Reglas de actualización

- Actualiza este índice al crear, aprobar, bloquear o cerrar un slice.
- No dupliques aquí los requisitos detallados ni las tablas completas de endpoints.
- Mantén una sola secuencia global de IDs de tareas; no reutilices T001–T041.
- No marques slice como Cerrado sin gates y aprobación humana.
