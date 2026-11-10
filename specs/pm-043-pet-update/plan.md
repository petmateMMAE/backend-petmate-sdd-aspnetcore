# Plan — pm-043 pet-update (retrospectivo)

> Se escribió después de implementar T033, que se hizo con la autorización del usuario del 2026-11-08 (opción A) sin plan previo: es una desviación del flujo de AGENTS.md §2, registrada aquí y en `Memoria.md`.

## Enfoque (actualizar)
- `PUT update/{id:guid}`; reemplaza solo los campos editables con las mismas validaciones que crear; filtro `{_id, ownerId}` en Atlas.
- Contrato en `docs/api/endpoints.md` (sección Mascotas); pruebas de contrato con `[Trait("Endpoint", "pets-update")]` y job propio en la matriz del pipeline.
- Sin paquetes nuevos.
