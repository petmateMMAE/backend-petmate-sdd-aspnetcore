# Gates — pm-079-image-storage-adapter

> Evidencia del 2026-11-17. El cierre queda pendiente de la revisión independiente y de la aprobación humana.

## Comandos ejecutados (contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, repositorio en solo lectura, sin `.env`)
- Restore y build Release con `-warnaserror`: 0 errores, 0 advertencias.
- Unitarias 348/348. Seguridad 206/206 (incluye el escaneo de secretos). Contrato 435/435.
- Integración: 12 pasan y 69 omitidas (sin Atlas ni Redis de prueba); las omitidas no cuentan como aprobadas.
- `dotnet list package --deprecated`: solo `xunit 2.9.3` (Legacy) en los proyectos de prueba; ningún proyecto de producción.
- `scripts/check-sdd.ps1`: OK.

## Evidencia por criterio
- Código ya existente (commit `4719ce4`); el registro lo marcaba «Propuesto» (desactualizado). Pruebas en `tests/PetMate.ContractTests/Images/PhotoEndpointsTests.cs`.
- CA-01 (privado por defecto): contenedor `pet-photos` privado en `BlobImageStorage` y `infra/azure/storage.tf` sin acceso anónimo; la infraestructura **no se verificó** en esta revisión.
- CA-02 (nada en el disco): el cuerpo se transmite al Blob; no revisé línea por línea que no exista otra escritura al filesystem.
- CA-03 (adaptador): puerto `IImageStorage` con `BlobImageStorage` y `DisabledImageStorage`; pruebas con `InMemoryImageStorage`.

## Limitaciones
- `dotnet format --verify-no-changes` **falla** por marcadores de fin de línea (CRLF) en archivos que este trabajo no modificó (p. ej. `UserProfileEndpointsTests.cs`); parece un efecto de la copia en Windows, sin confirmar. No se corrigió.
- La auditoría `--vulnerable --include-transitive` no imprimió resultado utilizable: **no se considera ejecutada**.
- Azure Blob real (y Azurite) sin probar.
- Sin revisión independiente de seguridad ni de trazabilidad. Aprobación humana de cierre: pendiente.
