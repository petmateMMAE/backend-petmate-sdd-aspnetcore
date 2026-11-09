# Spec — pm-094 pet-qr-read

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L8 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`GET /api/v1/pets/get-qr/{id}` devuelve el token y el código corto del QR de una mascota propia.

## 2. Dependencias

- [pm-093](../pm-093-pet-qr-token/spec.md) — pet-qr-token
- [pm-045](../pm-045-pet-ownership-bola/spec.md) — pet-ownership-bola

## 3. Origen

- Fuentes: Documento v3: RF-13, HU-011.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §6.5, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-094#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El dueño obtiene el token y el código corto de su mascota. | T112 |
| `CA-02` | Una mascota ajena o inexistente responde 404 sin revelar su existencia. | T112 |
| `CA-03` | La respuesta no se guarda en caché y el token nunca se registra en logs. | T112 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T112 | Nueva | Propuesto | Consultar el QR de una mascota propia |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
