# Spec — pm-156 reminder-list

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L11 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`GET /api/v1/reminders/get-all` lista lo próximo de las mascotas del dueño.

## 2. Dependencias

- [pm-154](../pm-154-reminder-generation/spec.md) — reminder-generation

## 3. Origen

- Fuentes: Maqueta `reminders`; documento v3: RF-19.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-156#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Lista próximas vacunas, desparasitaciones y revisiones de las mascotas propias, por cercanía. | T174 |
| `CA-02` | Incluye el estado de lectura. | T174 |
| `CA-03` | Solo devuelve datos propios. | T174 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T174 | Nueva | Propuesto | Listar los avisos de las mascotas |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
