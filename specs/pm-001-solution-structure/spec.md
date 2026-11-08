# Spec — pm-001 solution-structure

> Estado: En revisión | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L0 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Solución .NET 10 con 4 proyectos de código y 4 de pruebas que compilan en Release.

## 2. Dependencias

- Ninguna.

## 3. Origen

- Fuentes: 00-foundation/spec.md CA-001; task T001.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Constitución: §3.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-001#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | La solución y todos los proyectos compilan con `net10.0`. [origen: 00-foundation CA-001] | T001 |
| `CA-02` | Existen cuatro proyectos de código (Api, Application, Domain, Infrastructure) y cuatro de pruebas (Unit, Integration, Security, Contract). | T001 |
| `CA-03` | El build Release termina sin errores ni advertencias. | T001 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T001 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | Crear solución y proyectos |

Detalle en [task.md](task.md).

## 7. Aprobación

- [ ] Requisitos y criterios revisados por el usuario.
- [ ] Constitución cumplida.
- Revisor/usuario: pendiente.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
