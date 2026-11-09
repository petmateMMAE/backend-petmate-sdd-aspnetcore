# Spec — pm-102 allergy-create-list

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L9 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

El dueño registra y consulta las alergias y condiciones de su mascota con `POST /api/v1/allergies/create` y `GET /api/v1/allergies/get-all`.

## 2. Dependencias

- [pm-096](../pm-096-clinical-record-provenance/spec.md) — clinical-record-provenance

## 3. Origen

- Fuentes: Documento v3: RF-17, HU-016.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-102#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Se registra una alergia o condición con nombre, tipo (alergia o condición), notas y estado activa. | T120 |
| `CA-02` | El listado distingue las activas de las superadas. | T120 |
| `CA-03` | Una mascota ajena o inexistente responde 404. | T120 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T120 | Nueva | Propuesto | Registrar y listar las alergias y condiciones de una mascota |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Las alergias activas son el dato que ven todos los tipos de negocio; la categoría fina (alimentaria, medicamento, ambiental, piel) de la maqueta se decide en `plan.md`.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
