# Spec — pm-085 account-registration-name

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L7 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

`POST /api/v1/auth/register` exige el nombre del usuario además del correo y la contraseña.

## 2. Dependencias

- [pm-029](../pm-029-user-registration/spec.md) — user-registration
- [pm-038](../pm-038-owner-profile-update/spec.md) — owner-profile-update

## 3. Origen

- Fuentes: Documento v3: RF-02, HU-001; decisión del usuario del 2026-11-09 (nombre obligatorio, celular opcional y después).
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §6.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-085#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El registro rechaza con 400 una solicitud sin nombre, con nombre vacío o con más caracteres que el límite del nombre visible (`User.MaxDisplayNameLength`). | T103 |
| `CA-02` | El nombre se guarda como nombre visible del perfil y se devuelve en el perfil y en la respuesta de sesión. | T103 |
| `CA-03` | La política de contraseña no cambia: de 12 a 128 caracteres. | T103 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T103 | Nueva | Verificado (2026-11-09; revisión independiente sin hallazgos bloqueantes y aprobación humana en el chat) | Exigir el nombre en el registro de la cuenta |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- El documento v3 propone un mínimo de 8 caracteres (RNF-01); el usuario decidió el 2026-11-09 mantener la política vigente de 12 a 128. La aplicación debe aplicar la misma política.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-09, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
