# Spec — pm-026 password-hashing-argon2id

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L2 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Hash Argon2id con salt, parámetros mínimos, rehash y concurrencia acotada.

## 2. Dependencias

- [pm-022](../pm-022-user-domain-roles/spec.md) — user-domain-roles

## 3. Origen

- Fuentes: 01-initial-authentication/spec.md CA-002; task T015; plan.md §11.3.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-026#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La contraseña se persiste como hash Argon2id con salt, nunca en texto claro. [origen: 01-initial-authentication CA-002] | T015 |
| `CA-02` | Los parámetros no pueden bajar del mínimo OWASP (por defecto t=3, m=64 MiB, p=1). | T015 |
| `CA-03` | Se recalcula el hash al autenticar si se elevan los parámetros. | T015 |
| `CA-04` | La concurrencia de hashing está acotada y la espera máxima de 5 s produce 503. | T015 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T015 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | Hasher Argon2id |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
