# Plan — pm-044 pet-soft-delete (retrospectivo)

> Se escribió después de implementar T034, que se hizo con la autorización del usuario del 2026-11-08 (opción A) sin plan previo: es una desviación del flujo de AGENTS.md §2, registrada aquí y en `Memoria.md`.

## Enfoque (eliminar)
- `DELETE delete/{id:guid}` con borrado físico (decisión del usuario, 2026-11-08); `DeleteOne` filtra por propietario. El slug `pet-soft-delete` se conserva por ser inmutable (Constitución §12.3).
- Contrato en `docs/api/endpoints.md` (sección Mascotas); pruebas de contrato con `[Trait("Endpoint", "pets-delete")]` y job propio en la matriz del pipeline.
- Sin paquetes nuevos.
