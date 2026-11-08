# Spec — pm-047 pagination-convention

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L4 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Parámetros, tope y forma de paginación en listados.

## 2. Dependencias

- [pm-009](../pm-009-response-wrapper-contract/spec.md) — response-wrapper-contract

## 3. Origen

- Fuentes: nuevo (requisito de rendimiento).
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-047#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Los listados aceptan `page` y `pageSize` con valores por defecto y un tope máximo. | T058 |
| `CA-02` | Los listados devuelven una forma de paginación estándar dentro de `data`. | T058 |
| `CA-03` | Los parámetros inválidos devuelven 400 con el formato estándar. | T058 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T058 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | Implementar la convención de paginación |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- **Valores aprobados por el usuario (2026-11-08).** Los criterios no cambian; estos valores los concretan:
  - `page` empieza en 1 y su valor por defecto es 1.
  - `pageSize` tiene valor por defecto 20 y tope máximo 50.
  - Un valor fuera de rango, no numérico o menor que 1 devuelve 400 con el formato estándar y el nombre del parámetro; no se recorta en silencio.
  - Forma de la respuesta dentro de `data`: `items`, `page`, `pageSize`, `totalCount` y `totalPages`.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
