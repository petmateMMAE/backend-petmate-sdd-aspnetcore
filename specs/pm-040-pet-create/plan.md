# Plan — pm-040 pet-create (retrospectivo)

> Se escribió después de implementar T030, que se hizo con la autorización del usuario del 2026-11-08 (opción A) sin plan previo: es una desviación del flujo de AGENTS.md §2, registrada aquí y en `Memoria.md`.

## Enfoque (crear)
- Endpoint de creación con política `RequireOwner`; el propietario sale del claim `sub`; validación con `PetValidation` (reglas de pm-039).
- Contrato en `docs/api/endpoints.md` (sección Mascotas); pruebas de contrato con `[Trait("Endpoint", "pets-create")]` y job propio en la matriz del pipeline.
- Sin paquetes nuevos.
