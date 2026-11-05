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
| AC-14 | Primer hito funcional (11 endpoints) | CON-§11 | pm-037, pm-038, pm-039, pm-040, pm-041, pm-042, pm-043, pm-044, pm-045, pm-046 | T026, T027, T028, T029, T030, T031, T032, T033, T034, T035, T036, T037, T038, T040, T041 |
| AC-15 | Almacenamiento de imágenes en la nube y CDN para contenido público | CON-§8.10 | pm-079, pm-080, pm-081, pm-082 | T091, T092, T093, T094 |
| AC-16 | Protección de datos clínicos y persistencia durante el teardown | CON-§8.15 | pm-083 | T095 |

## 2. Constitución → Specs

| Sección | Specs |
|---|---|
| CON-§2 | pm-005 |
| CON-§3 | pm-001, pm-002, pm-003, pm-005, pm-006, pm-015 |
| CON-§4 | pm-004, pm-012, pm-016, pm-024, pm-039, pm-052, pm-064, pm-068, pm-069 |
| CON-§5 | pm-013, pm-016, pm-028, pm-050, pm-051, pm-070, pm-071 |
| CON-§6 | pm-022, pm-023, pm-024, pm-025, pm-026, pm-027, pm-028, pm-029, pm-030, pm-031, pm-032, pm-033, pm-034, pm-035, pm-036, pm-045, pm-051, pm-054, pm-080 |
| CON-§7 | pm-007, pm-008, pm-009, pm-010, pm-011, pm-014, pm-017, pm-029, pm-030, pm-031, pm-032, pm-036, pm-037, pm-038, pm-040, pm-041, pm-042, pm-043, pm-044, pm-047, pm-048, pm-049, pm-061 |
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
| CON-§11 | pm-037, pm-038, pm-039, pm-040, pm-041, pm-042, pm-043, pm-044, pm-045, pm-046 |
| CON-§12.3 | pm-060 |

## 3. Distinciones históricas explícitas

| Task | Spec principal | Criterios | Observación |
|---|---|---|---|
| T007 (histórica) | pm-008 | pm-008#CA-01..04 | Alcance histórico: OpenAPI/Scalar. No demuestra por sí sola los criterios de rutas de pm-007; su alcance no se modifica. |
| T042 (retrospectiva) | pm-007 | pm-007#CA-01..03 | Documenta la evidencia existente de rutas versionadas (origen T007) sin cerrarse. |
| T055 (nueva) | pm-007 | pm-007#CA-04..05 | Cubre los criterios que faltan (verificación automática de rutas y reglas de nombrado). |
| T012 (histórica) | pm-055 | evidencia local parcial | Implementada pero abierta; T066 y T067 cubren los criterios de pm-055. |
| T047 / T097 | pm-017 | pm-017#CA-01..03 / CA-04 | Evidencia existente indirecta; la directa contra spoofing es trabajo nuevo. |
| T054 / T057 | pm-036 | pm-036#CA-01,02,04 / CA-03 | Rate limiting de autenticación existente; la matriz global es trabajo nuevo. |

## 4. Criterios de aceptación → Tasks → evidencia

| CA | Task propietaria | Tipo | Estado de la Task | Evidencia |
|---|---|---|---|---|
| pm-001#CA-01 | T001 | Histórica | Verificado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-001#CA-02 | T001 | Histórica | Verificado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-001#CA-03 | T001 | Histórica | Verificado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
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
| pm-007#CA-01 | T042 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T042 (retrospectiva) |
| pm-007#CA-02 | T042 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T042 (retrospectiva) |
| pm-007#CA-03 | T042 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T042 (retrospectiva) |
| pm-007#CA-04 | T055 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-007#CA-05 | T055 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-008#CA-01 | T007 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-008#CA-02 | T007 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-008#CA-03 | T007 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-008#CA-04 | T007 | Histórica | Cerrado (2026-11-03) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-009#CA-01 | T008 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-009#CA-02 | T008 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-009#CA-03 | T008 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-010#CA-01 | T043 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T043 (retrospectiva) |
| pm-010#CA-02 | T043 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T043 (retrospectiva) |
| pm-010#CA-03 | T043 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T043 (retrospectiva) |
| pm-010#CA-04 | T043 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T043 (retrospectiva) |
| pm-011#CA-01 | T009 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana; política 400 ratificada) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-011#CA-02 | T009 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana; política 400 ratificada) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-011#CA-03 | T009 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana; política 400 ratificada) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-012#CA-01 | T010 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-012#CA-02 | T010 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-012#CA-03 | T010 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-013#CA-01 | T044 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T044 (retrospectiva) |
| pm-013#CA-02 | T044 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T044 (retrospectiva) |
| pm-013#CA-03 | T044 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T044 (retrospectiva) |
| pm-014#CA-01 | T045 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T045 (retrospectiva) |
| pm-014#CA-02 | T045 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T045 (retrospectiva) |
| pm-014#CA-03 | T045 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T045 (retrospectiva) |
| pm-015#CA-01 | T046 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T046 (retrospectiva) |
| pm-015#CA-02 | T046 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T046 (retrospectiva) |
| pm-015#CA-03 | T046 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T046 (retrospectiva) |
| pm-016#CA-01 | T011 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-016#CA-02 | T011 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-016#CA-03 | T011 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/00-foundation/task.md` y su `checklists/gates.md` |
| pm-017#CA-01 | T047 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T047 (retrospectiva) |
| pm-017#CA-02 | T047 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T047 (retrospectiva) |
| pm-017#CA-03 | T047 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T047 (retrospectiva) |
| pm-017#CA-04 | T097 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-018#CA-01 | T048 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T048 (retrospectiva) |
| pm-018#CA-02 | T048 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T048 (retrospectiva) |
| pm-019#CA-01 | T049 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T049 (retrospectiva) |
| pm-019#CA-02 | T049 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T049 (retrospectiva) |
| pm-020#CA-01 | T050 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T050 (retrospectiva) |
| pm-020#CA-02 | T050 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T050 (retrospectiva) |
| pm-020#CA-03 | T050 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T050 (retrospectiva) |
| pm-021#CA-01 | T051 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T051 (retrospectiva) |
| pm-021#CA-02 | T051 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T051 (retrospectiva) |
| pm-022#CA-01 | T013 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-022#CA-02 | T013 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-023#CA-01 | T052 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T052 (retrospectiva) |
| pm-023#CA-02 | T052 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T052 (retrospectiva) |
| pm-023#CA-03 | T052 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T052 (retrospectiva) |
| pm-024#CA-01 | T014 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-024#CA-02 | T014 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-024#CA-03 | T014 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-025#CA-01 | T053 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T053 (retrospectiva) |
| pm-025#CA-02 | T053 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T053 (retrospectiva) |
| pm-025#CA-03 | T053 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T053 (retrospectiva) |
| pm-025#CA-04 | T053 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T053 (retrospectiva) |
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
| pm-029#CA-01 | T018 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-029#CA-02 | T018 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-029#CA-03 | T018 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-029#CA-04 | T018 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-030#CA-01 | T019 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-030#CA-02 | T019 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-030#CA-03 | T019 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-031#CA-01 | T020 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-031#CA-02 | T020 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-031#CA-03 | T020 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-032#CA-01 | T021 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-032#CA-02 | T021 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-032#CA-03 | T021 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-032#CA-04 | T021 | Histórica | Cerrado y aprobado (2026-11-03, aprobación humana) | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-033#CA-01 | T022 | Histórica | Propuesto | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-033#CA-02 | T022 | Histórica | Propuesto | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-033#CA-03 | T022 | Histórica | Propuesto | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-033#CA-04 | T022 | Histórica | Propuesto | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-034#CA-01 | T056 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-034#CA-02 | T056 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-034#CA-03 | T056 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-034#CA-04 | T056 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-035#CA-01 | T025 | Histórica | Propuesto | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-035#CA-02 | T025 | Histórica | Propuesto | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-036#CA-01 | T054 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T054 (retrospectiva) |
| pm-036#CA-02 | T054 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T054 (retrospectiva) |
| pm-036#CA-03 | T057 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-036#CA-04 | T054 | Retrospectiva | Propuesto (retrospectiva por documentar) | Tabla de evidencia de T054 (retrospectiva) |
| pm-037#CA-01 | T026 | Histórica | Propuesto | `_historico/02-owner-profile/task.md` y su `checklists/gates.md` |
| pm-037#CA-02 | T026 | Histórica | Propuesto | `_historico/02-owner-profile/task.md` y su `checklists/gates.md` |
| pm-038#CA-01 | T027 | Histórica | Propuesto | `_historico/02-owner-profile/task.md` y su `checklists/gates.md` |
| pm-038#CA-02 | T027 | Histórica | Propuesto | `_historico/02-owner-profile/task.md` y su `checklists/gates.md` |
| pm-038#CA-03 | T028 | Histórica | Propuesto | `_historico/02-owner-profile/task.md` y su `checklists/gates.md` |
| pm-039#CA-01 | T029 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-039#CA-02 | T029 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-039#CA-03 | T029 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-039#CA-04 | T036 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-039#CA-05 | T037 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-040#CA-01 | T030 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-040#CA-02 | T030 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-040#CA-03 | T030 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-041#CA-01 | T031 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-041#CA-02 | T031 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-042#CA-01 | T032 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-042#CA-02 | T032 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-043#CA-01 | T033 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-043#CA-02 | T033 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-044#CA-01 | T034 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-044#CA-02 | T034 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-044#CA-03 | T034 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-045#CA-01 | T035 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-045#CA-02 | T038 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-045#CA-03 | T040 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-046#CA-01 | T041 | Histórica | Propuesto | `_historico/04-first-milestone-closure/task.md` y su `checklists/gates.md` |
| pm-046#CA-02 | T041 | Histórica | Propuesto | `_historico/04-first-milestone-closure/task.md` y su `checklists/gates.md` |
| pm-047#CA-01 | T058 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-047#CA-02 | T058 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-047#CA-03 | T058 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-048#CA-01 | T059 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-048#CA-02 | T059 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-048#CA-03 | T059 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-049#CA-01 | T060 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-049#CA-02 | T060 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-050#CA-01 | T061 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-050#CA-02 | T061 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-050#CA-03 | T061 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-051#CA-01 | T062 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-051#CA-02 | T062 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-051#CA-03 | T062 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-052#CA-01 | T063 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-052#CA-02 | T063 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-053#CA-01 | T064 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-053#CA-02 | T064 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-053#CA-03 | T064 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-054#CA-01 | T065 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-054#CA-02 | T065 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-054#CA-03 | T065 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-055#CA-01 | T066 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-055#CA-02 | T066 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-055#CA-03 | T067 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-055#CA-04 | T067 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-056#CA-01 | T068 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-056#CA-02 | T068 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-057#CA-01 | T069 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-057#CA-02 | T069 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-058#CA-01 | T070 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-058#CA-02 | T070 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-059#CA-01 | T071 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-059#CA-02 | T071 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-059#CA-03 | T071 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-060#CA-01 | T072 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-060#CA-02 | T072 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-060#CA-03 | T072 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-061#CA-01 | T024 | Histórica | Propuesto | `_historico/01-initial-authentication/task.md` y su `checklists/gates.md` |
| pm-061#CA-02 | T039 | Histórica | Propuesto | `_historico/03-pet-management/task.md` y su `checklists/gates.md` |
| pm-061#CA-03 | T073 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-061#CA-04 | T073 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-062#CA-01 | T074 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-062#CA-02 | T074 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-062#CA-03 | T074 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-062#CA-04 | T074 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-062#CA-05 | T074 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-063#CA-01 | T075 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-063#CA-02 | T075 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-063#CA-03 | T075 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-063#CA-04 | T075 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-064#CA-01 | T076 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-064#CA-02 | T076 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
| pm-064#CA-03 | T076 | Nueva | Propuesto | Pendiente (se registra en `checklists/gates.md` del Spec) |
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
