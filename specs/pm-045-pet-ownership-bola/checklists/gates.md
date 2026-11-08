# Gates — pm-045 pet-ownership-bola (T035, T038, T040)

> Estado (2026-11-08): T035, T038 y T040 implementadas y verificadas; la **revisión independiente del módulo se realizó** (seguridad: apto con observaciones; trazabilidad: consistente con observaciones, todo corregido, incluidos los dos riesgos medios). Ninguna de las tres está cerrada: falta la aprobación humana.

| CA | Task | Pruebas | Resultado |
|---|---|---|---|
| `pm-045#CA-01` | T035 | `PetOwnershipSecurityTests.Every_repository_operation_except_add_requires_the_owner`, `The_repository_has_no_operation_that_reads_or_writes_a_pet_by_id_alone`, `Every_pet_request_carries_the_owner_taken_from_the_token_as_its_first_member`, `No_pets_action_takes_the_owner_from_the_route_query_or_body`; `PetHandlersTests` (Get/Update/Delete de mascota ajena → `null`/`false` y sin cambios); filtro `{_id, ownerId}` en `MongoPetRepository` | En verde |
| `pm-045#CA-04` | T035 | `PetHandlersTests.Create_stops_at_the_maximum_per_owner_…` y `The_maximum_is_the_approved_value`; `PetLimitsTests`: `Create_at_the_maximum_per_owner_returns_409_wrapper_…`, `Pets_endpoints_are_throttled_per_user_with_429_wrapper_and_retry_after` (el límite es por usuario: otro usuario no se ve afectado), `A_body_over_16_KB_is_rejected_with_413_wrapper_before_it_is_read` (16 KB + 1 y 1 MB), `An_oversized_update_body_…`, `A_body_just_under_16_KB_still_reaches_validation`; `PetOwnershipSecurityTests.The_pets_controller_is_rate_limited_per_user_and_creation_is_capped_per_owner`; `RateLimitAndProxyTests` (`PETS_RATE_LIMIT_PER_MINUTE` inválido falla al arrancar sin mostrar el valor) | En verde |
| `pm-045#CA-02` | T038 | Contrato: `PetEndpointsTests` (detalle, actualizar y eliminar de mascota ajena → 404 y sin cambios; sin token 401; Business 403) y `PetListEndpointsTests` (el listado no mezcla propietarios, `ownerId` en la consulta ignorado, 401/403); Seguridad: `PetOwnershipSecurityTests` (política `RequireOwner` en la clase, ningún `AllowAnonymous`, el propietario no sale en `PetResponse`/`PetResult` ni se acepta en `PetRequest`, listado acotado a 50); Atlas: `Another_owner_cannot_read_update_or_delete_the_pet` | En verde |
| `pm-045#CA-03` | T040 | Evidencia de los gates reunida abajo | Evidencia reunida y revisión independiente realizada (ver abajo); pendiente de aprobación humana |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0` (repositorio de solo lectura, copia sin `.env`), Atlas de pruebas real y dos Redis temporales con contraseñas aleatorias |
| Suites | Unit 172/172, Security 162/162, Contract 157/157, Integration 49/49 (0 omitidas) | Mismo contenedor |
| Auditoría NuGet | Vulnerabilidades (desde High), desuso y licencias en verde; sin paquetes nuevos | `scripts/quality-gate.ps1 -AuditOnly` |
| Secret scan | Dentro de Security 162/162 | `SecretScanTests` |
| Imagen | 0 HIGH/CRITICAL (Trivy), imagen chiseled | Ver pm-057 |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |

## Módulo de mascotas: estado de los gates por Spec
- pm-039: cerrado (2026-11-08). pm-040, pm-042, pm-043, pm-044: cerrados (2026-11-08). pm-041: T031 verificada, pendiente de aprobación. pm-045: este documento.
- Hallazgos abiertos conocidos (no bloqueantes): desempate por `_id` fuera del índice (pm-052); `ignore-unfixed` ya eliminado (pm-057).

- [x] Revisión independiente de seguridad y de trazabilidad (2026-11-08, dos agentes de solo lectura; ver abajo).
- [x] Aprobación humana de T035, T038 y T040 (2026-11-08).

## Revisión independiente (2026-11-08)
Dos agentes distintos de quien implementó, en modo de solo lectura, sin ejecutar pruebas (los resultados de las suites son los de este documento).

### Seguridad — APTO CON OBSERVACIONES (sin hallazgos críticos ni altos)
Comprobado y correcto: filtro de propietario en cada consulta (`Owned`, `ListByOwner`, `CountByOwner`), propietario solo del claim `sub`, 404 uniforme, sin asignación masiva, filtros tipados sin inyección, sin `ownerId` en las respuestas ni en logs, validación de entradas, paginación acotada y orden estable, política `RequireOwner` con denegación por defecto, imagen chiseled sin root.

| # | Sev. | Hallazgo | Estado |
|---|---|---|---|
| S1 | Media | Sin tope de mascotas por Owner ni rate limiting en `/api/v1/pets/*` (el limitador solo cubre autenticación): un Owner autenticado puede crear mascotas en bucle y agotar la cuota de Atlas. | **Corregido (2026-11-08, decisión del usuario):** máximo 100 por propietario (409) y 60 peticiones por minuto por usuario (429); pm-045 CA-04 |
| S2 | Media | Sin límite explícito del tamaño del cuerpo de la petición (Kestrel permite ~30 MB por defecto): un cliente autenticado puede enviar cuerpos muy grandes que se deserializan antes de validar. | **Corregido (2026-11-08, decisión del usuario):** 16 KB para toda la API (`RequestBodyLimitMiddleware`, 413); pm-045 CA-04 |
| S3 | Baja | `photoUrl` acepta cualquier host HTTPS (incluidas IP privadas o localhost); no hay SSRF mientras el servidor no la resuelva. | Mitigado con documentación: la API nunca la resuelve (`docs/api/endpoints.md`); restringir el origen en pm-079/pm-080 |
| S4 | Baja | `Skip` con `page` grande. | Sin riesgo real: acotado por el total del propietario (≤ 100 con S1) y `pageSize` ≤ 50 |
| S5 | Info | Conversión estricta de `sex` (distingue mayúsculas, rechaza números). | Correcto |
| S6 | Info | Los textos admiten HTML (no hay XSS en el servidor; el cliente debe escapar). | Documentado en `docs/api/endpoints.md` |

### Trazabilidad — CONSISTENTE CON OBSERVACIONES (sin discrepancias entre código, contrato y pruebas)
Comprobado: cada CA tiene una Task propietaria única; todos los métodos de prueba citados existen; las rutas coinciden entre controlador, `docs/api/endpoints.md`, `EndpointTraitTests`, la matriz del pipeline, `docs/testing/endpoints.md` y la prueba de humo; los valores de paginación coinciden con pm-047 §6; el borrado físico se refleja en todos los documentos.

Observaciones corregidas el 2026-11-08: M1 (trazabilidad de pm-040 a pm-044 desactualizada), M2 (gates de pm-040/042/043/044 copiados de una plantilla: reescritos con la tabla criterio→prueba), B1 (casillas de aprobación de specs cerrados), B2 (fases y Definition of Done de las Tasks cerradas), B3 (texto de evidencia), B4 (planes retrospectivos de pm-040/042/043/044, con la desviación anotada), B5 y B6 (estado y tiempo verbal en `docs/api/endpoints.md`), B7 (totales de `docs/testing/endpoints.md`), B8 (redacción del registro), B9 (T058 y pm-047 en AC-14), B10 (nota de superado en `Memoria.md`) y B11 (conteo de pruebas de Atlas).

- [x] Decisión del usuario sobre S1 y S2 (2026-11-08): implementar los límites. Hecho y verificado (abajo).
- [x] Aprobación humana de T035, T038 y T040 (2026-11-08).

## Verificación de los límites (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y Redis temporales |
| Suites | Unit 174/174, Security 166/166, Contract 163/163, Integration 49/49 | Mismo contenedor |
| Kestrel real (imagen chiseled, `docker compose`) | Cuerpo de 20 KB con `Content-Length` → 413; el mismo cuerpo fragmentado (`Transfer-Encoding: chunked`) → 413; 20 KB hacia `/auth/login` anónimo → 413; 65 listados seguidos → 59 con 200 y 6 con 429 (más 1 petición previa contada: 60 por minuto) | `curl.exe` local |
| Paquetes nuevos | Ninguno | — |

- Limitación conocida: el tope de 100 se comprueba al crear (cuenta y alta no son atómicas); el límite de 60 por minuto acota cuánto puede excederse. Es una protección contra abuso, no un contador exacto.
- Efecto sobre otros endpoints: el límite de 16 KB aplica a toda la API; los cuerpos de autenticación y perfil son mucho menores.
