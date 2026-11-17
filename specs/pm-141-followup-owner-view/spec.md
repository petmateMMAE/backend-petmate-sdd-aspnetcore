# Spec — pm-141 followup-owner-view

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L14 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`GET /api/v1/followups/get-by-consultation/{id}` muestra al dueño el estado, la revisión y las notas.

## 2. Dependencias

- [pm-138](../pm-138-followup-open/spec.md) — followup-open

## 3. Origen

- Fuentes: Documento v3: HU-017; maqueta `consult`, `visit`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-141#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El dueño ve estado, próxima revisión y notas del seguimiento de su consulta. | T159 |
| `CA-02` | El seguimiento de una consulta ajena responde 404. | T159 |
| `CA-03` | Es de solo lectura. | T159 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T159 | Nueva | Verificado (2026-11-10; aprobación humana en el chat; sin revisión independiente de este bloque) | Consultar el seguimiento de una consulta propia |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
