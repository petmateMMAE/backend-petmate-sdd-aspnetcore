---
version: 1.0.1
status: ratified
ratified: 2026-11-03
methodology: Spec-Driven Development (SDD)
project: PetMate Backend
source_of_truth: .specify/memory/constitution.md
mirror: ./constitution.md
---

# Constitución — PetMate Backend

> Esta es la fuente única de las reglas no negociables. Ante un conflicto, esta constitución prevalece sobre `AGENTS.md`, planes, tareas, skills y README. Ningún agente puede cambiarla sin autorización explícita del usuario.

## 1. Propósito y alcance

PetMate Backend será una API REST para la aplicación móvil Android y los negocios afiliados de PetMate. Será desarrollada desde una solución .NET vacía y funcionará como núcleo de validación, reglas de negocio, autenticación, autorización, persistencia y comunicación con servicios externos.

La solución inicial utilizará .NET 10 y ASP.NET Core 10. El backend se organizará mediante Clean Architecture, CQRS, el patrón Mediator e inyección de dependencias. El proyecto debe quedar preparado para ejecutarse en contenedores y escalar horizontalmente.

El alcance funcional del producto se deriva de `docs/product-scope.md`, elaborado a partir del PDF descriptivo de PetMate. Este repositorio y esta metodología cubren únicamente el backend; no deben implementar la aplicación Android ni la landing page salvo que una especificación posterior lo autorice expresamente.

## 2. Principios no negociables

1. **Spec first:** ninguna funcionalidad, cambio de contrato, entidad persistente o endpoint se implementa sin `spec.md` aprobado, `plan.md`, `task.md` y `checklists/gates.md` asociados.
2. **Clean Architecture:** el dominio no depende de infraestructura, framework web, base de datos ni proveedores externos.
3. **CQRS + Mediator:** los casos de uso se representan mediante Commands y Queries separados y se despachan desde Presentation a Application a través del patrón Mediator. Los controladores no invocan directamente repositorios ni handlers concretos.
4. **Inyección de dependencias:** las dependencias se reciben por constructor y se registran mediante extensiones de composición. Se prohíbe el Service Locator y el estado mutable global.
5. **MongoDB Atlas exclusivo:** MongoDB Atlas es el único motor/proveedor de persistencia. No se permite SQL Server, PostgreSQL, SQLite, EF Core, MongoDB local ni un contenedor local de MongoDB como alternativa para desarrollo o pruebas. Las pruebas de integración usarán una base de datos de pruebas independiente en MongoDB Atlas.
6. **Dos Redis independientes:** se mantendrán dos servicios Redis separados en desarrollo y producción: uno exclusivo para revocación/lista negra de JWT y otro exclusivo para caché distribuida. No se permite reutilizar el mismo servicio Redis para ambas responsabilidades.
7. **Seguridad por defecto:** denegar por defecto, mínimo privilegio, validar en la frontera y verificar autorización sobre cada recurso. La autenticación no implica autorización sobre mascotas, expedientes o negocios.
8. **Sin secretos en el repositorio:** ningún secreto, token, URI con credenciales, certificado privado o contraseña aparece en código, archivos `.md`, logs, fixtures o historial Git.
9. **Cero costo obligatorio:** herramientas, paquetes, agentes, servicios y planes de desarrollo/despliegue deben poder utilizarse con costo de licencia y consumo de **$0**. Si una opción requiere pago, suscripción, ampliación de plan o excede una cuota gratuita, no se adopta ni despliega sin aprobación explícita del usuario.
10. **Escalabilidad horizontal:** la aplicación debe ser stateless y estar diseñada para varias instancias contenerizadas detrás de un balanceador. No se almacena estado de sesión, revocación, caché compartida ni archivos necesarios para otra instancia en memoria o disco local del contenedor.
11. **HTTPS obligatorio:** toda comunicación pública con la API debe utilizar HTTPS con certificados válidos. No se permite exponer HTTP sin cifrado en producción.
12. **DevSecOps continuo:** cada funcionalidad debe superar pruebas unitarias, de integración y de seguridad, además de análisis automatizados de código, dependencias, secretos y artefactos aplicables.
13. **Trazabilidad verificable:** los criterios de aceptación deben enlazarse con tareas, endpoints, componentes y pruebas reales. No se marca una tarea terminada sin evidencia reproducible.
14. **Sin invenciones:** los agentes separan hechos comprobados, propuestas y decisiones pendientes. No inventan reglas de negocio que no estén en una especificación o fuente del producto.

## 3. Stack y estructura de solución obligatorios

### 3.1 Tecnologías base

| Área | Regla |
|---|---|
| Runtime | .NET 10 / ASP.NET Core 10 |
| Presentación | ASP.NET Core Web API con MVC Controllers |
| Documentación de API | Documento OpenAPI generado y Scalar como única UI interactiva de documentación |
| Arquitectura | Clean Architecture por proyectos |
| Casos de uso | CQRS (Commands y Queries) |
| Despacho | Patrón Mediator entre Presentation y Application |
| Inyección | Microsoft DI y módulos de registro por capa |
| Persistencia | MongoDB.Driver conectado exclusivamente a MongoDB Atlas |
| Lista negra JWT | Servicio Redis independiente para autenticación |
| Caché | Segundo servicio Redis independiente |
| Contenedores | Docker/OCI, con configuración multi-stage |
| Despliegue objetivo | Azure, sujeto a disponibilidad de opciones de costo $0 |

No se fija aquí la versión de paquetes NuGet ni el paquete concreto de Mediator o Argon2id. Antes de incorporar cualquier dependencia, el agente debe comprobar compatibilidad con .NET 10, mantenimiento, vulnerabilidades y licencia/costo. La decisión debe quedar registrada en el `plan.md` o en una decisión técnica enlazada.

### 3.2 Proyectos y dirección de dependencias

La estructura esperada es:

```text
PetMate/
├── PetMate.sln
├── src/
│   ├── PetMate.Api/             # ASP.NET Core Web API MVC (Presentation)
│   ├── PetMate.Application/     # Class Library
│   ├── PetMate.Domain/          # Class Library
│   └── PetMate.Infrastructure/  # Class Library
├── tests/
│   ├── PetMate.UnitTests/
│   ├── PetMate.IntegrationTests/
│   ├── PetMate.SecurityTests/
│   └── PetMate.ContractTests/
├── .opencode/
├── .specify/
├── specs/
├── docs/
├── scripts/
├── .github/workflows/
├── AGENTS.md
├── Memoria.md
└── README.md
```

Dirección permitida de referencias:

- `PetMate.Domain`: no referencia a los otros proyectos.
- `PetMate.Application`: referencia `PetMate.Domain`.
- `PetMate.Infrastructure`: referencia `PetMate.Application` y `PetMate.Domain` para implementar sus abstracciones.
- `PetMate.Api`: referencia `PetMate.Application` y `PetMate.Infrastructure` como composition root.
- Los proyectos de pruebas referencian únicamente los proyectos que deban verificar.

`Domain`, `Application` e `Infrastructure` son Class Library. `Api` es un proyecto ASP.NET Core Web API MVC. Los nombres de proyectos son los nombres iniciales propuestos y deben conservarse salvo autorización explícita.

El registro de infraestructura se realiza desde el composition root de `Api`, llamando extensiones como `AddApplication(...)` y `AddInfrastructure(...)`. La implementación exacta se define en `specs/00-foundation/`.

## 4. MongoDB Atlas, configuración y secretos

1. MongoDB Atlas es obligatorio para desarrollo, integración y producción. No agregar otro motor ni un proveedor alternativo.
2. La URI y credenciales MongoDB se obtienen exclusivamente de variables de entorno; localmente se cargan desde `.env`, ignorado por Git.
3. Incluir `.env.example` con nombres de variables y valores ficticios/placeholders, nunca credenciales reales.
4. La base de datos de integración debe ser independiente de la de desarrollo/producción, con nombre y credenciales propios. Las pruebas deben comprobar que la configuración apunta a la base de pruebas antes de escribir o borrar datos.
5. Las credenciales de producción se inyectan mediante configuración segura del entorno Azure o un almacén de secretos si la opción elegida mantiene costo $0. No se despliega el archivo `.env`.
6. Las colecciones, índices, unicidad, TTL y estrategia de borrado deben justificarse por casos de uso. No se simula persistencia de negocio con almacenamiento en memoria en pruebas de integración.
7. No incluir dumps reales ni datos personales en el repositorio.

Nombres de variables previstos (ajustables en el spec de configuración): `MONGODB_URI`, `MONGODB_DATABASE`, `MONGODB_TEST_URI`, `MONGODB_TEST_DATABASE`, `REDIS_AUTH_CONNECTION`, `REDIS_CACHE_CONNECTION`, `JWT_ISSUER`, `JWT_AUDIENCE` y `JWT_SIGNING_KEY`. Los secretos nunca se documentan con sus valores.

## 5. Redis: responsabilidades separadas

### 5.1 Redis de autenticación

- Exclusivo para la lista negra de JWT revocados antes de su expiración.
- Clave recomendada: `jwt:blacklist:{jti}`.
- TTL: tiempo restante hasta `exp`; no conservar la entrada después de que el JWT ya no pueda aceptarse.
- El middleware/validador JWT consulta la lista negra después de validar firma, emisor, audiencia y expiración.
- Si el servicio de Redis de autenticación no está disponible, la validación debe fallar de forma segura (fail closed) y responder como indisponibilidad del servicio, sin conceder acceso por omisión.
- No almacenar contraseñas ni refresh tokens en texto claro.

### 5.2 Redis de caché

- Servicio Redis separado, con endpoint, credenciales, configuración y ciclo de vida independientes.
- Se utilizará caché distribuida para datos aptos para caché, con TTL e invalidación definidos en cada spec.
- La caché no es fuente de verdad. Si falla, los casos de uso deben recurrir a MongoDB cuando sea seguro hacerlo.
- No almacenar contraseñas, tokens de acceso, refresh tokens ni información clínica privada en caché compartida sin autorización y diseño de aislamiento explícitos.
- Cada clave debe evitar colisiones entre usuarios/negocios y versiones de esquema cuando aplique.

En desarrollo pueden ejecutarse como dos servicios/contendedores Redis diferentes. En producción deben ser dos servicios/instancias independientes y compartidos por todas las réplicas de API. No conectar el Redis de autenticación al Redis de caché por conveniencia.

## 6. Autenticación, sesiones y autorización

### 6.1 Roles

Los únicos roles del sistema son:

- `Owner`: propietario de mascotas.
- `Business`: cuenta de un negocio afiliado.

No existe rol `Admin`, `Administrator`, `SuperAdmin` ni equivalente. No crear endpoints o claims administrativos. Las operaciones operativas de infraestructura no se modelan como un rol de usuario de la API.

El registro público inicial crea cuentas `Owner` por defecto. El cliente no puede enviar libremente un rol para elevar privilegios. El alta, verificación y activación de `Business` tendrá una especificación propia posterior; hasta entonces, no inventar un flujo de aprobación ni habilitar acceso Business a datos protegidos. Esta restricción no impide definir el rol Business en el modelo de autorización.

### 6.2 Contraseñas

- Utilizar Argon2id para almacenar contraseñas.
- Utilizar un paquete o implementación mantenida cuya licencia permita el uso sin costo.
- Cada contraseña tendrá salt criptográficamente aleatorio y parámetros de costo configurados; guardar un formato verificable/versionado (preferentemente PHC) para poder rehashear al autenticar cuando los parámetros cambien.
- Parámetros definitivos deben calibrarse y documentarse en `plan.md`; no reducirlos para acelerar pruebas de producción.
- Prohibido almacenar o registrar contraseñas en texto claro, reversible o en logs.

### 6.3 Access Token y Refresh Token

- Access Token: JWT firmado, de vida corta. Valor inicial recomendado: 15 minutos, configurable.
- Refresh Token: valor opaco criptográficamente aleatorio, no JWT. Se emite durante Login/Register y se utiliza en `/api/v1/auth/refresh`.
- El backend almacena únicamente el hash del refresh token y metadatos mínimos de sesión en MongoDB Atlas; nunca el token en texto claro.
- La renovación utiliza rotación: cada uso válido invalida el refresh token anterior y emite uno nuevo. La reutilización de un token ya consumido debe detectarse y revocar la familia/sesión asociada.
- Política inicial configurable: 30 días de inactividad y 90 días de vida absoluta máxima por sesión. El uso activo renueva la ventana de inactividad, pero no elimina el límite absoluto.
- Algoritmo inicial propuesto: `HS256` con secreto aleatorio criptográficamente fuerte de al menos 256 bits; fijar y permitir exclusivamente el algoritmo aprobado. Si el análisis de amenazas justifica otra opción, documentarla como ADR antes de cambiarlo.
- Claims JWT mínimos: `sub`, `jti`, `role`, `iss`, `aud`, `iat`, `nbf` y `exp`. No incluir datos personales o clínicos innecesarios.
- Validar `issuer`, `audience`, firma, algoritmo permitido y expiración; configurar `ClockSkew` en cero con relojes sincronizados. La clave debe ser externa al código y compartida de manera segura por todas las instancias.

La sesión no debe cerrarse al expirar el Access Token mientras exista un Refresh Token vigente. El cliente móvil debe guardar los tokens en almacenamiento seguro del sistema operativo; esa responsabilidad del cliente se documenta como contrato de integración.

### 6.4 Logout y lista negra

Logout invalida la sesión actual:

1. Requiere identificar la sesión/refresh token correspondiente.
2. Revoca el refresh token/sesión persistido en MongoDB Atlas.
3. Añade el `jti` del Access Token aún vigente a Redis de autenticación con TTL igual al tiempo restante de vida del JWT.
4. Las peticiones posteriores con ese Access Token se rechazan aunque su firma y fecha sigan siendo válidas.
5. No registrar el Access Token ni el Refresh Token.

El contrato exacto de Logout (encabezado Authorization, cuerpo, idempotencia y comportamiento con token expirado) se define en `specs/01-initial-authentication/spec.md` antes de implementar.

### 6.5 Autorización

- Aplicar políticas por rol y verificación de propiedad/relación del recurso.
- Toda operación sobre una mascota debe verificar que pertenece al Owner autenticado o que existe una autorización explícita válida para el negocio.
- Se deben probar intentos IDOR/BOLA: cambiar un identificador no debe revelar ni modificar recursos ajenos.
- Los endpoints iniciales de mascotas son exclusivos de Owner.

## 7. Contrato de API y convenciones

### 7.1 Versionado y rutas con acción explícita

Todas las rutas de negocio deben incluir versión y acción literal. Formato inicial:

`/api/v1/{recurso}/{acción}`

No se debe inferir la operación únicamente a partir del verbo HTTP. Ejemplos:

- `POST /api/v1/pets/create`
- `GET /api/v1/pets/get-all`
- `GET /api/v1/pets/get-by-id/{id}`
- `PUT /api/v1/pets/update/{id}`
- `DELETE /api/v1/pets/delete/{id}`

La acción explícita no sustituye la semántica correcta del verbo HTTP. Mantener el catálogo canónico en `docs/api/endpoints.md`.

### 7.2 Response Wrapper obligatorio

Todas las respuestas de endpoints de la API, tanto exitosas como de error, deben utilizar un wrapper genérico equivalente a:

```json
{
  "statusCode": 200,
  "message": "Mascotas obtenidas correctamente",
  "data": []
}
```

Contrato:

- `statusCode`: entero igual al código HTTP real.
- `message`: mensaje legible en español.
- `data`: objeto, colección o `null`. Se permite explícitamente `null`.

No devolver `204 No Content` en operaciones de negocio porque el wrapper forma parte del contrato. Una eliminación correcta puede responder `200` con `data: null`. Los errores no deben exponer stack traces, nombres internos, cadenas de conexión ni secretos. El formato también se aplicará a errores de validación y excepciones centralizadas.

### 7.3 OpenAPI y Scalar

- La API debe generar un documento OpenAPI a partir de sus controladores, contratos, DTOs, códigos de respuesta y metadatos. El documento generado es la representación técnica del contrato; no se mantiene manualmente como una especificación paralela.
- **Scalar es la única interfaz gráfica/interactiva de documentación de la API.** La UI debe consumir el documento OpenAPI generado.
- No agregar Swagger UI, Swashbuckle UI, NSwag UI ni una segunda interfaz de documentación. No se prohíbe el uso de bibliotecas necesarias para generar OpenAPI; la UI debe seguir siendo Scalar.
- Rutas iniciales previstas: documento `/openapi/v1.json` y UI `/scalar/v1`. Si el mapeo del paquete requiere una configuración explícita para cumplir esas rutas, configurarla y probarla.
- La UI y el documento deben estar disponibles por defecto en desarrollo. En producción permanecerán deshabilitados salvo habilitación explícita por configuración y revisión de seguridad. No deben revelar secretos, valores de configuración, información interna ni ejemplos con datos reales.
- El esquema debe representar el Response Wrapper (`statusCode`, `message`, `data`, con `data` nullable) y los códigos HTTP correspondientes. Cuando se implemente autenticación, documentar el esquema HTTP Bearer/JWT y los requisitos de seguridad de cada operación protegida.
- Antes de agregar el paquete de integración de Scalar, verificar compatibilidad con .NET 10, licencia y costo $0, mantenimiento y vulnerabilidades. La selección y evidencia se registran en el plan.

### 7.4 Idioma y código

- Documentación SDD y técnica: español.
- Código: inglés (clases, métodos, propiedades, variables, namespaces y nombres de archivos).
- Comentarios del código: español, solo cuando aporten contexto no evidente.
- Mensajes de respuesta de la API: español.
- Nombres de roles almacenados/serializados: `Owner` y `Business`.

## 8. Azure, contenedores, escalabilidad y contenido multimedia

1. El destino de despliegue es Azure, utilizando únicamente planes y recursos que mantengan el costo en $0 para la demo inicial.
2. La arquitectura del backend debe ser stateless, contenerizada y lista para ejecutarse en varias instancias detrás de un balanceador de carga.
3. El estado compartido debe residir en MongoDB Atlas y en los dos Redis independientes. No usar memoria local para sesiones ni archivos temporales como fuente de verdad.
4. La planificación debe comparar los servicios Azure compatibles con contenedores, balanceo, HTTPS, variables secretas y escalado horizontal. No seleccionar un recurso basándose en que históricamente tuvo un nivel gratuito: verificar límites y costo en el momento de planificar el despliegue.
5. **Restricción de costo:** si varias instancias o balanceo no están disponibles con costo $0, no crear recursos de pago. Mantener la solución preparada para varias instancias y documentar que la demo gratuita opera temporalmente con una réplica hasta que una alternativa $0 permita desplegar más. La arquitectura horizontal sigue siendo obligatoria; no se permite diseñar una aplicación dependiente de una sola instancia.
6. HTTPS debe terminar en el punto de entrada público de Azure o en el componente de ingress elegido, con certificado válido y redirección/ rechazo de HTTP. Configurar correctamente forwarded headers y confiar únicamente en proxies conocidos.
7. Las imágenes se almacenarán en Azure Blob Storage como origen y se distribuirán mediante el CDN de Azure seleccionado. La elección concreta del producto de CDN y su modelo de acceso se valida por costo $0 y disponibilidad. No confundir CDN (distribución) con almacenamiento de origen.
8. Imágenes comerciales públicas (por ejemplo, logos/fotos de negocios) podrán tener distribución pública según la spec. Fotografías de mascotas, conversaciones o registros clínicos serán privadas y requerirán autorización y URLs/credenciales temporales cuando corresponda. No exponer contenedores privados ni usar URLs permanentes públicas para información sensible.
9. Los secretos de producción se inyectan como configuración segura del entorno Azure. Azure Key Vault puede utilizarse solo si la modalidad elegida no introduce costo; nunca es obligatorio pagar para cumplir la constitución.
10. CI/CD debe automatizar restore, build, pruebas y controles de seguridad. El despliegue continuo a Azure estará condicionado a pasar la revisión de costo $0 y las aprobaciones de release definidas.

## 9. DevSecOps y herramientas sin costo

El desarrollo debe aplicar DevSecOps desde el primer commit. El conjunto exacto de herramientas se decidirá mediante una evaluación de licencia, mantenimiento, compatibilidad .NET 10 y costo. Solo se aceptan herramientas que puedan operar con costo $0.

Cada funcionalidad debe aplicar, como mínimo:

- Validación de requisitos de seguridad basada en OWASP ASVS Nivel 2 (o versión vigente equivalente que se documente al fijarla).
- Pruebas unitarias de reglas de dominio y casos de uso.
- Pruebas de integración con MongoDB Atlas de pruebas y dependencias Redis de prueba.
- Pruebas de seguridad de autenticación, autorización, validación, exposición de datos y abuso de endpoints.
- Pruebas de contrato/API para rutas, códigos HTTP y Response Wrapper.
- Análisis estático (SAST) y reglas de calidad.
- Escaneo de secretos.
- Auditoría de vulnerabilidades y licencias de dependencias NuGet.
- Escaneo de imágenes de contenedor y configuración de despliegue cuando aplique.
- Generación de SBOM cuando exista una herramienta compatible de costo $0.

Un hallazgo crítico/alto no resuelto bloquea la aceptación. Las excepciones deben tener justificación, riesgo, mitigación, responsable y aprobación explícita; no se silencian hallazgos para conseguir un pipeline verde.

No se introduce ninguna dependencia pagada, servicio SaaS de pago, plan comercial ni herramienta de licencia desconocida sin comprobar su costo y obtener autorización.

## 10. Pruebas, calidad y aceptación

- Las suites se separan por propósito: Unit, Integration, Security y Contract.
- Pruebas de integración de MongoDB usan exclusivamente MongoDB Atlas con base de datos separada para test y credenciales restringidas.
- Pruebas de Redis deben aislar Redis de autenticación y Redis de caché. No apuntar a endpoints de producción.
- Las pruebas no deben depender del orden, de datos compartidos no aislados ni de servicios pagos.
- Cada criterio de aceptación de un spec debe estar cubierto por evidencia de prueba o por una justificación explícita de no automatización.
- Debe existir build Release antes de ejecutar suites que dependan de binarios compilados.
- Los gates de seguridad, pruebas y trazabilidad se ejecutan en CI y localmente con los mismos criterios siempre que sea viable.
- No establecer porcentajes arbitrarios de cobertura antes de medir el baseline. El umbral inicial se define en `specs/00-foundation/plan.md` y se registra aquí mediante enmienda cuando haya evidencia.

## 11. Producto y alcance funcional

`docs/product-scope.md` es el resumen canónico del documento descriptivo de PetMate y conserva la distinción entre requisitos expresamente descritos y decisiones aún no definidas.

Los módulos futuros incluyen gestión de usuarios, mascotas, carnet médico, QR y autorizaciones, negocios afiliados, búsqueda geográfica, atención veterinaria, seguimiento, servicios/cobros, recordatorios y notificaciones. Estos módulos no se consideran implementados por aparecer en el documento; requieren specs propias.

La primera entrega prioriza el flujo de autenticación y los siete endpoints funcionales definidos en `specs/README.md`. El hito se considera alcanzado únicamente al cerrar la tarea global `T041` con todos los gates y aprobaciones completados.

## 12. Gobernanza SDD y precedencia documental

### 12.1 Fuente de verdad por artefacto

| Artefacto | Responsabilidad única |
|---|---|
| `.specify/memory/constitution.md` | Reglas no negociables y decisiones normativas |
| `AGENTS.md` | Cómo deben operar los agentes y qué leer/ejecutar |
| `Memoria.md` | Estado actual, decisiones tomadas con evidencia y lecciones |
| `specs/README.md` | Índice, roadmap, dependencias y estado de cada slice |
| `spec.md` | Qué comportamiento se requiere y cómo se acepta |
| `plan.md` | Cómo se implementará técnicamente |
| `task.md` | Pasos ejecutables y verificables |
| `checklists/gates.md` | Evidencia de calidad, seguridad y cierre |
| `docs/api/endpoints.md` | Catálogo vivo de contratos de API |
| `docs/security/asvs-l2-checklist.md` | Trazabilidad de controles de seguridad |

No duplicar tablas completas de endpoints, políticas, costos o reglas en varios artefactos. Enlazar el documento canónico.

### 12.2 Flujo obligatorio por funcionalidad

1. Leer `AGENTS.md`, esta constitución y el estado de `Memoria.md`.
2. Consultar `specs/README.md` y las dependencias previas.
3. Elaborar `spec.md` usando la plantilla.
4. Revisar requisitos, alcance, roles, errores, contrato y criterios de aceptación con el usuario.
5. Elaborar `plan.md` con archivos/componentes, dependencias, paquetes y costos/licencias.
6. Descomponer el plan en `task.md` con tareas pequeñas, dependencias y verificación.
7. Definir `checklists/gates.md` antes de implementar.
8. Implementar solo las tareas autorizadas, en el orden definido.
9. Ejecutar pruebas unitarias, integración, seguridad y contrato, más análisis DevSecOps.
10. Solicitar revisión independiente de seguridad y trazabilidad.
11. Conciliar documentación viva y `Memoria.md` con el código real.
12. El usuario/revisor humano aprueba el cierre. Los agentes no se auto-firman.

### 12.3 Identificadores y estado

- Usar identificadores de slice secuenciales y descriptivos: `00-foundation`, `01-initial-authentication`, `02-owner-profile`, `03-pet-management`, etc.
- Dentro de la primera secuencia global se usan tareas `T001` a `T041`, sin reutilizar IDs.
- Estados permitidos: `Propuesto`, `En revisión`, `Aprobado`, `En progreso`, `Bloqueado`, `Verificado`, `Cerrado`.
- Una tarea `Verificado` requiere evidencia. `Cerrado` requiere aprobación humana.
- No marcar un spec como aprobado o firmado por inferencia del agente.

## 13. Historial de cambios

### 2026-11-03 — Ratificación humana (versión 1.0.1)

- El usuario ratificó formalmente el contenido revisado de `1.0.1-draft` sin cambios normativos.
- `status` pasa a `ratified` y la versión se normaliza a `1.0.1`; no se modificó ninguna regla.

### 2026-11-03 — Versión 1.0.1-draft

- Se incorpora la generación de documentación OpenAPI y Scalar como única UI interactiva de la API.
- Se establece que Swagger UI, Swashbuckle UI y NSwag UI no deben agregarse como interfaces alternativas.
- Se incluye la configuración inicial de OpenAPI/Scalar dentro de `00-foundation`, tarea T007.
- La constitución continúa pendiente de ratificación humana.

## 14. Cambios a la constitución

Toda modificación requiere propuesta explícita, motivo, impacto en specs/gates/agentes y aprobación del usuario. Actualizar `version`, `last_amended` y el historial de cambios solo tras la aprobación. El archivo raíz `constitution.md` siempre será un puntero, nunca una copia.
