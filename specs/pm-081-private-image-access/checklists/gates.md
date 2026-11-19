# Gates — pm-081-private-image-access

> Evidencia del 2026-11-17. El cierre queda pendiente de la revisión independiente y de la aprobación humana.

## Comandos ejecutados (contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, repositorio en solo lectura, sin `.env`)
- Restore y build Release con `-warnaserror`: 0 errores, 0 advertencias.
- Unitarias 348/348. Seguridad 206/206 (incluye el escaneo de secretos). Contrato 435/435.
- Integración: 12 pasan y 69 omitidas (sin Atlas ni Redis de prueba); las omitidas no cuentan como aprobadas.
- `dotnet list package --deprecated`: solo `xunit 2.9.3` (Legacy) en los proyectos de prueba; ningún proyecto de producción.
- `scripts/check-sdd.ps1`: OK.

## Evidencia por criterio
- Código ya existente (commit `4719ce4`); el registro lo marcaba «Propuesto» (desactualizado). Pruebas en `tests/PetMate.ContractTests/Images/PhotoEndpointsTests.cs`.
- CA-01 (URL temporal): `Upload_pet_photo_stores_it_and_returns_a_temporary_url`; SAS de lectura con vigencia corta en `BlobImageStorage`.
- CA-02 (autorización antes de emitir): `Get_pet_photo_url_only_signs_after_checking_the_owner` y `Get_pet_photo_url_responds_404_when_the_pet_has_no_uploaded_photo`.
- CA-03 (sin URL pública permanente de datos sensibles): las fotos de mascota solo se sirven con SAS; solo `business-photos` es público (pm-111). `Delete_pet_photo_removes_the_object_and_the_url_stops_being_issued`.

## Limitaciones
- `dotnet format --verify-no-changes` **falla** por marcadores de fin de línea (CRLF) en archivos que este trabajo no modificó (p. ej. `UserProfileEndpointsTests.cs`); parece un efecto de la copia en Windows, sin confirmar. No se corrigió.
- La auditoría `--vulnerable --include-transitive` no imprimió resultado utilizable: **no se considera ejecutada**.
- Azure Blob real sin probar.
- Sin revisión independiente de seguridad ni de trazabilidad. Aprobación humana de cierre: pendiente.
