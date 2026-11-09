# Spec — pm-153 push-provider-adapter

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L3 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Existe un puerto de aplicación para enviar notificaciones push y un adaptador a Firebase Cloud Messaging.

## 2. Dependencias

- [pm-004](../pm-004-external-configuration-secrets/spec.md) — external-configuration-secrets
- [pm-005](../pm-005-dependency-injection-composition/spec.md) — dependency-injection-composition

## 3. Origen

- Fuentes: Decisión del usuario del 2026-11-09 (push con el proveedor recomendado).
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §4, §9.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-153#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Los casos de uso dependen de un puerto de envío push y no del proveedor. | T171 |
| `CA-02` | La credencial de la cuenta de servicio se lee solo de configuración segura y nunca se registra. | T171 |
| `CA-03` | Un fallo del proveedor no afecta la operación que originó el aviso. | T171 |
| `CA-04` | El costo $0, la licencia y las vulnerabilidades de cualquier paquete se verifican y se registran en `plan.md`. | T171 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T171 | Nueva | Propuesto | Implementar el puerto y el adaptador de notificaciones push |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Firebase Cloud Messaging no tiene costo según la página de precios de Firebase consultada el 2026-11-09; se vuelve a verificar al planificar.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
