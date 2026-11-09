# Spec — pm-096 clinical-record-provenance

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L8 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Todo registro del carnet guarda quién lo creó, y los creados por un negocio son de solo lectura para el dueño.

## 2. Dependencias

- [pm-039](../pm-039-pet-domain-persistence/spec.md) — pet-domain-persistence
- [pm-045](../pm-045-pet-ownership-bola/spec.md) — pet-ownership-bola

## 3. Origen

- Fuentes: Documento v3: RF-14, RF-15, RF-18; HU-013, HU-014.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §6.5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-096#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El modelo común de registro guarda la mascota, la fecha de creación y el origen (dueño o negocio con su identificador y nombre). | T114 |
| `CA-02` | Un registro con origen de negocio no puede modificarse ni borrarse por el dueño. | T114 |
| `CA-03` | Un registro con origen de dueño solo puede modificarlo o borrarlo su dueño. | T114 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T114 | Nueva | Propuesto | Modelar el origen de los registros clínicos |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Hasta que exista el módulo de negocios el origen de negocio solo se ejercita en pruebas.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
