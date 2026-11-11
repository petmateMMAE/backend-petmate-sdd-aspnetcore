# Spec — pm-129 scan-log-record

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L11 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Cada vez que un negocio accede a una mascota se guarda un registro interno del escaneo.

## 2. Dependencias

- [pm-125](../pm-125-scan-resolve-qr/spec.md) — scan-resolve-qr

## 3. Origen

- Fuentes: Documento v3: RF-37, HU-035.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-129#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Cada acceso concedido registra negocio, mascota y fecha. | T147 |
| `CA-02` | El registro es interno: el dueño no puede consultarlo. | T147 |
| `CA-03` | Los registros se eliminan al eliminar la mascota o la cuenta. | T147 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T147 | Nueva | Verificado (2026-11-10; revisión independiente sin hallazgos bloqueantes y aprobación humana en el chat) | Registrar cada escaneo con acceso concedido |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
