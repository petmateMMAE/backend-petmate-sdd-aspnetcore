# Spec — pm-092 pet-species-sex-rules

> Estado: Propuesto | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L7 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

La mascota acepta solo las especies Perro y Gato y el sexo es opcional.

## 2. Dependencias

- [pm-040](../pm-040-pet-create/spec.md) — pet-create
- [pm-043](../pm-043-pet-update/spec.md) — pet-update

## 3. Origen

- Fuentes: Documento v3: RF-09, HU-007; decisión del usuario del 2026-11-09.
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-092#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Crear y actualizar una mascota aceptan solo `Perro` o `Gato`; cualquier otra especie devuelve 400. | T110 |
| `CA-02` | El sexo es opcional y, si no se envía, se guarda como desconocido. | T110 |
| `CA-03` | La fecha de nacimiento sigue siendo opcional. | T110 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T110 | Nueva | Propuesto | Restringir la especie a Perro y Gato y hacer opcional el sexo |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- La lista de especies es cerrada y por ahora solo tiene esos dos valores; ampliarla requiere un cambio de Spec.

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
