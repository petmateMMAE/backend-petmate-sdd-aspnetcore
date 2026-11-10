# Spec — pm-086 user-role-unification

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L5 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Toda cuenta tiene el rol único `User`; los roles excluyentes `Owner` y `Business` dejan de emitirse (Constitución v1.3.0 §6.1).

## 2. Dependencias

- [pm-022](../pm-022-user-domain-roles/spec.md) — user-domain-roles
- [pm-027](../pm-027-access-token-jwt/spec.md) — access-token-jwt
- [pm-033](../pm-033-authorization-policies/spec.md) — authorization-policies

## 3. Origen

- Fuentes: Documento v3: RF-03, RF-27; decisión del usuario del 2026-11-09 (cuenta con negocio asociado).
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §6.1, §6.5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-086#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Los usuarios nuevos y los access tokens llevan el rol `User`; ya no se emiten `Owner` ni `Business`. | T104 |
| `CA-02` | Los endpoints de perfil y de mascotas se autorizan a cualquier `User` autenticado, solo sobre sus propios recursos. | T104 |
| `CA-03` | Una solicitud no puede fijar ni elevar su rol y no existe ningún rol administrativo. | T104 |
| `CA-04` | Un token emitido con el rol anterior `Owner` se acepta como `User` hasta que expira. | T104 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T104 | Nueva | Verificado (2026-11-09; revisión independiente sin hallazgos bloqueantes y aprobación humana en el chat) | Unificar los roles Owner y Business en el rol único User |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Enmienda pm-022 (dominio de roles), pm-033 (políticas) y pm-045 (propiedad) sin reescribir su historia. No hay cuentas `Business` creadas, así que no se migran datos.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-09, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
