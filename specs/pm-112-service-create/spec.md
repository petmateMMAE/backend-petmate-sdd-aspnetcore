# Spec — pm-112 service-create

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/services/create` agrega un servicio al catálogo del negocio propio.

## 2. Dependencias

- [pm-107](../pm-107-business-affiliate-create/spec.md) — business-affiliate-create

## 3. Origen

- Fuentes: Documento v3: RF-32, HU-030; maqueta `serviceForm`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-112#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El afiliado crea un servicio con nombre obligatorio, y descripción y precio opcionales. | T130 |
| `CA-02` | El precio, si existe, es mayor o igual que cero; sin precio se devuelve nulo y nunca 0. | T130 |
| `CA-03` | Solo una cuenta con negocio asociado puede crear servicios. | T130 |
| `CA-04` | No existen existencias ni ventas asociadas a un servicio. | T130 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T130 | Nueva | Verificado (2026-11-10; revisión independiente sin hallazgos bloqueantes y aprobación humana en el chat) | Crear un servicio del negocio |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- El catálogo del negocio son sus servicios; no hay inventario (decisión del usuario del 2026-11-09).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
