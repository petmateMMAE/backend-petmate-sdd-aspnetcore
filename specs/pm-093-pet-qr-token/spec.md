# Spec — pm-093 pet-qr-token

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L7 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Al crear una mascota se genera su QR permanente: un token aleatorio y un código corto legible.

## 2. Dependencias

- [pm-040](../pm-040-pet-create/spec.md) — pet-create
- [pm-039](../pm-039-pet-domain-persistence/spec.md) — pet-domain-persistence

## 3. Origen

- Fuentes: Documento v3: RF-13, RNF-03; HU-007, HU-011.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §6, §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-093#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Al crear la mascota se genera un token aleatorio criptográfico que no se deriva de su identificador. | T111 |
| `CA-02` | También se genera un código corto legible para dictarlo cuando no se pueda escanear. | T111 |
| `CA-03` | El token y el código corto son únicos (índice único) y se devuelven solo a su dueño. | T111 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T111 | Nueva | Propuesto | Generar el token y el código corto del QR al crear la mascota |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- La longitud y entropía del token y el formato del código corto se fijan en `plan.md`.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
