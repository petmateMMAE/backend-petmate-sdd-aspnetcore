# Gates — pm-037 owner-profile-read (T026)

> Estado: cerrada con aprobación humana el 2026-11-08 (sin revisión independiente). Spec `Cerrado`.

| CA | Task | Pruebas | Resultado |
|---|---|---|---|
| `pm-037#CA-01` | T026 | `UserProfileEndpointsTests.Get_returns_own_profile_without_secrets_and_without_name_until_it_is_set` (contrato, `users-me-get`); `ProfileHandlersTests.Get_returns_the_own_profile_without_name_until_it_is_set_and_never_the_hash` (unitaria) | En verde |
| `pm-037#CA-02` | T026 | `UserProfileEndpointsTests.Anonymous_requests_get_401_wrapper` (401), `Each_account_only_sees_its_own_profile` (cada cuenta solo ve la suya); `ProfileHandlersTests.Get_returns_null_when_the_account_of_the_token_does_not_exist` (cuenta inexistente → 404 en el endpoint) | En verde |

## Evidencia (2026-11-08)
| Gate | Resultado | Cómo |
|---|---|---|
| Build Release con analizadores como error | 0 advertencias, 0 errores | Contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, repositorio en solo lectura y copia sin `.env`; Atlas de pruebas real (`--env-file .env`, sin imprimir valores) y dos Redis temporales con contraseñas aleatorias (los de Compose usan contraseñas marcadas como ficticias y las pruebas las omiten a propósito) |
| Unit | 152/152 (10 nuevas de `ProfileHandlersTests`) | Mismo contenedor |
| Security | 155/155 | Mismo contenedor |
| Contract | 142/142 (`users-me-get`: 5, `users-me-update`: 12) | Mismo contenedor |
| Integration | 48/48, 0 omitidas (1 nueva: `Profile_update_persists_only_the_name_and_the_update_date_of_that_user`) | Mismo contenedor |
| Paquetes nuevos | Ninguno | — |
| Gate SDD | OK | `scripts/check-sdd.ps1` |
- Seguridad: el identificador sale solo del claim `sub` del token validado; la respuesta nunca incluye el hash de la contraseña ni, en `ToString`, el correo (`ProfileHandlersTests`). Autorización: `[Authorize]` por defecto (Owner y Business), probada en `AuthorizationPolicyTests` y `PublicSurfaceTests`.
- [ ] Revisión independiente (no realizada; el usuario aprobó el cierre sin ella).
- [x] Aprobación humana del cierre (2026-11-08).
