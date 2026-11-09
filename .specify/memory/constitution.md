---
version: 1.3.0
status: ratified
ratified: 2026-11-03
amended: 2026-11-09
previous_version: 1.2.0 (historial de Git; 1.0.1 en specs/_historico/constitution-v1.0.1.md)
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

El alcance funcional del producto se deriva de `docs/product-scope.md`, elaborado a partir del documento v3 de requerimientos, historias de usuario y backlog de PetMate y de la maqueta HTML de la aplicación. Este repositorio y esta metodología cubren únicamente el backend; no deben implementar la aplicación Android ni la página legal salvo que una especificación posterior lo autorice expresamente.

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
| Despliegue objetivo | Microsoft Azure (suscripción Azure for Students), $0 MXN de desembolso directo, con dos o más instancias detrás de un Load Balancer o equivalente (§8) |

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
├── specs/                  # pm-NNN-slug/, README.md (registro), traceability.md, _historico/
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

El registro de infraestructura se realiza desde el composition root de `Api`, llamando extensiones como `AddApplication(...)` y `AddInfrastructure(...)`. La implementación exacta se define en `pm-005` (inyección de dependencias) y `pm-006` (CQRS y Mediator).

## 4. MongoDB Atlas, configuración y secretos

1. MongoDB Atlas es obligatorio para desarrollo, integración y producción. No agregar otro motor ni un proveedor alternativo.
2. La URI y credenciales MongoDB se obtienen exclusivamente de variables de entorno; localmente se cargan desde `.env`, ignorado por Git.
3. Incluir `.env.example` con nombres de variables y valores ficticios/placeholders, nunca credenciales reales.
4. La base de datos de integración debe ser independiente de la de desarrollo/producción, con nombre y credenciales propios. Las pruebas deben comprobar que la configuración apunta a la base de pruebas antes de escribir o borrar datos.
5. Las credenciales de producción se inyectan mediante configuración segura del entorno Azure o un almacén de secretos de Azure si la opción elegida mantiene costo $0. No se despliega el archivo `.env`.
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

### 6.1 Cuentas, negocio y roles

Toda cuenta es de **Usuario** (rol `User`). Un Usuario puede tener como máximo **un negocio asociado** de tipo Veterinaria, Tienda o Estética; esa asociación (Afiliado) concede capacidades adicionales según el tipo de negocio, sin retirar al Usuario el acceso a sus mascotas. La cuenta no cambia de rol al afiliarse.

No existe rol `Admin`, `Administrator`, `SuperAdmin` ni equivalente. No crear endpoints o claims administrativos. Las operaciones operativas de infraestructura no se modelan como un rol de usuario de la API.

El registro público crea siempre una cuenta de Usuario; el cliente no puede enviar un rol ni un tipo de negocio para elevar privilegios. La afiliación de un negocio es de autoservicio y **no requiere aprobación manual**; la cédula profesional de las veterinarias se captura pero no se valida. El alta del negocio tendrá sus propias especificaciones.

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

El contrato exacto de Logout (encabezado Authorization, cuerpo, idempotencia y comportamiento con token expirado) se define en `specs/pm-032-user-logout/spec.md` antes de implementar.

### 6.5 Autorización

- Aplicar políticas por rol y verificación de propiedad/relación del recurso.
- Toda operación sobre una mascota debe verificar que pertenece al Usuario autenticado o que existe una autorización explícita y vigente, otorgada por el dueño al negocio (negocio, datos y duración elegidos), limitada por el tipo de negocio. Sin autorización vigente, un negocio no accede a ningún dato.
- Se deben probar intentos IDOR/BOLA: cambiar un identificador no debe revelar ni modificar recursos ajenos.
- Los endpoints de mascotas son de cualquier Usuario autenticado y solo sobre sus propias mascotas.

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
- Nombre del rol almacenado/serializado: `User`. Los tipos de negocio se definen en sus Specs.

## 8. Azure, contenedores, escalabilidad y contenido multimedia

> Redacción vigente desde la versión 1.1.0. Sustituye íntegramente el §8 de la versión 1.0.1 (copia conservada en `specs/_historico/constitution-v1.0.1.md`).

1. **Plataforma exclusiva.** El proveedor cloud es **Microsoft Azure**, mediante la suscripción **Azure for Students**. No se utiliza AWS ni Google Cloud y no se diseña una arquitectura multi-cloud. La moneda de referencia para costos es MXN y el objetivo económico es **$0 MXN de desembolso directo**, apoyado en los servicios incluidos gratuitamente y en los créditos de Azure for Students.
2. **Tres categorías de consumo.** Toda decisión de servicio se clasifica como (a) incluido gratuitamente, (b) consumo cubierto por créditos de Azure for Students o (c) consumo que podría generar cargos adicionales. Pertenecer a Azure for Students no hace gratuito a ningún servicio. No se asume gratuidad por precedente: se verifica en fuentes oficiales al planificar (`pm-062`).
3. **Dos instancias y balanceo (obligatorio).** El backend es stateless, contenerizado y se despliega con **al menos dos instancias detrás de un Load Balancer o servicio equivalente de Azure**. No hay excepciones. **Queda derogada expresamente la cláusula 8.5 de la versión 1.0.1**, que permitía operar temporalmente con una sola réplica. La arquitectura no puede depender de una sola instancia.
4. **Estado compartido.** Reside en MongoDB Atlas y en los dos Redis independientes (Auth y Cache). No se usa memoria local ni disco local como fuente de verdad.
5. **Conflicto de costo.** Si la arquitectura requerida no puede cubrirse sin desembolso adicional, **no se crean recursos que generen cargos, no se sustituye la arquitectura por una sola instancia y no se omite el Load Balancer**. Se documenta la limitación como conflicto académico y se espera la decisión humana. Ningún requisito se reduce ni se vuelve opcional por costo o dificultad.
6. **Recursos reales.** No se crean recursos reales ni se habilitan servicios que puedan generar cargos sin autorización expresa del usuario.
7. **HTTPS.** Obligatorio en el punto de entrada público (Load Balancer o ingress) con certificado válido, redirección o rechazo de HTTP y HSTS. Los forwarded headers solo se aceptan de proxies conocidos.
8. **Seguridad de red.** Los servicios de datos de Azure (Redis y almacenamiento) no son accesibles desde Internet abierta; se restringen mediante red privada o controles equivalentes.
9. **Redis en Azure.** Redis Auth y Redis Cache son servicios separados, con TLS y autenticación obligatorios, credenciales distintas, sin secretos en logs y con pruebas de conectividad y de fallo. Redis Auth conserva `noeviction`, persistencia y comportamiento fail-closed; Redis Cache usa expulsión LRU y degrada sin comprometer la corrección funcional. *(Enmienda 1.2.0)* Se admiten como **contenedores autogestionados** (`redis:7.4`) dentro del ambiente privado de Azure Container Apps: solo ingreso TCP interno (nunca expuestos a Internet), TLS con una CA propia generada por la IaC, una réplica por servicio y la persistencia de Redis Auth en Azure Files. Un servicio Redis administrado de Azure sigue siendo una alternativa válida si su costo se aprueba.
10. **Imágenes y CDN.** Las imágenes usan Azure Blob Storage como origen y una distribución por CDN para contenido público, ambos validados por costo. No se confunde CDN con almacenamiento. Imágenes comerciales públicas pueden distribuirse públicamente; fotografías de mascotas, conversaciones y registros clínicos son privadas, con autorización y URLs o credenciales temporales.
11. **Secretos.** Se inyectan como configuración segura del entorno Azure; un almacén de secretos (p. ej. Azure Key Vault) solo se usa si su modalidad no introduce cargos. `.env` nunca se despliega.
12. **Registro de contenedores.** Azure Container Registry o una alternativa validada dentro de Azure, con etiquetas inmutables y trazabilidad entre imagen, versión y despliegue.
13. **CI y CD obligatorios.** CI automatiza restore, build, pruebas, controles de seguridad y el gate de integridad SDD. CD despliega versiones trazables (SemVer, etiqueta e imagen inmutable) con prueba de humo posterior y vía de reversa, y solo tras la revisión de costo y las aprobaciones de release.
14. **Observabilidad.** Eventos y errores se envían a un servicio de observabilidad en Azure, con alertas y sin PII ni secretos.
15. **Infraestructura como código y ciclo de vida.** La infraestructura se define como código (preferentemente Terraform; la herramienta se fija por ADR en `pm-066`, es una decisión de proyecto y no un requisito académico textual). El aprovisionamiento y la destrucción son reproducibles y requieren confirmación humana. El teardown **no destruye datos persistentes ni clínicos** (MongoDB Atlas y almacenamiento de datos privados) salvo orden expresa y separada.
16. **Arquitectura de despliegue vigente** *(enmienda 1.2.0)*. Primer despliegue (v0.1.0, los endpoints existentes, sin funciones nuevas): Azure Container Apps en un ambiente *workload profiles* (perfil Consumption) con VNet propia; la API con **dos réplicas fijas** detrás del **ingress administrado** del ambiente, que hace de balanceador y termina HTTPS con certificado válido; Redis Auth y Redis Cache según §8.9; MongoDB Atlas con lista de acceso limitada a las IPs de salida de la API (nunca `0.0.0.0/0` como solución permanente); Azure Container Registry Basic con imágenes inmutables; secretos de Container Apps; Log Analytics con alertas; Terraform (ADR-004) con estado remoto fuera del grupo de la app; GitHub Actions para CI, CD y creación/eliminación de la infraestructura con aprobación humana. Detalle, costos y pasos: `docs/infrastructure/azure-deployment.md`; decisiones: `docs/decisions/ADR-003-azure-container-apps.md` y `ADR-004-terraform.md`; versiones: `docs/versioning/version-matrix.md`. **Modo ahorro (fase de pruebas, aprobado por el equipo el 2026-11-05):** con `modo_ahorro = true` la API escala a cero sin peticiones, se levanta con la primera (hasta dos réplicas con balanceo) y se apaga tras 15 minutos sin uso, y Redis Auth y Redis Cache solo operan de 7:00 a 23:00 (hora de México). Para cada revisión o evaluación se aplica `modo_ahorro = false` (dos réplicas fijas y Redis 24 h), que es la configuración que cumple este apartado. Blob Storage y CDN se incorporan con los Specs de imágenes (`pm-079`…`pm-082`).

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
- Cada criterio de aceptación se referencia como `pm-NNN#CA-NN` y está cubierto por evidencia o por una justificación explícita de no automatización.
- No establecer porcentajes arbitrarios de cobertura antes de medir el baseline. El umbral inicial se define en `specs/pm-056-test-coverage-baseline/plan.md` y se registra aquí mediante enmienda cuando haya evidencia.

## 11. Producto y alcance funcional

`docs/product-scope.md` es el resumen canónico de las dos fuentes de producto vigentes (documento v3 de requerimientos, historias y backlog, y maqueta HTML) y de las decisiones del usuario que las completan. Estas fuentes sustituyen al PDF descriptivo del 26 de octubre de 2026.

Los módulos incluyen cuenta y sesión, mascotas y QR, carnet de salud, mapa y afiliación de negocios, panel del afiliado, escaneo y atención, seguimiento y chat, avisos push y página legal. Estos módulos no se consideran implementados por aparecer en las fuentes; requieren specs propias, pequeñas y atómicas.

**Fuera de alcance** (decisión del usuario, 2026-11-09): cobro, IVA y ticket de la atención; verificación de correo o de celular; SMS; inteligencia artificial y aprendizaje automático; inventario, punto de venta, suscripciones y pagos, empleados, citas y familia compartida.

La primera entrega prioriza el flujo de autenticación y los siete endpoints funcionales (perfil y mascotas) más los cuatro de autenticación (11 en total) definidos en `specs/README.md` y `docs/api/endpoints.md`. El hito se considera alcanzado únicamente al cerrar la tarea global `T041` (Spec principal `pm-046`) con todos los gates y aprobaciones completados.

## 12. Gobernanza SDD y precedencia documental

### 12.1 Fuente de verdad por artefacto

| Artefacto | Responsabilidad única |
|---|---|
| `.specify/memory/constitution.md` | Reglas no negociables y decisiones normativas |
| `AGENTS.md` | Cómo deben operar los agentes y qué leer/ejecutar |
| `Memoria.md` | Estado actual, decisiones tomadas con evidencia y lecciones |
| `specs/README.md` | Registro de Specs, Tasks y dependencias; estado y orden de trabajo |
| `specs/traceability.md` | Vínculo requisitos académicos ↔ constitución ↔ Specs ↔ Tasks ↔ CA ↔ evidencia |
| `spec.md` | Qué comportamiento se requiere y cómo se acepta |
| `plan.md` | Cómo se implementará técnicamente |
| `task.md` | Pasos ejecutables y verificables |
| `checklists/gates.md` | Evidencia de calidad, seguridad y cierre |
| `docs/api/endpoints.md` | Catálogo vivo de contratos de API |
| `docs/security/asvs-l2-checklist.md` | Trazabilidad de controles de seguridad |

No duplicar tablas completas de endpoints, políticas, costos o reglas en varios artefactos. Enlazar el documento canónico.

### 12.2 Flujo obligatorio por funcionalidad

1. Leer `AGENTS.md`, esta constitución y el estado de `Memoria.md`.
2. Consultar `specs/README.md` y el grafo de dependencias (los números de Spec no determinan el orden de trabajo).
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

### 12.3 Identificadores, estructura y estado

> Redacción vigente desde la versión 1.1.0. La estructura anterior por slices (`00-foundation` … `04-first-milestone-closure`) se conserva en `specs/_historico/`.

1. **Estructura plana.** Cada Spec vive en `specs/pm-NNN-slug/` con `spec.md` y `task.md`. `plan.md` y `checklists/gates.md` se crean en la fase de plan del Spec, tras su aprobación, conforme al flujo del §12.2; no se crean archivos vacíos.
2. **Numeración.** `NNN` es global, secuencial e **inmutable**: no se renumera, no se reutiliza y no se elimina. Un Spec retirado pasa a estado `Reemplazado` y apunta a su sucesor. Los 84 Specs iniciales (`pm-001` a `pm-084`) quedan fijados por esta versión.
3. **Los números identifican, no ordenan.** El orden de trabajo lo gobiernan las dependencias declaradas en `specs/README.md` (por ejemplo, `pm-041` depende de `pm-047` y `pm-034` de `pm-036`).
4. **Atomicidad.** Cada Spec tiene un objetivo único, criterios de aceptación **propios** (sin duplicar los de otro Spec), al menos una Task y dependencias válidas sin ciclos.
5. **Tasks.** La numeración global continúa desde `T042`; `T001` a `T041` conservan su identificador y su estado. Cada Task tiene **exactamente un Spec principal**; si verifica criterios de otro Spec lo declara en «También cubre» de forma inequívoca.
6. **Tipos de Task.** *Histórica* (`T001`–`T041`; su alcance no se modifica retrospectivamente), *retrospectiva* (documenta trabajo ya implementado) y *nueva*.
7. **Evidencia retrospectiva.** Toda Task retrospectiva documenta exclusivamente lo demostrable con evidencia existente: código, pruebas, filas de gates o task, revisiones independientes existentes, clasificación de cada evidencia (automatizada, manual, revisión estática, documento histórico) y limitaciones. Nunca se cierra automáticamente y no se inventa evidencia.
8. **Referencias.** Los criterios de aceptación se citan como `pm-NNN#CA-NN`.
9. **Registro y matriz.** `specs/README.md` es el registro de Specs, Tasks y dependencias; `specs/traceability.md` vincula requisitos académicos, constitución, Specs, Tasks, CA y evidencia.
10. **Estados** de Spec y Task: `Propuesto`, `En revisión`, `Aprobado`, `En progreso`, `Bloqueado`, `Verificado`, `Cerrado`. Solo los Specs admiten además `Reemplazado`. `Verificado` requiere evidencia; `Cerrado` requiere aprobación humana. Ningún agente marca un Spec como aprobado, ni cierra una Task, ni se auto-firma.
11. **Documentos históricos.** Los artefactos de la estructura anterior se conservan en `specs/_historico/` con su contenido original y verificables mediante `specs/_historico/MANIFIESTO-SHA256.txt`.

## 13. Historial de cambios

### 2026-11-09 — Versión 1.3.0 (alcance y modelo de cuenta)

Propuesta por el equipo de agentes a partir de las fuentes de producto vigentes y **aprobada por el usuario en el chat de la sesión el 2026-11-09** (§14).

- **Motivo:** el documento v3 de requerimientos e historias y la maqueta, declarados fuente de verdad, definen que la cuenta es siempre de Usuario y que el negocio se le asocia al afiliarse; el modelo de roles excluyentes `Owner`/`Business` deja de ser válido. El PDF descriptivo del 26 de octubre queda sustituido como fuente de alcance.
- **§1 y §11 enmendados:** nuevas fuentes de producto, lista de módulos y alcance excluido (cobro, verificación de correo, SMS, IA/ML, inventario, punto de venta, suscripciones, empleados, citas y familia).
- **§6.1 sustituido:** rol único `User` con un negocio asociado opcional; sin aprobación manual de afiliación.
- **§6.5 y §7 ajustados:** mascotas para cualquier Usuario sobre sus propios recursos; el acceso de un negocio exige una autorización vigente del dueño.
- **Sin cambios:** la política de contraseña (12 a 128 caracteres, definida en `pm-029`) se mantiene por decisión del usuario, aunque el documento v3 proponga 8; la aplicación debe aplicar la misma política.
- **Impacto:** `docs/product-scope.md` reescrito; `specs/traceability.md` con la matriz de requisitos; nuevos Specs `pm-085` a `pm-104` (Tasks `T103` a `T122`); los Specs `pm-022`, `pm-033` y `pm-045` se enmiendan mediante `pm-086`, sin reescribir su historia.

### 2026-11-05 — Versión 1.2.0 (despliegue en Azure)

Propuesta por Adrián Adonai García Jiménez (responsable de nube y despliegue) con el acuerdo del equipo para el primer despliegue; se ratifica al aprobar y fusionar el PR correspondiente (§14).

- **§8.9 enmendado:** Redis Auth y Redis Cache pueden ejecutarse como contenedores autogestionados dentro del ambiente privado de Azure Container Apps (solo TCP interno, TLS con CA propia, autenticación, una réplica por servicio, persistencia de Redis Auth en Azure Files). Motivo: Azure Cache for Redis se retira el 30/09/2028 y dos instancias administradas con red privada exceden el crédito de Azure for Students. Se mantienen todas las propiedades exigidas (separación, TLS, autenticación, `noeviction` + persistencia, LRU, fail-closed).
- **§8.16 nuevo:** arquitectura de despliegue vigente para la versión 0.1.0 y enlaces a la guía de despliegue, ADR-003, ADR-004 y la matriz de versiones.
- **Impacto:** Specs `pm-070` y `pm-071` (su objetivo dice «administrado» y debe ajustarse al aprobarlos), `docs/infrastructure/docker-redis-atlas.md` (nota de TLS), nuevos `infra/`, `.github/workflows/`, `docs/decisions/ADR-003` y `ADR-004`, `docs/versioning/version-matrix.md`. Ningún requisito se reduce: se mantienen dos instancias, balanceador y HTTPS.

### 2026-11-05 — Versión 1.1.0

Aprobación expresa del usuario (instrucción de ejecución de la migración SDD). La versión anterior (1.0.1) se conserva en `specs/_historico/constitution-v1.0.1.md` y en el historial de Git.

- **§8 reescrito:** Microsoft Azure con la suscripción Azure for Students como plataforma exclusiva (sin AWS ni GCP, sin multi-cloud); objetivo de $0 MXN de desembolso directo, con distinción entre servicios incluidos, consumo cubierto por créditos y consumo con posibles cargos.
- **Derogada** la cláusula 8.5 de 1.0.1 (operación temporal con una sola réplica). Se mantienen obligatorios dos o más instancias detrás de un Load Balancer o equivalente, HTTPS, seguridad de red, Redis administrado con TLS y autenticación, secretos, almacenamiento de imágenes con CDN, registro de contenedores, CI y CD, versionado, observabilidad, IaC y ciclo de vida de infraestructura con protección de datos persistentes.
- **§12.3 reescrito:** estructura plana `specs/pm-NNN-slug/`, numeración global inmutable, atomicidad, Tasks desde T042, un Spec principal por Task, tipos de Task (histórica, retrospectiva, nueva), evidencia retrospectiva, referencias `pm-NNN#CA-NN`, estado `Reemplazado`, registro `specs/README.md` y matriz `specs/traceability.md`.
- Ajustes de referencias en §3, §4, §6, §9, §10, §11, §12.1 y §12.2. Los principios del §2 no cambian.
- **Impacto:** reorganización de `specs/` (84 Specs, T001–T097), plantillas, agentes y skills, y gate de integridad `scripts/check-sdd.ps1`. Los estados históricos de T001–T041 no se alteran (T001 permanece `Verificado`; T012 permanece implementada y abierta).
- Disposiciones transitorias: los slices `00` a `04` quedan archivados en `specs/_historico/`; los Specs migrados entran como `En revisión` porque su texto es nuevo y requiere aprobación humana; ningún Task retrospectivo se cierra automáticamente.

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
