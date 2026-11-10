# Plan — pm-007 api-route-conventions (T042, T055)

> Tasks retrospectivas: documentan evidencia que ya existía y solo afirman lo que esa evidencia demuestra.

## Enfoque
- T042 documenta la evidencia retrospectiva de las rutas versionadas. T055 añade la verificación automática: `RouteCatalogTests` (ContractTests) compara por reflexión las rutas de los controladores con las filas del catálogo vivo `docs/api/endpoints.md` y aplica las reglas de nombrado, documentadas en la sección «Reglas de nombrado de rutas» del catálogo.
- Sin paquetes nuevos ni cambios de contrato.
