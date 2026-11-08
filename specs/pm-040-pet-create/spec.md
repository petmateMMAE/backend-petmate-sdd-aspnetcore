# Spec — pm-040 pet-create

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

`POST /api/v1/pets/create` con propietario tomado del token.

## 2. Dependencias

- [pm-039](../pm-039-pet-domain-persistence/spec.md) — pet-domain-persistence

## 3. Origen

- Fuentes: 03-pet-management/spec.md CA-001; task T030.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-14 (ver [traceability.md](../traceability.md)).
- Constitución: §11, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-040#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un Owner crea una mascota válida. [origen: 03-pet-management CA-001] | T030 |
| `CA-02` | El propietario se toma del token y nunca del cuerpo. | T030 |
| `CA-03` | La respuesta de creación usa el wrapper con 201. | T030 |
| `CA-04` | Datos inválidos o miembros no permitidos (p. ej. `ownerId`) devuelven 400 con el campo inválido y no crean nada. [añadido con aprobación del usuario, 2026-11-08] | T030 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T030 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | Crear mascota |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- **Implementación (aprobado por el usuario el 2026-11-08 (opción A)):** endpoint con política `RequireOwner`; el propietario sale del token; mascota ajena o inexistente responde 404. Contrato en [`docs/api/endpoints.md`](../../docs/api/endpoints.md) (sección Mascotas). Pruebas con `[Trait("Endpoint", "pets-create")]` en `PetEndpointsTests` y evidencia en [checklists/gates.md](checklists/gates.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
