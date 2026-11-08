# Spec — pm-034 auth-brute-force-protection

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L9 (los números identifican, no ordenan) | Migrado el 2026-11-05

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
| T023 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | Pruebas unitarias, integración y seguridad de auth |
| T056 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | Implementar cuota por correo, bloqueo y limitador asíncrono de hashing |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- **Valores aprobados por el usuario (2026-11-08).** Los criterios no cambian; estos valores los concretan:
  - Login: se permiten **5 intentos fallidos por correo en 15 minutos**; el sexto bloquea ese correo **15 minutos** (429 con `Retry-After`). Un login correcto borra el contador. El bloqueo se decide antes de buscar la cuenta o calcular hashes y es idéntico para correos existentes e inexistentes.
  - Refresh: **30 intentos por usuario en 15 minutos**; el 31.º bloquea 15 minutos. Se aplica antes de consumir el token.
  - Limitador de hashing Argon2id: tantos hashes simultáneos como núcleos de CPU y el doble en cola; lo que no cabe se rechaza al instante con 429 y `Retry-After: 1`.
  - Los contadores viven en Redis de autenticación (válidos en todas las réplicas) y guardan solo el hash del correo o del usuario. Si Redis falla, el login falla cerrado (503), igual que la lista negra de tokens.
  - Limitación conocida: un atacante puede bloquear el correo de una víctima enviando fallos (denegación de servicio dirigida); el límite por IP lo acota y el bloqueo expira solo.
- **Revisión de seguridad del 2026-11-08:** el conteo del intento se hace antes de verificar la contraseña (atómico en Redis), de modo que una ráfaga concurrente no evalúa más contraseñas que el máximo; el script de Redis no cuenta mientras hay un bloqueo activo; y el limitador de hashing libera su cupo cuando termina el hash, no cuando el cliente cancela. Riesgos aceptados o pendientes de decisión: ver `checklists/gates.md`.
- T023 aporta pruebas de seguridad de autenticación y no se reasigna su alcance.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
