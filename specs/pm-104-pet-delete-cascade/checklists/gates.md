# Gates — pm-104-pet-delete-cascade

> Evidencia reproducible de la implementación del 2026-11-09. El cierre del Spec queda pendiente de la revisión independiente y de la aprobación humana.

## Comandos ejecutados (contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, repositorio en solo lectura, sin `.env`)
- Build Release con `-warnaserror`: 0 advertencias, 0 errores.
- Unitarias: 233/233. Seguridad: 188/188. Contrato: 249/249 (incluye `EndpointTraitTests` y `RouteCatalogTests`).
- Integración contra la base de pruebas de Atlas (`PetPersistence*`, `CarnetPersistence*`): 30/30, con colecciones de prefijo único eliminadas al terminar.
- `scripts/check-sdd.ps1`: OK.

## Cobertura por criterio
- CA: cascada, QR, fallo a mitad, cobertura de colecciones — `PetCascadeTests` (incluye `Every_health_record_type_takes_part_in_the_pet_cascade`), `Deleting_the_pet_removes_the_carnet_records_and_the_qr_in_atlas`.

## Limitaciones
- Las pruebas de contrato usan repositorios en memoria; la persistencia real se verifica en las de integración (Atlas).
- La cascada de `pm-104` es por pasos (hijos primero, mascota al final) y no transaccional: ante un fallo la mascota sigue visible y repetir la eliminación termina el trabajo (idempotente). No se probó una transacción de Atlas.
- Revisión independiente (2026-11-09): sin hallazgos bloqueantes; se añadieron pruebas de mass assignment y de rate limit 429 en los cuatro controladores. El tope de 200 kg del peso es una guarda del agente (confirmar).
- Aprobación humana de cierre: usuario, 2026-11-09, en el chat.
