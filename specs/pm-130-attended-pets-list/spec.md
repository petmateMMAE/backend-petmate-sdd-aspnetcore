# Spec — pm-130 attended-pets-list

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L12 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`GET /api/v1/scans/get-attended` lista las mascotas que el negocio escaneó, sin abrir su ficha.

## 2. Dependencias

- [pm-129](../pm-129-scan-log-record/spec.md) — scan-log-record

## 3. Origen

- Fuentes: Documento v3: RF-37, HU-035; maqueta `attended`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-130#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Devuelve foto, nombre y fecha de las mascotas escaneadas, de la más reciente a la más antigua. | T148 |
| `CA-02` | No incluye datos clínicos ni da acceso a la ficha. | T148 |
| `CA-03` | Admite filtrar por día o por semana y usa la convención de paginación. | T148 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T148 | Nueva | Verificado (2026-11-10; aprobación humana en el chat; sin revisión independiente de este bloque) | Listar las mascotas atendidas por el negocio |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
