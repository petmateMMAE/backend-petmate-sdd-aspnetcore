# Spec — pm-012 atlas-connectivity

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L2 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Conexión tipada a MongoDB Atlas, base de pruebas separada y sin Mongo local.

## 2. Dependencias

- [pm-004](../pm-004-external-configuration-secrets/spec.md) — external-configuration-secrets

## 3. Origen

- Fuentes: 00-foundation/task.md T010; docs/infrastructure/docker-redis-atlas.md.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §4.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-012#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La conexión a MongoDB Atlas es tipada y se configura por `MONGODB_URI` y `MONGODB_DATABASE`. | T010 |
| `CA-02` | La base de pruebas (`MONGODB_TEST_*`) es distinta de la de desarrollo. | T010 |
| `CA-03` | No existe MongoDB local ni servicio MongoDB en Compose. [origen: 00-foundation CA-006] | T010 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T010 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | Configuración MongoDB Atlas y Redis separados |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
