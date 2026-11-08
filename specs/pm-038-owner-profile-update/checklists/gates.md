# Gates — pm-038 owner-profile-update (T027 y T028)

> Estado: cerrada con aprobación humana el 2026-11-08 (sin revisión independiente). Spec `Cerrado`.

| CA | Task | Pruebas | Resultado |
|---|---|---|---|
| `pm-038#CA-01` | T027 | `UserProfileEndpointsTests.Update_sets_trimmed_name_and_get_returns_it` (contrato, `users-me-update`); `ProfileHandlersTests.Update_trims_the_name_moves_the_update_date_and_keeps_email_role_and_creation`, `Update_only_changes_the_account_of_the_token`, `Update_returns_null_and_creates_nothing_when_the_account_does_not_exist`; `AuthPersistenceIntegrationTests.Profile_update_persists_only_the_name_and_the_update_date_of_that_user` (Atlas) | En verde |
| `pm-038#CA-02` | T027 | `UserProfileEndpointsTests.Update_rejects_attempts_to_change_role_or_email` (miembros desconocidos → 400), `Update_with_missing_or_blank_name_returns_400_with_the_field`, `Update_with_a_name_longer_than_80_returns_400`; `ProfileHandlersTests.Validator_rejects_blank_or_control_characters_naming_the_field`, `Validator_accepts_80_characters_and_rejects_81`, `The_update_command_has_no_member_to_change_role_or_email_and_does_not_print_the_name` | En verde |
| `pm-038#CA-03` | T028 | Suite del perfil completa en Unit (`ProfileHandlersTests`, `UserTests.ChangeDisplayName_*`, `DisplayName_accepts_80_characters_and_rejects_81`), Integration (Atlas), Security (155) y Contract (`users-me-get` y `users-me-update`); gates con evidencia en pm-037 y pm-038 | En verde |

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
- Brecha encontrada y corregida al cerrar T028: faltaban pruebas unitarias de los casos de uso del perfil y una prueba de Atlas para `UpdateProfile`; se añadieron (`ProfileHandlersTests`, 10; prueba de persistencia, 1).
- [ ] Revisión independiente (no realizada; el usuario aprobó el cierre sin ella).
- [x] Aprobación humana del cierre (2026-11-08).
