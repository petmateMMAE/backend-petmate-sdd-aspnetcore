# Registro SDD — PetMate Backend

> Registro de Specs, Tasks y dependencias según la Constitución v1.1.0 (§12.3). Los números `pm-NNN` **identifican** Specs; **no determinan el orden de trabajo**: lo gobiernan las dependencias. La matriz de trazabilidad está en [traceability.md](traceability.md) y la estructura anterior por slices está archivada en [_historico/](_historico/README.md).

## Convenciones

- Estructura plana `specs/pm-NNN-slug/` con `spec.md` y `task.md`; `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras la aprobación del Spec.
- Numeración de Specs `pm-001`…`pm-084`: global, secuencial e inmutable. Un Spec retirado pasa a `Reemplazado`.
- Tasks globales `T001`…`T102`: `T001`–`T041` históricas (identificador, alcance y estado intactos), `T042`–`T054` retrospectivas, `T055`–`T102` nuevas.
- Cada Task tiene un único Spec principal; criterios citados como `pm-NNN#CA-NN`.
- Estado de Spec al migrar: `En revisión` si tiene Tasks históricas cerradas, verificadas o implementadas, o retrospectivas (texto nuevo pendiente de aprobación humana); `Propuesto` en los demás casos. Ninguna Task cambia de estado por la migración.
- Plataforma cloud: Microsoft Azure con Azure for Students (Constitución §8). Los Specs cloud documentan servicios candidatos como hipótesis; ninguno se asume gratuito hasta completar pm-062.

## Specs

| Spec | Estado | Nivel | Dependencias | Tasks | Objetivo |
|---|---|---:|---|---|---|
| [pm-001-solution-structure](pm-001-solution-structure/spec.md) | Cerrado | L0 | — | T001 | Solución .NET 10 con 4 proyectos de código y 4 de pruebas que compilan en Release |
| [pm-002-clean-architecture-dependency-rules](pm-002-clean-architecture-dependency-rules/spec.md) | Cerrado | L1 | pm-001 | T002 | Dirección de referencias entre capas exigida por prueba de arquitectura |
| [pm-003-code-standards-central-packages](pm-003-code-standards-central-packages/spec.md) | Cerrado | L1 | pm-001 | T003 | Analizadores, advertencias como error y versiones centralizadas de paquetes |
| [pm-004-external-configuration-secrets](pm-004-external-configuration-secrets/spec.md) | Cerrado | L1 | pm-001 | T004 | Configuración por entorno, `.env` ignorado y validación de arranque sin exponer valores |
| [pm-005-dependency-injection-composition](pm-005-dependency-injection-composition/spec.md) | Cerrado | L2 | pm-002 | T005 | Módulos de DI por capa sin Service Locator |
| [pm-006-cqrs-mediator](pm-006-cqrs-mediator/spec.md) | Cerrado | L3 | pm-005 | T006 | Commands y Queries despachados por un Mediator propio con registro explícito |
| [pm-007-api-route-conventions](pm-007-api-route-conventions/spec.md) | Verificado | L3 | pm-005 | T042, T055 | Patrón `/api/v1/{recurso}/{acción}` y su verificación automática |
| [pm-008-openapi-scalar-documentation](pm-008-openapi-scalar-documentation/spec.md) | Cerrado | L4 | pm-005, pm-007 | T007 | Documento OpenAPI generado y Scalar como única UI, solo en Development |
| [pm-009-response-wrapper-contract](pm-009-response-wrapper-contract/spec.md) | Cerrado | L3 | pm-005 | T008 | Contrato `statusCode/message/data` con `data` nulo y coincidencia con HTTP |
| [pm-010-global-exception-handling](pm-010-global-exception-handling/spec.md) | Verificado | L4 | pm-009 | T043 | Manejo central de excepciones sin fuga de información, 500/503 y cancelación |
| [pm-011-request-validation](pm-011-request-validation/spec.md) | Cerrado | L5 | pm-006, pm-009, pm-010 | T009 | Validación de solicitudes antes del handler con 400 y errores en español |
| [pm-012-atlas-connectivity](pm-012-atlas-connectivity/spec.md) | Cerrado | L2 | pm-004 | T010 | Conexión tipada a MongoDB Atlas, base de pruebas separada y sin Mongo local |
| [pm-013-redis-dual-connectivity](pm-013-redis-dual-connectivity/spec.md) | Verificado | L2 | pm-004 | T044 | Dos conexiones Redis independientes (Auth y Cache) con TLS fuera de Development |
| [pm-014-health-probes](pm-014-health-probes/spec.md) | Verificado | L3 | pm-012, pm-013 | T045 | Liveness sin dependencias y readiness con detalle mínimo por dependencia |
| [pm-015-api-container-image](pm-015-api-container-image/spec.md) | Verificado | L2 | pm-001, pm-004 | T046 | Imagen multi-stage de la API, usuario no root y sin secretos |
| [pm-016-local-compose-environment](pm-016-local-compose-environment/spec.md) | Cerrado | L4 | pm-013, pm-014, pm-015 | T011 | Compose local con API y dos Redis, red, volumen y sin MongoDB |
| [pm-017-trusted-proxy-forwarded-headers](pm-017-trusted-proxy-forwarded-headers/spec.md) | Verificado | L3 | pm-005 | T047, T097 | IP real del cliente detrás de proxies confiables configurados |
| [pm-018-static-analysis-sast](pm-018-static-analysis-sast/spec.md) | Verificado | L2 | pm-003 | T048 | Reglas de seguridad de los analizadores como errores de compilación |
| [pm-019-secret-scanning](pm-019-secret-scanning/spec.md) | Verificado | L2 | pm-004 | T049 | Detección de secretos en archivos versionables con controles positivos y negativos |
| [pm-020-dependency-license-audit](pm-020-dependency-license-audit/spec.md) | Verificado | L2 | pm-003 | T050 | Auditoría de vulnerabilidades, desuso y licencias NuGet con gobierno de excepciones |
| [pm-021-local-quality-gate](pm-021-local-quality-gate/spec.md) | Verificado | L3 | pm-018, pm-019, pm-020 | T051 | Orquestación local de restore, build, formato, auditorías y suites |
| [pm-022-user-domain-roles](pm-022-user-domain-roles/spec.md) | Cerrado | L1 | pm-001 | T013 | Usuario, correo normalizado y roles Owner/Business sin Admin |
| [pm-023-refresh-session-domain](pm-023-refresh-session-domain/spec.md) | Verificado | L2 | pm-022 | T052 | Sesión con vencimiento por inactividad y absoluto y token de un solo uso |
| [pm-024-users-persistence](pm-024-users-persistence/spec.md) | Cerrado | L3 | pm-012, pm-022 | T014 | Persistencia de usuarios en Atlas con correo único y arranque de índices |
| [pm-025-refresh-sessions-persistence](pm-025-refresh-sessions-persistence/spec.md) | Verificado | L3 | pm-012, pm-023 | T053 | Sesiones y tokens en Atlas con hash único, consumo atómico y TTL |
| [pm-026-password-hashing-argon2id](pm-026-password-hashing-argon2id/spec.md) | Cerrado | L2 | pm-022 | T015 | Hash Argon2id con salt, parámetros mínimos, rehash y concurrencia acotada |
| [pm-027-access-token-jwt](pm-027-access-token-jwt/spec.md) | Cerrado | L2 | pm-004, pm-022 | T016 | Emisión y validación de JWT HS256 de corta vida y configuración segura |
| [pm-028-token-revocation-redis](pm-028-token-revocation-redis/spec.md) | Verificado | L3 | pm-013, pm-027 | T017, T098 | Lista negra de `jti` en Redis Auth con TTL y falla cerrada |
| [pm-029-user-registration](pm-029-user-registration/spec.md) | Verificado | L6 | pm-011, pm-024, pm-025, pm-026, pm-027 | T018, T102 | `POST /api/v1/auth/register` que crea siempre un Owner |
| [pm-030-user-login](pm-030-user-login/spec.md) | Cerrado | L6 | pm-011, pm-024, pm-025, pm-026, pm-027 | T019 | `POST /api/v1/auth/login` con respuesta idéntica ante credenciales inválidas |
| [pm-031-refresh-token-rotation](pm-031-refresh-token-rotation/spec.md) | Verificado | L7 | pm-025, pm-027, pm-030 | T020, T099 | `POST /api/v1/auth/refresh` con rotación atómica y detección de replay |
| [pm-032-user-logout](pm-032-user-logout/spec.md) | Cerrado | L8 | pm-028, pm-031 | T021 | `POST /api/v1/auth/logout` que revoca sesión y `jti` |
| [pm-033-authorization-policies](pm-033-authorization-policies/spec.md) | Cerrado | L4 | pm-027, pm-028 | T022 | Política de autorización por defecto y por rol Owner/Business |
| [pm-034-auth-brute-force-protection](pm-034-auth-brute-force-protection/spec.md) | Verificado | L9 | pm-030, pm-031, pm-036 | T023, T056, T101 | Cuota por correo y bloqueo contra credential stuffing, y limitador asíncrono de hashing |
| [pm-035-auth-session-lifecycle-acceptance](pm-035-auth-session-lifecycle-acceptance/spec.md) | Cerrado | L10 | pm-028, pm-029, pm-030, pm-031, pm-032, pm-033, pm-034 | T025 | Escenarios de ciclo de vida de sesión entre endpoints y cierre del grupo de autenticación |
| [pm-036-rate-limiting](pm-036-rate-limiting/spec.md) | Verificado | L8 | pm-017, pm-029, pm-030, pm-031 | T054, T057, T100 | Límites por política y por IP, y matriz de límites para los endpoints no autenticación |
| [pm-037-owner-profile-read](pm-037-owner-profile-read/spec.md) | Cerrado | L5 | pm-024, pm-033 | T026 | `GET /api/v1/users/me/get` del usuario autenticado |
| [pm-038-owner-profile-update](pm-038-owner-profile-update/spec.md) | Cerrado | L6 | pm-037 | T027, T028 | `PUT /api/v1/users/me/update` con campos aprobados |
| [pm-039-pet-domain-persistence](pm-039-pet-domain-persistence/spec.md) | Cerrado | L5 | pm-012, pm-033 | T029, T036, T037 | Modelo Pet, validadores, persistencia e índice por propietario |
| [pm-040-pet-create](pm-040-pet-create/spec.md) | Cerrado | L6 | pm-039 | T030 | `POST /api/v1/pets/create` con propietario tomado del token |
| [pm-041-pet-list](pm-041-pet-list/spec.md) | Cerrado | L6 | pm-039, pm-047 | T031 | `GET /api/v1/pets/get-all` solo de mascotas propias, paginado |
| [pm-042-pet-detail](pm-042-pet-detail/spec.md) | Cerrado | L6 | pm-039 | T032 | `GET /api/v1/pets/get-by-id/{id}` solo de mascota propia |
| [pm-043-pet-update](pm-043-pet-update/spec.md) | Cerrado | L6 | pm-039 | T033 | `PUT /api/v1/pets/update/{id}` de mascota propia con campos permitidos |
| [pm-044-pet-soft-delete](pm-044-pet-soft-delete/spec.md) | Cerrado | L6 | pm-039 | T034 | `DELETE /api/v1/pets/delete/{id}` con borrado físico y `data:null` |
| [pm-045-pet-ownership-bola](pm-045-pet-ownership-bola/spec.md) | Cerrado | L7 | pm-040, pm-041, pm-042, pm-043, pm-044 | T035, T038, T040 | Matriz de propiedad y BOLA del módulo de mascotas y cierre de seguridad del módulo |
| [pm-046-first-milestone-acceptance](pm-046-first-milestone-acceptance/spec.md) | Cerrado | L11 | pm-035, pm-038, pm-045 | T041 | Aceptación de extremo a extremo de los 11 endpoints del primer hito |
| [pm-047-pagination-convention](pm-047-pagination-convention/spec.md) | Cerrado | L4 | pm-009 | T058 | Parámetros, tope y forma de paginación en listados |
| [pm-048-response-compression](pm-048-response-compression/spec.md) | Verificado | L4 | pm-009 | T059 | Compresión de respuestas con evaluación de riesgos |
| [pm-049-json-serialization-performance](pm-049-json-serialization-performance/spec.md) | Verificado | L4 | pm-009 | T060 | Serialización JSON eficiente con medición |
| [pm-050-distributed-cache-infrastructure](pm-050-distributed-cache-infrastructure/spec.md) | Verificado | L3 | pm-013 | T061 | Adaptador de caché sobre Redis Cache, claves, TTL y política de falla |
| [pm-051-cache-policy-data-classification](pm-051-cache-policy-data-classification/spec.md) | Verificado | L4 | pm-050 | T062 | Política de qué datos pueden cachearse y criterios de decisión |
| [pm-052-database-indexing-efficiency](pm-052-database-indexing-efficiency/spec.md) | Verificado | L6 | pm-024, pm-025, pm-039 | T063 | Inventario de índices y evidencia de consultas críticas |
| [pm-053-api-performance-baseline](pm-053-api-performance-baseline/spec.md) | Verificado | L12 | pm-046 | T064 | Objetivos de latencia medibles y prueba de carga sin costo |
| [pm-054-structured-logging-pii-safe](pm-054-structured-logging-pii-safe/spec.md) | Verificado | L5 | pm-010 | T065 | Logging estructurado con correlación y sin datos personales |
| [pm-055-ci-pull-request-pipeline](pm-055-ci-pull-request-pipeline/spec.md) | En progreso | L4 | pm-021 | T012, T066, T067 | Workflow de pull request con paridad respecto al gate local y ramas protegidas |
| [pm-056-test-coverage-baseline](pm-056-test-coverage-baseline/spec.md) | En progreso | L4 | pm-021 | T068 | Línea base y umbral de cobertura de pruebas |
| [pm-057-container-image-scan](pm-057-container-image-scan/spec.md) | Cerrado | L5 | pm-015, pm-055 | T069 | Escaneo de la imagen de contenedor con herramienta $0 |
| [pm-058-sbom-generation](pm-058-sbom-generation/spec.md) | Cerrado | L5 | pm-055 | T070 | Generación de SBOM por compilación |
| [pm-059-release-versioning](pm-059-release-versioning/spec.md) | En progreso | L5 | pm-055 | T071 | Versionado SemVer, etiquetas y versión trazable a la imagen |
| [pm-060-sdd-integrity-gate](pm-060-sdd-integrity-gate/spec.md) | Cerrado | L5 | pm-055 | T072 | Validación automática de la integridad de specs, tasks y trazabilidad |
| [pm-061-api-docs-contract-sync](pm-061-api-docs-contract-sync/spec.md) | Verificado | L5 | pm-007, pm-008, pm-009 | T024, T039, T073 | Rutas del código iguales al catálogo y wrapper representado en OpenAPI |
| [pm-062-cloud-cost-feasibility-study](pm-062-cloud-cost-feasibility-study/spec.md) | Verificado | L0 | — | T074 | Estudio con fuentes oficiales del costo $0 MXN por componente en Azure for Students y del costo total de operación de la arquitectura |
| [pm-063-cloud-provider-selection](pm-063-cloud-provider-selection/spec.md) | Verificado | L1 | pm-062 | T075 | Registrar Azure for Students como proveedor definitivo y seleccionar mediante ADR el servicio de Azure para cada componente |
| [pm-064-environment-strategy](pm-064-environment-strategy/spec.md) | Verificado | L2 | pm-063 | T076 | Entornos, bases por entorno y convenciones de nombres |
| [pm-065-cost-guardrails](pm-065-cost-guardrails/spec.md) | Aprobado | L2 | pm-063 | T077 | Presupuesto, alertas y etiquetado de costos |
| [pm-066-iac-baseline](pm-066-iac-baseline/spec.md) | Aprobado | L3 | pm-063, pm-064 | T078 | Herramienta de IaC elegida por ADR, estructura y flujo plan/apply |
| [pm-067-iac-state-management](pm-067-iac-state-management/spec.md) | Aprobado | L4 | pm-066 | T079 | Estado remoto de IaC con bloqueo, cifrado y sin secretos |
| [pm-068-cloud-secrets-configuration](pm-068-cloud-secrets-configuration/spec.md) | Aprobado | L5 | pm-066, pm-067 | T080 | Inyección de secretos y configuración sin `.env` en la nube |
| [pm-069-atlas-cloud-integration](pm-069-atlas-cloud-integration/spec.md) | Aprobado | L6 | pm-012, pm-066, pm-068, pm-072 | T081 | Acceso de red y usuarios de Atlas para los entornos cloud |
| [pm-070-cloud-redis-auth](pm-070-cloud-redis-auth/spec.md) | Propuesto | L6 | pm-013, pm-066, pm-068, pm-072 | T082 | Redis Auth administrado en Azure con TLS, autenticación, red privada, `noeviction` y comportamiento fail-closed |
| [pm-071-cloud-redis-cache](pm-071-cloud-redis-cache/spec.md) | Propuesto | L6 | pm-013, pm-066, pm-068, pm-072 | T083 | Redis Cache administrado en Azure con TLS, autenticación, red privada, expulsión LRU y degradación segura |
| [pm-072-cloud-network-security](pm-072-cloud-network-security/spec.md) | Aprobado | L4 | pm-066 | T084 | Red, reglas de entrada y salida y segmentación |
| [pm-073-public-https-endpoint](pm-073-public-https-endpoint/spec.md) | Aprobado | L9 | pm-017, pm-072, pm-076 | T085 | Dominio, DNS, TLS, HSTS y redirección de HTTP a HTTPS |
| [pm-074-container-registry](pm-074-container-registry/spec.md) | Aprobado | L6 | pm-059, pm-066 | T086 | Azure Container Registry (o alternativa validada dentro de Azure) con etiquetas inmutables y trazabilidad imagen-versión |
| [pm-075-backend-two-instances](pm-075-backend-two-instances/spec.md) | Aprobado | L7 | pm-015, pm-068, pm-069, pm-070, pm-071, pm-072, pm-074 | T087 | Al menos dos instancias del backend con sondas y sin estado local |
| [pm-076-load-balancer](pm-076-load-balancer/spec.md) | Aprobado | L8 | pm-075 | T088 | Balanceador de carga con reparto verificable |
| [pm-077-cd-deployment-pipeline](pm-077-cd-deployment-pipeline/spec.md) | Aprobado | L10 | pm-055, pm-057, pm-058, pm-059, pm-073, pm-075, pm-076 | T089 | Despliegue continuo versionado con humo posterior y reversa |
| [pm-078-cloud-observability](pm-078-cloud-observability/spec.md) | Aprobado | L8 | pm-054, pm-075 | T090 | Eventos, errores y alertas en un servicio de observabilidad de Azure, sin PII ni secretos |
| [pm-079-image-storage-adapter](pm-079-image-storage-adapter/spec.md) | Propuesto | L6 | pm-039, pm-066 | T091 | Almacenamiento de imágenes privado por defecto |
| [pm-080-image-upload-endpoint](pm-080-image-upload-endpoint/spec.md) | Propuesto | L8 | pm-045, pm-079 | T092 | Contrato, límites y validación de subida de imágenes |
| [pm-081-private-image-access](pm-081-private-image-access/spec.md) | Propuesto | L9 | pm-079, pm-080 | T093 | URLs temporales para imágenes privadas |
| [pm-082-cdn-public-delivery](pm-082-cdn-public-delivery/spec.md) | Propuesto | L10 | pm-073, pm-079 | T094 | Distribución de imágenes públicas mediante CDN |
| [pm-083-infrastructure-teardown](pm-083-infrastructure-teardown/spec.md) | Aprobado | L11 | pm-066, pm-067, pm-068, pm-069, pm-070, pm-071, pm-072, pm-073, pm-074, pm-075, pm-076, pm-078, pm-079, pm-082 | T095 | Destrucción ordenada y protegida de la infraestructura |
| [pm-084-cloud-deployment-acceptance](pm-084-cloud-deployment-acceptance/spec.md) | Aprobado | L12 | pm-046, pm-073, pm-075, pm-076, pm-077, pm-078, pm-083 | T096 | Aceptación del sistema desplegado: dos instancias, balanceador, HTTPS y costo |

## Orden de ejecución por niveles

Un Spec solo puede iniciarse cuando sus dependencias están completas. Dentro de un nivel los Specs son independientes entre sí. Dependencias que no siguen el orden numérico: `pm-041` depende de `pm-047` (la paginación se define antes del listado) y `pm-034` depende de `pm-036` (la cuota por correo extiende la política de rate limiting).

| Nivel | Specs |
|---:|---|
| L0 | pm-001, pm-062 |
| L1 | pm-002, pm-003, pm-004, pm-022, pm-063 |
| L2 | pm-005, pm-012, pm-013, pm-015, pm-018, pm-019, pm-020, pm-023, pm-026, pm-027, pm-064, pm-065 |
| L3 | pm-006, pm-007, pm-009, pm-014, pm-017, pm-021, pm-024, pm-025, pm-028, pm-050, pm-066 |
| L4 | pm-008, pm-010, pm-016, pm-033, pm-047, pm-048, pm-049, pm-051, pm-055, pm-056, pm-067, pm-072 |
| L5 | pm-011, pm-037, pm-039, pm-054, pm-057, pm-058, pm-059, pm-060, pm-061, pm-068 |
| L6 | pm-029, pm-030, pm-038, pm-040, pm-041, pm-042, pm-043, pm-044, pm-052, pm-069, pm-070, pm-071, pm-074, pm-079 |
| L7 | pm-031, pm-045, pm-075 |
| L8 | pm-032, pm-036, pm-076, pm-078, pm-080 |
| L9 | pm-034, pm-073, pm-081 |
| L10 | pm-035, pm-077, pm-082 |
| L11 | pm-046, pm-083 |
| L12 | pm-053, pm-084 |

## Tasks T001–T102

| Task | Tipo | Spec principal | Estado | Título |
|---|---|---|---|---|
| T001 | Histórica | [pm-001](pm-001-solution-structure/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Crear solución y proyectos |
| T002 | Histórica | [pm-002](pm-002-clean-architecture-dependency-rules/spec.md) | Cerrado (2026-11-03) | Configurar referencias de Clean Architecture |
| T003 | Histórica | [pm-003](pm-003-code-standards-central-packages/spec.md) | Cerrado (2026-11-03) | Estándares y administración de dependencias |
| T004 | Histórica | [pm-004](pm-004-external-configuration-secrets/spec.md) | Cerrado (2026-11-03) | Configuración `.env` local y secretos |
| T005 | Histórica | [pm-005](pm-005-dependency-injection-composition/spec.md) | Cerrado (2026-11-03) | Módulos de inyección de dependencias |
| T006 | Histórica | [pm-006](pm-006-cqrs-mediator/spec.md) | Cerrado (2026-11-03) | CQRS y Mediator |
| T007 | Histórica | [pm-008](pm-008-openapi-scalar-documentation/spec.md) | Cerrado (2026-11-03) | Versionado, convenciones MVC, OpenAPI y Scalar |
| T008 | Histórica | [pm-009](pm-009-response-wrapper-contract/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Response Wrapper y middleware de errores |
| T009 | Histórica | [pm-011](pm-011-request-validation/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana; política 400 ratificada) | Validación de requests y contratos básicos |
| T010 | Histórica | [pm-012](pm-012-atlas-connectivity/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Configuración MongoDB Atlas y Redis separados |
| T011 | Histórica | [pm-016](pm-016-local-compose-environment/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Health, contenedor y ejecución local |
| T012 | Histórica | [pm-055](pm-055-ci-pull-request-pipeline/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | CI base DevSecOps |
| T013 | Histórica | [pm-022](pm-022-user-domain-roles/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Modelar usuario, roles y sesión |
| T014 | Histórica | [pm-024](pm-024-users-persistence/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Persistencia Atlas de usuarios y sesiones |
| T015 | Histórica | [pm-026](pm-026-password-hashing-argon2id/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Hasher Argon2id |
| T016 | Histórica | [pm-027](pm-027-access-token-jwt/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Servicio de Access JWT |
| T017 | Histórica | [pm-028](pm-028-token-revocation-redis/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Blacklist Redis Auth |
| T018 | Histórica | [pm-029](pm-029-user-registration/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Endpoint Register |
| T019 | Histórica | [pm-030](pm-030-user-login/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Endpoint Login |
| T020 | Histórica | [pm-031](pm-031-refresh-token-rotation/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Refresh Token persistente y rotatorio |
| T021 | Histórica | [pm-032](pm-032-user-logout/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Endpoint Logout |
| T022 | Histórica | [pm-033](pm-033-authorization-policies/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Políticas de autenticación y autorización |
| T023 | Histórica | [pm-034](pm-034-auth-brute-force-protection/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Pruebas unitarias, integración y seguridad de auth |
| T024 | Histórica | [pm-061](pm-061-api-docs-contract-sync/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Contratos y documentación de auth |
| T025 | Histórica | [pm-035](pm-035-auth-session-lifecycle-acceptance/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Gates y revisión independiente de auth |
| T026 | Histórica | [pm-037](pm-037-owner-profile-read/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Implementar GET perfil propio |
| T027 | Histórica | [pm-038](pm-038-owner-profile-update/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Implementar PUT perfil propio |
| T028 | Histórica | [pm-038](pm-038-owner-profile-update/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Pruebas y gates de perfil |
| T029 | Histórica | [pm-039](pm-039-pet-domain-persistence/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Definir modelo Pet y persistencia |
| T030 | Histórica | [pm-040](pm-040-pet-create/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Crear mascota |
| T031 | Histórica | [pm-041](pm-041-pet-list/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Listar mascotas |
| T032 | Histórica | [pm-042](pm-042-pet-detail/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Obtener detalle |
| T033 | Histórica | [pm-043](pm-043-pet-update/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Actualizar mascota |
| T034 | Histórica | [pm-044](pm-044-pet-soft-delete/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Eliminar mascota |
| T035 | Histórica | [pm-045](pm-045-pet-ownership-bola/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Asegurar autorización y ownership |
| T036 | Histórica | [pm-039](pm-039-pet-domain-persistence/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Pruebas unitarias Pet |
| T037 | Histórica | [pm-039](pm-039-pet-domain-persistence/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Pruebas de integración Atlas |
| T038 | Histórica | [pm-045](pm-045-pet-ownership-bola/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Pruebas de seguridad Pet |
| T039 | Histórica | [pm-061](pm-061-api-docs-contract-sync/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Pruebas de contrato y documentación |
| T040 | Histórica | [pm-045](pm-045-pet-ownership-bola/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Gates y revisión independiente Pet |
| T041 | Histórica | [pm-046](pm-046-first-milestone-acceptance/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Validación end-to-end y aceptación del primer hito |
| T042 | Retrospectiva | [pm-007](pm-007-api-route-conventions/spec.md) | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva de rutas versionadas |
| T043 | Retrospectiva | [pm-010](pm-010-global-exception-handling/spec.md) | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva del manejo global de excepciones |
| T044 | Retrospectiva | [pm-013](pm-013-redis-dual-connectivity/spec.md) | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva de conectividad Redis dual |
| T045 | Retrospectiva | [pm-014](pm-014-health-probes/spec.md) | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva de health probes |
| T046 | Retrospectiva | [pm-015](pm-015-api-container-image/spec.md) | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva de la imagen del contenedor |
| T047 | Retrospectiva | [pm-017](pm-017-trusted-proxy-forwarded-headers/spec.md) | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva de proxies confiables |
| T048 | Retrospectiva | [pm-018](pm-018-static-analysis-sast/spec.md) | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva de análisis estático |
| T049 | Retrospectiva | [pm-019](pm-019-secret-scanning/spec.md) | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva del escaneo de secretos |
| T050 | Retrospectiva | [pm-020](pm-020-dependency-license-audit/spec.md) | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva de la auditoría de dependencias y licencias |
| T051 | Retrospectiva | [pm-021](pm-021-local-quality-gate/spec.md) | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva del gate de calidad local |
| T052 | Retrospectiva | [pm-023](pm-023-refresh-session-domain/spec.md) | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva del dominio de sesiones |
| T053 | Retrospectiva | [pm-025](pm-025-refresh-sessions-persistence/spec.md) | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva de la persistencia de sesiones |
| T054 | Retrospectiva | [pm-036](pm-036-rate-limiting/spec.md) | Aprobada y documentada (2026-11-08, aprobación humana; retrospectiva) | Documentar la evidencia retrospectiva del rate limiting de autenticación |
| T055 | Nueva | [pm-007](pm-007-api-route-conventions/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Verificar automáticamente rutas contra el catálogo y documentar reglas de nombrado |
| T056 | Nueva | [pm-034](pm-034-auth-brute-force-protection/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Implementar cuota por correo, bloqueo y limitador asíncrono de hashing |
| T057 | Nueva | [pm-036](pm-036-rate-limiting/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Definir la matriz de rate limiting para endpoints no autenticación |
| T058 | Nueva | [pm-047](pm-047-pagination-convention/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Implementar la convención de paginación |
| T059 | Nueva | [pm-048](pm-048-response-compression/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Implementar y evaluar la compresión HTTP |
| T060 | Nueva | [pm-049](pm-049-json-serialization-performance/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Optimizar y medir la serialización JSON |
| T061 | Nueva | [pm-050](pm-050-distributed-cache-infrastructure/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Implementar el adaptador de caché distribuida sobre Redis Cache |
| T062 | Nueva | [pm-051](pm-051-cache-policy-data-classification/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Definir la política de clasificación de datos y caché |
| T063 | Nueva | [pm-052](pm-052-database-indexing-efficiency/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Inventariar índices y evidenciar consultas críticas |
| T064 | Nueva | [pm-053](pm-053-api-performance-baseline/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Medir la línea base de rendimiento |
| T065 | Nueva | [pm-054](pm-054-structured-logging-pii-safe/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Implementar logging estructurado sin PII |
| T066 | Nueva | [pm-055](pm-055-ci-pull-request-pipeline/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Crear el workflow de CI de pull request |
| T067 | Nueva | [pm-055](pm-055-ci-pull-request-pipeline/spec.md) | Propuesto | Configurar ramas protegidas y checks requeridos |
| T068 | Nueva | [pm-056](pm-056-test-coverage-baseline/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana; CA-02 pendiente de enmienda) | Medir la línea base de cobertura |
| T069 | Nueva | [pm-057](pm-057-container-image-scan/spec.md) | Cerrado | Integrar el escaneo de la imagen de contenedor |
| T070 | Nueva | [pm-058](pm-058-sbom-generation/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Generar el SBOM por compilación |
| T071 | Nueva | [pm-059](pm-059-release-versioning/spec.md) | En progreso | Definir el versionado de releases |
| T072 | Nueva | [pm-060](pm-060-sdd-integrity-gate/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Integrar el gate de integridad SDD en CI |
| T073 | Nueva | [pm-061](pm-061-api-docs-contract-sync/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Sincronizar contratos, catálogo y OpenAPI |
| T074 | Nueva | [pm-062](pm-062-cloud-cost-feasibility-study/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Elaborar el estudio de costos de Azure for Students |
| T075 | Nueva | [pm-063](pm-063-cloud-provider-selection/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Registrar la decisión de proveedor y los ADR de servicios Azure |
| T076 | Nueva | [pm-064](pm-064-environment-strategy/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Definir la estrategia de entornos |
| T077 | Nueva | [pm-065](pm-065-cost-guardrails/spec.md) | Propuesto | Definir presupuesto, alertas y etiquetado de costos |
| T078 | Nueva | [pm-066](pm-066-iac-baseline/spec.md) | Propuesto | Establecer la línea base de IaC |
| T079 | Nueva | [pm-067](pm-067-iac-state-management/spec.md) | Propuesto | Configurar el estado remoto de IaC |
| T080 | Nueva | [pm-068](pm-068-cloud-secrets-configuration/spec.md) | Propuesto | Configurar secretos y configuración en Azure |
| T081 | Nueva | [pm-069](pm-069-atlas-cloud-integration/spec.md) | Propuesto | Integrar MongoDB Atlas con el entorno Azure |
| T082 | Nueva | [pm-070](pm-070-cloud-redis-auth/spec.md) | Propuesto | Aprovisionar y verificar Redis Auth en Azure |
| T083 | Nueva | [pm-071](pm-071-cloud-redis-cache/spec.md) | Propuesto | Aprovisionar y verificar Redis Cache en Azure |
| T084 | Nueva | [pm-072](pm-072-cloud-network-security/spec.md) | Propuesto | Definir red y seguridad de red |
| T085 | Nueva | [pm-073](pm-073-public-https-endpoint/spec.md) | Propuesto | Publicar el endpoint HTTPS |
| T086 | Nueva | [pm-074](pm-074-container-registry/spec.md) | Propuesto | Configurar el registro de contenedores |
| T087 | Nueva | [pm-075](pm-075-backend-two-instances/spec.md) | Propuesto | Desplegar dos instancias del backend |
| T088 | Nueva | [pm-076](pm-076-load-balancer/spec.md) | Propuesto | Configurar el Load Balancer |
| T089 | Nueva | [pm-077](pm-077-cd-deployment-pipeline/spec.md) | Propuesto | Implementar el pipeline de CD |
| T090 | Nueva | [pm-078](pm-078-cloud-observability/spec.md) | Propuesto | Configurar la observabilidad cloud |
| T091 | Nueva | [pm-079](pm-079-image-storage-adapter/spec.md) | Propuesto | Implementar el adaptador de almacenamiento de imágenes |
| T092 | Nueva | [pm-080](pm-080-image-upload-endpoint/spec.md) | Propuesto | Implementar el endpoint de carga de imágenes |
| T093 | Nueva | [pm-081](pm-081-private-image-access/spec.md) | Propuesto | Implementar el acceso privado a imágenes |
| T094 | Nueva | [pm-082](pm-082-cdn-public-delivery/spec.md) | Propuesto | Configurar el CDN de contenido público |
| T095 | Nueva | [pm-083](pm-083-infrastructure-teardown/spec.md) | Propuesto | Definir y probar el procedimiento de teardown |
| T096 | Nueva | [pm-084](pm-084-cloud-deployment-acceptance/spec.md) | Propuesto | Ejecutar la aceptación del despliegue cloud |
| T097 | Nueva | [pm-017](pm-017-trusted-proxy-forwarded-headers/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana) | Verificar el rechazo de cabeceras reenviadas de proxies no confiables |
| T098 | Nueva | [pm-028](pm-028-token-revocation-redis/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana; sin revisión independiente de seguridad) | Ligar el access token a la sesión (claim `sid`) y revocar por sesión en Redis Auth |
| T099 | Nueva | [pm-031](pm-031-refresh-token-rotation/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana; sin revisión independiente de seguridad) | Ventana de gracia del refresh ante solicitudes concurrentes o reintentos |
| T100 | Nueva | [pm-036](pm-036-rate-limiting/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana; sin revisión independiente de seguridad) | Límite por IP distribuido en Redis Auth, validación de proxies al arrancar y documentación de saltos |
| T101 | Nueva | [pm-034](pm-034-auth-brute-force-protection/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana; sin revisión independiente de seguridad) | Bloqueo de login por par (correo, IP) y techo por correo con espera corta |
| T102 | Nueva | [pm-029](pm-029-user-registration/spec.md) | Cerrado y aprobado (2026-11-08, aprobación humana; sin revisión independiente de seguridad) | Igualar tiempos del registro y documentar el riesgo aceptado de enumeración (H4) |

## Primer hito

Los cuatro endpoints de autenticación y los siete endpoints funcionales (11 en total) están definidos en [`docs/api/endpoints.md`](../docs/api/endpoints.md). El hito se cumple únicamente al cerrar `T041` (Spec principal `pm-046`) con todos los gates y aprobaciones.

## Reglas de actualización

- Actualiza este registro y [traceability.md](traceability.md) al crear, aprobar, bloquear, reemplazar o cerrar un Spec o una Task.
- No dupliques aquí requisitos detallados ni tablas completas de endpoints.
- No marques como `Cerrado` sin gates y aprobación humana; los agentes no se auto-firman.
- Valida con `scripts/check-sdd.ps1` antes de proponer cambios al registro.

## Historial del registro

- **2026-11-05 — pm-035:** se añadieron `pm-033` y `pm-034` como dependencias directas (aprobado por el usuario). Justificación en [pm-035](pm-035-auth-session-lifecycle-acceptance/spec.md) §6. Efecto en niveles: pm-035 pasa a L10, pm-046 a L11 y pm-053 a L12. No se modificó ninguna Task histórica ni la propiedad de ningún criterio.
- **2026-11-05 — pm-033 / T022:** pasan a `En progreso` por autorización de implementación del usuario. T022 no está cerrada (ver [task.md](pm-033-authorization-policies/task.md)).
- **2026-11-08 — pm-039 / T029:** pasan a `En progreso` por autorización del usuario (campos de la maqueta móvil y borrado físico). Se reescribieron pm-039 CA-01 y CA-03, pm-044 CA-01 a CA-03 y pm-041 CA-01, con sus mismos identificadores y propietarios.
- **2026-11-08 — pm-040, pm-042, pm-043, pm-044 (T030, T032, T033, T034):** aprobados por el usuario (opción A) y pasan a `En progreso`. Se añadió pm-040 CA-04 (400 con el campo). pm-041 (T031) espera a la paginación (pm-047).
- **2026-11-08 — T030, T032, T033, T034:** cierre aprobado por el usuario (sin revisión independiente); pm-040, pm-042, pm-043 y pm-044 pasan a `Cerrado`.
- **2026-11-08 — T036, T037 (pm-039):** pasan a `En progreso` por pedido del usuario. Se verificaron CA-04 y CA-05 y se añadieron dos pruebas de integración (uso del índice por propietario y borrado concurrente). Falta la aprobación humana del cierre.
- **2026-11-08 — T036, T037:** cierre aprobado por el usuario (sin revisión independiente); pm-039 pasa a `Cerrado`.
- **2026-11-08 — T066, T069, T070, T071, T072 (pm-055, pm-057, pm-058, pm-059, pm-060):** pasan a `En progreso` por pedido del usuario (grupo de CI). Se crean `plan.md` y `checklists/gates.md`; `check-sdd.ps1` gana tres validaciones y pruebas negativas. Decisiones pendientes: `ignore-unfixed` de Trivy (T069), umbral NuGet Low/High (T066) e interpretación de la autorización de etiquetas (T071).
- **2026-11-08 — T001, T070, T072:** cierre aprobado por el usuario (sin revisión independiente); pm-058 y pm-060 pasan a `Cerrado`. pm-001 sigue `En revisión` hasta que el usuario apruebe el Spec. `check-sdd.ps1` acepta el cierre aprobado de T001.
- **2026-11-08 — T069:** a pedido del usuario se quita `ignore-unfixed` de Trivy y la imagen pasa a `aspnet:10.0-noble-chiseled` (0 HIGH/CRITICAL). Pendiente de aprobación humana.
- **2026-11-08 — T069:** cierre aprobado por el usuario (sin revisión independiente); pm-057 pasa a `Cerrado`.
- **2026-11-08 — T012:** cierre aprobado por el usuario (sin revisión independiente); pm-055 sigue `En progreso` por T066 y T067. `check-sdd.ps1` acepta el cierre de T012.
- **2026-11-08 — T066:** umbral de vulnerabilidades NuGet igualado a High en local y en CI (decisión del usuario); T066 sigue pendiente de aprobación.
- **2026-11-08 — T022:** alcance histórico cumplido (`RequireOwner` en pm-040 a pm-044) y verificado con Atlas y Redis reales; sigue `En progreso` (revisión independiente y aprobación humana).
- **2026-11-08 — T026, T027, T028:** evidencia escrita (`checklists/gates.md` de pm-037 y pm-038), pruebas añadidas y cierre aprobado por el usuario (sin revisión independiente); pm-037 y pm-038 pasan a `Cerrado`.
- **2026-11-08 — T022 y T066:** cierre aprobado por el usuario (sin revisión independiente); pm-033 pasa a `Cerrado`. pm-055 sigue `En progreso` por T067.
- **2026-11-08 — pm-041, pm-045, pm-047:** specs aprobados por el usuario (`Aprobado`). Valores de paginación aprobados en pm-047 §6: `page` desde 1, `pageSize` 20 por defecto y 50 como máximo, respuesta con `items`, `page`, `pageSize`, `totalCount` y `totalPages`. Las Tasks T058, T031, T035, T038 y T040 siguen `Propuesto` hasta que se autorice su implementación.
- **2026-11-08 — T058, T031, T035, T038, T040 (pm-047, pm-041, pm-045):** implementación autorizada por el usuario; pasan a `En progreso` con `plan.md` y `checklists/gates.md`. T058, T031, T035 y T038 están verificadas y esperan aprobación humana; T040 espera además la revisión independiente del módulo. Queda implementado `GET /api/v1/pets/get-all` (paginado), uno de los 11 endpoints del primer hito; la API suma 14 endpoints.
- **2026-11-08 — pm-045 CA-04 (T035):** criterio añadido con aprobación del usuario tras la revisión de seguridad: máximo 100 mascotas por propietario, 60 peticiones por minuto por usuario en `/api/v1/pets/*` y 16 KB de cuerpo. La propiedad del criterio es de T035; ningún otro criterio cambió de Task.
- **2026-11-08 — T031, T035, T038, T040, T058:** cierre aprobado por el usuario; pm-041, pm-045 y pm-047 pasan a `Cerrado`. La revisión independiente del módulo de mascotas se realizó el mismo día (ver pm-045 gates).
- **2026-11-08 — pm-034, pm-035, pm-036:** specs aprobados por el usuario. pm-034 y pm-036 pasan a `En progreso` con T056, T057 y T054 implementadas/documentadas y pendientes de aprobación; pm-035 queda `Aprobado` (T025 sin iniciar). Valores de pm-034 aprobados: 5 fallos de login por correo en 15 minutos (el sexto bloquea 15 minutos), 30 refrescos por usuario en 15 minutos y hashing acotado a los núcleos con cola del doble. T023, T025 y T041 siguen abiertas.
- **2026-11-08 — T054, T056, T057:** cierre aprobado por el usuario (sin revisión independiente de las Tasks); T054 queda «Aprobada y documentada» (el gate SDD prohíbe marcar una retrospectiva como «Cerrado»); T056 y T057 quedan cerradas y pm-036 pasa a `Verificado`. pm-034 sigue `En progreso` hasta cerrar T023.
- **2026-11-08 — T023 y T025:** implementadas/verificadas con la revisión independiente del grupo de autenticación (seguridad: apto con observaciones; trazabilidad: consistente con observaciones). Se corrigieron H1, H2 y H5 y una carrera del script de Redis; quedan abiertos H3, H4, H6, H7, H8 e I2 para decisión. pm-035 pasa a `En progreso`; T025 no puede cerrarse hasta aprobar pm-028 a pm-032.
- **2026-11-08 — pm-028 a pm-032, T023 y T025:** specs pm-028 a pm-032 aprobados (sus Tasks T017 a T021 ya estaban cerradas); T023 y T025 cerradas con aprobación humana; pm-034 y pm-035 pasan a `Cerrado`. Con esto el grupo de autenticación queda cerrado salvo los hallazgos H3, H4, H6, H7, H8 e I2, pendientes de decisión (ver pm-034 gates). T041 (pm-046) pasa a depender solo de la aprobación de su propio spec.
- **2026-11-08 — pm-046 / T041:** spec aprobado por el usuario; validación de extremo a extremo de los 11 endpoints realizada y en verde (39/39, imagen chiseled, Atlas y Redis reales; suites y escaneos en verde). T041 queda `En progreso` hasta la aprobación humana explícita del hito (CA-02).
- **2026-11-08 — T041 / pm-046:** el usuario aprobó explícitamente el primer hito; T041 y pm-046 pasan a `Cerrado`. Primer hito (11 endpoints) aceptado. Quedan pendientes los hallazgos de seguridad H3, H4, H6, H7, H8 e I2, la ejecución del pipeline en GitHub y el despliegue en Azure con esta versión, y los specs base «En revisión».
- **2026-11-08 — pm-001 a pm-027:** los 27 specs que seguían `En revisión` quedan aprobados por el usuario. Pasan a `Cerrado` los 15 cuya Task histórica ya estaba cerrada (pm-001 a pm-006, pm-008, pm-009, pm-011, pm-012, pm-016, pm-022, pm-024, pm-026 y pm-027) y a `Aprobado` los 12 con Tasks pendientes (pm-007, pm-010, pm-013, pm-014, pm-015, pm-017 a pm-021, pm-023 y pm-025). Siguen abiertas las Tasks retrospectivas T042 a T053, T055 y T097; aprobar un spec no las cierra.
- **2026-11-08 — T042 a T053, T055 y T097:** documentada la evidencia retrospectiva de las 12 Tasks retrospectivas restantes (pm-007, pm-010, pm-013, pm-014, pm-015, pm-017, pm-018, pm-019, pm-020, pm-021, pm-023 y pm-025) y construidas T055 (`RouteCatalogTests` y reglas de nombrado en `docs/api/endpoints.md`) y T097 (`ForwardedHeadersSpoofingTests`). Los 12 specs pasan a `En progreso`; todas esperan revisión y aprobación humana. Los criterios pm-010 CA-03 (cancelación del cliente) quedan solo con cobertura parcial.
- **2026-11-08 — T074, T075 y T076 (pm-062, pm-063, pm-064):** ejecutadas por encargo del usuario como las siguientes tres Tasks viables (specs aprobados, trabajo documental sin recursos reales). Estudio de costos con fuentes oficiales, ADR-005 a ADR-010 con alternativas dentro de Azure, índice de decisiones y estrategia de entornos con pruebas. Las tres esperan revisión y aprobación humana; los ADR siguen «Propuesto».
- **2026-11-08 — Aprobación humana de T042–T053, T055 y T097:** las 12 retrospectivas quedan «Aprobada y documentada»; T055 y T097 «Cerrado y aprobado». T071 y T074–T076 siguen en progreso por decisión del usuario.
- **2026-11-08 — pm-054, pm-056 y pm-061 aprobados por el usuario y ejecutados (T065, T068, T073; T024 y T039 verificadas con evidencia):** logging JSON con correlación y sin datos sensibles (`RequestLoggingMiddleware`), línea base de cobertura 93.7 % y documento OpenAPI verificado contra el Response Wrapper. Esperan revisión independiente y aprobación humana; el umbral de cobertura espera enmienda.
- **2026-11-08 — pm-048 a pm-053 aprobados por el usuario y ejecutados (T059 a T064):** compresión HTTP con exclusión de `/auth/*`, JSON generado en compilación, adaptador de caché sobre Redis Cache con fallo seguro, política de caché (hoy nada se cachea), inventario de índices con `explain` en Atlas y línea base de rendimiento en proceso. Esperan revisión independiente y aprobación humana.
- **2026-11-08 — Aprobación de T074, T075 y T076 por el usuario:** quedan «Cerrado y aprobado»; los ADR-005 a ADR-010 siguen «Propuesto» (la aprobación de la Task no aprueba cada ADR). Se corrigió la prueba de concurrencia de refresh, se registraron eventos de seguridad (I2) y se hicieron revisiones independientes de seguridad y de trazabilidad.
- **2026-11-08 — Aprobación humana de T059 a T065, T068, T073, T024 y T039:** quedan «Cerrado y aprobado» (T068 con CA-02, el umbral de cobertura, pendiente de enmienda) y sus specs «Verificado», salvo pm-056 que sigue «En progreso». T071 queda sin aprobar por decisión del usuario.
- **2026-11-08 — Hallazgos de seguridad H3, H4, H6, H7 y H8:** el usuario aprueba las soluciones (A en H7, H3 y H8; B en H4; A, B y C en H6). Se registran como T098 a T102 bajo pm-028, pm-031, pm-036, pm-034 y pm-029, que vuelven a «En progreso». El gate `check-sdd` amplía su rango de Tasks a T001–T102.
- **2026-11-08 — T098 a T102 implementadas y verificadas** (H7 `sid`, H8 gracia de 10 s, H6 límite por IP en Redis y validación de proxies, H3 bloqueo por par y techo por correo, H4 riesgo aceptado). Esperan revisión independiente y aprobación humana.
- **2026-11-08 — Aprobación humana de T098 a T102:** quedan «Cerrado y aprobado» y sus specs pm-028, pm-029, pm-031, pm-034 y pm-036 «Verificado». Sin revisión independiente de seguridad de estos cambios (el usuario aprobó sin ella).
