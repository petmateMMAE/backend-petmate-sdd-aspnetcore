# PetMate — alcance de producto

> Resumen canónico del producto (Constitución §11), vigente desde 2026-11-09 y **aprobado por el usuario ese día**. Sustituye al resumen anterior, derivado del PDF descriptivo del 26 de octubre de 2026, que deja de ser fuente de alcance.

## 1. Fuentes de producto vigentes

| Fuente | Contenido | SHA-256 (referencia) |
|---|---|---|
| `00_PetMate_v3_Documentacion_Completa.pdf` (135 páginas, 12/10/2026) | 41 requerimientos funcionales (RF-01 a RF-41), 24 no funcionales (RNF-01 a RNF-24), 36 historias de usuario (HU-001 a HU-036), backlog de 114 puntos y 5 sprints del 13/10 al 18/12/2026 | `ad94a9eb63e21ff2080537e85b4d75ef258ca1613230399eddcd699edb667e5b` |
| `PetMate_App.html` (maqueta) | Aplicación HTML con 55 pantallas y datos de ejemplo; referencia visual y de flujos | `42ee9b85bef8f80a6226c32aa5cc2cd9941e2626cb719cd4a1aa1b3275b6da34` |

Los archivos no se versionan en este repositorio (la maqueta pesa más de 1 MB); su huella permite comprobar que no cambian. Las historias de usuario siguen en estado «Por refinar» con el VoBo funcional pendiente en el propio documento.

## 2. Producto

PetMate es una aplicación móvil Android con tres funciones: **carnet de salud de la mascota**, **código QR por mascota** y **mapa de negocios afiliados**. El backend (este repositorio) es la API REST que sirve a la app; la app Android, la maqueta y la página legal no se implementan aquí (Constitución §1).

**Actores:** Visitante (sin cuenta: demo y mapa), Usuario (toda cuenta registrada), Afiliado (Usuario con un negocio asociado: Veterinaria, Tienda o Estética) y Sistema (procesos automáticos).

## 3. Módulos funcionales

| Módulo | Capacidades (RF) |
|---|---|
| A Acceso | Crear cuenta con nombre, correo y contraseña (RF-02); iniciar y cerrar sesión (RF-03, RF-05); recuperar contraseña por correo (RF-04); eliminar cuenta (RF-08) |
| B Mascotas | Registrar, listar, ver, editar y eliminar mascotas (RF-09 a RF-12); QR permanente con token aleatorio y código corto (RF-13); regenerar el QR (RF-41) |
| C Carnet de salud | Vacunas, desparasitaciones, peso, alergias y condiciones, consultas y recetas (RF-14 a RF-18); los registros creados por una veterinaria son de solo lectura para el dueño; recordatorios (RF-19) |
| D Mapa | Negocios cercanos, filtro por tipo, tarjeta del marcador y perfil con contacto (RF-20 a RF-23), también para el Visitante |
| E Afiliación | Formulario, ubicación, alta y baja del negocio; una cuenta tiene como máximo un negocio (RF-24 a RF-28, RF-40) |
| F Panel del afiliado | Editar negocio y ubicación, servicios, escanear QR, ficha según el tipo de negocio, consulta con receta, vacuna o desparasitación aplicada y mascotas atendidas (RF-29 a RF-37) |
| G Página legal | Privacidad, términos y solicitud de eliminación de cuenta (RF-38, RF-39); el sitio es externo, el backend solo procesa la solicitud |
| Maqueta | Compartir historial con negocio, datos y duración elegidos; seguimiento de la atención; chat con fotos; ajustes de notificación y push; búsqueda por nombre; horario estructurado |

## 4. Decisiones del usuario que completan las fuentes (2026-11-09)

| Tema | Decisión |
|---|---|
| Fuentes | Documento v3 y maqueta reemplazan al PDF del 26 de octubre |
| Modelo de cuenta | La cuenta es siempre de Usuario; el negocio se le asocia al afiliarse (Constitución §6.1) |
| Registro | Obligatorios: nombre, correo y contraseña. El celular es opcional y se agrega después en el perfil; no hay verificación de correo ni de celular |
| Contraseña | Se mantiene la política vigente de **12 a 128 caracteres** (el documento propone 8; la app debe aplicar la misma política) |
| Mascotas | Especie en lista cerrada: **Perro y Gato**; fecha de nacimiento y sexo opcionales |
| QR y acceso | El QR es permanente de la mascota. El dueño da acceso a un negocio eligiendo negocio (o «cualquier negocio que escanee»), datos y duración (30 min, 1 día o 7 días). Sin autorización vigente, un escaneo no muestra nada. El dueño no revoca un acceso antes de que venza ni ve quién entró; regenerar el QR cancela las autorizaciones activas. El servidor conserva el registro interno de escaneos del negocio |
| Avisos | Push desde el servidor (Firebase Cloud Messaging, gratuito) para recordatorios, seguimiento y chat |
| Seguimiento y chat | Entran en el alcance. El chat parte como API REST con aviso push; el tiempo real se agrega si resulta viable |
| Negocio | Un solo tipo por negocio; la afiliación es autoservicio, sin aprobación manual; la cédula profesional se captura sin validarse |
| Datos públicos del negocio | Nombre, descripción, horarios, ubicación geográfica y catálogo de servicios; contacto (teléfono, WhatsApp, dirección), tipo y fotos también son públicos; la cédula profesional es privada. Horario estructurado por día, zona horaria America/Mexico_City |
| Fotos | Azure Blob Storage aprobado. Las fotos de negocios se sirven con acceso público; las de mascotas son privadas con URLs temporales (Constitución §8.10). El CDN no está aprobado |
| Correo | Solo para recuperar la contraseña. El proveedor de costo $0 se elige en el plan del Spec; todos exigen un dominio de envío verificado |
| Alcance | No se recorta ninguna funcionalidad de las fuentes, salvo lo excluido en el punto siguiente |

## 5. Fuera de alcance

Cobro, IVA y ticket de la atención; verificación de correo o de celular; SMS; inteligencia artificial y aprendizaje automático (podrían incorporarse más adelante); inventario, punto de venta, suscripciones y pagos, empleados y roles personalizados, sitio web para negocios, agenda de citas, familia compartida y pedidos.

## 6. Requisitos técnicos y de nube

Las fuentes de producto no prescriben la infraestructura. Los requisitos técnicos, de seguridad, despliegue en Azure, costos y calidad provienen de la lista académica `AC-01`…`AC-16` de `specs/traceability.md` y de la Constitución (§3 a §10). Por decisión del usuario, primero va la integración con la aplicación móvil y los Specs de nube (`pm-065` a `pm-084`) quedan en segundo plano, salvo lo mínimo para un entorno compartido de integración.

## 7. Trazabilidad

La matriz de requisitos de las fuentes frente a los Specs está en la sección 5 de [`specs/traceability.md`](../specs/traceability.md). Todas las funcionalidades de las fuentes tienen ya un Spec (`pm-085` a `pm-167`), salvo lo excluido en la sección 5.
