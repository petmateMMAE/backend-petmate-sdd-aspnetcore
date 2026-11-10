# Spec — pm-139 followup-update

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L14 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`PUT /api/v1/followups/update/{id}` cambia el estado y la próxima revisión.

## 2. Dependencias

- [pm-138](../pm-138-followup-open/spec.md) — followup-open

## 3. Origen

- Fuentes: Maqueta `visit`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-139#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La veterinaria autora cambia el estado entre En tratamiento, Mejorando y Dado de alta. | T157 |
| `CA-02` | Marcar Dado de alta elimina la próxima revisión. | T157 |
| `CA-03` | Solo la veterinaria autora puede cambiarlo. | T157 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T157 | Nueva | Propuesto | Cambiar el estado y la revisión del seguimiento |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
