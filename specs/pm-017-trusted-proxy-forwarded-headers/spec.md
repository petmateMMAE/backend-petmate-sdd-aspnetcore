# Spec — pm-017 trusted-proxy-forwarded-headers

> Estado: Verificado | Constitución: `.specify/memory/constitution.md` v1.1.0 | Nivel de ejecución: L3 (los números identifican, no ordenan) | Migrado el 2026-11-05

## 1. Objetivo

IP real del cliente detrás de proxies confiables configurados.

## 2. Dependencias

- [pm-005](../pm-005-dependency-injection-composition/spec.md) — dependency-injection-composition

## 3. Origen

- Fuentes: 01-initial-authentication/task.md T018–T021 (evidencia indirecta); `RateLimitAndProxyTests`.
- Los documentos de la estructura anterior se conservan sin cambios en [`specs/_historico/`](../_historico/README.md).
- Requisitos académicos: AC-05 (ver [traceability.md](../traceability.md)).
- Constitución: §7, §8.

## 4. Criterios de aceptación

Cada criterio se cita como `pm-017#CA-NN`. La columna final indica la Task que lo cubre.

| CA | Criterio | Task propietaria |
|---|---|---|
| `CA-01` | Con `TRUSTED_PROXIES` configurada, la IP del cliente se toma de `X-Forwarded-For` únicamente de los proxies listados. | T047 |
| `CA-02` | Sin `TRUSTED_PROXIES`, las cabeceras `X-Forwarded-*` se ignoran. | T047 |
| `CA-03` | Un valor inválido de `TRUSTED_PROXIES` impide el arranque sin mostrar valores. | T047 |
| `CA-04` | Existe evidencia directa de que un cliente no confiable no puede falsificar su IP mediante `X-Forwarded-For`. | T097 |

## 5. Tasks

| Task | Tipo | Estado | Título |
|---|---|---|---|
| T047 | Retrospectiva | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva de proxies confiables |
| T097 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | Verificar el rechazo de cabeceras reenviadas de proxies no confiables |

Detalle en [task.md](task.md).

## 6. Notas, limitaciones y decisiones pendientes

- CA-04 se atribuye a T097 porque la evidencia existente es indirecta.

## 7. Aprobación

- [x] Requisitos y criterios revisados por el usuario.
- [x] Constitución cumplida.
- Revisor/usuario: usuario del proyecto, aprobado el 2026-11-08.
- Nota: `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras aprobar este Spec. La evidencia histórica de esta funcionalidad permanece en `specs/_historico/`.
