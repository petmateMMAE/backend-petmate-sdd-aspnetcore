# Task — 02 Perfil Owner

> Tareas globales T026–T028. Depende de autenticación.

## T026 — Implementar GET perfil propio
- **Estado:** Propuesto
- **Crear:** Query/Handler/DTO y `GET /api/v1/users/me/get`.
- **Regla:** identificar usuario desde principal JWT; nunca aceptar userId destino.
- **Verificar:** 200 wrapper correcto, anónimo 401, datos sensibles omitidos.

## T027 — Implementar PUT perfil propio
- **Estado:** Propuesto
- **Crear:** Command/Handler/DTO/validator y `PUT /api/v1/users/me/update`.
- **Regla:** solo campos aprobados; no permitir cambiar role, password hash, email verificado ni identificador.
- **Verificar:** actualización correcta, validaciones, intento de mass assignment y userId spoofing.
- **Bloqueo:** confirmar lista exacta de campos editables antes de codificar.

## T028 — Pruebas y gates de perfil
- **Crear:** unit, integration Atlas, security y contract tests.
- **Modificar:** endpoint catalog, gates y Memoria.
- **Verificar:** CA-001–CA-006 y revisión independiente.
