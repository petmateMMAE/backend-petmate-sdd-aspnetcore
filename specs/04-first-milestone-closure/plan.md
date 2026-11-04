# Plan — 04 Cierre del primer hito

## 1. Contexto

Validación cruzada final de los slices 00–03. Esta tarea no agrega nuevos endpoints ni módulos.

## 2. Diseño técnico

Preparar pruebas end-to-end que ejerciten la API MVC real, Mediator, handlers, repositorios Atlas y Redis. Usar cuentas de prueba con datos sintéticos; nunca fixtures con credenciales reales.

## 3. Secuencia de validación

1. Registrar Owner A y Owner B.
2. Login de Owner A y recibir Access/Refresh.
3. Consultar y actualizar perfil A.
4. Crear, listar, obtener, actualizar y eliminar lógicamente mascota A.
5. Intentar acceder a la mascota A desde Owner B y confirmar denegación/no revelación.
6. Renovar Access Token mediante Refresh y confirmar rotación.
7. Ejecutar Logout y reusar Access Token vigente para comprobar blacklist.
8. Intentar refresh con token revocado/consumido y comprobar rechazo.
9. Ejecutar suites y escáneres de calidad/seguridad.
10. Revisar documentación y estado de tareas con security/traceability reviewers.

## 4. Entornos y datos

Atlas test DB independiente; Redis Auth y Redis Cache de prueba separados. No tocar bases productivas. El entorno de prueba debe abortar si detecta el nombre de base de producción.

## 5. Seguridad y DevSecOps

Revisar ASVS L2 aplicable, SAST, secretos, dependencias/licencias, imagen del contenedor y exposición de logs. No hacer deployment pagado.

## 6. Entregables

Reporte de pruebas, gates cerrados, endpoint catalog actualizado, Memoria conciliada y aprobación humana.

## 7. Verificación

Los 11 endpoints deben coincidir en catálogo, código, tests de contrato y documentación. La revocación se verifica antes del `exp` del token.

## 8. Aprobación

El usuario acepta explícitamente el hito.
