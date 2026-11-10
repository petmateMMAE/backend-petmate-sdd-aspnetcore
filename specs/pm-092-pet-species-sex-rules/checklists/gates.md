# Gates — pm-092-pet-species-sex-rules

> Evidencia reproducible de la implementación del 2026-11-09. El cierre del Spec queda pendiente de la revisión independiente y de la aprobación humana.

## Comandos ejecutados (contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, repositorio en solo lectura, sin `.env`)
- Build Release con `-warnaserror`: 0 advertencias, 0 errores.
- Unitarias: 233/233. Seguridad: 188/188. Contrato: 242/242 (incluye `EndpointTraitTests` y `RouteCatalogTests`).
- Integración contra la base de pruebas de Atlas (`PetPersistence*`, `CarnetPersistence*`): 30/30, con colecciones de prefijo único eliminadas al terminar.
- `scripts/check-sdd.ps1`: OK.

## Cobertura por criterio
- CA: especie solo Perro/Gato y sexo opcional — `Species_is_a_closed_list_of_Perro_and_Gato`, `Create_without_sex_stores_Unknown_and_accepts_Gato`, `Create_with_an_invalid_field_returns_400_naming_the_field`.

## Limitaciones
- Las pruebas de contrato usan repositorios en memoria; la persistencia real se verifica en las de integración (Atlas).
- La cascada de `pm-104` es por pasos (hijos primero, mascota al final) y no transaccional: ante un fallo la mascota sigue visible y repetir la eliminación termina el trabajo (idempotente). No se probó una transacción de Atlas.
- Revisión independiente (2026-11-09): sin hallazgos bloqueantes. Corregido: `Restore` acepta una especie heredada para no romper listados; el `PUT` sin `sex` deja `Unknown` (documentado en `docs/api/endpoints.md`).
- Aprobación humana de cierre: usuario, 2026-11-09, en el chat.
