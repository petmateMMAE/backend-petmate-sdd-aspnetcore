# Matriz de trazabilidad — PetMate Backend

> Vincula requisitos académicos (`AC-NN`), la Constitución v1.1.0 (`CON-§N`), Specs, Tasks, criterios de aceptación y evidencia. Mantener sincronizada con [README.md](README.md). Los criterios se citan como `pm-NNN#CA-NN`.

**Nota:** la lista `AC-NN` recoge los requisitos académicos de backend indicados por el usuario durante la reorganización; el documento académico fuente no consta en el repositorio. Si difiere, esta lista debe corregirse.

## 1. Requisitos académicos → Constitución → Specs → Tasks

| AC | Requisito | Constitución | Specs | Tasks propietarias |
|---|---|---|---|---|
| AC-01 | Plataforma cloud: Microsoft Azure con la suscripción Azure for Students (sin AWS ni GCP) | CON-§8.1, §8.2 | pm-062, pm-063, pm-084 | T074, T075, T096 |
| AC-02 | Costo de $0 MXN de desembolso directo con servicios incluidos y créditos de Azure for Students | CON-§8.1, §8.2, §8.5, §8.6 | pm-062, pm-063, pm-065, pm-084 | T074, T075, T077, T096 |
| AC-03 | Dos o más instancias del backend | CON-§8.3 | pm-075, pm-084 | T087, T096 |
| AC-04 | Load Balancer o equivalente de Azure | CON-§8.3 | pm-076, pm-084 | T088, T096 |
| AC-05 | HTTPS con certificado válido | CON-§8.7 | pm-017, pm-073, pm-084 | T047, T085, T096, T097 |
| AC-06 | CI obligatorio con controles de seguridad | CON-§8.13, §9, §10 | pm-018, pm-019, pm-020, pm-021, pm-055, pm-056, pm-057, pm-058, pm-060 | T048, T049, T050, T051, T066, T067, T068, T069, T070, T072 |
| AC-07 | CD obligatorio | CON-§8.13 | pm-077, pm-084 | T089, T096 |
| AC-08 | Observabilidad cloud | CON-§8.14 | pm-054, pm-078, pm-084 | T065, T090, T096 |
| AC-09 | Versionado de releases y trazabilidad de imágenes desplegadas | CON-§8.12, §8.13 | pm-059, pm-074, pm-077 | T071, T086, T089 |
| AC-10 | Procedimiento reproducible de creación y destrucción de infraestructura | CON-§8.15 | pm-064, pm-066, pm-067, pm-083 | T076, T078, T079, T095 |
| AC-11 | Gestión segura de secretos | CON-§4, §8.11 | pm-004, pm-027, pm-068, pm-069 | T004, T016, T080, T081 |
| AC-12 | Redis administrado con TLS y autenticación; datos no expuestos a Internet abierta | CON-§5, §8.8, §8.9 | pm-013, pm-028, pm-050, pm-070, pm-071, pm-072 | T017, T044, T061, T082, T083, T084 |
| AC-13 | Infraestructura como código (decisión de proyecto, preferentemente Terraform) | CON-§8.15 | pm-066 | T078 |
| AC-14 | Primer hito funcional (11 endpoints) | CON-§11 | pm-037, pm-038, pm-039, pm-040, pm-041, pm-042, pm-043, pm-044, pm-045, pm-046, pm-047 | T026, T027, T028, T029, T030, T031, T032, T033, T034, T035, T036, T037, T038, T040, T041, T058 |
| AC-15 | Almacenamiento de imágenes en la nube y CDN para contenido público | CON-§8.10 | pm-079, pm-080, pm-081, pm-082 | T091, T092, T093, T094 |
| AC-16 | Protección de datos clínicos y persistencia durante el teardown | CON-§8.15 | pm-083 | T095 |

## 2. Constitución → Specs

| Sección | Specs |
|---|---|
| CON-§2 | pm-005 |
| CON-§3 | pm-001, pm-002, pm-003, pm-005, pm-006, pm-015 |
| CON-§4 | pm-004, pm-012, pm-016, pm-024, pm-039, pm-052, pm-064, pm-068, pm-069 |
| CON-§5 | pm-013, pm-016, pm-028, pm-050, pm-051, pm-070, pm-071 |
| CON-§6 | pm-022, pm-023, pm-024, pm-025, pm-026, pm-027, pm-028, pm-029, pm-030, pm-031, pm-032, pm-033, pm-034, pm-035, pm-036, pm-045, pm-051, pm-054, pm-080, pm-085, pm-086, pm-087, pm-088, pm-089, pm-090, pm-091, pm-092, pm-093, pm-094, pm-095 |
| CON-§7 | pm-007, pm-008, pm-009, pm-010, pm-011, pm-014, pm-017, pm-029, pm-030, pm-031, pm-032, pm-036, pm-037, pm-038, pm-040, pm-041, pm-042, pm-043, pm-044, pm-047, pm-048, pm-049, pm-061, pm-105, pm-106, pm-107, pm-108, pm-109, pm-110, pm-111, pm-112, pm-113, pm-114, pm-115, pm-116, pm-117, pm-118, pm-119, pm-120, pm-121, pm-122, pm-123, pm-124, pm-125, pm-126, pm-127, pm-128, pm-129, pm-130, pm-131, pm-132, pm-133, pm-134, pm-135, pm-136, pm-137, pm-138, pm-139, pm-140, pm-141, pm-142, pm-143, pm-144, pm-145, pm-146, pm-147, pm-148, pm-149, pm-150, pm-151, pm-152, pm-153, pm-154, pm-155, pm-156, pm-157, pm-158, pm-159, pm-160, pm-161, pm-162, pm-163, pm-164, pm-165, pm-166, pm-167 |
| CON-§8 | pm-014, pm-015, pm-017, pm-064, pm-084 |
| CON-§8.1 | pm-062, pm-063 |
| CON-§8.2 | pm-062, pm-063, pm-065 |
| CON-§8.3 | pm-075, pm-076 |
| CON-§8.5 | pm-062, pm-065 |
| CON-§8.6 | pm-062, pm-065 |
| CON-§8.7 | pm-073 |
| CON-§8.8 | pm-069, pm-072 |
| CON-§8.9 | pm-070, pm-071 |
| CON-§8.10 | pm-079, pm-080, pm-081, pm-082 |
| CON-§8.11 | pm-068 |
| CON-§8.12 | pm-074 |
| CON-§8.13 | pm-055, pm-057, pm-059, pm-077 |
| CON-§8.14 | pm-078 |
| CON-§8.15 | pm-066, pm-067, pm-083 |
| CON-§9 | pm-003, pm-018, pm-019, pm-020, pm-021, pm-054, pm-055, pm-057, pm-058 |
| CON-§10 | pm-021, pm-035, pm-053, pm-055, pm-056, pm-060 |
| CON-§11 | pm-037, pm-038, pm-039, pm-040, pm-041, pm-042, pm-043, pm-044, pm-045, pm-046, pm-085, pm-086, pm-087, pm-088, pm-089, pm-090, pm-091, pm-092, pm-093, pm-094, pm-095, pm-096, pm-097, pm-098, pm-099, pm-100, pm-101, pm-102, pm-103, pm-104 |
| CON-§12.3 | pm-060 |

## 3. Distinciones históricas explícitas

| Task | Spec principal | Criterios | Observación |
|---|---|---|---|
| T007 (histórica) | pm-008 | pm-008#CA-01..04 | Alcance histórico: OpenAPI/Scalar. No demuestra por sí sola los criterios de rutas de pm-007; su alcance no se modifica. |
| T042 (retrospectiva) | pm-007 | pm-007#CA-01..03 | Documenta la evidencia existente de rutas versionadas (origen T007) sin cerrarse. |
| T055 (nueva) | pm-007 | pm-007#CA-04..05 | Cubre los criterios que faltan (verificación automática de rutas y reglas de nombrado). |
| T022 (histórica) | pm-033 | pm-033#CA-01..04 | Cerrado y aprobado (2026-11-08, aprobación humana); cumple: la aplicación de Owner a los endpoints de mascotas se cumple en pm-040 a pm-044 y se traza con pm-045 CA-02. |
| T012 (histórica) | pm-055 | workflow, Trivy y Syft en `pipeline.yml` | Cerrado y aprobado (2026-11-08, aprobación humana); T066 y T067 cubren los criterios de pm-055. |
| T047 / T097 | pm-017 | pm-017#CA-01..03 / CA-04 | Evidencia existente indirecta; la directa contra spoofing es trabajo nuevo. |
| T054 / T057 | pm-036 | pm-036#CA-01,02,04 / CA-03 | Rate limiting de autenticación existente; la matriz global la entregó T057 (cerrada el 2026-11-08). |

## 4. Criterios de aceptación → Tasks → evidencia

| CA | Task propietaria | Tipo | Estado de la Task | Evidencia |
|---|---|---|---|---|
| pm-001#CA-01 | T001 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-001#CA-02 | T001 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-001#CA-03 | T001 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-002#CA-01 | T002 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-002#CA-02 | T002 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-003#CA-01 | T003 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-003#CA-02 | T003 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-003#CA-03 | T003 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-004#CA-01 | T004 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-004#CA-02 | T004 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-004#CA-03 | T004 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-004#CA-04 | T004 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-005#CA-01 | T005 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-005#CA-02 | T005 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-006#CA-01 | T006 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-006#CA-02 | T006 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-006#CA-03 | T006 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-007#CA-01 | T042 | Retrospectiva | Aprobada y documentada | [gates](pm-007-api-route-conventions/checklists/gates.md) |
| pm-007#CA-02 | T042 | Retrospectiva | Aprobada y documentada | [gates](pm-007-api-route-conventions/checklists/gates.md) |
| pm-007#CA-03 | T042 | Retrospectiva | Aprobada y documentada | [gates](pm-007-api-route-conventions/checklists/gates.md) |
| pm-007#CA-04 | T055 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-007-api-route-conventions/checklists/gates.md) |
| pm-007#CA-05 | T055 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-007-api-route-conventions/checklists/gates.md) |
| pm-008#CA-01 | T007 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-008#CA-02 | T007 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-008#CA-03 | T007 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-008#CA-04 | T007 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-009#CA-01 | T008 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-009#CA-02 | T008 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-009#CA-03 | T008 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-010#CA-01 | T043 | Retrospectiva | Aprobada y documentada | [gates](pm-010-global-exception-handling/checklists/gates.md) |
| pm-010#CA-02 | T043 | Retrospectiva | Aprobada y documentada | [gates](pm-010-global-exception-handling/checklists/gates.md) |
| pm-010#CA-03 | T043 | Retrospectiva | Aprobada y documentada | [gates](pm-010-global-exception-handling/checklists/gates.md) |
| pm-010#CA-04 | T043 | Retrospectiva | Aprobada y documentada | [gates](pm-010-global-exception-handling/checklists/gates.md) |
| pm-011#CA-01 | T009 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana; política 400 ratificada) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-011#CA-02 | T009 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana; política 400 ratificada) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-011#CA-03 | T009 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana; política 400 ratificada) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-012#CA-01 | T010 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-012#CA-02 | T010 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-012#CA-03 | T010 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-013#CA-01 | T044 | Retrospectiva | Aprobada y documentada | [gates](pm-013-redis-dual-connectivity/checklists/gates.md) |
| pm-013#CA-02 | T044 | Retrospectiva | Aprobada y documentada | [gates](pm-013-redis-dual-connectivity/checklists/gates.md) |
| pm-013#CA-03 | T044 | Retrospectiva | Aprobada y documentada | [gates](pm-013-redis-dual-connectivity/checklists/gates.md) |
| pm-014#CA-01 | T045 | Retrospectiva | Aprobada y documentada | [gates](pm-014-health-probes/checklists/gates.md) |
| pm-014#CA-02 | T045 | Retrospectiva | Aprobada y documentada | [gates](pm-014-health-probes/checklists/gates.md) |
| pm-014#CA-03 | T045 | Retrospectiva | Aprobada y documentada | [gates](pm-014-health-probes/checklists/gates.md) |
| pm-015#CA-01 | T046 | Retrospectiva | Aprobada y documentada | [gates](pm-015-api-container-image/checklists/gates.md) |
| pm-015#CA-02 | T046 | Retrospectiva | Aprobada y documentada | [gates](pm-015-api-container-image/checklists/gates.md) |
| pm-015#CA-03 | T046 | Retrospectiva | Aprobada y documentada | [gates](pm-015-api-container-image/checklists/gates.md) |
| pm-016#CA-01 | T011 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-016#CA-02 | T011 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-016#CA-03 | T011 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-017#CA-01 | T047 | Retrospectiva | Aprobada y documentada | [gates](pm-017-trusted-proxy-forwarded-headers/checklists/gates.md) |
| pm-017#CA-02 | T047 | Retrospectiva | Aprobada y documentada | [gates](pm-017-trusted-proxy-forwarded-headers/checklists/gates.md) |
| pm-017#CA-03 | T047 | Retrospectiva | Aprobada y documentada | [gates](pm-017-trusted-proxy-forwarded-headers/checklists/gates.md) |
| pm-017#CA-04 | T097 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-017-trusted-proxy-forwarded-headers/checklists/gates.md) |
| pm-018#CA-01 | T048 | Retrospectiva | Aprobada y documentada | [gates](pm-018-static-analysis-sast/checklists/gates.md) |
| pm-018#CA-02 | T048 | Retrospectiva | Aprobada y documentada | [gates](pm-018-static-analysis-sast/checklists/gates.md) |
| pm-019#CA-01 | T049 | Retrospectiva | Aprobada y documentada | [gates](pm-019-secret-scanning/checklists/gates.md) |
| pm-019#CA-02 | T049 | Retrospectiva | Aprobada y documentada | [gates](pm-019-secret-scanning/checklists/gates.md) |
| pm-020#CA-01 | T050 | Retrospectiva | Aprobada y documentada | [gates](pm-020-dependency-license-audit/checklists/gates.md) |
| pm-020#CA-02 | T050 | Retrospectiva | Aprobada y documentada | [gates](pm-020-dependency-license-audit/checklists/gates.md) |
| pm-020#CA-03 | T050 | Retrospectiva | Aprobada y documentada | [gates](pm-020-dependency-license-audit/checklists/gates.md) |
| pm-021#CA-01 | T051 | Retrospectiva | Aprobada y documentada | [gates](pm-021-local-quality-gate/checklists/gates.md) |
| pm-021#CA-02 | T051 | Retrospectiva | Aprobada y documentada | [gates](pm-021-local-quality-gate/checklists/gates.md) |
| pm-022#CA-01 | T013 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-022#CA-02 | T013 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-023#CA-01 | T052 | Retrospectiva | Aprobada y documentada | [gates](pm-023-refresh-session-domain/checklists/gates.md) |
| pm-023#CA-02 | T052 | Retrospectiva | Aprobada y documentada | [gates](pm-023-refresh-session-domain/checklists/gates.md) |
| pm-023#CA-03 | T052 | Retrospectiva | Aprobada y documentada | [gates](pm-023-refresh-session-domain/checklists/gates.md) |
| pm-024#CA-01 | T014 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-024#CA-02 | T014 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-024#CA-03 | T014 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-025#CA-01 | T053 | Retrospectiva | Aprobada y documentada | [gates](pm-025-refresh-sessions-persistence/checklists/gates.md) |
| pm-025#CA-02 | T053 | Retrospectiva | Aprobada y documentada | [gates](pm-025-refresh-sessions-persistence/checklists/gates.md) |
| pm-025#CA-03 | T053 | Retrospectiva | Aprobada y documentada | [gates](pm-025-refresh-sessions-persistence/checklists/gates.md) |
| pm-025#CA-04 | T053 | Retrospectiva | Aprobada y documentada | [gates](pm-025-refresh-sessions-persistence/checklists/gates.md) |
| pm-026#CA-01 | T015 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-026#CA-02 | T015 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-026#CA-03 | T015 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-026#CA-04 | T015 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-027#CA-01 | T016 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-027#CA-02 | T016 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-027#CA-03 | T016 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-028#CA-01 | T017 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-028#CA-02 | T017 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-028#CA-03 | T017 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-028#CA-04 | T098 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana; sin revisión independiente de seguridad) | [gates](pm-028-token-revocation-redis/checklists/gates.md) |
| pm-029#CA-01 | T018 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-029#CA-02 | T018 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-029#CA-03 | T018 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-029#CA-04 | T018 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-029#CA-05 | T102 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana; sin revisión independiente de seguridad) | [gates](pm-029-user-registration/checklists/gates.md) |
| pm-030#CA-01 | T019 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-030#CA-02 | T019 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-030#CA-03 | T019 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-031#CA-01 | T020 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-031#CA-02 | T020 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-031#CA-03 | T020 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-031#CA-04 | T099 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana; sin revisión independiente de seguridad) | [gates](pm-031-refresh-token-rotation/checklists/gates.md) |
| pm-032#CA-01 | T021 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-032#CA-02 | T021 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-032#CA-03 | T021 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-032#CA-04 | T021 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-033#CA-01 | T022 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-033 gates](pm-033-authorization-policies/checklists/gates.md) |
| pm-033#CA-02 | T022 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-033 gates](pm-033-authorization-policies/checklists/gates.md) |
| pm-033#CA-03 | T022 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-033 gates](pm-033-authorization-policies/checklists/gates.md) |
| pm-033#CA-04 | T022 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-033 gates](pm-033-authorization-policies/checklists/gates.md) |
| pm-034#CA-01 | T056 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-034-auth-brute-force-protection/checklists/gates.md) |
| pm-034#CA-02 | T056 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-034-auth-brute-force-protection/checklists/gates.md) |
| pm-034#CA-03 | T056 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-034-auth-brute-force-protection/checklists/gates.md) |
| pm-034#CA-04 | T056 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-034-auth-brute-force-protection/checklists/gates.md) |
| pm-034#CA-05 | T101 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana; sin revisión independiente de seguridad) | [gates](pm-034-auth-brute-force-protection/checklists/gates.md) |
| pm-034#CA-06 | T101 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana; sin revisión independiente de seguridad) | [gates](pm-034-auth-brute-force-protection/checklists/gates.md) |
| pm-035#CA-01 | T025 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-035-auth-session-lifecycle-acceptance/checklists/gates.md) |
| pm-035#CA-02 | T025 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-035-auth-session-lifecycle-acceptance/checklists/gates.md) |
| pm-036#CA-01 | T054 | Retrospectiva | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | [gates](pm-036-rate-limiting/checklists/gates.md) |
| pm-036#CA-02 | T054 | Retrospectiva | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | [gates](pm-036-rate-limiting/checklists/gates.md) |
| pm-036#CA-03 | T057 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-036-rate-limiting/checklists/gates.md) |
| pm-036#CA-04 | T054 | Retrospectiva | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | [gates](pm-036-rate-limiting/checklists/gates.md) |
| pm-036#CA-05 | T100 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana; sin revisión independiente de seguridad) | [gates](pm-036-rate-limiting/checklists/gates.md) |
| pm-036#CA-06 | T100 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana; sin revisión independiente de seguridad) | [gates](pm-036-rate-limiting/checklists/gates.md) |
| pm-037#CA-01 | T026 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-037 gates](pm-037-owner-profile-read/checklists/gates.md) |
| pm-037#CA-02 | T026 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-037 gates](pm-037-owner-profile-read/checklists/gates.md) |
| pm-038#CA-01 | T027 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-038 gates](pm-038-owner-profile-update/checklists/gates.md) |
| pm-038#CA-02 | T027 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-038 gates](pm-038-owner-profile-update/checklists/gates.md) |
| pm-038#CA-03 | T028 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-038 gates](pm-038-owner-profile-update/checklists/gates.md) |
| pm-039#CA-01 | T029 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-039 gates](pm-039-pet-domain-persistence/checklists/gates.md) |
| pm-039#CA-02 | T029 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-039 gates](pm-039-pet-domain-persistence/checklists/gates.md) |
| pm-039#CA-03 | T029 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-039 gates](pm-039-pet-domain-persistence/checklists/gates.md) |
| pm-039#CA-04 | T036 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | `PetTests` y `PetHandlersTests`; evidencia en [pm-039 gates](pm-039-pet-domain-persistence/checklists/gates.md) |
| pm-039#CA-05 | T037 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | `PetPersistenceIntegrationTests` (Atlas); evidencia en [pm-039 gates](pm-039-pet-domain-persistence/checklists/gates.md) |
| pm-040#CA-01 | T030 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-040 gates](pm-040-pet-create/checklists/gates.md) |
| pm-040#CA-02 | T030 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-040 gates](pm-040-pet-create/checklists/gates.md) |
| pm-040#CA-03 | T030 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-040 gates](pm-040-pet-create/checklists/gates.md) |
| pm-040#CA-04 | T030 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-040 gates](pm-040-pet-create/checklists/gates.md) |
| pm-041#CA-01 | T031 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-041-pet-list/checklists/gates.md) |
| pm-041#CA-02 | T031 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-041-pet-list/checklists/gates.md) |
| pm-042#CA-01 | T032 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-042 gates](pm-042-pet-detail/checklists/gates.md) |
| pm-042#CA-02 | T032 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-042 gates](pm-042-pet-detail/checklists/gates.md) |
| pm-043#CA-01 | T033 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-043 gates](pm-043-pet-update/checklists/gates.md) |
| pm-043#CA-02 | T033 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-043 gates](pm-043-pet-update/checklists/gates.md) |
| pm-044#CA-01 | T034 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-044 gates](pm-044-pet-soft-delete/checklists/gates.md) |
| pm-044#CA-02 | T034 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-044 gates](pm-044-pet-soft-delete/checklists/gates.md) |
| pm-044#CA-03 | T034 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [pm-044 gates](pm-044-pet-soft-delete/checklists/gates.md) |
| pm-045#CA-01 | T035 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-045-pet-ownership-bola/checklists/gates.md) |
| pm-045#CA-02 | T038 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-045-pet-ownership-bola/checklists/gates.md) |
| pm-045#CA-04 | T035 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-045-pet-ownership-bola/checklists/gates.md) |
| pm-045#CA-03 | T040 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-045-pet-ownership-bola/checklists/gates.md) |
| pm-046#CA-01 | T041 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-046-first-milestone-acceptance/checklists/gates.md) |
| pm-046#CA-02 | T041 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-046-first-milestone-acceptance/checklists/gates.md) |
| pm-047#CA-01 | T058 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-047-pagination-convention/checklists/gates.md) |
| pm-047#CA-02 | T058 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-047-pagination-convention/checklists/gates.md) |
| pm-047#CA-03 | T058 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-047-pagination-convention/checklists/gates.md) |
| pm-048#CA-01 | T059 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-048-response-compression/checklists/gates.md) |
| pm-048#CA-02 | T059 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-048-response-compression/checklists/gates.md) |
| pm-048#CA-03 | T059 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-048-response-compression/checklists/gates.md) |
| pm-049#CA-01 | T060 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-049-json-serialization-performance/checklists/gates.md) |
| pm-049#CA-02 | T060 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-049-json-serialization-performance/checklists/gates.md) |
| pm-050#CA-01 | T061 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-050-distributed-cache-infrastructure/checklists/gates.md) |
| pm-050#CA-02 | T061 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-050-distributed-cache-infrastructure/checklists/gates.md) |
| pm-050#CA-03 | T061 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-050-distributed-cache-infrastructure/checklists/gates.md) |
| pm-051#CA-01 | T062 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-051-cache-policy-data-classification/checklists/gates.md) |
| pm-051#CA-02 | T062 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-051-cache-policy-data-classification/checklists/gates.md) |
| pm-051#CA-03 | T062 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-051-cache-policy-data-classification/checklists/gates.md) |
| pm-052#CA-01 | T063 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-052-database-indexing-efficiency/checklists/gates.md) |
| pm-052#CA-02 | T063 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-052-database-indexing-efficiency/checklists/gates.md) |
| pm-053#CA-01 | T064 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-053-api-performance-baseline/checklists/gates.md) |
| pm-053#CA-02 | T064 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-053-api-performance-baseline/checklists/gates.md) |
| pm-053#CA-03 | T064 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-053-api-performance-baseline/checklists/gates.md) |
| pm-054#CA-01 | T065 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-054-structured-logging-pii-safe/checklists/gates.md) |
| pm-054#CA-02 | T065 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-054-structured-logging-pii-safe/checklists/gates.md) |
| pm-054#CA-03 | T065 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-054-structured-logging-pii-safe/checklists/gates.md) |
| pm-055#CA-01 | T066 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-055-ci-pull-request-pipeline/checklists/gates.md) |
| pm-055#CA-02 | T066 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-055-ci-pull-request-pipeline/checklists/gates.md) |
| pm-055#CA-03 | T067 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-055#CA-04 | T067 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-056#CA-01 | T068 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana; CA-02 pendiente de enmienda) | [gates](pm-056-test-coverage-baseline/checklists/gates.md) |
| pm-056#CA-02 | T068 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana; CA-02 pendiente de enmienda) | [gates](pm-056-test-coverage-baseline/checklists/gates.md) |
| pm-057#CA-01 | T069 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-057-container-image-scan/checklists/gates.md) |
| pm-057#CA-02 | T069 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-057-container-image-scan/checklists/gates.md) |
| pm-058#CA-01 | T070 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-058-sbom-generation/checklists/gates.md) |
| pm-058#CA-02 | T070 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-058-sbom-generation/checklists/gates.md) |
| pm-059#CA-01 | T071 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana; primera ejecución real en GitHub pendiente) | [gates](pm-059-release-versioning/checklists/gates.md) |
| pm-059#CA-02 | T071 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana; primera ejecución real en GitHub pendiente) | [gates](pm-059-release-versioning/checklists/gates.md) |
| pm-059#CA-03 | T071 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana; primera ejecución real en GitHub pendiente) | [gates](pm-059-release-versioning/checklists/gates.md) |
| pm-060#CA-01 | T072 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-060-sdd-integrity-gate/checklists/gates.md) |
| pm-060#CA-02 | T072 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-060-sdd-integrity-gate/checklists/gates.md) |
| pm-060#CA-03 | T072 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-060-sdd-integrity-gate/checklists/gates.md) |
| pm-061#CA-01 | T024 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-061-api-docs-contract-sync/checklists/gates.md) y `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-061#CA-02 | T039 | Histórica | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-061-api-docs-contract-sync/checklists/gates.md) y `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-061#CA-03 | T073 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-061-api-docs-contract-sync/checklists/gates.md) |
| pm-061#CA-04 | T073 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-061-api-docs-contract-sync/checklists/gates.md) |
| pm-062#CA-01 | T074 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-062-cloud-cost-feasibility-study/checklists/gates.md) |
| pm-062#CA-02 | T074 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-062-cloud-cost-feasibility-study/checklists/gates.md) |
| pm-062#CA-03 | T074 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-062-cloud-cost-feasibility-study/checklists/gates.md) |
| pm-062#CA-04 | T074 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-062-cloud-cost-feasibility-study/checklists/gates.md) |
| pm-062#CA-05 | T074 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-062-cloud-cost-feasibility-study/checklists/gates.md) |
| pm-063#CA-01 | T075 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-063-cloud-provider-selection/checklists/gates.md) |
| pm-063#CA-02 | T075 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-063-cloud-provider-selection/checklists/gates.md) |
| pm-063#CA-03 | T075 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-063-cloud-provider-selection/checklists/gates.md) |
| pm-063#CA-04 | T075 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-063-cloud-provider-selection/checklists/gates.md) |
| pm-064#CA-01 | T076 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-064-environment-strategy/checklists/gates.md) |
| pm-064#CA-02 | T076 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-064-environment-strategy/checklists/gates.md) |
| pm-064#CA-03 | T076 | Nueva | Cerrado y aprobado (2026-11-08, aprobación humana) | [gates](pm-064-environment-strategy/checklists/gates.md) |
| pm-065#CA-01 | T077 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-065#CA-02 | T077 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-065#CA-03 | T077 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-066#CA-01 | T078 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-066#CA-02 | T078 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-066#CA-03 | T078 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-067#CA-01 | T079 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-067#CA-02 | T079 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-067#CA-03 | T079 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-068#CA-01 | T080 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-068#CA-02 | T080 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-068#CA-03 | T080 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-068#CA-04 | T080 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-069#CA-01 | T081 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-069#CA-02 | T081 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-069#CA-03 | T081 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-070#CA-01 | T082 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-070#CA-02 | T082 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-070#CA-03 | T082 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-070#CA-04 | T082 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-070#CA-05 | T082 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-070#CA-06 | T082 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-070#CA-07 | T082 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-070#CA-08 | T082 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-070#CA-09 | T082 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-071#CA-01 | T083 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-071#CA-02 | T083 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-071#CA-03 | T083 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-071#CA-04 | T083 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-071#CA-05 | T083 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-071#CA-06 | T083 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-071#CA-07 | T083 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-071#CA-08 | T083 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-071#CA-09 | T083 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-071#CA-10 | T083 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-072#CA-01 | T084 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-072#CA-02 | T084 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-072#CA-03 | T084 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-073#CA-01 | T085 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-073#CA-02 | T085 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-073#CA-03 | T085 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-073#CA-04 | T085 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-074#CA-01 | T086 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-074#CA-02 | T086 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-074#CA-03 | T086 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-075#CA-01 | T087 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-075#CA-02 | T087 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-075#CA-03 | T087 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-075#CA-04 | T087 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-076#CA-01 | T088 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-076#CA-02 | T088 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-076#CA-03 | T088 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-077#CA-01 | T089 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-077#CA-02 | T089 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-077#CA-03 | T089 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-077#CA-04 | T089 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-078#CA-01 | T090 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-078#CA-02 | T090 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-078#CA-03 | T090 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-079#CA-01 | T091 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-079#CA-02 | T091 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-079#CA-03 | T091 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-080#CA-01 | T092 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-080#CA-02 | T092 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-080#CA-03 | T092 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-081#CA-01 | T093 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-081#CA-02 | T093 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-081#CA-03 | T093 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-082#CA-01 | T094 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-082#CA-02 | T094 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-083#CA-01 | T095 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-083#CA-02 | T095 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-083#CA-03 | T095 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-083#CA-04 | T095 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-084#CA-01 | T096 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-084#CA-02 | T096 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-084#CA-03 | T096 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-084#CA-04 | T096 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-085#CA-01 | T103 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-085#CA-02 | T103 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-085#CA-03 | T103 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-086#CA-01 | T104 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-086#CA-02 | T104 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-086#CA-03 | T104 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-086#CA-04 | T104 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-087#CA-01 | T105 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-087#CA-02 | T105 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-087#CA-03 | T105 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-088#CA-01 | T106 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-088#CA-02 | T106 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-088#CA-03 | T106 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-088#CA-04 | T106 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-089#CA-01 | T107 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-089#CA-02 | T107 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-089#CA-03 | T107 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-090#CA-01 | T108 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-090#CA-02 | T108 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-090#CA-03 | T108 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-091#CA-01 | T109 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-091#CA-02 | T109 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-091#CA-03 | T109 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-092#CA-01 | T110 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-092#CA-02 | T110 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-092#CA-03 | T110 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-093#CA-01 | T111 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-093#CA-02 | T111 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-093#CA-03 | T111 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-094#CA-01 | T112 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-094#CA-02 | T112 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-094#CA-03 | T112 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-095#CA-01 | T113 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-095#CA-02 | T113 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-095#CA-03 | T113 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-096#CA-01 | T114 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-096#CA-02 | T114 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-096#CA-03 | T114 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-097#CA-01 | T115 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-097#CA-02 | T115 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-097#CA-03 | T115 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-097#CA-04 | T115 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-098#CA-01 | T116 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-098#CA-02 | T116 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-098#CA-03 | T116 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-099#CA-01 | T117 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-099#CA-02 | T117 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-099#CA-03 | T117 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-099#CA-04 | T117 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-100#CA-01 | T118 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-100#CA-02 | T118 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-100#CA-03 | T118 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-101#CA-01 | T119 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-101#CA-02 | T119 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-101#CA-03 | T119 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-102#CA-01 | T120 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-102#CA-02 | T120 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-102#CA-03 | T120 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-103#CA-01 | T121 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-103#CA-02 | T121 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-103#CA-03 | T121 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-104#CA-01 | T122 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-104#CA-02 | T122 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-104#CA-03 | T122 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-104#CA-04 | T122 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-105#CA-01 | T123 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-105#CA-02 | T123 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-105#CA-03 | T123 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-105#CA-04 | T123 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-106#CA-01 | T124 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-106#CA-02 | T124 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-106#CA-03 | T124 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-106#CA-04 | T124 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-107#CA-01 | T125 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-107#CA-02 | T125 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-107#CA-03 | T125 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-107#CA-04 | T125 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-108#CA-01 | T126 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-108#CA-02 | T126 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-108#CA-03 | T126 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-108#CA-04 | T126 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-109#CA-01 | T127 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-109#CA-02 | T127 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-109#CA-03 | T127 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-109#CA-04 | T127 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-110#CA-01 | T128 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-110#CA-02 | T128 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-110#CA-03 | T128 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-110#CA-04 | T128 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-111#CA-01 | T129 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-111#CA-02 | T129 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-111#CA-03 | T129 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-111#CA-04 | T129 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-112#CA-01 | T130 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-112#CA-02 | T130 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-112#CA-03 | T130 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-112#CA-04 | T130 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-113#CA-01 | T131 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-113#CA-02 | T131 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-113#CA-03 | T131 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-114#CA-01 | T132 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-114#CA-02 | T132 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-114#CA-03 | T132 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-115#CA-01 | T133 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-115#CA-02 | T133 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-115#CA-03 | T133 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-116#CA-01 | T134 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-116#CA-02 | T134 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-116#CA-03 | T134 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-116#CA-04 | T134 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-117#CA-01 | T135 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-117#CA-02 | T135 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-117#CA-03 | T135 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-118#CA-01 | T136 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-118#CA-02 | T136 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-118#CA-03 | T136 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-119#CA-01 | T137 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-119#CA-02 | T137 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-119#CA-03 | T137 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-119#CA-04 | T137 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-120#CA-01 | T138 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-120#CA-02 | T138 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-120#CA-03 | T138 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-121#CA-01 | T139 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-121#CA-02 | T139 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-121#CA-03 | T139 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-121#CA-04 | T139 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-122#CA-01 | T140 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-122#CA-02 | T140 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-122#CA-03 | T140 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-122#CA-04 | T140 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-123#CA-01 | T141 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-123#CA-02 | T141 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-123#CA-03 | T141 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-123#CA-04 | T141 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-124#CA-01 | T142 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-124#CA-02 | T142 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-124#CA-03 | T142 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-125#CA-01 | T143 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-125#CA-02 | T143 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-125#CA-03 | T143 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-125#CA-04 | T143 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-126#CA-01 | T144 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-126#CA-02 | T144 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-126#CA-03 | T144 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-126#CA-04 | T144 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-127#CA-01 | T145 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-127#CA-02 | T145 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-127#CA-03 | T145 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-128#CA-01 | T146 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-128#CA-02 | T146 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-128#CA-03 | T146 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-129#CA-01 | T147 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-129#CA-02 | T147 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-129#CA-03 | T147 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-130#CA-01 | T148 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-130#CA-02 | T148 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-130#CA-03 | T148 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-131#CA-01 | T149 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-131#CA-02 | T149 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-131#CA-03 | T149 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-132#CA-01 | T150 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-132#CA-02 | T150 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-132#CA-03 | T150 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-133#CA-01 | T151 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-133#CA-02 | T151 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-133#CA-03 | T151 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-134#CA-01 | T152 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-134#CA-02 | T152 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-134#CA-03 | T152 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-135#CA-01 | T153 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-135#CA-02 | T153 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-135#CA-03 | T153 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-135#CA-04 | T153 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-136#CA-01 | T154 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-136#CA-02 | T154 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-136#CA-03 | T154 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-136#CA-04 | T154 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-137#CA-01 | T155 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-137#CA-02 | T155 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-137#CA-03 | T155 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-138#CA-01 | T156 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-138#CA-02 | T156 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-138#CA-03 | T156 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-139#CA-01 | T157 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-139#CA-02 | T157 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-139#CA-03 | T157 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-140#CA-01 | T158 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-140#CA-02 | T158 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-140#CA-03 | T158 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-141#CA-01 | T159 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-141#CA-02 | T159 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-141#CA-03 | T159 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-142#CA-01 | T160 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-142#CA-02 | T160 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-142#CA-03 | T160 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-143#CA-01 | T161 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-143#CA-02 | T161 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-143#CA-03 | T161 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-144#CA-01 | T162 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-144#CA-02 | T162 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-144#CA-03 | T162 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-145#CA-01 | T163 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-145#CA-02 | T163 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-145#CA-03 | T163 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-146#CA-01 | T164 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-146#CA-02 | T164 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-146#CA-03 | T164 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-147#CA-01 | T165 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-147#CA-02 | T165 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-147#CA-03 | T165 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-148#CA-01 | T166 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-148#CA-02 | T166 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-148#CA-03 | T166 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-149#CA-01 | T167 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-149#CA-02 | T167 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-149#CA-03 | T167 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-150#CA-01 | T168 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-150#CA-02 | T168 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-150#CA-03 | T168 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-150#CA-04 | T168 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-151#CA-01 | T169 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-151#CA-02 | T169 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-151#CA-03 | T169 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-152#CA-01 | T170 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-152#CA-02 | T170 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-152#CA-03 | T170 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-153#CA-01 | T171 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-153#CA-02 | T171 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-153#CA-03 | T171 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-153#CA-04 | T171 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-154#CA-01 | T172 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-154#CA-02 | T172 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-154#CA-03 | T172 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-155#CA-01 | T173 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-155#CA-02 | T173 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-155#CA-03 | T173 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-155#CA-04 | T173 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-156#CA-01 | T174 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-156#CA-02 | T174 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-156#CA-03 | T174 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-157#CA-01 | T175 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-157#CA-02 | T175 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-157#CA-03 | T175 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-158#CA-01 | T176 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-158#CA-02 | T176 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-158#CA-03 | T176 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-159#CA-01 | T177 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-159#CA-02 | T177 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-159#CA-03 | T177 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-160#CA-01 | T178 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-160#CA-02 | T178 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-160#CA-03 | T178 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-161#CA-01 | T179 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-161#CA-02 | T179 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-161#CA-03 | T179 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-161#CA-04 | T179 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-162#CA-01 | T180 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-162#CA-02 | T180 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-162#CA-03 | T180 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-163#CA-01 | T181 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-163#CA-02 | T181 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-163#CA-03 | T181 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-164#CA-01 | T182 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-164#CA-02 | T182 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-164#CA-03 | T182 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-165#CA-01 | T183 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-165#CA-02 | T183 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-165#CA-03 | T183 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-166#CA-01 | T184 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-166#CA-02 | T184 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-166#CA-03 | T184 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-167#CA-01 | T185 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-167#CA-02 | T185 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-167#CA-03 | T185 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |

## 5. Requisitos de las fuentes de producto → Specs

> Fuentes vigentes desde 2026-11-09 (`docs/product-scope.md`): documento v3 (RF-01…RF-41, RNF-01…RNF-24, HU-001…HU-036) y maqueta HTML. Los Specs de nube y calidad (`AC-01`…`AC-16`) son requisitos académicos y técnicos, no de estas fuentes. Todos los Specs nuevos están «Propuesto».

| Requisito | Funcionalidad | Spec(s) |
|---|---|---|
| RF-02 | Crear cuenta (nombre, correo, contraseña) | pm-029, pm-085 |
| RF-03 | Iniciar sesión e indicador de negocio | pm-030, pm-086, pm-087, pm-107 |
| RF-04 | Recuperar contraseña por correo | pm-088, pm-089, pm-090 |
| RF-05 | Cerrar sesión | pm-032, pm-152 |
| RF-08 | Eliminar cuenta | pm-161, pm-162, pm-163 |
| RF-09 | Registrar mascota (especies, QR, foto) | pm-040, pm-092, pm-093, pm-079, pm-080, pm-081 |
| RF-10, RF-11 | Ver y editar mascotas | pm-041, pm-047, pm-043 |
| RF-12 | Eliminar mascota y su carnet | pm-044, pm-104 |
| RF-13, RF-41 | Mostrar y regenerar el QR | pm-093, pm-094, pm-095, pm-124 |
| RF-14 | Vacunas | pm-096, pm-097, pm-098, pm-135 |
| RF-15 | Desparasitaciones | pm-096, pm-099, pm-100, pm-136 |
| RF-16 | Control de peso | pm-101 |
| RF-17 | Alergias y condiciones | pm-102, pm-103 |
| RF-18 | Consultas y recetas (lectura) | pm-133, pm-134, pm-137 |
| RF-19 | Recordatorios (push desde el servidor) | pm-154, pm-155, pm-156 |
| RF-20 a RF-22 | Mapa, filtros y tarjeta del negocio | pm-116, pm-117, pm-106 |
| RF-23 | Perfil y contacto del negocio | pm-119 |
| RF-25 a RF-27 | Afiliación, ubicación y alta del negocio | pm-105, pm-107, pm-108 |
| RF-28 | Baja del negocio | pm-110 |
| RF-30, RF-31 | Editar negocio y ubicación | pm-109, pm-108 |
| RF-32 | Servicios del negocio | pm-112, pm-113, pm-114, pm-115 |
| RF-33 | Escanear el QR | pm-125 |
| RF-34 | Ficha según el tipo de negocio | pm-126, pm-127, pm-128 |
| RF-35 | Consulta con receta | pm-131, pm-132 |
| RF-36 | Vacuna o desparasitación por veterinaria | pm-135, pm-136 |
| RF-37 | Mascotas atendidas | pm-129, pm-130 |
| RF-39 | Solicitar eliminación sin entrar a la app | pm-164, pm-165 |
| Maqueta | Compartir historial con negocio, datos y duración | pm-122, pm-123, pm-124 |
| Maqueta | Seguimiento de la atención | pm-138 a pm-142 |
| Maqueta | Chat con fotos | pm-143 a pm-150 |
| Maqueta | Ajustes de notificación y push | pm-151 a pm-153, pm-157 a pm-160 |
| Maqueta | Búsqueda por nombre y horario estructurado | pm-118, pm-106 |
| Perfil | Celular opcional | pm-091 |
| RNF-01 | Contraseñas con hash | pm-026, pm-029 (el mínimo es 12 por decisión del usuario; el documento propone 8) |
| RNF-03 | QR no adivinable y revocable | pm-093, pm-095, pm-124 |
| RNF-04 | Permisos en el servidor | pm-045, pm-096, pm-126 |
| RNF-05, RNF-07 | Acceso temporal y mínimo de datos por tipo de negocio | pm-122, pm-126, pm-128 |
| RNF-08 | Borrado completo de la cuenta | pm-161, pm-162, pm-163 |
| RNF-10, RNF-11 | Rendimiento del mapa y del escaneo | pm-116, pm-121, pm-125 |
| RNF-12 | Fotos de hasta 1 MB | pm-080, pm-111, pm-146 |
| Integración | Contrato OpenAPI y entorno compartido | pm-166, pm-167 |

Sin Spec por decisión del usuario: cobro, IVA y ticket; verificación de correo o celular; SMS; IA/ML; inventario, punto de venta, suscripciones, empleados, citas y familia. RF-01, RF-06, RF-07, RF-24, RF-29, RF-38 y RF-40 y los RNF móviles son de la app o de la página legal, no del backend.
