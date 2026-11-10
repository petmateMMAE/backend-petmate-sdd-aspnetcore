# Spec — pm-004 external-configuration-secrets

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L1 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Configuración por entorno, `.env` ignorado y validación de arranque sin exponer valores.

## 2. Dependencias

- [pm-001](../pm-001-solution-structure/spec.md) — solution-structure

## 3. Origen

- Fuentes: 00-foundation/spec.md CA-005; task T004.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-11 (ver [traceability.md](../traceability.md)).
- Constitución: §4.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-004#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La configuración local se lee de `.env` solo fuera de Production y el archivo real está ignorado por Git. [origen: 00-foundation CA-005] | T004 |
| `CA-02` | Production no lee `.env`. | T004 |
| `CA-03` | Ante configuración obligatoria ausente o inválida el arranque falla sin exponer valores. | T004 |
| `CA-04` | `.env.example` contiene únicamente nombres y placeholders. | T004 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T004 | Histórica | Cerrado (2026-11-03) | Configuración `.env` local y secretos |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
