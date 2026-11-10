# Spec — pm-122 access-grant-model

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Existe el modelo de autorización con el que el dueño da acceso a un negocio a datos de su mascota por un tiempo limitado.

## 2. Dependencias

- [pm-093](../pm-093-pet-qr-token/spec.md) — pet-qr-token
- [pm-107](../pm-107-business-affiliate-create/spec.md) — business-affiliate-create
- [pm-045](../pm-045-pet-ownership-bola/spec.md) — pet-ownership-bola

## 3. Origen

- Fuentes: Maqueta `share`; documento v3: RNF-04, RNF-05; decisiones del usuario del 2026-11-09.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-122#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La autorización guarda la mascota, el destinatario (un negocio o cualquier negocio que escanee), los datos permitidos y su vencimiento. | T140 |
| `CA-02` | Las alergias siempre forman parte de los datos permitidos. | T140 |
| `CA-03` | Una autorización vencida deja de conceder acceso aunque el registro siga existiendo. | T140 |
| `CA-04` | Existen índices por mascota y por vencimiento. | T140 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T140 | Nueva | Propuesto | Modelar la autorización de acceso a una mascota |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- El dueño no revoca una autorización antes de que venza ni ve quién entró (decisión del usuario); la única forma de cortar el acceso es regenerar el QR.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
