# PetMate — alcance de producto derivado del documento descriptivo

> Fuente: `Descripcion_PetMate_Backend_Movil.pdf`, 17 páginas, documento “PetMate: Descripción del proyecto — Especificación funcional y técnica del Backend y la Aplicación Móvil”, periodo septiembre–diciembre de 2026. Este resumen conserva lo expresado por el documento; las decisiones técnicas nuevas están señaladas como tales.

## 1. Descripción general

PetMate centraliza información médica y de cuidado de mascotas mediante un carnet digital. El sistema contempla propietarios y negocios afiliados (clínicas veterinarias, tiendas y estéticas). La solución completa incluye aplicación Android nativa Kotlin, backend, base NoSQL, servicios cloud y landing page. Este repositorio se limita al backend.

## 2. Backend descrito por el documento

El backend se plantea como API REST con Clean Architecture y casos de uso CQRS. La descripción separa Domain, Application, Infrastructure y Presentation/API. Los controladores reciben peticiones y delegan a casos de uso; la lógica de negocio no debe residir en controllers.

La capa Domain representa usuarios, mascotas, vacunas, desparasitaciones, peso, alergias, consultas, recetas, seguimientos, negocios, servicios y autorizaciones QR. Application coordina Commands/Queries, validaciones y abstracciones. Infrastructure implementa persistencia NoSQL, caché y servicios externos. Presentation expone endpoints HTTP con autenticación/autorización y respuestas JSON consistentes.

## 3. Módulos funcionales identificados

| Módulo | Capacidades descritas |
|---|---|
| Usuarios/autenticación | Registro, login, JWT, vigencia/renovación de sesiones, logout, recuperación de contraseña, permisos y eliminación de cuenta. El documento contempla verificación de correo como posibilidad. |
| Mascotas | Alta, listado, detalle, actualización y eliminación; datos como nombre, especie, raza, fecha de nacimiento, sexo y fotografía. |
| Carnet de salud | Vacunas, desparasitaciones, controles de peso, alergias, consultas e historial médico. |
| QR/autorizaciones | Generar/renovar códigos, revocar anteriores, autorizar acceso temporal y limitar datos según tipo de negocio. |
| Negocios afiliados | Perfiles de veterinarias, tiendas y estéticas; afiliación, dirección, ubicación, horarios, servicios y permisos. |
| Búsqueda | Consulta de negocios por ubicación, categoría y filtros; paginación cuando sea necesaria. |
| Atención veterinaria | Consultas, diagnósticos, observaciones, recetas, tratamientos, vacunas y servicios prestados. |
| Seguimiento/comunicación | Estado de atención, conversaciones y fotografías entre propietario y negocio cuando esté habilitado. |
| Servicios/cobros | Catálogo de servicios, conceptos e importes, desglose y total de atención. |
| Recordatorios/notificaciones | Fechas de vacunación/desparasitación, eventos de seguimiento y notificaciones push; FCM aparece como proveedor posible, no como decisión cerrada. |

## 4. Seguridad, operación y calidad descritas

El documento exige JWT, autorización por identidad/rol/permisos, validación de entrada, protección ante accesos no autorizados y HTTPS. Para recursos de mascotas no basta con recibir un ID: se debe comprobar propiedad o autorización vigente. QR no debe convertirse en acceso público ilimitado al carnet.

También contempla Redis como caché distribuida, paginación, consultas eficientes, múltiples instancias, logs de eventos/errores, pruebas unitarias, integración y seguridad, y CI/CD.

## 5. Funcionalidad de la aplicación móvil fuera de este repositorio

El documento describe una aplicación Android nativa Kotlin con navegación de Owner y negocios, carnet, QR, mapa, recordatorios, seguimiento, escáner y almacenamiento local. También menciona una función de IA/ML para la app móvil. No se asume que estos componentes se implementen en el backend sin un requerimiento y spec de integración.

## 6. Decisiones específicas añadidas por el usuario para el backend

Estas decisiones no se atribuyen al PDF; fueron definidas para la implementación actual:

- .NET 10 / ASP.NET Core 10.
- Clean Architecture, CQRS, Mediator e inyección de dependencias.
- MongoDB Atlas como proveedor exclusivo.
- Dos Redis independientes: blacklist JWT y caché.
- Argon2id, JWT y refresh token rotatorio.
- Roles únicos Owner y Business; sin Admin.
- Response Wrapper `statusCode`, `message`, `data` nullable.
- Rutas versionadas `/api/v1/` con acción literal.
- Azure, costo $0, CI/CD, contenedores, HTTPS, horizontal readiness y Blob/CDN.
- DevSecOps con pruebas unitarias, integración, seguridad y OWASP ASVS L2.

## 7. Fuentes del PDF por página

- Descripción general/backend/arquitectura: páginas 2–4.
- Mascotas, carnet y QR: páginas 5–6.
- Negocios, búsqueda, atención y seguimiento: páginas 6–7.
- Seguridad, rendimiento, persistencia, observabilidad, pruebas y despliegue: páginas 7–8.
- Flujos y requisitos de la aplicación móvil: páginas 9–15.
- Relación backend-móvil y consideraciones finales: páginas 16–17.
