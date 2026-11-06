# Task — 03 Administración inicial de mascotas

> Tareas globales T029–T040. Depende de T013–T028.

## T029 — Definir modelo Pet y persistencia
- **Estado:** Propuesto
- **Crear:** entidad/reglas Domain, contrato de repositorio Application, documento/mapper e índices Atlas Infrastructure.
- **Verificar:** campos confirmados contra formulario; invariantes unitarias; índice por OwnerId y consultas justificadas.
- **Bloqueo:** confirmar campos exactos, tipo de ID y política de soft delete antes de cerrar el modelo.

## T030 — Crear mascota
- **Crear:** Command/Handler/Validator y `POST /api/v1/pets/create`.
- **Regla:** OwnerId siempre desde principal autenticado.
- **Verificar:** creación válida, inválida, ownership no falsificable y wrapper.

## T031 — Listar mascotas
- **Crear:** Query/Handler y `GET /api/v1/pets/get-all`.
- **Regla:** filtrar OwnerId autenticado; excluir eliminadas; límites/paginación si el contrato lo define.
- **Verificar:** no mezcla mascotas de usuarios y resultados ordenados/limitados.

## T032 — Obtener detalle
- **Crear:** Query/Handler y `GET /api/v1/pets/get-by-id/{id}`.
- **Verificar:** propia devuelve 200; ajena/inexistente no revela existencia (404 según política); ID inválido wrapper consistente.

## T033 — Actualizar mascota
- **Crear:** Command/Handler/Validator y `PUT /api/v1/pets/update/{id}`.
- **Verificar:** solo propietario y campos permitidos; IDOR y mass assignment.

## T034 — Eliminar mascota
- **Crear:** Command/Handler y `DELETE /api/v1/pets/delete/{id}`.
- **Política:** soft delete inicial; conservar documento para futuras referencias médicas.
- **Verificar:** recurso deja de aparecer en consultas normales; respuesta HTTP 200 con wrapper y `data:null`.

## T035 — Asegurar autorización y ownership
- **Modificar:** políticas/handlers/repositorios para validar ownership en todas las rutas.
- **Verificar:** matriz Owner A/Owner B, anónimo, Business y mascota eliminada; probar IDOR/BOLA.

## T036 — Pruebas unitarias Pet
- **Crear:** pruebas Domain, validators y handlers.
- **Verificar:** reglas/casos límite y criterios CA asociados.

## T037 — Pruebas de integración Atlas
- **Crear:** pruebas CRUD e índices en MongoDB Atlas test DB independiente.
- **Verificar:** aislamiento de base, concurrencia relevante, índices y consultas de propietario.

## T038 — Pruebas de seguridad Pet
- **Crear:** pruebas IDOR/BOLA, rol, userId spoofing, mass assignment y soft delete.
- **Verificar:** fallos deniegan acceso sin filtrar información ajena.

## T039 — Pruebas de contrato y documentación
- **Modificar:** contract tests y `docs/api/endpoints.md`.
- **Verificar:** rutas/métodos/status/wrapper coinciden con controllers y OpenAPI/contrato publicado.

## T040 — Gates y revisión independiente Pet
- **Ejecutar:** todas las suites, SAST, secret/dependency/license scans, ASVS L2; security-reviewer y traceability-reviewer.
- **Verificar:** CA-001–CA-010 y evidencia completa; no auto-firmar.
