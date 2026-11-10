# Spec — pm-087 session-business-link

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L8 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Registro, inicio de sesión y refresh informan si la cuenta tiene un negocio asociado y su tipo.

## 2. Dependencias

- [pm-086](../pm-086-user-role-unification/spec.md) — user-role-unification
- [pm-029](../pm-029-user-registration/spec.md) — user-registration
- [pm-030](../pm-030-user-login/spec.md) — user-login
- [pm-031](../pm-031-refresh-token-rotation/spec.md) — refresh-token-rotation

## 3. Origen

- Fuentes: Documento v3: RF-03, RF-40; HU-002, HU-026.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §6.1.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-087#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La respuesta de sesión incluye `business`, con el tipo del negocio asociado o `null` si la cuenta no tiene negocio. | T105 |
| `CA-02` | Mientras no exista el módulo de negocios, `business` es siempre `null` y el contrato queda documentado en `docs/api/endpoints.md`. | T105 |
| `CA-03` | El dato lo calcula el servidor; el cliente no puede enviarlo. | T105 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T105 | Nueva | Verificado (2026-11-09; revisión independiente sin hallazgos bloqueantes y aprobación humana en el chat) | Informar el negocio asociado en la respuesta de sesión |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- La app usa este dato para abrir «Mi negocio» o «Mis mascotas» y mostrar el botón de cambio de modo.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-09, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
