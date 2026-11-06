# Spec — pm-070 cloud-redis-auth

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Redis Auth administrado en Azure con TLS, autenticación, red privada, `noeviction` y comportamiento fail-closed.

## 2. Dependencias

- [pm-013](../pm-013-redis-dual-connectivity/spec.md) — redis-dual-connectivity
- [pm-066](../pm-066-iac-baseline/spec.md) — iac-baseline
- [pm-068](../pm-068-cloud-secrets-configuration/spec.md) — cloud-secrets-configuration
- [pm-072](../pm-072-cloud-network-security/spec.md) — cloud-network-security

## 3. Origen

- Fuentes: nuevo; docs/infrastructure/docker-redis-atlas.md (nota de TLS pendiente).
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-12 (ver [traceability.md](../traceability.md)).
- Constitución: §5, §8.9.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-070#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La conexión usa TLS y una conexión sin TLS es rechazada. | T082 |
| `CA-02` | La autenticación es obligatoria y una conexión sin credenciales falla. | T082 |
| `CA-03` | El acceso es solo por red privada o control equivalente y una prueba desde fuera de la red falla. | T082 |
| `CA-04` | Las credenciales se inyectan por secreto del entorno, son rotables y no aparecen en repositorio, imagen ni logs. | T082 |
| `CA-05` | Ningún secreto aparece en logs ni mensajes de error (prueba y revisión estática). | T082 |
| `CA-06` | La política es `noeviction` con persistencia, de modo que las revocaciones vigentes no se pierden. | T082 |
| `CA-07` | La prueba de conectividad confirma que `/api/v1/health/ready` reporta `redis-auth`. | T082 |
| `CA-08` | Con Redis Auth caído los endpoints autenticados devuelven 503 y nunca 200 (fail-closed). | T082 |
| `CA-09` | Tras un reinicio del servicio la API se reconecta sin perder los `jti` vigentes. | T082 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T082 | Nueva | Propuesto | Aprovisionar y verificar Redis Auth en Azure |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- **Azure for Students:** Servicio Redis administrado de Azure por validar en pm-062 (disponibilidad, TLS, red privada y costo). Si ningún servicio cumple a costo $0, se documenta el conflicto sin reducir TLS ni autenticación.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
