# Plan — pm-097-vaccine-create-list

## Enfoque
- Entidad `Vaccine` (nombre, fecha de aplicación no futura, próxima dosis opcional posterior, notas), repositorio `IVaccineRepository` (Mongo) con índice `petId+appliedAt`, `CreateVaccine`/`ListVaccines` con paginación. `POST /api/v1/vaccines/create` y `GET /api/v1/vaccines/get-all?petId=`.
- Pruebas: unitarias (dominio y casos de uso), contrato (con `[Trait("Endpoint", …)]` cuando hay endpoint), seguridad (BOLA/origen) e integración contra la base de pruebas de Atlas.
- Contratos: se actualizan `docs/api/endpoints.md`, `docs/testing/endpoints.md` y la matriz `endpoint-tests` del pipeline cuando cambia un endpoint.

## Decisiones de detalle aprobadas (2026-11-09)
- Fecha de aplicación/medición no futura; tope de 200 registros por mascota y tipo (409 al excederlo); listados paginados con la convención `pm-047` (20 por defecto, 50 máximo).
- Registro creado por un negocio: borrar o editar desde el dueño responde 403 con el mensaje «lo registró el negocio».
- Rate limit 60 solicitudes/min por usuario (política propia del carnet, variable `CARNET_RATE_LIMIT_PER_MINUTE`, 1–10000) y cuerpo máximo 16 KB, igual que mascotas.
- Propiedad (BOLA): toda consulta filtra por `OwnerId` de la mascota; mascota ajena o inexistente responde 404 idéntico.

## Costos y dependencias
Sin paquetes, servicios ni acciones de CI nuevos (costo $0). Persistencia exclusivamente en MongoDB Atlas.
