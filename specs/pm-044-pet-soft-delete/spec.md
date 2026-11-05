# Spec — pm-044 pet-soft-delete

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

`DELETE /api/v1/pets/delete/{id}` con eliminación lógica y `data:null`.

## 2. Dependencias

- [pm-039](../pm-039-pet-domain-persistence/spec.md) — pet-domain-persistence

## 3. Origen

- Fuentes: 03-pet-management/spec.md CA-005; task T034.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-14 (ver [traceability.md](../traceability.md)).
- Constitución: §11, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-044#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un Owner elimina lógicamente una mascota propia y la respuesta lleva `data: null`. [origen: 03-pet-management CA-005] | T034 |
| `CA-02` | La mascota eliminada deja de aparecer en listados normales. | T034 |
| `CA-03` | No se borra físicamente sin política de retención aprobada. | T034 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T034 | Histórica | Propuesto | Eliminar mascota |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
