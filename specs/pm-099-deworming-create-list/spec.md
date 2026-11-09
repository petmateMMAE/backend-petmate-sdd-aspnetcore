# Spec — pm-099 deworming-create-list

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L9 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

El dueño registra y consulta las desparasitaciones de su mascota con `POST /api/v1/dewormings/create` y `GET /api/v1/dewormings/get-all`.

## 2. Dependencias

- [pm-096](../pm-096-clinical-record-provenance/spec.md) — clinical-record-provenance

## 3. Origen

- Fuentes: Documento v3: RF-15, HU-014.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-099#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Se registra una desparasitación con fecha y producto obligatorios, y tipo (interna o externa), aplicada por y próxima fecha opcionales. | T117 |
| `CA-02` | La próxima fecha debe ser futura. | T117 |
| `CA-03` | El listado devuelve las desparasitaciones de una mascota propia ordenadas por fecha, con la próxima fecha. | T117 |
| `CA-04` | Una mascota ajena o inexistente responde 404. | T117 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T117 | Nueva | Propuesto | Registrar y listar las desparasitaciones de una mascota |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
