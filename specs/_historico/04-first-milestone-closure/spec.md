# Spec — 04 Cierre del primer hito

> Estado: Propuesto. Este slice verifica el resultado conjunto de los slices 00–03.

## 1. Objetivo y contexto

Comprobar que la API base, el flujo completo de autenticación y los siete endpoints funcionales Owner trabajan juntos, con persistencia Atlas, dos servicios Redis y controles de seguridad completos.

## 2. Alcance y fuera de alcance

Incluye validación end-to-end de Register, Login, Refresh, Logout, GET/PUT perfil y los cinco endpoints de mascotas; pruebas de integración, seguridad, contratos, revisión de trazabilidad y aprobación. No incluye despliegue Azure productivo, módulos Business, carnet médico, QR ni notificaciones.

## 3. Actores, permisos y reglas

Owner autenticado. Un segundo Owner se utiliza para probar aislamiento. Business no obtiene acceso a endpoints Owner. No existe Admin.

## 4. Requisitos funcionales y flujos

- RF-001 la secuencia Register → Login → perfil → CRUD mascota funciona.
- RF-002 Refresh renueva Access/Refresh sin volver a pedir credenciales.
- RF-003 Logout revoca sesión y el Access Token previamente emitido deja de funcionar antes de expirar.
- RF-004 un Owner no puede leer ni modificar mascotas de otro Owner.
- RF-005 todos los endpoints responden con wrapper y códigos HTTP correctos.
- RF-006 la API puede ejecutarse contenerizada sin estado local de sesión.

## 5. Contrato de API y datos

El conjunto completo son los 4 endpoints de auth y los 7 endpoints funcionales documentados en `docs/api/endpoints.md`. El catálogo, los contratos de pruebas y OpenAPI deben coincidir con la implementación.

## 6. Requisitos no funcionales y seguridad

Todas las pruebas usan MongoDB Atlas test database y Redis no productivos; no usar datos reales ni servicios pagos. Ejecutar Unit, Integration, Security, Contract, SAST, secret scan y vulnerability/license audit. Comprobar ASVS L2 aplicable.

## 7. Criterios de aceptación

- [ ] CA-001 los 11 endpoints están implementados y probados.
- [ ] CA-002 la secuencia de autenticación y renovación funciona sin login al expirar Access Token.
- [ ] CA-003 Logout bloquea el Access Token vigente y revoca refresh session.
- [ ] CA-004 los siete endpoints funcionales cumplen sus criterios individuales.
- [ ] CA-005 no hay IDOR/BOLA entre Owners.
- [ ] CA-006 respuesta wrapper, statusCode HTTP y español consistentes.
- [ ] CA-007 Unit, Integration, Security y Contract pasan.
- [ ] CA-008 análisis DevSecOps y ASVS L2 aplicables pasan o tienen excepción aprobada.
- [ ] CA-009 los contratos y Memoria corresponden al código y tests reales.
- [ ] CA-010 security y traceability reviewers no tienen hallazgos bloqueantes.
- [ ] CA-011 aprobación humana explícita del primer hito.

## 8. Estrategia de pruebas y gates

Ejecutar un flujo completo con Atlas test DB y Redis Auth/Cache no productivos, escenarios de reuso/replay de refresh, blacklist, dos Owners, rol incorrecto, errores y wrappers. No desplegar en Azure como condición del hito porque el despliegue está sujeto a verificación de costo $0.

## 9. Dependencias, riesgos y decisiones pendientes

Depende de T001–T040. Cualquier criterio pendiente debe bloquear el cierre; no se permite declarar el hito completo por compilación solamente.

## 10. Aprobación

Pendiente de aceptación del usuario/revisor.
