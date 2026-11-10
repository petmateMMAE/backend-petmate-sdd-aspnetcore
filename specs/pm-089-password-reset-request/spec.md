# Spec — pm-089 password-reset-request

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L9 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/auth/forgot-password` envía al correo registrado un enlace de un solo uso para crear una contraseña nueva.

## 2. Dependencias

- [pm-088](../pm-088-transactional-email-adapter/spec.md) — transactional-email-adapter
- [pm-024](../pm-024-users-persistence/spec.md) — users-persistence
- [pm-036](../pm-036-rate-limiting/spec.md) — rate-limiting

## 3. Origen

- Fuentes: Documento v3: RF-04, HU-003.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §6, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-089#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La respuesta es la misma (200 genérico) exista o no el correo. | T107 |
| `CA-02` | Para una cuenta existente se envía un enlace con un token aleatorio de un solo uso que vence en 1 hora; solo se guarda su hash. | T107 |
| `CA-03` | La solicitud está limitada por IP y por correo (429 con `Retry-After`). | T107 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T107 | Nueva | Verificado (2026-11-09; revisión independiente sin hallazgos bloqueantes y aprobación humana en el chat) | Solicitar el enlace de restablecimiento de contraseña |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-09, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
