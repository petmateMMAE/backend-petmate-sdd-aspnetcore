# Spec — pm-033 authorization-policies

> Estado: En progreso | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L4 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Política de autorización por defecto y por rol Owner/Business.

## 2. Dependencias

- [pm-027](../pm-027-access-token-jwt/spec.md) — access-token-jwt
- [pm-028](../pm-028-token-revocation-redis/spec.md) — token-revocation-redis

## 3. Origen

- Fuentes: 01-initial-authentication/task.md T022.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-033#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Existe una política de autorización por defecto y una política de respaldo (denegación por defecto) que exigen usuario autenticado con rol Owner o Business; un endpoint sin atributos de autorización deniega a los anónimos. | T022 |
| `CA-02` | Existen políticas por rol Owner y Business. | T022 |
| `CA-03` | Un anónimo recibe 401 y un rol insuficiente recibe 403. | T022 |
| `CA-04` | El rol Business no obtiene acceso por defecto a la API de propietario: la política `RequireOwner` devuelve 403 a un token Business. | T022 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T022 | Histórica | En progreso | Políticas de autenticación y autorización |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- **Aprobación y estado.** El usuario aprobó el plan y las decisiones D-01 a D-08 y autorizó la implementación (2026-11-05); por eso el Spec pasa a `En progreso`. La aprobación de cierre sigue pendiente.
- **Criterios.** Los cuatro criterios siguen siendo propiedad exclusiva de T022; no se reasignó ninguno. Por la decisión P-02 se precisó la redacción de CA-01 y CA-04 (identificadores e intención intactos). No se marca ningún criterio como cumplido: la evidencia de pruebas está en [checklists/gates.md](checklists/gates.md) y el cumplimiento formal queda pendiente de revisión independiente y aprobación humana.
- **Implementación.** Políticas y pruebas implementadas; detalle, desviaciones y decisiones en [plan.md](plan.md) §15. El rechazo de tokens con rol ausente o desconocido (401) ya existía en la autenticación y no se reimplementó.
- **T022 no está cerrada.** Su alcance histórico incluye aplicar la política Owner a los endpoints de mascotas, que aún no existen (decisión P-01): se aplicará `RequireOwner` en pm-040 a pm-044 y la trazabilidad se conserva con pm-045 CA-02.
- **Endpoint técnico `mediator-probe` (decisión P-04).** Se conserva público de forma temporal para no ampliar el alcance. **Pendiente de un trabajo de seguridad posterior:** revisar su exposición en producción (restringirlo a Development o protegerlo sin crear un rol Admin de negocio). No es un endpoint público permanente por defecto.
- Los pasos de ejecución son internos de T022 y no constituyen Tasks nuevas.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: el [plan.md](plan.md) y los [gates](checklists/gates.md) fueron aprobados por el usuario para implementar (2026-11-05); el cierre del Spec requiere revisión independiente y aprobación humana. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
