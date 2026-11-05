# Spec — 02 Perfil Owner

> Estado: Propuesto. Este slice implementa los endpoints funcionales 1 y 2 del primer hito.

## 1. Objetivo y contexto

Permitir al usuario autenticado consultar y actualizar su propio perfil sin acceder a datos de otra cuenta.

## 2. Alcance y fuera de alcance

Incluye GET y PUT de perfil propio. No incluye gestión de perfil Business, eliminación de cuenta, verificación de correo ni recuperación de contraseña.

## 3. Actores, permisos y reglas

- Owner autenticado.
- Business podrá utilizar el perfil común cuando se defina su onboarding.
- El ID del usuario se obtiene de claims validados; el cliente no puede elegir el usuario destino.
- Email y rol no se pueden modificar mediante este endpoint salvo spec explícita posterior.

## 4. Requisitos funcionales y flujos

- RF-001 obtener perfil del sujeto autenticado.
- RF-002 actualizar campos permitidos del perfil propio.
- RF-003 validar longitud/formato y evitar asignación masiva de campos sensibles.

## 5. Contrato de API y datos

| Método | Endpoint | Resultado |
|---|---|---|
| GET | `/api/v1/users/me/get` | Perfil actual |
| PUT | `/api/v1/users/me/update` | Perfil actualizado |

Ambos usan Response Wrapper. La lista definitiva de campos editables se confirma contra los formularios de la aplicación móvil antes de implementar.

## 6. Requisitos no funcionales y seguridad

- Requiere autenticación.
- Resolver userId exclusivamente desde `sub`/identidad validada.
- No exponer password hash, refresh token, claims internos ni datos de sesión.
- Unit, Integration, Security y Contract obligatorias; OWASP ASVS L2 aplicable.

## 7. Criterios de aceptación

- [ ] CA-001 un Owner autenticado obtiene solo su perfil.
- [ ] CA-002 actualiza únicamente campos permitidos.
- [ ] CA-003 el request no puede cambiar el ID/rol/email protegido.
- [ ] CA-004 usuario anónimo recibe 401.
- [ ] CA-005 wrapper, status code y mensajes en español.
- [ ] CA-006 pruebas y gates pasan.

## 8. Estrategia de pruebas y gates

Unit: mapping/validación; Integration: Atlas test DB; Security: mass assignment, user ID spoofing y datos sensibles; Contract: rutas y wrapper.

## 9. Dependencias, riesgos y decisiones pendientes

Depende de autenticación. La lista de campos de perfil debe confirmarse con la maqueta móvil o decisión del usuario. No inventar campos opcionales.

## 10. Aprobación

Pendiente.
