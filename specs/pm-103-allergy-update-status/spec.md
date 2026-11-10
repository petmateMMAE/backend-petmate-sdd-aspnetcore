# Spec — pm-103 allergy-update-status

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L10 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`PUT /api/v1/allergies/update/{id}` edita una alergia propia y cambia su estado entre activa y superada.

## 2. Dependencias

- [pm-102](../pm-102-allergy-create-list/spec.md) — allergy-create-list

## 3. Origen

- Fuentes: Documento v3: RF-17, HU-016.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §6.5.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-103#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El dueño edita los datos de una alergia o condición propia. | T121 |
| `CA-02` | El dueño puede marcarla como superada y volver a activarla. | T121 |
| `CA-03` | Una alergia de una mascota ajena o inexistente responde 404. | T121 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T121 | Nueva | Verificado (2026-11-09; revisión independiente sin hallazgos bloqueantes y aprobación humana en el chat) | Editar y marcar como superada una alergia |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-09, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
