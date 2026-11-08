# Gates — pm-034 auth-brute-force-protection (T056)

> Estado: T056 cerrada con aprobación humana el 2026-11-08 (sin revisión independiente de la Task). T023 (pruebas del grupo de autenticación) sigue abierta, por lo que pm-034 sigue `En progreso`.

| CA | Task | Pruebas | Resultado |
|---|---|---|---|
| `pm-034#CA-01` | T056 | `ThrottlingTests.Five_failures_are_answered_normally_and_the_sixth_locks_…`, `The_lock_is_per_email_and_ends_after_the_lock_duration`, `A_successful_login_clears_the_failure_count`, `Failures_older_than_the_window_do_not_count`, `Refresh_is_limited_per_user_to_30_attempts_per_window`; `AuthAttemptThrottleTests` (Redis real: bloqueo con TTL, contadores independientes, conteo atómico); contrato: `Login_lock_of_one_email_does_not_affect_another_account` | En verde |
| `pm-034#CA-02` | T056 | `ThrottlingTests.The_lock_does_not_reveal_whether_the_email_exists`, `A_locked_email_does_not_cost_any_password_hash`; contrato: `Login_lock_looks_the_same_for_an_unknown_email`; `Attempts_beyond_the_maximum_lock_the_subject_…_raw_subject_is_never_stored` (las claves de Redis solo llevan el hash) | En verde |
| `pm-034#CA-03` | T056 | `ThrottlingTests.The_limiter_runs_at_most_the_allowed_hashes_at_once_and_rejects_what_does_not_fit_in_the_queue`, `The_limiter_requires_valid_sizes`, `Login_and_register_answer_too_many_requests_when_the_limiter_rejects_and_create_nothing` | En verde |
| `pm-034#CA-04` | T056 | Contrato: `Login_locks_an_email_after_five_failures_with_429_wrapper_and_retry_after` (wrapper de tres claves, `data: null`, `Retry-After` > 14 min); además `Login_fails_closed_with_503_when_the_attempt_store_is_down`; `An_unreachable_redis_fails_closed_with_the_auth_dependency_error` | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, Atlas de pruebas real y dos Redis temporales |
| Suites | Unit 186/186, Security 169/169, Contract 168/168, Integration 53/53 | Mismo contenedor |
| Kestrel y Redis reales (`docker compose`) | Correo existente: intentos 1 a 5 → 401, el 6.º → 429 con `Retry-After: 900`; la contraseña correcta estando bloqueado → 429. Las claves de Redis son `auth:throttle:login:<sha256>` y `auth:lock:login:<sha256>` (sin correos). | `Invoke-WebRequest` local |
| Paquetes nuevos | Ninguno | — |

- Limitación de la comprobación manual: la parte de «correo inexistente» se mezcló con el límite por IP (10 por minuto), por lo que la equivalencia entre correos existentes e inexistentes se demuestra con las pruebas automáticas, no con esa ejecución.
- Limitación del diseño: el bloqueo permite que alguien deje sin acceso temporal a un correo ajeno (15 minutos por ronda); el límite por IP lo acota.
- Observación de rendimiento: el hashing real hace más lento ContractTests (de ~30 s a ~1 m 45 s) por las pruebas nuevas de login.
## T023 — pruebas de seguridad del grupo de autenticación (2026-11-08)
T023 no tiene criterios propios; aporta la evidencia de pruebas de seguridad de autenticación:

| Área | Pruebas |
|---|---|
| Argon2id | `Argon2idPasswordHasherTests` (SecurityTests) |
| JWT | `JwtAccessTokenServiceTests`, `ForgedClaimsTests`, `JwtPipelineTests` |
| Refresh y sesiones | `AuthHandlersTests` (rotación, replay, concurrencia, expiración), `RefreshResilienceTests`, `AuthFlowIntegrationTests` (Atlas y Redis reales) |
| Logout y lista negra | `AuthEndpointsTests`, `TokenRevocationStoreTests`, `AuthFlowIntegrationTests` |
| Anti-enumeración | `Login_failures_for_unknown_email_and_wrong_password_return_the_same_401_body`, `The_lock_does_not_reveal_whether_the_email_exists` |
| Fuerza bruta | `ThrottlingTests`, `AuthThrottlingEndpointsTests`, `AuthAttemptThrottleTests`, `AuthFlowIntegrationTests` (`Login_lockout_works_end_to_end_…`, `A_concurrent_burst_of_guesses_…`) |
| Ciclo de vida | `SessionLifecycleTests` (ver pm-035) |

Suites actuales: Unit 188/188, Security 170/170, Contract 170/170, Integration 56/56. Revisión independiente: ver [pm-035 gates](../../pm-035-auth-session-lifecycle-acceptance/checklists/gates.md).

## Revisión de seguridad (2026-11-08): hallazgos del grupo de autenticación y su estado
| # | Sev. | Hallazgo | Estado |
|---|---|---|---|
| H1 | Media | Comprobar el bloqueo y contar el intento no eran atómicos: una ráfaga podía evaluar más contraseñas que el máximo | **Corregido:** el intento se cuenta antes de verificar; prueba unitaria y de integración con Redis real (ráfaga de 20 solicitudes: a lo sumo 5 verificaciones) |
| H2 | Media | El limitador de hashing liberaba el cupo al cancelar la espera aunque el hash siguiera corriendo | **Corregido:** el cupo se libera al terminar el hash; `A_cancelled_caller_does_not_free_the_slot_…` |
| (nuevo) | Media | Hallado al probar H1 con Redis real: tras fijarse el bloqueo, los intentos de una ráfaga reiniciaban el contador | **Corregido:** el script de Redis no cuenta con un bloqueo activo; `Attempts_during_an_active_lock_are_refused_…` |
| H5 | Baja | `logout` sin límite de tasa | **Corregido:** política por usuario (60 por minuto) |
| H3 | Media | Denegación de servicio dirigida: 6 intentos fallidos bloquean la cuenta 15 minutos | **Abierto; requiere decisión del usuario** (aceptar, o clave `(correo, IP)`, retardo progresivo o canal de desbloqueo) |
| H4 | Baja | El registro devuelve 409 para un correo existente (enumeración); mitigado por el límite por IP | **Abierto; decisión de producto** (ya documentado como excepción) |
| H6 | Baja | El límite por IP es por instancia y depende de `TRUSTED_PROXIES` detrás del balanceador | **Abierto:** requisito de despliegue que debe constar en el runbook (pm-073 a pm-077) |
| H7 | Baja | Los access tokens de una sesión revocada por replay viven hasta 15 minutos (`jti` sin ligar a la sesión) | **Abierto; riesgo aceptado hasta decidir** (el logout sí revoca el token presentado) |
| H8 | Baja | Si falla algo tras consumir el refresh token, el reintento parece un replay y revoca la sesión | **Abierto; riesgo aceptado hasta decidir** (no hay ventana de gracia) |
| I2 | Info | Sin registro de eventos de seguridad (login fallido, bloqueo, replay) ni `Cache-Control: no-store` en respuestas con tokens | **Abierto:** mejora para pm-054 (logging) y para el contrato |

- [ ] Decisión del usuario sobre H3, H4, H6, H7, H8 e I2 (corregir, planificar o aceptar con constancia en `Memoria.md`). Pendiente: el cierre de T023 y T056 no la sustituye.
- [x] Aprobación humana del cierre de T023 (2026-11-08); pm-034 `Cerrado`.
- [x] Aprobación humana del cierre de T056 (2026-11-08).
