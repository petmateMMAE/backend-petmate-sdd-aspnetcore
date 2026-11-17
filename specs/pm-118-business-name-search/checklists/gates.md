# Gates — pm-118-business-name-search

> Evidencia reproducible de la implementación del 2026-11-10. El cierre queda pendiente de la revisión independiente y de la aprobación humana.

## Comandos ejecutados (contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, repositorio en solo lectura, sin `.env`)
- Build Release con `-warnaserror`: 0 advertencias, 0 errores.
- Unitarias: 309/309. Seguridad: 192/192. Contrato: 330/330 (incluye `EndpointTraitTests`, `RouteCatalogTests`, `PublicSurfaceTests` y `CachePolicyTests`).
- Integración contra Atlas (`MapAndAccessPersistenceIntegrationTests`, escritas): **no se pudieron ejecutar el 2026-11-10**: Atlas no respondió desde este equipo (espera de selección de servidor agotada a los 30 s). Deben ejecutarse antes del cierre, en especial `$geoNear`, la búsqueda por nombre y `FindByQr`.
- `scripts/check-sdd.ps1`: OK.

## Limitaciones
- Revisión independiente (2026-11-10): sin hallazgos bloqueantes; ver `Memoria.md` para lo corregido y lo aceptado.
- Aprobación humana de cierre: usuario, 2026-11-10, en el chat. Siguen pendientes las pruebas contra Atlas, la falla cerrada con Redis Auth caído y la medición del mapa.
- La foto del negocio llega con `pm-111` (Blob, sin aprobar); mientras tanto `photoUrl` es nulo.
