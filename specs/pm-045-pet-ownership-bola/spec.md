# Spec — pm-045 pet-ownership-bola

> Estado: Cerrado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L7 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Matriz de propiedad y BOLA del módulo de mascotas y cierre de seguridad del módulo.

## 2. Dependencias

- [pm-040](../pm-040-pet-create/spec.md) — pet-create
- [pm-041](../pm-041-pet-list/spec.md) — pet-list
- [pm-042](../pm-042-pet-detail/spec.md) — pet-detail
- [pm-043](../pm-043-pet-update/spec.md) — pet-update
- [pm-044](../pm-044-pet-soft-delete/spec.md) — pet-soft-delete

## 3. Origen

- Fuentes: 03-pet-management/spec.md CA-006..007; tasks T035, T038, T040.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-14 (ver [traceability.md](../traceability.md)).
- Constitución: §6, §11.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-045#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La propiedad se verifica en cada handler y consulta, no solo en el controlador. | T035 |
| `CA-02` | Un Owner A no puede consultar, actualizar ni eliminar una mascota de un Owner B (IDOR/BOLA); un anónimo recibe 401 y Business no obtiene acceso por defecto. [origen: 03-pet-management CA-006, CA-007] | T038 |
| `CA-03` | Los gates y la revisión independiente del módulo de mascotas tienen evidencia. | T040 |
| `CA-04` | El módulo limita el abuso: máximo 100 mascotas por propietario (409 al superarlo), 60 peticiones por minuto por usuario en `/api/v1/pets/*` (429 con `Retry-After`) y cuerpo de petición de hasta 16 KB (413). [añadido con aprobación del usuario, 2026-11-08, a raíz de la revisión de seguridad S1 y S2] | T035 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T035 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | Asegurar autorización y ownership |
| T038 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | Pruebas de seguridad Pet |
| T040 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | Gates y revisión independiente Pet |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
