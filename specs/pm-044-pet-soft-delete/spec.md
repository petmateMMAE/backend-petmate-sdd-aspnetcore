# Spec — pm-044 pet-soft-delete

> Estado: En progreso | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

`DELETE /api/v1/pets/delete/{id}` con borrado físico y `data:null`.

## 2. Dependencias

- [pm-039](../pm-039-pet-domain-persistence/spec.md) — pet-domain-persistence

## 3. Origen

- Fuentes: 03-pet-management/spec.md CA-005; task T034.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-14 (ver [traceability.md](../traceability.md)).
- Constitución: §11, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-044#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un Owner elimina físicamente una mascota propia y la respuesta lleva `data: null`. [origen: 03-pet-management CA-005; borrado físico por decisión del usuario del 2026-11-08] | T034 |
| `CA-02` | La mascota eliminada deja de existir: no aparece en el listado y su detalle responde 404. | T034 |
| `CA-03` | Solo el propietario puede eliminarla; una mascota ajena o inexistente responde 404 y no se borra nada. | T034 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T034 | Histórica | En progreso | Eliminar mascota |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- **Borrado físico (decisión del usuario del 2026-11-08):** sustituye a la eliminación lógica del slice histórico. El identificador y el slug `pet-soft-delete` se conservan porque son inmutables (Constitución §12.3). Los criterios se reescribieron con su mismo identificador y propietario (T034).

- **Implementación (aprobado por el usuario el 2026-11-08 (opción A)):** endpoint con política `RequireOwner`; el propietario sale del token; mascota ajena o inexistente responde 404. Contrato en [`docs/api/endpoints.md`](../../docs/api/endpoints.md) (sección Mascotas). Pruebas con `[Trait("Endpoint", "pets-delete")]` en `PetEndpointsTests` y evidencia en [checklists/gates.md](checklists/gates.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
