# Gates — pm-086-user-role-unification

> Evidencia reproducible de la implementación del 2026-11-09. El cierre queda pendiente de la revisión independiente y de la aprobación humana.

## Comandos ejecutados (contenedor `mcr.microsoft.com/dotnet/sdk:10.0`, repositorio en solo lectura, sin `.env`)
- Build Release con `-warnaserror`: 0 advertencias, 0 errores.
- Unitarias: 261/261. Seguridad: 186/186. Contrato: 269/269 (incluye `EndpointTraitTests`, `RouteCatalogTests` y `PublicSurfaceTests`).
- Integración contra la base de pruebas de Atlas (`Persistence*`): 33/33. Las pruebas de integración que usan Redis quedan omitidas en este entorno.
- `scripts/check-sdd.ps1`: OK.

## Cobertura
- Ver las pruebas de `pm-086` en `tests/` y la fila del endpoint en `docs/testing/endpoints.md`.

## Limitaciones
- Revisión independiente (2026-11-09): sin hallazgos bloqueantes. Corregido tras ella: un fallo de Atlas en `forgot-password` ya no responde 503 (no delata la cuenta); solo vale el enlace más reciente y al terminar un restablecimiento se invalidan los pendientes; las sesiones se revocan antes de cambiar la contraseña. Limitaciones aceptadas: diferencia de latencia por la llamada al proveedor, sin prueba automática de que el token no llegue a los logs ni de la cuota por IP de `forgot-password`, ventana mínima de `RevokeAllForUser` ante un login simultáneo.
- Aprobación humana de cierre: usuario, 2026-11-09, en el chat.
- Sin limitaciones adicionales conocidas.
