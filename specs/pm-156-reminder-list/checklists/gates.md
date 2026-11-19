# Gates — pm-156-reminder-list

> Evidencia del 2026-11-17. El cierre queda pendiente de la revisión independiente y de la aprobación humana.

## Comandos ejecutados (contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, repositorio en solo lectura, sin `.env`)
- Restore y build Release con `-warnaserror`: 0 errores, 0 advertencias.
- Unitarias 348/348. Seguridad 206/206 (incluye el escaneo de secretos). Contrato 435/435.
- Integración: 12 pasan y 69 omitidas (sin Atlas ni Redis de prueba); las omitidas no cuentan como aprobadas.
- `dotnet list package --deprecated`: solo `xunit 2.9.3` (Legacy) en los proyectos de prueba; ningún proyecto de producción.
- `scripts/check-sdd.ps1`: OK.

## Evidencia por criterio
- CA-01 (revisiones): las revisiones del seguimiento ya generan recordatorios `Review` (pm-160, `FollowupChatUseCases`), así que el listado las incluye; `docs/api/endpoints.md` lo documenta. **Sin prueba de contrato propia** del listado con una revisión.
- CA-02 (estado de lectura): nuevo `POST /api/v1/reminders/mark-read` `{id}` (marca por aviso, decisión del usuario del 2026-11-17); `isRead` refleja `readAtUtc`. Pruebas: `Marking_a_reminder_read_shows_it_read_is_idempotent_and_a_stranger_gets_404` y `Marking_read_with_a_missing_empty_or_unknown_member_returns_400` (3 casos).
- CA-03 (solo datos propios): `Deleting_the_record_or_the_pet_removes_the_reminder_and_each_owner_only_sees_their_own`; `mark-read` filtra por dueño en el repositorio (otro usuario → 404).

## Limitaciones
- `dotnet format --verify-no-changes` **falla** por marcadores de fin de línea (CRLF) en archivos que este trabajo no modificó (p. ej. `UserProfileEndpointsTests.cs`); parece un efecto de la copia en Windows, sin confirmar. No se corrigió.
- La auditoría `--vulnerable --include-transitive` no imprimió resultado utilizable: **no se considera ejecutada**.
- `MongoReminderRepository.MarkRead` sin probar contra Atlas; solo se probó con el repositorio en memoria.
- Sin revisión independiente de seguridad ni de trazabilidad. Aprobación humana de cierre: pendiente.
