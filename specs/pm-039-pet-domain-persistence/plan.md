# Plan — pm-039 pet-domain-persistence

> Derivado de [spec.md](spec.md). Estado: aprobado para implementar por el usuario el 2026-11-08 (campos de la maqueta móvil y borrado físico). No incluye endpoints: llegan con pm-040 a pm-044.

## 1. Contexto
- Task: T029 (propietaria de `pm-039#CA-01` a `CA-03`). Las pruebas unitarias e de integración aportan evidencia a CA-04 (T036) y CA-05 (T037), que siguen siendo de esas Tasks.
- Dependencias: pm-012 (Atlas) y pm-033 (autorización), ya implementadas.

## 2. Diseño técnico
| Capa | Elemento | Responsabilidad |
|---|---|---|
| Domain | `Pets/Pet.cs`: `Pet`, `PetDetails`, `PetSex` | Invariantes: textos recortados, longitudes, fecha de nacimiento no futura ni de hace más de 50 años, sexo definido, foto HTTPS sin credenciales. El propietario se fija al crear y no cambia. `Restore` vuelve a validar al leer. |
| Application | `Pets/PetValidation.cs` | Validación por campo (`name`, `species`, `breed`, `birthDate`, `sex`, `photoUrl`) con mensajes en español, reutilizable por los validadores de crear y actualizar. |
| Application | `Persistence/IPetRepository.cs` | `Add`, `FindOwned`, `ListByOwner(skip, take)`, `UpdateOwned`, `DeleteOwned`. Todas exigen el propietario. |
| Infrastructure | `MongoPetRepository`, `PetDocument`, mapeo | Filtro `{_id, ownerId}` en cada operación; `DeleteOne` (borrado físico); errores del proveedor como `PersistenceUnavailableException` sin detalles. |
| Infrastructure | `MongoIndexInitializer` | Índice `ix_pets_ownerId_createdAtUtc` (`ownerId` ascendente, `createdAtUtc` descendente). |

## 3. Persistencia
- Colección `pets` (con prefijo en pruebas de integración). Identificador Guid v7 estándar. Fecha de nacimiento como texto `yyyy-MM-dd` (fecha de calendario, sin zona horaria). Fechas de alta y cambio en UTC.
- Sin MongoDB local: las pruebas de integración usan la base de pruebas de Atlas y colecciones con prefijo único que se eliminan al terminar.

## 4. Seguridad
- Defensa BOLA desde la persistencia: no existe ninguna operación por Id sin propietario.
- La foto no se descarga ni se guarda: solo se acepta una URL HTTPS sin usuario ni contraseña.
- Sin paquetes nuevos (costo $0, sin cambios de licencias).

## 5. Verificación
- Unitarias: `PetTests` (modelo y `PetValidation`).
- Integración (Atlas): `PetPersistenceIntegrationTests` (ida y vuelta, ajeno no puede leer/cambiar/borrar, listado por propietario y paginado, actualización, borrado físico, índice).
- Regresión de las 4 suites, formato y `scripts/check-sdd.ps1`.
