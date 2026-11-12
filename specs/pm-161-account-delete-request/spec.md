# Spec — pm-161 account-delete-request

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`DELETE /api/v1/users/me/delete` elimina la cuenta del usuario autenticado tras confirmar su contraseña.

## 2. Dependencias

- [pm-086](../pm-086-user-role-unification/spec.md) — user-role-unification
- [pm-028](../pm-028-token-revocation-redis/spec.md) — token-revocation-redis
- [pm-025](../pm-025-refresh-sessions-persistence/spec.md) — refresh-sessions-persistence

## 3. Origen

- Fuentes: Documento v3: RF-08, RNF-08, HU-006; maqueta `deleteAccount`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-161#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La eliminación exige la contraseña actual; una contraseña incorrecta no elimina nada. | T179 |
| `CA-02` | Se revocan todas las sesiones y los tokens de la cuenta. | T179 |
| `CA-03` | Se dispara el borrado de los datos asociados (pm-162 y pm-163). | T179 |
| `CA-04` | La operación está limitada por usuario y la respuesta no revela datos. | T179 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T179 | Nueva | Aprobada para iniciar (2026-11-10, aprobación humana; pendiente de plan.md) | Eliminar la cuenta con confirmación de contraseña |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- El borrado es inmediato; el aviso de privacidad de la maqueta promete un máximo de 30 días para cubrir las copias de seguridad (pendiente de confirmar).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
