# Spec — pm-142 followup-business-list

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L14 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`GET /api/v1/followups/get-all` lista los seguimientos de la veterinaria con filtros.

## 2. Dependencias

- [pm-138](../pm-138-followup-open/spec.md) — followup-open
- [pm-130](../pm-130-attended-pets-list/spec.md) — attended-pets-list

## 3. Origen

- Fuentes: Maqueta `attended`, `panel`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-142#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Lista los seguimientos de la veterinaria con mascota, estado y próxima revisión. | T160 |
| `CA-02` | Admite filtrar por día y por «en seguimiento» y marca las revisiones atrasadas. | T160 |
| `CA-03` | Solo devuelve los de su propio negocio y usa la convención de paginación. | T160 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T160 | Nueva | Propuesto | Listar los seguimientos del negocio |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
