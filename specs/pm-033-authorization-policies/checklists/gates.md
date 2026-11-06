# Gates — pm-033 authorization-policies

> Marcar PASS solo con evidencia reproducible. Estado: **implementación y pruebas ejecutadas; revisiones independientes y aprobación humana pendientes. T022 no está cerrada.** Referencias: [spec.md](../spec.md), [plan.md](../plan.md), [task.md](../task.md).

## Trazabilidad

- [x] Plan aprobado para implementar por el usuario (2026-11-05). El Spec está `En progreso`; su aprobación de cierre sigue pendiente.
- [ ] T022 con estado final y evidencia completa (permanece `En progreso`).
- [x] CA ↔ task ↔ test ↔ componente trazables (tabla inferior).

| CA | Task | Pruebas (clase → casos) | Componentes | Resultado |
|---|---|---|---|---|
| `pm-033#CA-01` | T022 | `AuthorizationPoliciesTests` (política por defecto y de respaldo; admite solo roles conocidos), `AuthorizationPolicyTests` (anónimo en `default`/`owner`/`business`/`unmarked` → 401; `unmarked` denegado por defecto; Owner y Business en `default`/`unmarked` → 200), `PublicSurfaceTests` (lista exacta de endpoints públicos en Development y Production; Logout no público), regresiones de Health, OpenAPI/Scalar y Auth | `AuthorizationPolicies`, `UnmatchedRouteResultHandler`, `[AllowAnonymous]` explícitos | Pruebas en verde; cumplimiento formal pendiente de revisión |
| `pm-033#CA-02` | T022 | `AuthorizationPoliciesTests` (conjunto exacto de políticas, ninguna `RequireAdmin`, constantes de rol iguales a `UserRole`, evaluación por rol exacto), `AuthorizationPolicyTests` (Owner en `owner`, Business en `business` → 200) | `AuthorizationPolicies` | Pruebas en verde; cumplimiento formal pendiente |
| `pm-033#CA-03` | T022 | `AuthorizationPolicyTests` (401 anónimo, 403 cruzado Owner/Business, mensaje estándar sin fuga, revocado → 401, Redis caído → 503), `ForgedClaimsTests` (rol `Admin`/`owner`/`OWNER`/`1`/vacío/`Owner `, sin rol, rol arreglo, claim largo de Microsoft, clave ajena, payload editado, `alg: none`, expirado → 401), ruta inexistente → 404 | Políticas + `UseStatusCodePages` + `UnmatchedRouteResultHandler` | Pruebas en verde; cumplimiento formal pendiente |
| `pm-033#CA-04` | T022 | `AuthorizationPolicyTests` (Business en `RequireOwner` → 403 con wrapper), `AuthorizationPoliciesTests` (`Business` y `Admin` no pasan `RequireOwner`) | `RequireOwner` | Verificado con controlador de prueba; la aplicación a endpoints de mascotas es de pm-040 a pm-044 (traza en pm-045 CA-02) |

## Build y pruebas

- [x] Build Release sin errores ni advertencias (analizadores como error).
- [x] Unit tests: 110 de 110 (sin regresión).
- [ ] Integration tests con Atlas de pruebas y Redis aislado: **no ejecutadas con servicios reales**. 10 pasan y 29 se omiten por falta de credenciales (igual que la línea base). Ejecutar con credenciales antes del cierre.
- [x] Security tests: 155 de 155 (línea base 135; 20 casos nuevos).
- [x] Contract tests: 69 de 69 (línea base 46; 23 casos nuevos).
- [ ] Fase roja previa: **no ejecutada.** Las pruebas se escribieron junto con el código (desviación del paso 1 del plan, ver plan §15). Las primeras ejecuciones fallaron por regresiones (404/405 convertidos en 401) y condujeron a `UnmatchedRouteResultHandler`.

## Seguridad DevSecOps

- [x] OWASP ASVS L2, control de acceso (V4): fila añadida a `docs/security/asvs-l2-checklist.md`.
- [x] SAST: build Release con analizadores de seguridad como error, 0 advertencias.
- [x] Secret scan (`SecretScanTests`): pasa dentro de las 155 pruebas de seguridad.
- [ ] NuGet vulnerability/license audit: **no ejecutada** en esta sesión (no se añadieron paquetes; el análisis requiere `scripts/quality-gate.ps1`).
- [ ] Container/IaC scan: no aplica en este Spec.
- [ ] Sin hallazgos críticos/altos sin tratamiento aprobado: pendiente de la revisión independiente de seguridad.
- [x] Ninguna decisión de autorización usa cabeceras, cuerpo ni query: solo el claim `role` del JWT validado.
- [ ] `scripts/quality-gate.ps1` completo: **no ejecutado en el host** (limitación conocida de Smart App Control). Sustituido por contenedor SDK (build, pruebas) y `dotnet format --verify-no-changes` en el contenedor con finales de línea LF (exit 0).

## Operación y documentación

- [x] Response Wrapper y códigos 401/403 correctos con `data: null`.
- [x] `docs/api/endpoints.md` actualizado (sección «Autorización» y lista de endpoints públicos).
- [x] Lista de endpoints públicos de la API real igual a la del plan §3.6 (`PublicSurfaceTests`).
- [x] Costos y licencias $0 verificados: sin dependencias nuevas.
- [x] Sin secretos ni PII en documentos ni en logs añadidos.
- [x] `Memoria.md` actualizada con el estado (T022 no cerrada).
- [x] `scripts/check-sdd.ps1` en OK (84 Specs, 97 Tasks, 34 archivos históricos).

## Evidencia

| Gate | Resultado | Comando/enlace/evidencia | Fecha |
|---|---|---|---|
| Línea base previa | Unit 110, Contract 46, Security 135, Integration 10 pasan y 29 omitidas | Contenedor SDK, sin `.env` | 2026-11-05 |
| Build y suites tras implementar | Unit 110/110, Integration 10 + 29 omitidas, Security 155/155, Contract 69/69, 0 fallos | `docker run --rm -v <repo>:/src:ro mcr.microsoft.com/dotnet/sdk:10.0` copiando el árbol a un directorio temporal sin `.env`, `bin`, `obj` ni `.git`, y `dotnet test PetMate.sln -c Release` | 2026-11-05 |
| Formato | exit 0 | Mismo contenedor, `dotnet format PetMate.sln --verify-no-changes` con finales de línea LF (con CRLF del árbol de trabajo de Windows `ENDOFLINE` falla de forma preexistente) | 2026-11-05 |
| Gate SDD | OK | `powershell -File scripts/check-sdd.ps1` | 2026-11-05 |
| Revisión independiente de seguridad | Pendiente | — | — |
| Revisión independiente de trazabilidad | Pendiente | — | — |

## Revisión y aprobación

- Security reviewer: pendiente.
- Traceability reviewer: pendiente.
- Aprobación humana: pendiente.
- Estado final: En progreso (T022 no cerrada)
