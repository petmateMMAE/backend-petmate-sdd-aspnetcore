# Gates — pm-093-pet-qr-token

> Evidencia reproducible de la implementación del 2026-11-09. El cierre del Spec queda pendiente de la revisión independiente y de la aprobación humana.

## Comandos ejecutados (contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, repositorio en solo lectura, sin `.env`)
- Build Release con `-warnaserror`: 0 advertencias, 0 errores.
- Unitarias: 233/233. Seguridad: 188/188. Contrato: 242/242 (incluye `EndpointTraitTests` y `RouteCatalogTests`).
- Integración contra la base de pruebas de Atlas (`PetPersistence*`, `CarnetPersistence*`): 30/30, con colecciones de prefijo único eliminadas al terminar.
- `scripts/check-sdd.ps1`: OK.

## Cobertura por criterio
- CA: token (43 car.) y código (8 car.) aleatorios, únicos, no expuestos — `PetQrTests`, `Qr_is_persisted_replaced_only_by_the_owner_and_both_values_are_unique` (índices únicos en Atlas), `A_pet_created_before_the_qr_receives_one_…`.

## Limitaciones
- Las pruebas de contrato usan repositorios en memoria; la persistencia real se verifica en las de integración (Atlas).
- La cascada de `pm-104` es por pasos (hijos primero, mascota al final) y no transaccional: ante un fallo la mascota sigue visible y repetir la eliminación termina el trabajo (idempotente). No se probó una transacción de Atlas.
- Revisión independiente: sin hallazgos de seguridad. La compleción perezosa del QR de mascotas anteriores (escritura dentro de una lectura) es una decisión de migración documentada en `docs/api/endpoints.md`. No hay prueba automática de que el token no llegue a los logs: el código de mascotas no registra datos.
- Aprobación humana de cierre: usuario, 2026-11-09, en el chat.
