# Plan — pm-045 pet-ownership-bola

## Enfoque
- **T035 (CA-01):** la propiedad se verifica en la consulta de persistencia, el caso de uso y el contrato: todo método de `IPetRepository` salvo `Add` y `UpdateOwned` recibe `ownerId` (y esos dos reciben la entidad con su propietario), todo comando o consulta de mascotas lleva `OwnerId` como primer miembro, el controlador lo toma solo del claim `sub` y nunca de la ruta, la consulta o el cuerpo. Una mascota ajena y una inexistente son indistinguibles (404).
- **T035 (CA-04, añadido el 2026-11-08):** límites contra el abuso tras la revisión de seguridad: `PetLimits.MaxPetsPerOwner` = 100 (409 desde `CreatePetHandler`), política de rate limiting `pets` por claim `sub` con `PETS_RATE_LIMIT_PER_MINUTE` (60 por defecto, 429 con `Retry-After`) y `RequestBodyLimitMiddleware` de 16 KB (413) para toda la API.
- **T038 (CA-02):** pruebas estructurales en SecurityTests, de contrato en ContractTests (Owner A frente a Owner B, anónimo 401, Business 403) y de persistencia en IntegrationTests (Atlas).
- **T040 (CA-03):** gates con evidencia reproducible de pm-039 a pm-045 y revisión independiente (pendiente: no se ha ejecutado; la hace el usuario o los agentes revisores).
