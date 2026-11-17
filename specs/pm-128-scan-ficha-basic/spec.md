# Spec — pm-128 scan-ficha-basic

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L12 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Una tienda o estética con autorización vigente recibe solo los datos básicos de la mascota.

## 2. Dependencias

- [pm-126](../pm-126-scan-access-check/spec.md) — scan-access-check
- [pm-102](../pm-102-allergy-create-list/spec.md) — allergy-create-list

## 3. Origen

- Fuentes: Documento v3: RF-34, RNF-07, HU-032; maqueta `ficha`.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`).
- Constitución: §6.5, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-128#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La ficha incluye foto, nombre, especie, raza, edad y las alergias y condiciones activas. | T146 |
| `CA-02` | El servidor no envía ningún otro campo, comprobado por una prueba de contrato. | T146 |
| `CA-03` | El acceso vence junto con la autorización. | T146 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T146 | Nueva | Verificado (2026-11-10; revisión independiente sin hallazgos bloqueantes y aprobación humana en el chat) | Entregar la ficha básica a una tienda o estética autorizada |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-10, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
