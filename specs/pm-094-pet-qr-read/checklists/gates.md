# Gates — pm-094-pet-qr-read

> Evidencia reproducible de la implementación del 2026-11-09. El cierre del Spec queda pendiente de la revisión independiente y de la aprobación humana.

## Comandos ejecutados (contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, repositorio en solo lectura, sin `.env`)
- Build Release con `-warnaserror`: 0 advertencias, 0 errores.
- Unitarias: 233/233. Seguridad: 188/188. Contrato: 242/242 (incluye `EndpointTraitTests` y `RouteCatalogTests`).
- Integración contra la base de pruebas de Atlas (`PetPersistence*`, `CarnetPersistence*`): 30/30, con colecciones de prefijo único eliminadas al terminar.
- `scripts/check-sdd.ps1`: OK.

## Cobertura por criterio
- CA: lectura del QR por el dueño, 404 ajena, no-store — `Get_qr_returns_token_and_code_…`, `Get_qr_of_another_owner_pet_or_a_missing_pet_returns_the_same_404`, `Get_qr_requires_an_owner_token`.

## Limitaciones
- Las pruebas de contrato usan repositorios en memoria; la persistencia real se verifica en las de integración (Atlas).
- La cascada de `pm-104` es por pasos (hijos primero, mascota al final) y no transaccional: ante un fallo la mascota sigue visible y repetir la eliminación termina el trabajo (idempotente). No se probó una transacción de Atlas.
- Revisión independiente: sin hallazgos de seguridad. No se prueba `Cache-Control: no-store` en las respuestas 401/404.
- Aprobación humana de cierre: usuario, 2026-11-09, en el chat.
