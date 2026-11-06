# Spec — pm-073 public-https-endpoint

> Estado: Aprobado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L9 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

Dominio, DNS, TLS, HSTS y redirección de HTTP a HTTPS.

## 2. Dependencias

- [pm-017](../pm-017-trusted-proxy-forwarded-headers/spec.md) — trusted-proxy-forwarded-headers
- [pm-072](../pm-072-cloud-network-security/spec.md) — cloud-network-security
- [pm-076](../pm-076-load-balancer/spec.md) — load-balancer

## 3. Origen

- Fuentes: nuevo.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-05 (ver [traceability.md](../traceability.md)).
- Constitución: §8.7.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-073#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | El certificado TLS es válido. | T085 |
| `CA-02` | HTTP se redirige a HTTPS o se rechaza. | T085 |
| `CA-03` | HSTS está habilitado. | T085 |
| `CA-04` | Los forwarded headers solo se aceptan de los proxies conocidos (relación con pm-017). | T085 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T085 | Nueva | Propuesto | Publicar el endpoint HTTPS |

Detalle en [task.md](task.md).

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: Adrián Adonai García Jiménez (responsable de nube y despliegue), 2026-11-05, alcance: primer despliegue del backend actual (7 endpoints). Pendiente de ratificación de Miguel Ángel Núñez en la revisión del PR.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
