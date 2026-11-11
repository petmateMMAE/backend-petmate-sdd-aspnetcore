# Spec — pm-138 followup-open

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L13 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/followups/create` abre el seguimiento de una consulta con estado y próxima revisión.

## 2. Dependencias

- [pm-131](../pm-131-consultation-create/spec.md) — consultation-create

## 3. Origen

- Fuentes: Maqueta `visit`, `consultForm`; decisión del usuario del 2026-11-09 (el seguimiento entra).
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-138#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La veterinaria autora abre un seguimiento con estado En tratamiento y una próxima revisión opcional (3, 7 o 15 días). | T156 |
| `CA-02` | Una consulta tiene como máximo un seguimiento. | T156 |
| `CA-03` | Solo la veterinaria que registró la consulta puede abrirlo. | T156 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T156 | Nueva | Verificado (2026-11-10; aprobación humana en el chat; sin revisión independiente de este bloque) | Abrir el seguimiento de una consulta |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
