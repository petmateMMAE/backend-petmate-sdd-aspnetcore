# Plan — 03 Administración inicial de mascotas

## 1. Contexto

Implementar endpoints funcionales 3–7. Depende de auth y perfil Owner.

## 2. Diseño técnico

- Domain: Pet y reglas de validación/invariantes.
- Application: Commands Create/Update/Delete y Queries GetAll/GetById; cada caso con handler.
- Application define `IPetRepository` o abstracción equivalente.
- Infrastructure implementa repositorio MongoDB Atlas.
- Api: PetsController con actions explícitas y dispatch Mediator.
- El ownerId proviene de claims validados y se añade como filtro en cada operación.

## 3. Persistencia y servicios

Definir colección de mascotas e índices por OwnerId/estado/fecha según consultas. Usar soft delete para preservar referencias futuras; no borrar relaciones de carnet todavía inexistentes. Evitar índices redundantes y justificar índices compuestos con consultas reales.

## 4. Contratos

Usar las cinco rutas del spec y mantener `docs/api/endpoints.md`. Los DTOs no exponen campos internos de Mongo ni ownerId editable. La respuesta Delete usa HTTP 200 y `data: null`.

## 5. Seguridad y pruebas

- Prueba por cada operación con Owner A y Owner B.
- Filtro por OwnerId en repositorio y handler.
- Integration test con Atlas test DB.
- Paginación limitada y validación de entradas.
- Pruebas unitarias, integración, seguridad y contrato obligatorias.

## 6. Decisiones pendientes

Campos definitivos, tipo de ID serializado y política final de borrado deben cerrarse antes de ejecutar T029/T034.

## 7. Verificación

Cubrir CA-001–CA-010, gates OWASP ASVS L2, SAST, secret scan, package audit y reviewers.

## 8. Aprobación

Pendiente.
