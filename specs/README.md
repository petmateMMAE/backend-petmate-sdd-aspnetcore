# Registro SDD — PetMate Backend

> Registro de Specs, Tasks y dependencias según la Constitución v1.1.0 (§12.3). Los números `pm-NNN` **identifican** Specs; **no determinan el orden de trabajo**: lo gobiernan las dependencias. La matriz de trazabilidad está en [traceability.md](traceability.md) y la estructura anterior por slices está archivada en [_historico/](_historico/README.md).

## Convenciones

- Estructura plana `specs/pm-NNN-slug/` con `spec.md` y `task.md`; `plan.md` y `checklists/gates.md` se crean en la fase de plan, tras la aprobación del Spec.
- Numeración de Specs `pm-001`…`pm-084`: global, secuencial e inmutable. Un Spec retirado pasa a `Reemplazado`.
- Tasks globales `T001`…`T097`: `T001`–`T041` históricas (identificador, alcance y estado intactos), `T042`–`T054` retrospectivas, `T055`–`T097` nuevas.
- Cada Task tiene un único Spec principal; criterios citados como `pm-NNN#CA-NN`.
- Estado de Spec al migrar: `En revisión` si tiene Tasks históricas cerradas, verificadas o implementadas, o retrospectivas (texto nuevo pendiente de aprobación humana); `Propuesto` en los demás casos. Ninguna Task cambia de estado por la migración.
- Plataforma cloud: Microsoft Azure con Azure for Students (Constitución §8). Los Specs cloud documentan servicios candidatos como hipótesis; ninguno se asume gratuito hasta completar pm-062.

## Specs

| Spec | Estado | Nivel | Dependencias | Tasks | Objetivo |
|---|---|---:|---|---|---|
| [pm-001-solution-structure](pm-001-solution-structure/spec.md) | En revisión | L0 | — | T001 | Solución .NET 10 con 4 proyectos de código y 4 de pruebas que compilan en Release |
| [pm-002-clean-architecture-dependency-rules](pm-002-clean-architecture-dependency-rules/spec.md) | En revisión | L1 | pm-001 | T002 | Dirección de referencias entre capas exigida por prueba de arquitectura |
| [pm-003-code-standards-central-packages](pm-003-code-standards-central-packages/spec.md) | En revisión | L1 | pm-001 | T003 | Analizadores, advertencias como error y versiones centralizadas de paquetes |
| [pm-004-external-configuration-secrets](pm-004-external-configuration-secrets/spec.md) | En revisión | L1 | pm-001 | T004 | Configuración por entorno, `.env` ignorado y validación de arranque sin exponer valores |
| [pm-005-dependency-injection-composition](pm-005-dependency-injection-composition/spec.md) | En revisión | L2 | pm-002 | T005 | Módulos de DI por capa sin Service Locator |
| [pm-006-cqrs-mediator](pm-006-cqrs-mediator/spec.md) | En revisión | L3 | pm-005 | T006 | Commands y Queries despachados por un Mediator propio con registro explícito |
| [pm-007-api-route-conventions](pm-007-api-route-conventions/spec.md) | En revisión | L3 | pm-005 | T042, T055 | Patrón `/api/v1/{recurso}/{acción}` y su verificación automática |
| [pm-008-openapi-scalar-documentation](pm-008-openapi-scalar-documentation/spec.md) | En revisión | L4 | pm-005, pm-007 | T007 | Documento OpenAPI generado y Scalar como única UI, solo en Development |
| [pm-009-response-wrapper-contract](pm-009-response-wrapper-contract/spec.md) | En revisión | L3 | pm-005 | T008 | Contrato `statusCode/message/data` con `data` nulo y coincidencia con HTTP |
| [pm-010-global-exception-handling](pm-010-global-exception-handling/spec.md) | En revisión | L4 | pm-009 | T043 | Manejo central de excepciones sin fuga de información, 500/503 y cancelación |
| [pm-011-request-validation](pm-011-request-validation/spec.md) | En revisión | L5 | pm-006, pm-009, pm-010 | T009 | Validación de solicitudes antes del handler con 400 y errores en español |
| [pm-012-atlas-connectivity](pm-012-atlas-connectivity/spec.md) | En revisión | L2 | pm-004 | T010 | Conexión tipada a MongoDB Atlas, base de pruebas separada y sin Mongo local |
| [pm-013-redis-dual-connectivity](pm-013-redis-dual-connectivity/spec.md) | En revisión | L2 | pm-004 | T044 | Dos conexiones Redis independientes (Auth y Cache) con TLS fuera de Development |
| [pm-014-health-probes](pm-014-health-probes/spec.md) | En revisión | L3 | pm-012, pm-013 | T045 | Liveness sin dependencias y readiness con detalle mínimo por dependencia |
| [pm-015-api-container-image](pm-015-api-container-image/spec.md) | En revisión | L2 | pm-001, pm-004 | T046 | Imagen multi-stage de la API, usuario no root y sin secretos |
| [pm-016-local-compose-environment](pm-016-local-compose-environment/spec.md) | En revisión | L4 | pm-013, pm-014, pm-015 | T011 | Compose local con API y dos Redis, red, volumen y sin MongoDB |
| [pm-017-trusted-proxy-forwarded-headers](pm-017-trusted-proxy-forwarded-headers/spec.md) | En revisión | L3 | pm-005 | T047, T097 | IP real del cliente detrás de proxies confiables configurados |
| [pm-018-static-analysis-sast](pm-018-static-analysis-sast/spec.md) | En revisión | L2 | pm-003 | T048 | Reglas de seguridad de los analizadores como errores de compilación |
| [pm-019-secret-scanning](pm-019-secret-scanning/spec.md) | En revisión | L2 | pm-004 | T049 | Detección de secretos en archivos versionables con controles positivos y negativos |
| [pm-020-dependency-license-audit](pm-020-dependency-license-audit/spec.md) | En revisión | L2 | pm-003 | T050 | Auditoría de vulnerabilidades, desuso y licencias NuGet con gobierno de excepciones |
| [pm-021-local-quality-gate](pm-021-local-quality-gate/spec.md) | En revisión | L3 | pm-018, pm-019, pm-020 | T051 | Orquestación local de restore, build, formato, auditorías y suites |
| [pm-022-user-domain-roles](pm-022-user-domain-roles/spec.md) | En revisión | L1 | pm-001 | T013 | Usuario, correo normalizado y roles Owner/Business sin Admin |
| [pm-023-refresh-session-domain](pm-023-refresh-session-domain/spec.md) | En revisión | L2 | pm-022 | T052 | Sesión con vencimiento por inactividad y absoluto y token de un solo uso |
| [pm-024-users-persistence](pm-024-users-persistence/spec.md) | En revisión | L3 | pm-012, pm-022 | T014 | Persistencia de usuarios en Atlas con correo único y arranque de índices |
| [pm-025-refresh-sessions-persistence](pm-025-refresh-sessions-persistence/spec.md) | En revisión | L3 | pm-012, pm-023 | T053 | Sesiones y tokens en Atlas con hash único, consumo atómico y TTL |
| [pm-026-password-hashing-argon2id](pm-026-password-hashing-argon2id/spec.md) | En revisión | L2 | pm-022 | T015 | Hash Argon2id con salt, parámetros mínimos, rehash y concurrencia acotada |
| [pm-027-access-token-jwt](pm-027-access-token-jwt/spec.md) | En revisión | L2 | pm-004, pm-022 | T016 | Emisión y validación de JWT HS256 de corta vida y configuración segura |
| [pm-028-token-revocation-redis](pm-028-token-revocation-redis/spec.md) | En revisión | L3 | pm-013, pm-027 | T017 | Lista negra de `jti` en Redis Auth con TTL y falla cerrada |
| [pm-029-user-registration](pm-029-user-registration/spec.md) | En revisión | L6 | pm-011, pm-024, pm-025, pm-026, pm-027 | T018 | `POST /api/v1/auth/register` que crea siempre un Owner |
| [pm-030-user-login](pm-030-user-login/spec.md) | En revisión | L6 | pm-011, pm-024, pm-025, pm-026, pm-027 | T019 | `POST /api/v1/auth/login` con respuesta idéntica ante credenciales inválidas |
| [pm-031-refresh-token-rotation](pm-031-refresh-token-rotation/spec.md) | En revisión | L7 | pm-025, pm-027, pm-030 | T020 | `POST /api/v1/auth/refresh` con rotación atómica y detección de replay |
| [pm-032-user-logout](pm-032-user-logout/spec.md) | En revisión | L8 | pm-028, pm-031 | T021 | `POST /api/v1/auth/logout` que revoca sesión y `jti` |
| [pm-033-authorization-policies](pm-033-authorization-policies/spec.md) | En progreso | L4 | pm-027, pm-028 | T022 | Política de autorización por defecto y por rol Owner/Business |
| [pm-034-auth-brute-force-protection](pm-034-auth-brute-force-protection/spec.md) | Propuesto | L9 | pm-030, pm-031, pm-036 | T023, T056 | Cuota por correo y bloqueo contra credential stuffing, y limitador asíncrono de hashing |
| [pm-035-auth-session-lifecycle-acceptance](pm-035-auth-session-lifecycle-acceptance/spec.md) | Propuesto | L10 | pm-028, pm-029, pm-030, pm-031, pm-032, pm-033, pm-034 | T025 | Escenarios de ciclo de vida de sesión entre endpoints y cierre del grupo de autenticación |
| [pm-036-rate-limiting](pm-036-rate-limiting/spec.md) | En revisión | L8 | pm-017, pm-029, pm-030, pm-031 | T054, T057 | Límites por política y por IP, auth ya implementado y matriz global pendiente |
| [pm-037-owner-profile-read](pm-037-owner-profile-read/spec.md) | Propuesto | L5 | pm-024, pm-033 | T026 | `GET /api/v1/users/me/get` del usuario autenticado |
| [pm-038-owner-profile-update](pm-038-owner-profile-update/spec.md) | Propuesto | L6 | pm-037 | T027, T028 | `PUT /api/v1/users/me/update` con campos aprobados |
| [pm-039-pet-domain-persistence](pm-039-pet-domain-persistence/spec.md) | Propuesto | L5 | pm-012, pm-033 | T029, T036, T037 | Modelo Pet, validadores, persistencia e índice por propietario |
| [pm-040-pet-create](pm-040-pet-create/spec.md) | Propuesto | L6 | pm-039 | T030 | `POST /api/v1/pets/create` con propietario tomado del token |
| [pm-041-pet-list](pm-041-pet-list/spec.md) | Propuesto | L6 | pm-039, pm-047 | T031 | `GET /api/v1/pets/get-all` solo de mascotas propias y activas, paginado |
| [pm-042-pet-detail](pm-042-pet-detail/spec.md) | Propuesto | L6 | pm-039 | T032 | `GET /api/v1/pets/get-by-id/{id}` solo de mascota propia |
| [pm-043-pet-update](pm-043-pet-update/spec.md) | Propuesto | L6 | pm-039 | T033 | `PUT /api/v1/pets/update/{id}` de mascota propia con campos permitidos |
| [pm-044-pet-soft-delete](pm-044-pet-soft-delete/spec.md) | Propuesto | L6 | pm-039 | T034 | `DELETE /api/v1/pets/delete/{id}` con eliminación lógica y `data:null` |
| [pm-045-pet-ownership-bola](pm-045-pet-ownership-bola/spec.md) | Propuesto | L7 | pm-040, pm-041, pm-042, pm-043, pm-044 | T035, T038, T040 | Matriz de propiedad y BOLA del módulo de mascotas y cierre de seguridad del módulo |
| [pm-046-first-milestone-acceptance](pm-046-first-milestone-acceptance/spec.md) | Propuesto | L11 | pm-035, pm-038, pm-045 | T041 | Aceptación de extremo a extremo de los 11 endpoints del primer hito |
| [pm-047-pagination-convention](pm-047-pagination-convention/spec.md) | Propuesto | L4 | pm-009 | T058 | Parámetros, tope y forma de paginación en listados |
| [pm-048-response-compression](pm-048-response-compression/spec.md) | Propuesto | L4 | pm-009 | T059 | Compresión de respuestas con evaluación de riesgos |
| [pm-049-json-serialization-performance](pm-049-json-serialization-performance/spec.md) | Propuesto | L4 | pm-009 | T060 | Serialización JSON eficiente con medición |
| [pm-050-distributed-cache-infrastructure](pm-050-distributed-cache-infrastructure/spec.md) | Propuesto | L3 | pm-013 | T061 | Adaptador de caché sobre Redis Cache, claves, TTL y política de falla |
| [pm-051-cache-policy-data-classification](pm-051-cache-policy-data-classification/spec.md) | Propuesto | L4 | pm-050 | T062 | Política de qué datos pueden cachearse y criterios de decisión |
| [pm-052-database-indexing-efficiency](pm-052-database-indexing-efficiency/spec.md) | Propuesto | L6 | pm-024, pm-025, pm-039 | T063 | Inventario de índices y evidencia de consultas críticas |
| [pm-053-api-performance-baseline](pm-053-api-performance-baseline/spec.md) | Propuesto | L12 | pm-046 | T064 | Objetivos de latencia medibles y prueba de carga sin costo |
| [pm-054-structured-logging-pii-safe](pm-054-structured-logging-pii-safe/spec.md) | Propuesto | L5 | pm-010 | T065 | Logging estructurado con correlación y sin datos personales |
| [pm-055-ci-pull-request-pipeline](pm-055-ci-pull-request-pipeline/spec.md) | En revisión | L4 | pm-021 | T012, T066, T067 | Workflow de pull request con paridad respecto al gate local y ramas protegidas |
| [pm-056-test-coverage-baseline](pm-056-test-coverage-baseline/spec.md) | Propuesto | L4 | pm-021 | T068 | Línea base y umbral de cobertura de pruebas |
| [pm-057-container-image-scan](pm-057-container-image-scan/spec.md) | Propuesto | L5 | pm-015, pm-055 | T069 | Escaneo de la imagen de contenedor con herramienta $0 |
| [pm-058-sbom-generation](pm-058-sbom-generation/spec.md) | Propuesto | L5 | pm-055 | T070 | Generación de SBOM por compilación |
| [pm-059-release-versioning](pm-059-release-versioning/spec.md) | Propuesto | L5 | pm-055 | T071 | Versionado SemVer, etiquetas y versión trazable a la imagen |
| [pm-060-sdd-integrity-gate](pm-060-sdd-integrity-gate/spec.md) | Propuesto | L5 | pm-055 | T072 | Validación automática de la integridad de specs, tasks y trazabilidad |
| [pm-061-api-docs-contract-sync](pm-061-api-docs-contract-sync/spec.md) | Propuesto | L5 | pm-007, pm-008, pm-009 | T024, T039, T073 | Rutas del código iguales al catálogo y wrapper representado en OpenAPI |
| [pm-062-cloud-cost-feasibility-study](pm-062-cloud-cost-feasibility-study/spec.md) | Propuesto | L0 | — | T074 | Estudio con fuentes oficiales del costo $0 MXN por componente en Azure for Students y del costo total de operación de la arquitectura |
| [pm-063-cloud-provider-selection](pm-063-cloud-provider-selection/spec.md) | Propuesto | L1 | pm-062 | T075 | Registrar Azure for Students como proveedor definitivo y seleccionar mediante ADR el servicio de Azure para cada componente |
| [pm-064-environment-strategy](pm-064-environment-strategy/spec.md) | Propuesto | L2 | pm-063 | T076 | Entornos, bases por entorno y convenciones de nombres |
| [pm-065-cost-guardrails](pm-065-cost-guardrails/spec.md) | Propuesto | L2 | pm-063 | T077 | Presupuesto, alertas y etiquetado de costos |
| [pm-066-iac-baseline](pm-066-iac-baseline/spec.md) | Propuesto | L3 | pm-063, pm-064 | T078 | Herramienta de IaC elegida por ADR, estructura y flujo plan/apply |
| [pm-067-iac-state-management](pm-067-iac-state-management/spec.md) | Propuesto | L4 | pm-066 | T079 | Estado remoto de IaC con bloqueo, cifrado y sin secretos |
| [pm-068-cloud-secrets-configuration](pm-068-cloud-secrets-configuration/spec.md) | Propuesto | L5 | pm-066, pm-067 | T080 | Inyección de secretos y configuración sin `.env` en la nube |
| [pm-069-atlas-cloud-integration](pm-069-atlas-cloud-integration/spec.md) | Propuesto | L6 | pm-012, pm-066, pm-068, pm-072 | T081 | Acceso de red y usuarios de Atlas para los entornos cloud |
| [pm-070-cloud-redis-auth](pm-070-cloud-redis-auth/spec.md) | Propuesto | L6 | pm-013, pm-066, pm-068, pm-072 | T082 | Redis Auth administrado en Azure con TLS, autenticación, red privada, `noeviction` y comportamiento fail-closed |
| [pm-071-cloud-redis-cache](pm-071-cloud-redis-cache/spec.md) | Propuesto | L6 | pm-013, pm-066, pm-068, pm-072 | T083 | Redis Cache administrado en Azure con TLS, autenticación, red privada, expulsión LRU y degradación segura |
| [pm-072-cloud-network-security](pm-072-cloud-network-security/spec.md) | Propuesto | L4 | pm-066 | T084 | Red, reglas de entrada y salida y segmentación |
| [pm-073-public-https-endpoint](pm-073-public-https-endpoint/spec.md) | Propuesto | L9 | pm-017, pm-072, pm-076 | T085 | Dominio, DNS, TLS, HSTS y redirección de HTTP a HTTPS |
| [pm-074-container-registry](pm-074-container-registry/spec.md) | Propuesto | L6 | pm-059, pm-066 | T086 | Azure Container Registry (o alternativa validada dentro de Azure) con etiquetas inmutables y trazabilidad imagen-versión |
| [pm-075-backend-two-instances](pm-075-backend-two-instances/spec.md) | Propuesto | L7 | pm-015, pm-068, pm-069, pm-070, pm-071, pm-072, pm-074 | T087 | Al menos dos instancias del backend con sondas y sin estado local |
| [pm-076-load-balancer](pm-076-load-balancer/spec.md) | Propuesto | L8 | pm-075 | T088 | Balanceador de carga con reparto verificable |
| [pm-077-cd-deployment-pipeline](pm-077-cd-deployment-pipeline/spec.md) | Propuesto | L10 | pm-055, pm-057, pm-058, pm-059, pm-073, pm-075, pm-076 | T089 | Despliegue continuo versionado con humo posterior y reversa |
| [pm-078-cloud-observability](pm-078-cloud-observability/spec.md) | Propuesto | L8 | pm-054, pm-075 | T090 | Eventos, errores y alertas en un servicio de observabilidad de Azure, sin PII ni secretos |
| [pm-079-image-storage-adapter](pm-079-image-storage-adapter/spec.md) | Propuesto | L6 | pm-039, pm-066 | T091 | Almacenamiento de imágenes privado por defecto |
| [pm-080-image-upload-endpoint](pm-080-image-upload-endpoint/spec.md) | Propuesto | L8 | pm-045, pm-079 | T092 | Contrato, límites y validación de subida de imágenes |
| [pm-081-private-image-access](pm-081-private-image-access/spec.md) | Propuesto | L9 | pm-079, pm-080 | T093 | URLs temporales para imágenes privadas |
| [pm-082-cdn-public-delivery](pm-082-cdn-public-delivery/spec.md) | Propuesto | L10 | pm-073, pm-079 | T094 | Distribución de imágenes públicas mediante CDN |
| [pm-083-infrastructure-teardown](pm-083-infrastructure-teardown/spec.md) | Propuesto | L11 | pm-066, pm-067, pm-068, pm-069, pm-070, pm-071, pm-072, pm-073, pm-074, pm-075, pm-076, pm-078, pm-079, pm-082 | T095 | Destrucción ordenada y protegida de la infraestructura |
| [pm-084-cloud-deployment-acceptance](pm-084-cloud-deployment-acceptance/spec.md) | Propuesto | L12 | pm-046, pm-073, pm-075, pm-076, pm-077, pm-078, pm-083 | T096 | Aceptación del sistema desplegado: dos instancias, balanceador, HTTPS y costo |

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

## Tasks T001–T097

| Task | Tipo | Spec principal | Estado | Título |
|---|---|---|---|---|
| T001 | Histórica | [pm-001](pm-001-solution-structure/spec.md) | Verificado (2026-11-03) | Crear solución y proyectos |
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
| T012 | Histórica | [pm-055](pm-055-ci-pull-request-pipeline/spec.md) | Implementada; NO cerrada (2026-11-03) | CI base DevSecOps |
| T013 | Histórica | [pm-022](pm-022-user-domain-roles/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Modelar usuario, roles y sesión |
| T014 | Histórica | [pm-024](pm-024-users-persistence/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Persistencia Atlas de usuarios y sesiones |
| T015 | Histórica | [pm-026](pm-026-password-hashing-argon2id/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Hasher Argon2id |
| T016 | Histórica | [pm-027](pm-027-access-token-jwt/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Servicio de Access JWT |
| T017 | Histórica | [pm-028](pm-028-token-revocation-redis/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Blacklist Redis Auth |
| T018 | Histórica | [pm-029](pm-029-user-registration/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Endpoint Register |
| T019 | Histórica | [pm-030](pm-030-user-login/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Endpoint Login |
| T020 | Histórica | [pm-031](pm-031-refresh-token-rotation/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Refresh Token persistente y rotatorio |
| T021 | Histórica | [pm-032](pm-032-user-logout/spec.md) | Cerrado y aprobado (2026-11-03, aprobación humana) | Endpoint Logout |
| T022 | Histórica | [pm-033](pm-033-authorization-policies/spec.md) | En progreso | Políticas de autenticación y autorización |
| T023 | Histórica | [pm-034](pm-034-auth-brute-force-protection/spec.md) | Propuesto | Pruebas unitarias, integración y seguridad de auth |
| T024 | Histórica | [pm-061](pm-061-api-docs-contract-sync/spec.md) | Propuesto | Contratos y documentación de auth |
| T025 | Histórica | [pm-035](pm-035-auth-session-lifecycle-acceptance/spec.md) | Propuesto | Gates y revisión independiente de auth |
| T026 | Histórica | [pm-037](pm-037-owner-profile-read/spec.md) | Propuesto | Implementar GET perfil propio |
| T027 | Histórica | [pm-038](pm-038-owner-profile-update/spec.md) | Propuesto | Implementar PUT perfil propio |
| T028 | Histórica | [pm-038](pm-038-owner-profile-update/spec.md) | Propuesto | Pruebas y gates de perfil |
| T029 | Histórica | [pm-039](pm-039-pet-domain-persistence/spec.md) | Propuesto | Definir modelo Pet y persistencia |
| T030 | Histórica | [pm-040](pm-040-pet-create/spec.md) | Propuesto | Crear mascota |
| T031 | Histórica | [pm-041](pm-041-pet-list/spec.md) | Propuesto | Listar mascotas |
| T032 | Histórica | [pm-042](pm-042-pet-detail/spec.md) | Propuesto | Obtener detalle |
| T033 | Histórica | [pm-043](pm-043-pet-update/spec.md) | Propuesto | Actualizar mascota |
| T034 | Histórica | [pm-044](pm-044-pet-soft-delete/spec.md) | Propuesto | Eliminar mascota |
| T035 | Histórica | [pm-045](pm-045-pet-ownership-bola/spec.md) | Propuesto | Asegurar autorización y ownership |
| T036 | Histórica | [pm-039](pm-039-pet-domain-persistence/spec.md) | Propuesto | Pruebas unitarias Pet |
| T037 | Histórica | [pm-039](pm-039-pet-domain-persistence/spec.md) | Propuesto | Pruebas de integración Atlas |
| T038 | Histórica | [pm-045](pm-045-pet-ownership-bola/spec.md) | Propuesto | Pruebas de seguridad Pet |
| T039 | Histórica | [pm-061](pm-061-api-docs-contract-sync/spec.md) | Propuesto | Pruebas de contrato y documentación |
| T040 | Histórica | [pm-045](pm-045-pet-ownership-bola/spec.md) | Propuesto | Gates y revisión independiente Pet |
| T041 | Histórica | [pm-046](pm-046-first-milestone-acceptance/spec.md) | Propuesto | Validación end-to-end y aceptación del primer hito |
| T042 | Retrospectiva | [pm-007](pm-007-api-route-conventions/spec.md) | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva de rutas versionadas |
| T043 | Retrospectiva | [pm-010](pm-010-global-exception-handling/spec.md) | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva del manejo global de excepciones |
| T044 | Retrospectiva | [pm-013](pm-013-redis-dual-connectivity/spec.md) | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva de conectividad Redis dual |
| T045 | Retrospectiva | [pm-014](pm-014-health-probes/spec.md) | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva de health probes |
| T046 | Retrospectiva | [pm-015](pm-015-api-container-image/spec.md) | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva de la imagen del contenedor |
| T047 | Retrospectiva | [pm-017](pm-017-trusted-proxy-forwarded-headers/spec.md) | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva de proxies confiables |
| T048 | Retrospectiva | [pm-018](pm-018-static-analysis-sast/spec.md) | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva de análisis estático |
| T049 | Retrospectiva | [pm-019](pm-019-secret-scanning/spec.md) | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva del escaneo de secretos |
| T050 | Retrospectiva | [pm-020](pm-020-dependency-license-audit/spec.md) | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva de la auditoría de dependencias y licencias |
| T051 | Retrospectiva | [pm-021](pm-021-local-quality-gate/spec.md) | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva del gate de calidad local |
| T052 | Retrospectiva | [pm-023](pm-023-refresh-session-domain/spec.md) | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva del dominio de sesiones |
| T053 | Retrospectiva | [pm-025](pm-025-refresh-sessions-persistence/spec.md) | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva de la persistencia de sesiones |
| T054 | Retrospectiva | [pm-036](pm-036-rate-limiting/spec.md) | Propuesto (retrospectiva por documentar) | Documentar la evidencia retrospectiva del rate limiting de autenticación |
| T055 | Nueva | [pm-007](pm-007-api-route-conventions/spec.md) | Propuesto | Verificar automáticamente rutas contra el catálogo y documentar reglas de nombrado |
| T056 | Nueva | [pm-034](pm-034-auth-brute-force-protection/spec.md) | Propuesto | Implementar cuota por correo, bloqueo y limitador asíncrono de hashing |
| T057 | Nueva | [pm-036](pm-036-rate-limiting/spec.md) | Propuesto | Definir la matriz de rate limiting para endpoints no autenticación |
| T058 | Nueva | [pm-047](pm-047-pagination-convention/spec.md) | Propuesto | Implementar la convención de paginación |
| T059 | Nueva | [pm-048](pm-048-response-compression/spec.md) | Propuesto | Implementar y evaluar la compresión HTTP |
| T060 | Nueva | [pm-049](pm-049-json-serialization-performance/spec.md) | Propuesto | Optimizar y medir la serialización JSON |
| T061 | Nueva | [pm-050](pm-050-distributed-cache-infrastructure/spec.md) | Propuesto | Implementar el adaptador de caché distribuida sobre Redis Cache |
| T062 | Nueva | [pm-051](pm-051-cache-policy-data-classification/spec.md) | Propuesto | Definir la política de clasificación de datos y caché |
| T063 | Nueva | [pm-052](pm-052-database-indexing-efficiency/spec.md) | Propuesto | Inventariar índices y evidenciar consultas críticas |
| T064 | Nueva | [pm-053](pm-053-api-performance-baseline/spec.md) | Propuesto | Medir la línea base de rendimiento |
| T065 | Nueva | [pm-054](pm-054-structured-logging-pii-safe/spec.md) | Propuesto | Implementar logging estructurado sin PII |
| T066 | Nueva | [pm-055](pm-055-ci-pull-request-pipeline/spec.md) | Propuesto | Crear el workflow de CI de pull request |
| T067 | Nueva | [pm-055](pm-055-ci-pull-request-pipeline/spec.md) | Propuesto | Configurar ramas protegidas y checks requeridos |
| T068 | Nueva | [pm-056](pm-056-test-coverage-baseline/spec.md) | Propuesto | Medir la línea base de cobertura |
| T069 | Nueva | [pm-057](pm-057-container-image-scan/spec.md) | Propuesto | Integrar el escaneo de la imagen de contenedor |
| T070 | Nueva | [pm-058](pm-058-sbom-generation/spec.md) | Propuesto | Generar el SBOM por compilación |
| T071 | Nueva | [pm-059](pm-059-release-versioning/spec.md) | Propuesto | Definir el versionado de releases |
| T072 | Nueva | [pm-060](pm-060-sdd-integrity-gate/spec.md) | Propuesto | Integrar el gate de integridad SDD en CI |
| T073 | Nueva | [pm-061](pm-061-api-docs-contract-sync/spec.md) | Propuesto | Sincronizar contratos, catálogo y OpenAPI |
| T074 | Nueva | [pm-062](pm-062-cloud-cost-feasibility-study/spec.md) | Propuesto | Elaborar el estudio de costos de Azure for Students |
| T075 | Nueva | [pm-063](pm-063-cloud-provider-selection/spec.md) | Propuesto | Registrar la decisión de proveedor y los ADR de servicios Azure |
| T076 | Nueva | [pm-064](pm-064-environment-strategy/spec.md) | Propuesto | Definir la estrategia de entornos |
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
| T097 | Nueva | [pm-017](pm-017-trusted-proxy-forwarded-headers/spec.md) | Propuesto | Verificar el rechazo de cabeceras reenviadas de proxies no confiables |

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
