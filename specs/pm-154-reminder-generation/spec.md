# Spec — pm-154 reminder-generation

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

El servidor calcula el recordatorio de cada próxima dosis o fecha registrada en el carnet.

## 2. Dependencias

- [pm-097](../pm-097-vaccine-create-list/spec.md) — vaccine-create-list
- [pm-099](../pm-099-deworming-create-list/spec.md) — deworming-create-list

## 3. Origen

- Fuentes: Documento v3: RF-19, HU-018; decisión del usuario del 2026-11-09 (push desde el servidor).
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-154#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Al registrar o cambiar una próxima dosis o fecha se programa un recordatorio con 3 días de anticipación por defecto. | T172 |
| `CA-02` | Al cambiar o borrar la fecha, el recordatorio se reprograma o se cancela. | T172 |
| `CA-03` | La generación es idempotente. | T172 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T172 | Nueva | Aprobada para iniciar (2026-11-10, aprobación humana; pendiente de plan.md) | Calcular los recordatorios de vacunas y desparasitaciones |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
