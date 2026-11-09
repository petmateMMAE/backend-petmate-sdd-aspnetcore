# Spec — pm-101 weight-create-list

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L9 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

El dueño registra y consulta el peso de su mascota con `POST /api/v1/weights/create` y `GET /api/v1/weights/get-all`.

## 2. Dependencias

- [pm-096](../pm-096-clinical-record-provenance/spec.md) — clinical-record-provenance

## 3. Origen

- Fuentes: Documento v3: RF-16, HU-015.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-101#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Se registra un peso en kilogramos con un decimal, mayor que cero, y una fecha que no sea futura. | T119 |
| `CA-02` | El listado va del registro más reciente al más antiguo. | T119 |
| `CA-03` | Una mascota ajena o inexistente responde 404. | T119 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T119 | Nueva | Propuesto | Registrar y listar el peso de una mascota |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- El peso de una consulta veterinaria se sumará a este historial cuando exista el módulo de consultas.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
