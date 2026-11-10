# Spec — pm-043 pet-update

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

`PUT /api/v1/pets/update/{id}` de mascota propia con campos permitidos.

## 2. Dependencias

- [pm-039](../pm-039-pet-domain-persistence/spec.md) — pet-domain-persistence

## 3. Origen

- Fuentes: 03-pet-management/spec.md CA-004; task T033.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-14 (ver [traceability.md](../traceability.md)).
- Constitución: §11, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-043#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un Owner actualiza solo una mascota propia y únicamente campos permitidos. [origen: 03-pet-management CA-004] | T033 |
| `CA-02` | Las validaciones devuelven 400 con el formato estándar. | T033 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T033 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | Actualizar mascota |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- **Implementación (aprobado por el usuario el 2026-11-08 (opción A)):** endpoint con política `RequireOwner`; el propietario sale del token; mascota ajena o inexistente responde 404. Contrato en [`docs/api/endpoints.md`](../../docs/api/endpoints.md) (sección Mascotas). Pruebas con `[Trait("Endpoint", "pets-update")]` en `PetEndpointsTests` y evidencia en [checklists/gates.md](checklists/gates.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto; aprobó el alcance el 2026-11-08 al autorizar su implementación y cerró la Task el mismo día.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
