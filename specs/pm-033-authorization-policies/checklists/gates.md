# Gates — pm-033 authorization-policies

> Marcar PASS solo con evidencia reproducible. Estado (2026-11-08): **T022 cerrada con aprobación humana (sin revisión independiente); pm-033 `Cerrado`.** Referencias: [spec.md](../spec.md), [plan.md](../plan.md), [task.md](../task.md).

## Trazabilidad

- [x] Plan aprobado para implementar por el usuario (2026-11-05). El Spec está `En progreso`; su aprobación de cierre sigue pendiente.
- [x] T022 con estado final y evidencia completa (cerrada el 2026-11-08).
- [x] CA ↔ task ↔ test ↔ componente trazables (tabla inferior).

| CA | Task | Pruebas (clase → casos) | Componentes | Resultado |
|---|---|---|---|---|
| `pm-033#CA-01` | T022 | `AuthorizationPoliciesTests` (política por defecto y de respaldo; admite solo roles conocidos), `AuthorizationPolicyTests` (anónimo en `default`/`owner`/`business`/`unmarked` → 401; `unmarked` denegado por defecto; Owner y Business en `default`/`unmarked` → 200), `PublicSurfaceTests` (lista exacta de endpoints públicos en Development y Production; Logout no público), regresiones de Health, OpenAPI/Scalar y Auth | `AuthorizationPolicies`, `UnmatchedRouteResultHandler`, `[AllowAnonymous]` explícitos | Pruebas en verde; cumplimiento formal pendiente de revisión |
| `pm-033#CA-02` | T022 | `AuthorizationPoliciesTests` (conjunto exacto de políticas, ninguna `RequireAdmin`, constantes de rol iguales a `UserRole`, evaluación por rol exacto), `AuthorizationPolicyTests` (Owner en `owner`, Business en `business` → 200) | `AuthorizationPolicies` | Pruebas en verde; cumplimiento formal pendiente |
| `pm-033#CA-03` | T022 | `AuthorizationPolicyTests` (401 anónimo, 403 cruzado Owner/Business, mensaje estándar sin fuga, revocado → 401, Redis caído → 503), `ForgedClaimsTests` (rol `Admin`/`owner`/`OWNER`/`1`/vacío/`Owner `, sin rol, rol arreglo, claim largo de Microsoft, clave ajena, payload editado, `alg: none`, expirado → 401), ruta inexistente → 404 | Políticas + `UseStatusCodePages` + `UnmatchedRouteResultHandler` | Pruebas en verde; cumplimiento formal pendiente |
| `pm-033#CA-04` | T022 | `AuthorizationPolicyTests` (Business en `RequireOwner` → 403 con wrapper), `AuthorizationPoliciesTests` (`Business` y `Admin` no pasan `RequireOwner`) | `RequireOwner` | Verificado con controlador de prueba y, desde el 2026-11-08, con los endpoints reales de mascotas (`PetEndpointsTests`: sin token 401, Business 403); traza de seguridad en pm-045 CA-02 |

## Build y pruebas

- [x] Build Release sin errores ni advertencias (analizadores como error).
- [x] Unit tests: 110 de 110 (sin regresión).
- [x] Integration tests con Atlas de pruebas y Redis aislado: 48/48 sin omitidas el 2026-11-08 (ver «Verificación 2026-11-08»).
- [x] Security tests: 155 de 155 (línea base 135; 20 casos nuevos).
- [x] Contract tests: 69 de 69 (línea base 46; 23 casos nuevos).
- [ ] Fase roja previa: **no ejecutada.** Las pruebas se escribieron junto con el código (desviación del paso 1 del plan, ver plan §15). Las primeras ejecuciones fallaron por regresiones (404/405 convertidos en 401) y condujeron a `UnmatchedRouteResultHandler`.

## Seguridad DevSecOps

- [x] OWASP ASVS L2, control de acceso (V4): fila añadida a `docs/security/asvs-l2-checklist.md`.
- [x] SAST: build Release con analizadores de seguridad como error, 0 advertencias.
- [x] Secret scan (`SecretScanTests`): pasa dentro de las 155 pruebas de seguridad.
- [x] NuGet vulnerability/license audit: `scripts/quality-gate.ps1 -AuditOnly` en verde el 2026-11-08 (vulnerabilidades desde High, desuso, 50 licencias).
- [x] Container/IaC scan: no aplica en este Spec.
- [ ] Sin hallazgos críticos/altos sin tratamiento aprobado: pendiente de la revisión independiente de seguridad.
- [x] Ninguna decisión de autorización usa cabeceras, cuerpo ni query: solo el claim `role` del JWT validado.
- [x] `scripts/quality-gate.ps1`: auditoría NuGet en el host y build, formato y suites en el contenedor SDK (Smart App Control bloquea los binarios en el host). El formato solo falla por `ENDOFLINE` del árbol CRLF de Windows (4045 avisos, ningún otro tipo); en CI (Linux, LF) no aplica.

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

- Security reviewer: no realizada (el usuario aprobó el cierre sin ella).
- Traceability reviewer: no realizada (ídem).
- Aprobación humana: 2026-11-08.
- Estado final: Cerrado (T022 cerrada y aprobada)

## Verificación 2026-11-08 (alcance completo)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, repositorio en solo lectura y copia sin `.env`; Atlas de pruebas real (`--env-file .env`, sin imprimir valores) y dos Redis temporales con contraseñas aleatorias (los de Compose usan contraseñas marcadas como ficticias y las pruebas las omiten a propósito) |
| Unit | 152/152 | Mismo contenedor |
| Security | 155/155 | Mismo contenedor |
| Contract | 142/142 | Mismo contenedor |
| Integration | 48/48, 0 omitidas (Atlas real y Redis temporales) | Mismo contenedor |
| Aplicación de `RequireOwner` a mascotas | `PetsController` con `[Authorize(Policy = RequireOwner)]`; `PetEndpointsTests`: sin token 401, Business 403, Owner propio 200/201 | Contract 142/142 |
| Auditoría NuGet | vulnerabilidades (desde High), desuso y 50 licencias en verde | `powershell scripts/quality-gate.ps1 -AuditOnly` en el host |
| Gate SDD | OK | `scripts/check-sdd.ps1` y `scripts/check-sdd.tests.ps1` |

- Observación: la fase roja previa (pruebas antes del código) no se ejecutó; sigue siendo una desviación documentada en `plan.md` §15.
- [ ] Revisión independiente de seguridad y trazabilidad (no realizada; el usuario aprobó el cierre sin ella).
- [x] Aprobación humana del cierre de T022 (2026-11-08).
