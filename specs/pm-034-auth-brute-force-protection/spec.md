# Spec — pm-034 auth-brute-force-protection

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L9 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Cuota por correo y bloqueo contra credential stuffing, y limitador asíncrono de hashing.

## 2. Dependencias

- [pm-030](../pm-030-user-login/spec.md) — user-login
- [pm-031](../pm-031-refresh-token-rotation/spec.md) — refresh-token-rotation
- [pm-036](../pm-036-rate-limiting/spec.md) — rate-limiting

## 3. Origen

- Fuentes: 01-initial-authentication/task.md T023 (pruebas de abuso); plan.md.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-034#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Existe una cuota por correo para login y refresh, independiente de la IP. | T056 |
| `CA-02` | Un bloqueo temporal frena el credential stuffing sin revelar si el correo existe. | T056 |
| `CA-03` | Un limitador asíncrono de hashing evita agotar la CPU bajo abuso. | T056 |
| `CA-04` | Las respuestas 429 usan el wrapper y `Retry-After`. | T056 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T023 | Histórica | Propuesto | Pruebas unitarias, integración y seguridad de auth |
| T056 | Nueva | Propuesto | Implementar cuota por correo, bloqueo y limitador asíncrono de hashing |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Parámetros (umbrales y ventanas) pendientes de decisión; T023 aporta pruebas de seguridad de autenticación y no se reasigna su alcance.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
