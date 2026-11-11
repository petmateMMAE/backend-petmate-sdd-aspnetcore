# Spec — pm-105 business-persistence-model

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L6 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Existe el modelo y la colección del negocio afiliado, con un único negocio por cuenta y datos listos para el mapa.

## 2. Dependencias

- [pm-086](../pm-086-user-role-unification/spec.md) — user-role-unification
- [pm-039](../pm-039-pet-domain-persistence/spec.md) — pet-domain-persistence

## 3. Origen

- Fuentes: Documento v3: RF-25, RF-27; maqueta `affiliate`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §4, §6.1.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-105#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El negocio guarda tipo (Veterinaria, Tienda o Estética), nombre, teléfono, WhatsApp opcional, dirección, descripción, ubicación geográfica y cédula profesional opcional. | T123 |
| `CA-02` | Un índice único por cuenta impide más de un negocio por usuario. | T123 |
| `CA-03` | Un índice geoespacial permite consultar los negocios por cercanía. | T123 |
| `CA-04` | La cédula profesional es un dato privado y no forma parte de ningún modelo de lectura público. | T123 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T123 | Nueva | Verificado (2026-11-10; revisión independiente sin hallazgos bloqueantes y aprobación humana en el chat) | Crear el modelo y la colección del negocio |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- El tipo es único por negocio (decisión del usuario del 2026-11-09).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
