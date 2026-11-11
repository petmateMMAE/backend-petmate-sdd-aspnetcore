# Spec — pm-162 account-delete-user-data

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L11 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Al eliminar la cuenta se borran las mascotas y todo lo asociado a ellas.

## 2. Dependencias

- [pm-161](../pm-161-account-delete-request/spec.md) — account-delete-request
- [pm-104](../pm-104-pet-delete-cascade/spec.md) — pet-delete-cascade
- [pm-122](../pm-122-access-grant-model/spec.md) — access-grant-model

## 3. Origen

- Fuentes: Documento v3: RNF-08, HU-006.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-162#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Se borran mascotas, carnet, QR, autorizaciones, registros de escaneo, conversaciones, mensajes y fotos privadas. | T180 |
| `CA-02` | Las consultas que veterinarias registraron en el carnet se eliminan con la cuenta del dueño. | T180 |
| `CA-03` | No quedan registros huérfanos y una prueba falla si una colección con referencia al usuario no participa. | T180 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T180 | Nueva | Aprobada para iniciar (2026-11-10, aprobación humana; pendiente de plan.md) | Borrar los datos de mascotas al eliminar la cuenta |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
