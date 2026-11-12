# Spec — pm-109 business-profile-update

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`PUT /api/v1/businesses/update` edita el nombre, la descripción, los teléfonos y el horario del negocio propio.

## 2. Dependencias

- [pm-107](../pm-107-business-affiliate-create/spec.md) — business-affiliate-create

## 3. Origen

- Fuentes: Documento v3: RF-30, HU-029; maqueta `bizEdit`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-109#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El afiliado edita nombre, descripción, teléfono, WhatsApp y horario. | T127 |
| `CA-02` | El tipo de negocio no se puede cambiar y la solicitud que lo intente devuelve 400. | T127 |
| `CA-03` | Solo el dueño del negocio puede editarlo. | T127 |
| `CA-04` | Los cambios se reflejan en las consultas públicas. | T127 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T127 | Nueva | Verificado (2026-11-10; revisión independiente sin hallazgos bloqueantes y aprobación humana en el chat) | Actualizar los datos del negocio propio |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Cambiar el tipo exige dar de baja el negocio y afiliarlo de nuevo.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
