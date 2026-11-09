# Spec — pm-091 profile-optional-phone

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L7 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

El perfil permite agregar, cambiar y quitar un número de celular opcional.

## 2. Dependencias

- [pm-038](../pm-038-owner-profile-update/spec.md) — owner-profile-update

## 3. Origen

- Fuentes: Decisión del usuario del 2026-11-09 (celular opcional, se agrega después del registro).
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-091#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | `PUT /api/v1/users/me/update` acepta un celular con formato válido o lo quita. | T109 |
| `CA-02` | El celular no es obligatorio ni se pide en el registro y no se verifica por SMS. | T109 |
| `CA-03` | `GET /api/v1/users/me/get` devuelve el celular solo a su propietario. | T109 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T109 | Nueva | Propuesto | Permitir agregar un celular opcional al perfil |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- El formato válido del número (por ejemplo, diez dígitos de México) se fija en `plan.md`.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
