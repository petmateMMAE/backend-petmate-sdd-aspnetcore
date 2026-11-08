# Plan — pm-042 pet-detail (retrospectivo)

> Se escribió después de implementar T032, que se hizo con la autorización del usuario del 2026-11-08 (opción A) sin plan previo: es una desviación del flujo de AGENTS.md §2, registrada aquí y en `Memoria.md`.

## Enfoque (detalle)
- `GET get-by-id/{id:guid}`; `GetMyPetHandler` consulta con propietario y devuelve `null` (404) si no existe o es ajena.
- Contrato en `docs/api/endpoints.md` (sección Mascotas); pruebas de contrato con `[Trait("Endpoint", "pets-get-by-id")]` y job propio en la matriz del pipeline.
- Sin paquetes nuevos.
