# Gates — 01 Autenticación inicial

- [ ] T013–T025 completados. _(T013–T015 cerradas 2026-11-03; T016–T021 verificadas, pendientes de aprobación humana de cierre; T022–T025 pendientes)_
- [ ] Register crea Owner y no permite elevar rol.
- [ ] Login verifica Argon2id y no enumera usuarios.
- [ ] Access Token firmado/validado con claims y expiración correctos.
- [ ] Refresh Token opaco; solo hash persistido en Atlas.
- [ ] Refresh rota token y detecta reuso/replay.
- [ ] Política idle 30 días y absoluta 90 días es configurable y probada.
- [ ] Logout revoca refresh session y añade jti a blacklist Redis Auth.
- [ ] Redis Auth outage falla cerrada.
- [ ] Ningún token/password aparece en logs.
- [ ] No existe Admin; Business no se asigna desde registro público.
- [ ] Unit, Integration, Security y Contract pasan.
- [ ] OWASP ASVS L2 aplicable revisado.
- [ ] SAST, secret scan, NuGet vulnerability/license scan pasan.
- [ ] Endpoint catalog y memoria actualizados.
- [ ] Security review y traceability review completados.
- [ ] Aprobación humana registrada.

## Evidencia
| Gate | Resultado | Evidencia | Fecha |
|---|---|---|---|
| T016–T021 — Cierre humano | Cerrado | El usuario aprobó el cierre de T016, T017, T018, T019, T020 y T021; T022 no autorizada. Observaciones abiertas trasladadas a T022 y posteriores (plan.md §11.6) | 2026-11-03 |
| T016–T021 — Regresión acumulada | Cumple | `scripts/quality-gate.ps1` (Atlas de pruebas + Redis de Compose) exit 0: build Release + SAST, `dotnet format`, NuGet 0 vulnerables / 0 en desuso, licencias 50 paquetes, UnitTests 110/110, SecurityTests 135/135, ContractTests 46/46, IntegrationTests 39/39 (0 omitidas). Pruebas en contenedor del SDK (Smart App Control bloquea binarios nuevos en el host) | 2026-11-03 |
| T016 — Access JWT | Cumple | HS256 fijo; claims sub/jti/role/iss/aud/iat/nbf/exp; ClockSkew 0; 15 min; clave ≥ 32 bytes y plantillas rechazadas fuera de Development; JwtBearer 10.0.14 y JsonWebTokens 8.23.0 (MIT) | 2026-11-03 |
| T017 — Blacklist Redis Auth | Cumple | `jwt:blacklist:{jti}` con TTL = vida restante; solo Redis Auth; falla cerrada → 503; revocado → 401; prueba real de caída de Redis | 2026-11-03 |
| T018 — Register | Cumple | 201/400/409/429; rol no elegible (miembro `role` → 400); carrera de registro real → una cuenta | 2026-11-03 |
| T019 — Login | Cumple | 401 idéntico para correo inexistente y contraseña errónea; hash ficticio; rehash real; rate limit por IP; concurrencia de hashing acotada | 2026-11-03 |
| T020 — Refresh rotatorio | Cumple | Opaco 256 bits; solo SHA-256 en Atlas; consumo atómico; replay revoca sesión; pruebas reales: refresh concurrente, replay, aislamiento entre usuarios | 2026-11-03 |
| T021 — Logout | Cumple | Revoca sesión Atlas y `jti` en Redis Auth; ajeno/desconocido → 403; flujo real register→login→refresh→logout | 2026-11-03 |
| T016–T021 — Revisiones | PASS / PASS con observaciones | traceability-reviewer PASS; security-reviewer PASS (3 medias y 5 bajas; corregidas: carrera UpdateActivity, reloj atrasado, jti inválido → 401, rate limit IPv6/proxies, filtro de logs JwtBearer, descripción OpenAPI; documentadas en plan.md §11.6 y Memoria.md). Estado: verificadas, pendientes de aprobación humana de cierre | 2026-11-03 |
| T013, T014, T015 — Cierre humano | Cerrado | El usuario aprobó el cierre de las tres tareas; T012 sigue abierta (workflow YAML, escaneo de contenedores y SBOM diferidos); T016 no autorizada | 2026-11-03 |
| T015 — Hasher Argon2id | Cumple | Geralt 4.5.0 (MIT) + libsodium 1.0.22 (ISC); PHC `$argon2id$v=19$m=65536,t=3,p=1`; Security 97/97 en contenedor; benchmark mediana 139–143 ms; hashes hostiles rechazados antes de verificar | 2026-11-03 |
| T015 — Revisiones | PASS / PASS | security-reviewer (medias trasladadas a T019/T022) y traceability-reviewer | 2026-11-03 |
| T014 — Persistencia Atlas | Cumple | Repositorios y documentos Mongo, índices (email único, tokenHash único, sessionId, userId, TTL por absoluteExpiresAtUtc), 503 genérico ante fallo; Integration 24/24 en contenedor, 13 pruebas reales contra MONGODB_TEST_DATABASE (guarda: distinta de MONGODB_DATABASE y con «test»; colecciones con prefijo propio) | 2026-11-03 |
| T014 — Revisiones | PASS / PASS | security-reviewer (2 medias y 4 bajas; corregidas o documentadas para T020) y traceability-reviewer (sin deriva) | 2026-11-03 |
| T013 — Modelo de dominio | Cumple | User/Email/PasswordHash/UserRole (Owner, Business; sin Admin), RefreshSession/RefreshToken/RefreshTokenHash/SessionPolicy; Restore interno; UnitTests 71/71 en contenedor SDK; build Release 0/0 | 2026-11-03 |
| T013 — Revisiones | PASS / PASS | security-reviewer (1 media y 5 bajas, resueltas) y traceability-reviewer (sin deriva) | 2026-11-03 |

Estado: Pendiente.
