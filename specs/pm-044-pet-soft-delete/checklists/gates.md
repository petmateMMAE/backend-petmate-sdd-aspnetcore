# Gates — pm-044 pet-soft-delete

> Estado: implementado y probado; revisión independiente y aprobación humana pendientes. T034 no está cerrada.

| Gate | Resultado | Evidencia |
|---|---|---|
| Pruebas de contrato del endpoint (`Endpoint=pets-delete`) | En verde | `tests/PetMate.ContractTests/Pets/PetEndpointsTests.cs`: caso feliz, 400 con el campo, miembros no permitidos, 401 sin token, 403 Business, 404 ajena o inexistente |
| Casos de uso | En verde | `tests/PetMate.UnitTests/Pets/PetHandlersTests.cs` |
| Persistencia contra Atlas | En verde | `PetPersistenceIntegrationTests` (pm-039) |
| Regresión (2026-11-08) | Unit 142/142, Security 155/155, Contract 142/142, Integration 10 + 35 omitidas sin servicios | Contenedor SDK, repositorio en solo lectura y sin `.env` |
| Build Release con analizadores | 0 advertencias | Mismo contenedor |
| Integridad SDD | OK | `scripts/check-sdd.ps1` |

- [ ] Revisión independiente de seguridad y trazabilidad.
- [ ] Aprobación humana del cierre de T034.
