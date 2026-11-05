# Spec — pm-027 access-token-jwt

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L2 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Emisión y validación de JWT HS256 de corta vida y configuración segura.

## 2. Dependencias

- [pm-004](../pm-004-external-configuration-secrets/spec.md) — external-configuration-secrets
- [pm-022](../pm-022-user-domain-roles/spec.md) — user-domain-roles

## 3. Origen

- Fuentes: 01-initial-authentication/task.md T016; plan.md §11.6.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-11 (ver [traceability.md](../traceability.md)).
- Constitución: §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-027#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El access token es un JWT HS256 de vida configurable (15 minutos por defecto) con `jti` único. | T016 |
| `CA-02` | La validación comprueba firma, algoritmo, issuer, audience, `exp` y `nbf`. | T016 |
| `CA-03` | La clave es de al menos 256 bits y fuera de Development se rechazan plantillas. | T016 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T016 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | Servicio de Access JWT |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
