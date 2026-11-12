# Spec — pm-126 scan-access-check

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L11 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Un negocio solo accede a una mascota con una autorización vigente que lo incluya.

## 2. Dependencias

- [pm-125](../pm-125-scan-resolve-qr/spec.md) — scan-resolve-qr
- [pm-122](../pm-122-access-grant-model/spec.md) — access-grant-model

## 3. Origen

- Fuentes: Documento v3: RNF-04, RNF-05; decisión del usuario del 2026-11-09.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-126#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Sin una autorización vigente para ese negocio o para cualquier negocio, el acceso se rechaza con 403 y sin datos de la mascota. | T144 |
| `CA-02` | Con una autorización vigente se concede el acceso a los datos que permite. | T144 |
| `CA-03` | El acceso termina cuando vence la autorización. | T144 |
| `CA-04` | Conocer el QR no basta para acceder. | T144 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T144 | Nueva | Verificado (2026-11-10; revisión independiente sin hallazgos bloqueantes y aprobación humana en el chat) | Exigir una autorización vigente para ver la mascota |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
