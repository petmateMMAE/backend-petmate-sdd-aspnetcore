# Spec — pm-132 prescription-items

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L13 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Una consulta admite una receta con varios medicamentos.

## 2. Dependencias

- [pm-131](../pm-131-consultation-create/spec.md) — consultation-create

## 3. Origen

- Fuentes: Documento v3: RF-35, HU-033.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-132#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Cada medicamento tiene nombre, dosis, frecuencia, duración e indicaciones. | T150 |
| `CA-02` | La receta admite varios medicamentos hasta un máximo fijado en el plan. | T150 |
| `CA-03` | El dueño no puede modificar la receta. | T150 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T150 | Nueva | Verificado (2026-11-10; aprobación humana en el chat; sin revisión independiente de este bloque) | Agregar medicamentos a la receta de una consulta |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
