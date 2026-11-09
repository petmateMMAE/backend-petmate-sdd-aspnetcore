# Spec — pm-164 account-deletion-web-request

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L9 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/account-deletion/request` inicia la eliminación de una cuenta sin entrar a la app.

## 2. Dependencias

- [pm-088](../pm-088-transactional-email-adapter/spec.md) — transactional-email-adapter
- [pm-036](../pm-036-rate-limiting/spec.md) — rate-limiting

## 3. Origen

- Fuentes: Documento v3: RF-39, HU-036; maqueta `landing`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-164#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La respuesta es la misma exista o no el correo. | T182 |
| `CA-02` | Para una cuenta existente se envía al correo un enlace de confirmación de un solo uso que vence en 1 hora. | T182 |
| `CA-03` | La solicitud está limitada por IP y por correo. | T182 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T182 | Nueva | Propuesto | Solicitar la eliminación de la cuenta desde la web |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- El método de verificación de identidad (enlace al correo) es una propuesta pendiente de confirmar por el usuario.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
