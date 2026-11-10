# Plan — pm-061 api-docs-contract-sync (T024, T039, T073)

## Enfoque
- T024 y T039 (históricas) se verifican con la evidencia existente: el catálogo `docs/api/endpoints.md` y las pruebas de contrato de autenticación y mascotas; no se reescribe el histórico.
- T073: CA-03 la cubre la prueba `RouteCatalogTests` (entregada en T055, pm-007); CA-04 se añade con `OpenApiWrapperTests`, que lee `/openapi/v1.json` y comprueba que toda respuesta con cuerpo referencia un esquema `ApiResponse*` con `statusCode`, `message` y `data`, y que toda ruta de negocio documenta 2xx y 401 (salvo las públicas).
