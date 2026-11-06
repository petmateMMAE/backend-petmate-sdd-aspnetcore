# Spec — 03 Administración inicial de mascotas

> Estado: Propuesto. Este slice implementa los endpoints funcionales 3 a 7 del primer hito.

## 1. Objetivo y contexto

Permitir que un Owner registre y administre sus mascotas dentro de PetMate, garantizando que cada operación respete la propiedad del recurso.

## 2. Alcance y fuera de alcance

### Incluye
- Crear mascota.
- Listar mascotas del Owner autenticado.
- Consultar detalle de mascota propia.
- Actualizar mascota propia.
- Eliminar lógicamente mascota propia para preservar referencias futuras del carnet médico.

### No incluye
- Vacunas, desparasitaciones, alergias, peso, historial clínico.
- QR y autorización de negocios.
- Búsqueda geográfica, negocios y servicios.
- Carga de fotografías a Blob/CDN (se define en un slice posterior; el campo de imagen no debe guardar archivos en el filesystem del contenedor).

## 3. Actores, permisos y reglas

- Owner autenticado.
- Solo el propietario puede crear/listar/consultar/actualizar/eliminar sus mascotas.
- Business no accede a esta API de propietario; tendrá un flujo de autorización QR posterior.
- Toda consulta incluye filtro por OwnerId del principal autenticado.
- Los IDs proporcionados por el cliente no sustituyen el filtro por propietario.
- Eliminación inicial propuesta como lógica (`IsDeleted`/estado equivalente) para no romper futuras relaciones médicas. No borrar físicamente sin política de retención aprobada.

## 4. Requisitos funcionales y flujos

- RF-001 crear mascota con campos obligatorios definidos en el contrato.
- RF-002 devolver lista propia, con paginación si el diseño lo requiere.
- RF-003 devolver detalle únicamente si pertenece al Owner.
- RF-004 actualizar solo campos permitidos de mascota propia.
- RF-005 eliminar lógicamente mascota propia y excluirla de consultas normales.
- RF-006 validar especie, fechas, strings y rangos conforme a reglas definidas en el modelo.

Los campos definitivos deben contrastarse con el formulario/maqueta móvil antes de implementar. El PDF menciona nombre, especie, raza, fecha de nacimiento, sexo, fotografía y otros datos de identificación.

## 5. Contrato de API y datos

| # | Método | Endpoint | Resultado |
|---:|---|---|---|
| 3 | POST | `/api/v1/pets/create` | Mascota creada |
| 4 | GET | `/api/v1/pets/get-all` | Mascotas propias |
| 5 | GET | `/api/v1/pets/get-by-id/{id}` | Detalle de mascota propia |
| 6 | PUT | `/api/v1/pets/update/{id}` | Mascota actualizada |
| 7 | DELETE | `/api/v1/pets/delete/{id}` | Mascota eliminada lógicamente, `data: null` |

Todos los endpoints usan Response Wrapper. El modelo Mongo se diseña desde las consultas y casos de uso; documentar colección e índices en el plan.

## 6. Requisitos no funcionales y seguridad

- Requiere rol Owner.
- Verificar ownership en cada handler/consulta, no solo en UI/controller.
- Responder de forma que no permita enumerar mascotas de otras cuentas (404 recomendado para recurso ajeno/inexistente; cerrar en spec/plan).
- Paginación y límites para listados.
- Nunca almacenar fotografías sensibles en disco local del contenedor.
- Unit, Integration Atlas, Security y Contract; OWASP ASVS L2 aplicable.

## 7. Criterios de aceptación

- [ ] CA-001 Owner crea una mascota válida.
- [ ] CA-002 Owner lista únicamente sus mascotas activas.
- [ ] CA-003 Owner obtiene detalle solo de una mascota propia.
- [ ] CA-004 Owner actualiza solo una mascota propia y campos permitidos.
- [ ] CA-005 Owner elimina lógicamente mascota propia y deja de aparecer en listados normales.
- [ ] CA-006 Owner A no puede consultar/actualizar/eliminar mascota de Owner B (IDOR/BOLA).
- [ ] CA-007 anónimo recibe 401 y rol Business no obtiene acceso por defecto.
- [ ] CA-008 validaciones y códigos HTTP son consistentes.
- [ ] CA-009 todos los endpoints devuelven Response Wrapper.
- [ ] CA-010 pruebas unitarias, integración, seguridad y contrato pasan.

## 8. Estrategia de pruebas y gates

- Unit: validaciones e invariantes.
- Integration: CRUD sobre base MongoDB Atlas test independiente; índices y filtros owner.
- Security: IDOR/BOLA, user spoofing, rol incorrecto, soft-delete y filtrado.
- Contract: rutas, métodos, códigos, paginación y wrapper.

## 9. Dependencias, riesgos y decisiones pendientes

- Depende de autenticación y perfil Owner.
- Confirmar campos del formulario móvil.
- Confirmar tipo de ID Mongo y serialización pública antes de exponerlo.
- Confirmar política de soft delete/retención; propuesta inicial es soft delete.
- Fotos y almacenamiento Blob/CDN se posponen.

## 10. Aprobación

Pendiente.
