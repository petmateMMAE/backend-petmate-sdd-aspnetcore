# Spec — 00 Foundation y solución inicial

> Estado: Aprobado para implementación (2026-11-03). T001–T009 cerradas; T010–T011 cerradas; T012 no autorizada. Cierre del slice pendiente de gates y aprobación humana.

## 1. Objetivo y contexto

Crear la base compilable del backend PetMate desde una solución .NET vacía, con separación de responsabilidades, composición de dependencias y contratos comunes que permitan desarrollar casos de uso posteriores con SDD.

## 2. Alcance y fuera de alcance

### Incluye
- Solución .NET 10 con proyectos Api, Application, Domain e Infrastructure.
- Proyectos de pruebas Unit, Integration, Security y Contract.
- Referencias de proyectos respetando Clean Architecture.
- CQRS y Mediator con una dependencia cuyo uso sea $0 y licencia verificada.
- DI por capa.
- Configuración local `.env` y `.env.example`.
- Response Wrapper, manejo uniforme de errores, MVC Controllers y rutas versionadas con acción explícita.
- Generación del documento OpenAPI y Scalar como única UI interactiva de documentación de la API.
- Conexiones configurables a MongoDB Atlas y dos Redis independientes.
- Contenedorización de API y servicios Redis de desarrollo; nunca MongoDB local.
- Base de CI con controles automáticos de calidad y seguridad.

### No incluye
- Casos de uso de autenticación, perfil ni mascotas; se desarrollan en slices posteriores.
- Despliegue Azure real ni creación de recursos cloud.
- Selección definitiva de servicios Azure con cargo o de un paquete sin licencia verificada.

## 3. Actores, permisos y reglas

Esta slice no implementa roles funcionales, pero prepara autenticación/autorización sin crear un rol Admin ni un mecanismo de permisos implícito.

## 4. Requisitos funcionales y flujos

- RF-001: la solución compila en .NET 10.
- RF-002: Presentation puede invocar Application mediante Mediator.
- RF-003: Application depende de Domain y no de Infrastructure ni Api.
- RF-004: Infrastructure implementa abstracciones definidas por capas internas.
- RF-005: API registra dependencias mediante DI.
- RF-006: las respuestas de negocio usan el Response Wrapper genérico.
- RF-010: toda validación de forma de requests devuelve 400 con el wrapper y `data.errors[{field,message}]` (máx. 50 errores, 200 caracteres); 422 no se adopta (ratificada 2026-11-03). El handler no se ejecuta si hay errores.
- RF-007: la configuración separa URI Atlas, Redis de auth, Redis de caché y JWT.
- RF-008: la API se ejecuta en contenedor sin depender de estado local.
- RF-009: la API publica un documento OpenAPI válido y Scalar puede cargarlo como única UI interactiva de documentación.

## 5. Contrato de API y datos

No se crean endpoints de negocio en esta slice. El contrato común es `ApiResponse<T>` equivalente a `statusCode`, `message`, `data` nullable. Las respuestas deben conservar el código HTTP real. La configuración de servicios se basa en variables de entorno definidas en `.env.example`.

## 6. Requisitos no funcionales y seguridad

- Sin secretos en código ni archivos versionados.
- Solo dependencias con costo $0 y licencia revisada.
- Atlas como único proveedor Mongo; no Docker Mongo.
- Dos servicios Redis separados.
- API stateless y preparada para varias réplicas.
- HTTPS en producción; local HTTP se permite solo para desarrollo.
- Build Release, pruebas base y análisis de secretos/dependencias en CI.

## 7. Criterios de aceptación

- [x] CA-001: la solución y todos los proyectos compilan con `net10.0`. _(validado en T001; evidencia en `checklists/gates.md`)_
- [x] CA-002: las referencias cumplen la dirección definida en la constitución. _(validado en T002; evidencia en `checklists/gates.md`)_
- [x] CA-003: un controlador de prueba puede enviar un request a un caso de uso por Mediator sin acoplarse a handlers concretos. _(validado en T006 mediante `TechnicalMediatorProbeController`, `ISender`, registro explícito de invocador y `MediatorDispatchTests`.)_
- [x] CA-004: un endpoint de prueba devuelve el Response Wrapper y `data: null` es serializable. _(T008: `ResponseWrapperHttpTests` en ContractTests; cerrado (2026-11-03).)_
- [x] CA-011: un request inválido devuelve wrapper 400 con `data.errors` en español y no ejecuta el handler. _(T009: ResponseWrapperHttpTests, RequestValidationPipelineTests; cerrado (2026-11-03).)_
- [x] CA-005: configuración local lee variables desde `.env` y el archivo real está ignorado por Git. _(validado en T004: carga no productiva limitada al content root, omisión con Production efectivo —incluido el valor por defecto—, precedencia del entorno y pruebas de bootstrap. Ver `checklists/gates.md` y reportes de corrección T004.)_
- [x] CA-006: no hay MongoDB local; existen configuraciones diferenciadas para Mongo Atlas, Redis Auth y Redis Cache. _(T010/T011: Compose sin MongoDB, configuración tipada; conexión real a Atlas verificada 2026-11-03.)_
- [x] CA-007: health checks distinguen liveness y readiness sin exponer secretos. _(T011: /api/v1/health/live y /ready.)_
- [x] CA-008: el Dockerfile compila/publica la API en una imagen multi-stage. _(T011.)_
- [ ] CA-009: CI ejecuta restore, build, suites de prueba existentes y escaneos disponibles sin pasos pagos.
- [x] CA-010: el documento `/openapi/v1.json` es válido y la UI `/scalar/v1` lo carga correctamente en Development; no se agrega Swagger UI ni otra UI alternativa. La exposición en producción permanece deshabilitada por defecto. _(validado en T007 mediante 4 ContractTests y 1 SecurityTest; evidencia en `checklists/gates.md`.)_

## 8. Estrategia de pruebas y gates

- Unit: pruebas de Response Wrapper, convenciones y composición.
- Integration: Mongo Atlas test DB y dos Redis no productivos cuando la conexión sea parte de la prueba.
- Security: configuración ausente, secretos, headers, exposición de errores y fail-closed de Redis Auth.
- Contract: serialización y códigos HTTP.
- Quality: build Release, SAST, secret scan, NuGet vulnerability/license audit.

## 9. Dependencias, riesgos y decisiones pendientes

- Selección de Mediator compatible .NET 10 y $0.
- Selección de proveedor `.env` sin agregar un paquete de licencia/costo no aprobado.
- Selección de herramientas SAST/secret/dependency scan $0.
- ~~Verificación de compatibilidad, licencia, costo $0 y mantenimiento del paquete de integración Scalar con .NET 10.~~ **Resuelto para T007:** ADR-002 aprueba `Microsoft.AspNetCore.OpenApi` 10.0.12, `Microsoft.OpenApi` 2.12.0 y `Scalar.AspNetCore` 2.11.2 tras auditoría sin vulnerabilidades.
- Definir health payload y readiness checks sin acoplar API a datos sensibles.

## 10. Aprobación

- [x] Plan y tareas revisados.
- [x] Estructura aprobada por el usuario.

Aprobado por el usuario el 2026-11-03 junto con la ratificación de la constitución `1.0.1`. La aprobación del slice no autoriza tareas individuales por sí sola ni implica el cierre del slice. Los paquetes concretos (Mediator, Argon2id, Scalar) no están aprobados: requieren auditoría de licencia, costo $0, compatibilidad y vulnerabilidades antes de incorporarse.
