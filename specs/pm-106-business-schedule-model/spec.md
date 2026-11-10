# Spec — pm-106 business-schedule-model

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L7 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

El horario del negocio se guarda estructurado por día y permite calcular si está abierto en este momento.

## 2. Dependencias

- [pm-105](../pm-105-business-persistence-model/spec.md) — business-persistence-model

## 3. Origen

- Fuentes: Documento v3: RF-22; maqueta `bizProfile`; decisión del usuario del 2026-11-09 (el horario lo define el negocio y es público).
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.1.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-106#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El horario tiene, por día de la semana, cero o más franjas de apertura y cierre. | T124 |
| `CA-02` | Una franja válida tiene cierre posterior a la apertura y no se solapa con otra del mismo día. | T124 |
| `CA-03` | El estado abierto ahora se calcula con la zona horaria America/Mexico_City. | T124 |
| `CA-04` | Un día sin franjas se interpreta como cerrado. | T124 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T124 | Nueva | Propuesto | Modelar el horario estructurado y el estado abierto ahora |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
