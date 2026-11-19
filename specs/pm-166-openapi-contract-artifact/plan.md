# Plan — pm-166-openapi-contract-artifact

## Diseño (pm-166#CA-01 a CA-03)
- **CA-01:** la prueba `The_openapi_document_is_valid_and_is_exported_when_the_ci_asks_for_it` (contrato transversal) arranca la API en Development, valida el documento y, si existe `PETMATE_OPENAPI_OUT`, lo guarda ahí. El job `contract-transversal` de `pipeline.yml` fija esa variable y publica `openapi/openapi.json` como artefacto `openapi` (30 días, falla si no existe) en cada build, incluidos los pull requests. Se evita arrancar la API real en CI, que necesitaría Atlas y Redis.
- **CA-02:** ya lo cubre `OpenApiWrapperTests` (cada respuesta con cuerpo referencia un esquema `ApiResponse*` con `statusCode`, `message`, `data`), que corre en el mismo job.
- **CA-03:** `Production_keeps_openapi_and_scalar_hidden` comprueba 404 en `/openapi/v1.json` y `/scalar/v1` en Production.
- Sin paquetes ni acciones nuevas: `actions/upload-artifact@v5` ya se usa en el pipeline.

## Verificación
- Local: el documento exportado pesa 336 KB y las 8 pruebas de OpenAPI pasan. La publicación real del artefacto solo se verá en la primera ejecución de CI.
