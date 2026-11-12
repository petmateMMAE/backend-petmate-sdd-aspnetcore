# Spec — pm-125 scan-resolve-qr

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/scans/resolve` identifica la mascota de un token o código corto escaneado por un negocio.

## 2. Dependencias

- [pm-093](../pm-093-pet-qr-token/spec.md) — pet-qr-token
- [pm-107](../pm-107-business-affiliate-create/spec.md) — business-affiliate-create

## 3. Origen

- Fuentes: Documento v3: RF-33, RNF-03, HU-031; maqueta `scanner`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.5, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-125#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Un token o un código corto válido identifica la mascota; el código corto no distingue mayúsculas. | T143 |
| `CA-02` | Un token inexistente o regenerado devuelve 404 con el mismo mensaje. | T143 |
| `CA-03` | Solo una cuenta con negocio asociado puede resolver un QR. | T143 |
| `CA-04` | Los intentos están limitados por negocio para impedir la enumeración de códigos. | T143 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T143 | Nueva | Verificado (2026-11-10; revisión independiente sin hallazgos bloqueantes y aprobación humana en el chat) | Resolver un token o código corto a una mascota |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
