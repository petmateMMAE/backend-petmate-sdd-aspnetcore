# Plan — 02 Perfil Owner

## 1. Contexto

Endpoints funcionales 1 y 2 del hito inicial; dependen de T013–T025.

## 2. Diseño técnico

- Application: Query `GetMyProfile` y Command `UpdateMyProfile` con handlers/validators.
- Domain: modelo de perfil y reglas permitidas.
- Infrastructure: MongoDB Atlas repository.
- Api: `UsersController` con dispatch Mediator y Response Wrapper.
- El user ID se obtiene del principal autenticado; no del body/query.

## 3. Persistencia y servicios

Actualizar documento de usuario existente en Atlas. Mantener campos de autenticación no editables desde el endpoint. Actualización parcial/completa se define por el DTO y spec revisado.

## 4. Seguridad y pruebas

Unit, Integration, Security y Contract; probar ID spoofing, mass assignment, anónimo y datos sensibles.

## 5. Dependencias y riesgos

La lista exacta de campos editables es decisión pendiente y debe acordarse antes de T027.

## 6. Verificación

Pruebas por CA; verificar wrapper y catálogo de endpoints.

## 7. Aprobación

Pendiente.
