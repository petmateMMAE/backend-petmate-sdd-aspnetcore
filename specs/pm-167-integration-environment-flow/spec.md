# Spec — pm-167 integration-environment-flow

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Existe un documento que fija cómo el equipo móvil consume el backend durante el desarrollo.

## 2. Dependencias

- [pm-055](../pm-055-ci-pull-request-pipeline/spec.md) — ci-pull-request-pipeline
- [pm-059](../pm-059-release-versioning/spec.md) — release-versioning

## 3. Origen

- Fuentes: Decisión del usuario del 2026-11-09 (el flujo lo determina el equipo de agentes; la integración va primero).
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §8, §9.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-167#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El documento describe el flujo `developer` → pull request a `main` → CI → despliegue con prueba de humo y reversa, y la etiqueta de versión solo a mano. | T185 |
| `CA-02` | Define el entorno compartido, su estado verificado y los datos de prueba. | T185 |
| `CA-03` | Establece que crear o cambiar recursos de Azure y fusionar a `main` requieren autorización expresa del usuario. | T185 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T185 | Nueva | Aprobada para iniciar (2026-11-10, aprobación humana; pendiente de plan.md) | Documentar el flujo del entorno de integración con la app |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- El estado del despliegue de Azure no está verificado (la URL declarada no respondió el 2026-11-09).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
