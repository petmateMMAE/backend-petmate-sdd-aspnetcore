# Spec — pm-127 scan-ficha-veterinary

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L14 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Una veterinaria con autorización vigente recibe la ficha de la mascota con los datos autorizados.

## 2. Dependencias

- [pm-126](../pm-126-scan-access-check/spec.md) — scan-access-check
- [pm-097](../pm-097-vaccine-create-list/spec.md) — vaccine-create-list
- [pm-099](../pm-099-deworming-create-list/spec.md) — deworming-create-list
- [pm-101](../pm-101-weight-create-list/spec.md) — weight-create-list
- [pm-102](../pm-102-allergy-create-list/spec.md) — allergy-create-list
- [pm-133](../pm-133-consultation-owner-list/spec.md) — consultation-owner-list

## 3. Origen

- Fuentes: Documento v3: RF-34, HU-032; maqueta `ficha`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.5, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-127#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La veterinaria recibe vacunas, desparasitaciones, peso, consultas y alergias, solo de los grupos autorizados. | T145 |
| `CA-02` | Una veterinaria sin autorización para un grupo no recibe ese grupo. | T145 |
| `CA-03` | La ficha es de solo lectura. | T145 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T145 | Nueva | Verificado (2026-11-10; aprobación humana en el chat; sin revisión independiente) | Entregar la ficha completa a una veterinaria autorizada |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
