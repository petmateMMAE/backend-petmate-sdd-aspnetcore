# Spec — pm-123 access-grant-create

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L11 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/access-grants/create` crea la autorización que el dueño da a un negocio.

## 2. Dependencias

- [pm-122](../pm-122-access-grant-model/spec.md) — access-grant-model

## 3. Origen

- Fuentes: Maqueta `share`; documento v3: HU-011.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-123#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El dueño crea una autorización para un negocio o para cualquier negocio, con los datos elegidos y una duración de 30 minutos, 1 día o 7 días. | T141 |
| `CA-02` | Cualquier otra duración devuelve 400 y las alergias siempre se incluyen. | T141 |
| `CA-03` | Solo se puede crear sobre una mascota propia; una ajena responde 404. | T141 |
| `CA-04` | No existe ningún endpoint para revocar o listar autorizaciones. | T141 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T141 | Nueva | Verificado (2026-11-10; revisión independiente sin hallazgos bloqueantes y aprobación humana en el chat) | Crear una autorización de acceso |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
