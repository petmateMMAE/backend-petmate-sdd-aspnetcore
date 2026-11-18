# Gates — pm-168-business-get-own

> Evidencia reproducible de la implementación del 2026-11-18. Aprobación humana de cierre: usuario, 2026-11-18, en el chat, sin revisión independiente.

## Comandos ejecutados (SDK .NET 10 local, sin `.env`)
- `dotnet build -c Release -warnaserror`: 0 advertencias, 0 errores.
- Unitarias: 348/348. Seguridad: 206/206. Contrato: 436/436 (incluye `EndpointTraitTests`, `RouteCatalogTests` y la prueba nueva `Get_returns_the_full_own_business_including_the_license_and_404_without_one`).
- Integración contra Atlas: no aplica; la consulta reutiliza `FindByOwner` (ya cubierta) y no añade persistencia.

## Cobertura de criterios
| CA | Evidencia |
|---|---|
| CA-01 | Contrato: 200, mismo `id` que `create` y `licenseNumber` presente |
| CA-02 | Sin parámetros en el endpoint; la prueba usa dos cuentas y cada una solo ve lo suyo |
| CA-03 | Contrato: cuenta sin negocio → 404 |
| CA-04 | Contrato: sin token → 401 con wrapper |
| CA-05 | Política `business` heredada del controlador; fila en `docs/api/endpoints.md` y `docs/testing/endpoints.md` |

## Limitaciones
- No se ejecutó `scripts/check-sdd.ps1` (`pwsh` no instalado).
- Sin revisión independiente de seguridad ni de trazabilidad. Sin prueba de que el límite por usuario responda 429 en este endpoint (se hereda del controlador).
