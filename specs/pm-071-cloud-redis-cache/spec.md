# Spec — pm-071 cloud-redis-cache

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Redis Cache administrado en Azure con TLS, autenticación, red privada, expulsión LRU y degradación segura.

## 2. Dependencias

- [pm-013](../pm-013-redis-dual-connectivity/spec.md) — redis-dual-connectivity
- [pm-066](../pm-066-iac-baseline/spec.md) — iac-baseline
- [pm-068](../pm-068-cloud-secrets-configuration/spec.md) — cloud-secrets-configuration
- [pm-072](../pm-072-cloud-network-security/spec.md) — cloud-network-security

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-12 (ver [traceability.md](../traceability.md)).
- Constitución: §5, §8.9.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-071#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La conexión usa TLS y una conexión sin TLS es rechazada. | T083 |
| `CA-02` | La autenticación es obligatoria y una conexión sin credenciales falla. | T083 |
| `CA-03` | El acceso es solo por red privada o control equivalente y una prueba desde fuera de la red falla. | T083 |
| `CA-04` | Las credenciales son distintas de las de Redis Auth, rotables y ausentes de repositorio, imagen y logs. | T083 |
| `CA-05` | Ningún secreto aparece en logs ni mensajes de error (prueba y revisión estática). | T083 |
| `CA-06` | La política es expulsión LRU sin persistencia obligatoria. | T083 |
| `CA-07` | La prueba de conectividad confirma que `/api/v1/health/ready` reporta `redis-cache`. | T083 |
| `CA-08` | Con Redis Cache caído la API sigue respondiendo correctamente sin afectar autenticación ni resultados de negocio. | T083 |
| `CA-09` | Tras un reinicio la API se reconecta y recarga sin errores. | T083 |
| `CA-10` | Ninguna clave de autenticación reside en este Redis. | T083 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T083 | Nueva | Propuesto | Aprovisionar y verificar Redis Cache en Azure |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- **Azure for Students:** Igual que pm-070: servicio por validar en pm-062.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
