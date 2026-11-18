# Gates — pm-111-business-photo-public-storage

> Evidencia del 2026-11-17. **Cerrada el 2026-11-17 por aprobación humana del usuario en el chat, sin revisión independiente (excepción del usuario).**

## Comandos ejecutados
- `pwsh scripts/quality-gate.ps1` (Docker 29.9.3): unitarias 348/348, seguridad 206/206, contrato 431/431; integración 12 pasan y 69 omitidas (sin Atlas ni Redis de prueba).
- `scripts/check-sdd.ps1`: OK.

## Evidencia
- Código ya existente (commit `4719ce4`, junto con pm-079 a pm-081): `businesses/upload-photo`, `BlobImageStorage` y contenedor `business-photos`. Pruebas de contrato en `PhotoEndpointsTests`: subida con URL pública visible en el perfil (CA-01) y rechazo de más de 1 MB (CA-02). CA-04: ADR-006 y `docs/api/endpoints.md` ya documentan el contenedor público.
- CA-03: pruebas `Upload_business_photo_without_a_business_responds_403`, `Delete_business_photo_clears_it_from_the_public_profile` y `Delete_business_photo_without_a_business_responds_403`; el negocio se resuelve siempre desde el token.
- CA-02 (tipo): comparte el validador por bytes mágicos con las fotos de mascota, que sí tiene prueba; no hay una prueba de tipo específica del negocio.

## Limitaciones
- Restore, build Release con analizadores, formato y auditorías NuGet no se ejecutaron (el script las corre con `dotnet` en el host, no instalado).
- Azure Blob real sin probar; el registro marcaba T129 como «pendiente de plan.md» pero el código y el plan ya existían (registro desactualizado).
- Sin revisión independiente de seguridad ni de trazabilidad. Aprobación humana de cierre: usuario, 2026-11-17, chat.
