# Spec — pm-088 transactional-email-adapter

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.3.0 | Nivel de ejecución: L3 (los números identifican, no ordenan) | Creado el 2026-11-09

## 1. Objetivo

Existe un puerto de aplicación para enviar correos y un adaptador al proveedor de costo $0 elegido en el plan.

## 2. Dependencias

- [pm-004](../pm-004-external-configuration-secrets/spec.md) — external-configuration-secrets
- [pm-005](../pm-005-dependency-injection-composition/spec.md) — dependency-injection-composition

## 3. Origen

- Fuentes: Documento v3: RF-04, HU-003; decisión del usuario del 2026-11-09 (el correo se usa solo para recuperar contraseña; sin verificación de correo).
- Los requisitos provienen de las fuentes de producto vigentes (`docs/product-scope.md`); ninguno proviene de la estructura anterior por slices.
- Constitución: §4, §9.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-088#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Los casos de uso dependen de un puerto de envío de correo y no del proveedor. | T106 |
| `CA-02` | La credencial del proveedor se lee solo de configuración segura del entorno y nunca se registra. | T106 |
| `CA-03` | Un fallo o una espera agotada del proveedor no rompe la solicitud ni revela si el correo existe. | T106 |
| `CA-04` | El proveedor, su costo $0 y su licencia se verifican en fuentes oficiales y se registran en `plan.md` (`AGENTS.md` §4). | T106 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T106 | Nueva | Verificado (2026-11-09; revisión independiente sin hallazgos bloqueantes y aprobación humana en el chat) | Implementar el puerto y el adaptador de envío de correo |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- Candidatos evaluados el 2026-11-09: Resend (3 000 correos al mes, 100 al día) y Brevo; ambos exigen un dominio de envío verificado (SPF y DKIM). El usuario no tiene dominio: puede reclamar uno gratis con el GitHub Student Developer Pack. Sin dominio solo funcionan las pruebas.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Miguel (usuario), 2026-11-09, aprobación en el chat de la sesión (specs completos del primer hito).
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec.
