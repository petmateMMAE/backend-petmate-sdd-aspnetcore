# Spec — pm-115 service-delete

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L11 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`DELETE /api/v1/services/delete/{id}` elimina un servicio del negocio propio.

## 2. Dependencias

- [pm-112](../pm-112-service-create/spec.md) — service-create

## 3. Origen

- Fuentes: Documento v3: RF-32, HU-030.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-115#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El afiliado borra un servicio propio. | T133 |
| `CA-02` | El servicio deja de aparecer en el perfil público. | T133 |
| `CA-03` | Un servicio de otro negocio o inexistente responde 404. | T133 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T133 | Nueva | Propuesto | Borrar un servicio del negocio propio |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
