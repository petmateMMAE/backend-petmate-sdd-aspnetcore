# Spec — pm-168 business-get-own

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Creado el 2026-11-18

## 1. Objetivo

`GET /api/v1/businesses/get` devuelve al afiliado los datos completos de su propio negocio, para que la app los cargue al editarlo sin depender de copias locales.

## 2. Dependencias

- [pm-107](../pm-107-business-affiliate-create/spec.md) — business-affiliate-create
- [pm-109](../pm-109-business-profile-update/spec.md) — business-profile-update

## 3. Origen

- Fuentes: Documento v3: RF-30, RF-31, HU-029 (editar negocio y ubicación). Hueco detectado el 2026-11-18 al revisar el alcance: la sesión solo informa el tipo del negocio (pm-087) y `BusinessResponse` únicamente lo devuelven `create`, `update` y `update-location`; el perfil público (pm-119) no incluye la cédula ni se puede consultar sin el id.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.5, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-168#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un usuario autenticado con negocio recibe 200 con el mismo `BusinessResponse` que devuelven `create` y `update` (incluye `id`, cédula, horario, ubicación y contacto). | T186 |
| `CA-02` | La cuenta se toma solo del token; no hay `id` en la ruta, la query ni el cuerpo, de modo que nadie lee el negocio de otra cuenta. | T186 |
| `CA-03` | Un usuario autenticado sin negocio, o cuyo negocio fue dado de baja, recibe 404 con `data: null`. | T186 |
| `CA-04` | Sin token válido responde 401, con el Response Wrapper (`statusCode`, `message`, `data`). | T186 |
| `CA-05` | La consulta tiene límite de peticiones por usuario y queda en el catálogo `docs/api/endpoints.md`. | T186 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T186 | Nueva | Cerrado y aprobado (2026-11-18, aprobación humana en el chat, sin revisión independiente; ver Memoria.md) | Consultar el negocio propio |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- No cambia contratos existentes: reutiliza `BusinessResponse` y la búsqueda por dueño que ya usan la sesión y el resto del panel.
- Decisión pendiente: el límite por usuario de CA-05 (propuesta: el mismo de los demás endpoints del panel del negocio). El plan lo fija.
- La cédula profesional solo se devuelve aquí, al propio dueño; el perfil público (pm-119) sigue sin incluirla.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-18, aprobación en el chat de la sesión.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
