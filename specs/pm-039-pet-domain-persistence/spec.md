# Spec — pm-039 pet-domain-persistence

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L5 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Modelo Pet, validadores, persistencia e índice por propietario.

## 2. Dependencias

- [pm-012](../pm-012-atlas-connectivity/spec.md) — atlas-connectivity
- [pm-033](../pm-033-authorization-policies/spec.md) — authorization-policies

## 3. Origen

- Fuentes: 03-pet-management/spec.md; tasks T029, T036, T037.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-14 (ver [traceability.md](../traceability.md)).
- Constitución: §11, §4.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-039#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El modelo Pet tiene los campos de la maqueta móvil y sus validadores: nombre (1–50), especie (1–40), raza opcional (hasta 60), fecha de nacimiento opcional (no futura ni de hace más de 50 años), sexo (`Male`, `Female`, `Unknown`) y fotografía opcional (URL HTTPS sin credenciales, hasta 2048). | T029 |
| `CA-02` | La colección tiene un índice por `OwnerId`. | T029 |
| `CA-03` | La eliminación es física: no existe campo de estado y el repositorio borra el documento solo si pertenece al propietario. | T029 |
| `CA-04` | Las pruebas unitarias del modelo pasan. | T036 |
| `CA-05` | Las pruebas de integración contra Atlas de pruebas (índices y filtros por propietario) pasan. | T037 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T029 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | Definir modelo Pet y persistencia |
| T036 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | Pruebas unitarias Pet |
| T037 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | Pruebas de integración Atlas |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- **Campos (decisión del usuario del 2026-11-08):** los de la maqueta móvil: nombre, especie, raza, fecha de nacimiento, sexo y fotografía. Las longitudes, la lista de sexos y la antigüedad máxima de 50 años son decisiones técnicas de T029 y pueden ajustarse.
- **Borrado físico (decisión del usuario del 2026-11-08):** sustituye a la propuesta histórica de eliminación lógica (`IsDeleted`). Por eso se reescribió CA-03 (mismo identificador y propietario) y se ajustaron los criterios de pm-044 y pm-041.
- **Fotografía:** por ahora es una URL HTTPS. La carga de archivos llega con pm-079 y pm-080, y las imágenes nunca se guardan en el disco del contenedor.
- **Propiedad:** todas las operaciones del repositorio filtran por propietario. Una mascota ajena se comporta como inexistente, lo que es la base de la protección BOLA de pm-045.
- **Identificadores:** Guid v7 (como los usuarios). La fecha de nacimiento se guarda como fecha de calendario `yyyy-MM-dd`.
- Plan en [plan.md](plan.md) y evidencia en [checklists/gates.md](checklists/gates.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto; aprobó el alcance el 2026-11-08 al autorizar su implementación y cerró la Task el mismo día.
- Nota: el usuario autorizó implementar T029 con estos campos y borrado físico (decisión del usuario del 2026-11-08); el cierre requiere revisión y aprobación humana. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
