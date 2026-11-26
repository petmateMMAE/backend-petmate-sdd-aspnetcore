# Auditoría QA — Backend PetMate

> Respuestas al documento **Prompts de evaluación v2 (18/nov/2026)**, sección 1 (Backend).
> Medido el 2026-11-26 contra `main` (`bd40ae4`) y contra el entorno de producción.
> Cada dato proviene del repositorio o de una corrida de CI verificable, no de estimación.

**Corrida de CI de referencia:** [`44659661590`](https://github.com/petmateMMAE/backend-petmate-sdd-aspnetcore/actions/runs/44659661590) (exitosa, 89 jobs).

---

## 1. Nube e infraestructura

| Pregunta | Respuesta |
|---|---|
| Nombre de la nube | **Microsoft Azure**, región Chile Central (`rg-petmate-prod`) |
| ¿Se despliega en la nube? | **Sí.** Azure Container Apps, revisión `Single`, despliegue automático con cada push a `main` |
| Servicios externos fuera de la nube principal | **Sí, tres:**<br>• **MongoDB Atlas** — base de datos (clúster compartido `petmate.nwhianv.mongodb.net`)<br>• **Firebase Cloud Messaging** — notificaciones push<br>• **Resend** — correo transaccional (verificación, recuperación de contraseña) |
| Instancias de despliegue | **2 réplicas fijas** (`minReplicas = 2`, `maxReplicas = 2`). El mínimo de 2 es deliberado: con una sola instancia no hay balanceo real ni tolerancia a fallo |
| ¿Load Balancer? | **Sí.** Ingress de Container Apps. Verificado en vivo: 40 peticiones etiquetadas se repartieron **20/20** entre las dos réplicas (`pm-076#CA-02`) |
| ¿IaC? | **Sí, Terraform** (`infra/azure/`), providers `azurerm`, `tls`, `random`. Estado remoto en `stpetmatetf7m3d91ki`. Protecciones `prevent_destroy` sobre la cuenta de imágenes |
| ¿SSL/TLS en la URL? | **Sí.** HTTPS obligatorio, certificado gestionado por Azure. `UseHsts()` activo fuera de desarrollo (365 días) |
| Arquitectura | **Monolito modular con API REST.** Capas Api / Application / Domain / Infrastructure (arquitectura limpia). No microservicios: para el tamaño del proyecto, un monolito con capas bien separadas es más simple de operar y desplegar |
| ¿CDN para imágenes? | **No.** Diferido por costo y documentado en `ADR-007-cdn.md`. Las imágenes se sirven desde Azure Blob Storage: contenedor público de solo lectura para fotos de negocios (se ven en el mapa sin cuenta) y contenedor **privado** con URL firmada de 15 min para fotos de mascotas |
| ¿Observabilidad? | **Sí.** Azure Log Analytics Workspace + Azure Monitor con alertas métricas (`azurerm_monitor_metric_alert`) y grupo de acción para notificación. Incluye alerta de severidad 1 si la API cae por debajo de 2 réplicas |

---

## 2. Seguridad y DevSecOps

### Controles implementados

| Control | Herramienta | Etapa |
|---|---|---|
| Gestión de secretos | **Gitleaks** | CI (job `secrets-scan`) |
| SAST | **CodeQL** + compilación con `-warnaserror` | CI (job `sast`) |
| Análisis de contenedor | **Trivy** (imagen) | CI (job `docker-build`) |
| Análisis de IaC | **Trivy** + **tfsec** | CI (job `iac-scan`) |
| SBOM | **Syft** | CI |
| DAST | **OWASP ZAP** (`zaproxy`) | CD, contra el despliegue real (job `dast`) |
| Integridad de especificaciones | `scripts/check-sdd.ps1` | CI (job `sdd-integrity`) |
| Formato / estilo | `dotnet format` | CI |
| Prueba de humo | 29 verificaciones sobre 14 endpoints por HTTPS | CD (job `smoke-test`) |
| Puerta de calidad | Bloquea el despliegue si cualquier etapa bloqueante falla | CI (job `quality-gate`) |

### Controles en la aplicación

| Control | Implementación |
|---|---|
| Validación de entradas | Validador por caso de uso (`IRequestValidator`), obligatorio antes de ejecutar |
| Autenticación | JWT con firma HS256; clave en Azure Key Vault, nunca en el repositorio |
| Revocación de sesión | `RedisTokenRevocationStore` — el logout invalida de inmediato, sin esperar la expiración |
| Rate limiting | 15 archivos; por endpoint y por identidad |
| Limitación de intentos de login | `RedisAuthAttemptThrottle` — frena fuerza bruta |
| Segmentación de red | NSG con 11 reglas, salida cerrada por defecto (`deny` prioridad 4096). Solo Atlas:27017, Storage:443, AzureCloud:443, DNS y la propia subred |
| Identidad administrada | La API entra a Storage sin llaves compartidas (`shared_access_key_enabled = false`) |
| Protección de datos personales | Los registros nunca incluyen mensajes de error de base de datos ni identificadores de usuario sin pseudonimizar (`SecurityEventPseudonym`) |

### Porcentaje de seguridad

**Estimación: 85–90 %** respecto a las prácticas DevSecOps esperables en un proyecto de este alcance.

Ese número es un juicio, no una medición: no existe una escala única y objetiva. Lo sostiene que las cuatro etapas del ciclo (secretos, SAST, IaC, DAST) están automatizadas y **bloquean** el despliegue, no solo informan. Lo que resta para el 100 % se detalla abajo.

### Cobertura OWASP Top 10 (2021)

| Riesgo | Estado |
|---|---|
| A01 Control de acceso roto | **Cubierto** — autorización por propietario en cada consulta; pruebas de contrato por endpoint |
| A02 Fallas criptográficas | **Cubierto** — TLS obligatorio, HSTS, contraseñas con hash, secretos en Key Vault |
| A03 Inyección | **Cubierto** — el texto de búsqueda se escapa (`Regex.Escape`) y se trata como dato, nunca como expresión |
| A04 Diseño inseguro | **Cubierto** — 168 especificaciones con criterios de aceptación verificables |
| A05 Mala configuración | **Cubierto** — Trivy + tfsec sobre la IaC; NSG con salida cerrada |
| A06 Componentes vulnerables | **Cubierto** — Trivy sobre la imagen, SBOM con Syft |
| A07 Fallas de identificación | **Cubierto** — JWT, revocación en Redis, limitación de intentos |
| A08 Fallas de integridad | **Cubierto** — etiqueta de imagen inmutable en ACR (`writeEnabled: false`) **y firma con cosign** por digest, con OIDC de GitHub (sin llaves). La firma se verifica en el mismo job contra la identidad esperada |
| A09 Fallas de registro | **Cubierto** — Log Analytics, alertas, identificadores de correlación |
| A10 SSRF | **No aplica** — la API no hace peticiones a URLs provistas por el usuario |

### Lo que falta

| Faltante | Impacto | Por qué |
|---|---|---|
| Private Link a Atlas | La regla de salida a 27017 va a Internet | El clúster compartido de Atlas no lo ofrece en el plan actual |
| CDN delante del Blob | El contenedor de fotos de negocios es público | Diferido por costo (ADR-007) |
| Redis gestionado | Redis corre en contenedor | Azure Cache for Redis excede el presupuesto; el contenedor usa TLS y contraseña |

### ¿Está al nivel de aplicaciones bancarias o fintech?

**No, y conviene ser claro en esto.**

Lo que sí comparte con ese nivel: pipeline con cuatro controles bloqueantes, secretos fuera del repositorio, identidad administrada, red cerrada por defecto, revocación inmediata de sesión y reversa automática ensayada contra producción.

Lo que lo separa:

- **Sin certificación** (PCI-DSS, SOC 2, ISO 27001), que es lo que define ese nivel en la práctica
- **Sin cifrado a nivel de campo** para datos sensibles
- **Sin HSM** para las llaves de firma
- **Sin WAF** delante de la API
- **Sin segundo factor** de autenticación
- **Sin auditoría externa** de penetración
- Base de datos en **clúster compartido**, no dedicado
- **Sin aprobación humana** obligatoria antes de desplegar a producción (ver más abajo: es configuración pendiente, no limitación del plan)

Para una aplicación de gestión de mascotas el nivel es sólido y está por encima de lo habitual en un proyecto académico. Equipararlo a banca sería inexacto.

### Vulnerabilidades críticas

**No hay vulnerabilidades críticas abiertas.** El job `iac-scan` pasa en verde y bloquearía cualquier hallazgo nuevo con severidad HIGH o CRITICAL.

Existen **4 excepciones documentadas** en `.trivyignore.yaml`, todas con motivo, forma de cierre y fecha de caducidad (2027-02-12), revisadas y aprobadas:

| Regla | Qué silencia | Por qué |
|---|---|---|
| `AVD-AZU-0007` | Contenedor `business-photos` público | La foto del negocio se ve en el mapa público, que funciona sin cuenta. Solo contiene fotos de locales, ningún dato personal |
| `AVD-AZU-0012` | La cuenta de imágenes no niega tráfico de red | Los teléfonos leen las fotos públicas directo del Blob |
| `AVD-AZU-0051` | Salida a Atlas con destino Internet | Clúster compartido con IPs variables; acotado a puerto 27017, solo desde la subred, y Atlas solo admite las IPs de salida de esta API |
| `AVD-AZU-0051` | Regla `denegar-resto-salida` | **Falso positivo:** Trivy no distingue `access = Deny` de `Allow`. Es la regla que cierra la red; quitarla la abriría |

**Un defecto abierto, no de seguridad:** `GET /api/v1/businesses/get-nearby` devuelve HTTP 500 en producción. La prueba de humo lo detecta y la reversa automática funciona correctamente, por lo que el servicio sigue sano en la versión anterior. Diagnóstico en la sección 6.

---

## 3. Endpoints y trazabilidad

| Pregunta | Respuesta |
|---|---|
| Número de endpoints | **76** (atributos `[HttpGet/Post/Put/Delete]` en 14 controladores) |
| Endpoints sin especificación | **Ninguno.** Las 168 especificaciones `pm-NNN` cubren el total. El job `sdd-integrity` valida la cadena spec → plan → task → código → prueba y **falla** si algo queda huérfano |
| ¿Versionamiento? | **Sí, en los dos niveles:**<br>• API: prefijo `/api/v1/` en las 76 rutas<br>• Código: versión semántica en la etiqueta de imagen (`0.5.0-<sha>`), inmutable en ACR |

### Endpoints con cobertura de pruebas

**Los 74 endpoints de negocio tienen un job de contrato dedicado en CI** (`4 · Endpoint · <nombre>`), más un job transversal de wrapper, OpenAPI y autorización. Ninguno queda sin prueba de contrato.

**Lo que sí falta, y es la brecha real de pruebas:**

| Tipo de prueba | Situación |
|---|---|
| Unitarias | Cubren casos de uso y dominio, no endpoints uno a uno (es lo correcto: la unidad es el caso de uso) |
| Seguridad | 206 pruebas, concentradas en autenticación, autorización, límites y manejo de secretos |
| Integración | **Se ejecutan las 81.** Antes 53 quedaban omitidas porque la lista de acceso de Atlas no admite las IP de los runners de GitHub; ahora MongoDB corre en contenedor efímero del runner, igual que Redis |

Esas 53 pruebas ejercitan los repositorios de MongoDB y los adaptadores de Redis. Mientras quedaban omitidas, **la capa de persistencia se desplegaba sin red de protección en el pipeline**, y es exactamente ahí donde se originó el defecto de `get-nearby`: un fallo en la creación de un índice que ninguna prueba de CI podía detectar.

Se corrigió levantando MongoDB en un contenedor efímero del runner. Se ejercitan el mismo driver, los mismos repositorios y los mismos índices; lo único que cambia es dónde vive el servidor. Nada de lo que se verifica depende de funciones exclusivas de Atlas: no hay transacciones, ni Atlas Search, ni change streams. Si hay secretos de Atlas y Atlas responde desde el runner, se sigue usando Atlas; el contenedor es el respaldo que garantiza que las pruebas **siempre** se ejecuten.

---

## 4. Pruebas

| Suite | Pasadas | Omitidas |
|---|---|---|
| Unitarias | **348** | 0 |
| Seguridad | **206** | 0 |
| Contrato | **253** | 0 |
| Integración | **28** | **53** |
| **Total ejecutado** | **835** | 53 |

> Estos números son los de la corrida de referencia, **anterior** al cambio que levanta MongoDB en contenedor. Con ese cambio las 53 omitidas pasan a ejecutarse, de modo que el total sube a **888** y la columna de omitidas queda en 0. La cifra exacta se confirma en la primera corrida después de fusionar; se deja aquí el dato medido y no el esperado.

### Otras pruebas realizadas

| Prueba | Qué verifica |
|---|---|
| **Contrato por endpoint** | 74 jobs, uno por endpoint: forma de la respuesta, códigos de estado, autorización |
| **DAST (OWASP ZAP)** | Escaneo dinámico contra el despliegue real, no contra una copia |
| **Prueba de humo** | 29 verificaciones por HTTPS tras cada despliegue; dispara la reversa si falla |
| **Ensayo de reversa** | Ejecutado contra producción real el 2026-11-19, en ventana acordada y con los 12 testers avisados. Se desplegó a propósito una versión mala, la prueba de humo la detectó y la reversa recuperó el servicio |
| **Prueba de balanceo** | 40 peticiones etiquetadas, atribuidas por réplica: 20/20 |
| **Integridad SDD** | Valida la trazabilidad completa de las 168 especificaciones |
| **Plan de consulta** | `QueryPlanIntegrationTests` verifica que las consultas usen índice |
| **Cobertura** | 93.7 % de líneas con las cuatro suites; umbral aprobado ≥ 90 % |

**Hallazgo del ensayo de reversa que vale destacar:** con la versión mala, `health/live` **nunca dejó de responder 200**. Un chequeo de salud por sí solo habría aprobado un despliegue roto. Por eso la prueba de humo verifica endpoints de negocio y no solo el pulso de la aplicación.

---

## 5. Datos, caché y rendimiento

| Pregunta | Respuesta |
|---|---|
| Bases de datos | **MongoDB Atlas**. **Sí, NoSQL** (documental) |
| Optimización de la base | Índices creados en el arranque por `MongoIndexInitializer`: únicos (correo, hash de token, QR), compuestos por propietario, **TTL** para expiración automática de tokens y autorizaciones, e **índice geoespacial `2dsphere`** para la búsqueda por cercanía. `QueryPlanIntegrationTests` verifica que las consultas los usen |
| ¿Redis? | **Sí**, imagen `redis:7.4` en contenedor, con TLS (`--port 0 --tls-port 6379`, puerto plano desactivado), contraseña y sin ingress externo |
| Dónde se aplica caché | **Tres lugares:** perfil público de negocio y búsqueda por cercanía (`PublicBusinessCache`, con invalidación al editar vía `BusinessCacheInvalidation`); TTL centralizado en `CacheTtl`; almacén en `RedisCacheStore` |
| ¿JWT con Redis? | **Sí.** `RedisTokenRevocationStore` mantiene la lista de tokens revocados: el logout surte efecto de inmediato, sin esperar a que el token expire |
| ¿Logout implementado? | **Sí.** `POST /api/v1/auth/logout`, con 8 pruebas de contrato |

### Elementos de rendimiento solicitados

| Elemento | Estado |
|---|---|
| Caching | **Sí** — Redis, 3 puntos de aplicación |
| Rate limiting | **Sí** — 15 archivos, por endpoint e identidad |
| Middleware | **Sí** — tubería de excepciones, correlación y registro |
| Pagination | **Sí** — `PagedResult` / `PagedResponse`, 18 archivos |
| Indexing | **Sí** — únicos, compuestos, TTL y geoespacial |
| JSON serializers | **Sí** — `System.Text.Json` con opciones explícitas |
| Payload compression | **Sí** — `ResponseCompression` |

**Siete de siete.**

---

## 6. Herramientas de desarrollo asistido

| Pregunta | Respuesta |
|---|---|
| Agentes de IA | **5**, declarados en `AGENTS.md` §7 (`.opencode/agents/`): `foundation-builder`, `feature-builder`, `security-reviewer`, `traceability-reviewer`, `documentation-keeper`. Los agentes de revisión son de solo lectura: reportan sin corregir, para que el hallazgo pase por una persona |
| Skills | **7**, en `.opencode/skills/`: `00-constitution`, `01-foundation`, `02-spec-first`, `03-authentication`, `04-mongodb-redis`, `05-testing`, `06-devsecops-azure`. Cada una encapsula las reglas de un área para que el agente no las reinvente en cada tarea |
| MCP | **Ninguno en el repositorio.** No existe `.mcp.json` ni dependencia de servidores MCP: el backend no necesitó acceso a sistemas externos durante el desarrollo. Las herramientas usadas fueron la CLI de Azure, `gh`, Terraform y `dotnet` |
| ¿Archivo de memoria? | **Sí, `Memoria.md`** en la raíz. Acompañado de `FUENTES_Y_DECISIONES.md`, `constitution.md` (reglas duras del proyecto) y 8 ADR en `docs/decisions/` |

---

## 7. Defecto abierto: `get-nearby` en HTTP 500

Se documenta aquí por transparencia: es el único fallo funcional conocido en producción.

**Síntoma.** `GET /api/v1/businesses/get-nearby` devuelve 500. La prueba de humo lo detecta y la reversa automática se ejecuta, por lo que producción sigue sirviendo la versión anterior sana.

**Cadena de causa.**

1. La consulta usa `$geoNear` ([`MongoBusinesses.cs:256`](../../src/PetMate.Infrastructure/Persistence/MongoBusinesses.cs)), que **exige** un índice `2dsphere`; sin él Mongo falla siempre.
2. Ese índice **se creaba** junto al índice único de `ownerId` en un solo `CreateManyAsync` ([`MongoIndexInitializer.cs`](../../src/PetMate.Infrastructure/Persistence/MongoIndexInitializer.cs)). **La operación es atómica:** si el único falla —por ejemplo, porque la colección tiene dos negocios del mismo dueño— el geoespacial tampoco se crea.
3. El error quedaba invisible por dos capas de protección: el servicio de arranque registraba solo `exception.GetType().Name`, nunca el mensaje, y `FindNearby` convierte toda `MongoException` en un 500 genérico.

**Lección.** El diseño que protege los secretos en los registros es el mismo que ocultó la causa. Un dato duplicado en una colección dejaba sin funcionar la pantalla principal de la app, y el registro no daba ninguna pista de por qué.

**Arreglo aplicado.**

| Cambio | Efecto |
|---|---|
| Los dos índices se crean en operaciones separadas, el geoespacial primero | El fallo del único ya no arrastra al geoespacial: el mapa sigue sirviendo |
| El registro incluye el mensaje de Mongo, no solo el tipo de excepción | Un índice que no se crea deja de ser indistinguible de una caída de red |
| Tres pruebas de integración nuevas (`BusinessIndexIntegrationTests`) | Una inserta negocios duplicados a propósito y comprueba que el geoespacial **sí** queda creado |

Estas pruebas solo tienen valor porque ahora las de integración se ejecutan en CI: antes habrían quedado omitidas, igual que las que no detectaron el defecto original.

**Pendiente de confirmación.** Falta verificar en el panel de Atlas si `ix_businesses_location_2dsphere` existe en la colección `businesses` de producción. El arreglo evita que el problema se repita, pero **si el índice ya falta en la base, hay que crearlo ahí**: eso no lo resuelve el código. Si la causa son negocios duplicados, hay que limpiarlos primero.

---

## Resumen

| Dimensión | Estado |
|---|---|
| Especificaciones cerradas | **168 / 168** |
| Endpoints | **76**, todos con especificación y prueba de contrato |
| Pruebas ejecutándose en CI | **835** |
| Cobertura de líneas | **93.7 %** (umbral aprobado ≥ 90 %) |
| Jobs del pipeline | **89**, con puerta de calidad bloqueante |
| Vulnerabilidades críticas abiertas | **0** (4 excepciones documentadas con caducidad) |
| Elementos de rendimiento | **7 / 7** |
| OWASP Top 10 | **10 de 10 cubiertos** (A08 cerrado con firma cosign) |

**Lo que un evaluador debe conocer:**

1. **Sin aprobación humana antes de fusionar a `main`.** Es la brecha abierta más relevante y se detalla en la sección 8. No es limitación del plan: es configuración pendiente.
2. **`get-nearby` en 500.** Diagnóstico completo en la sección 7, con el arreglo de código ya aplicado. Queda por confirmar el estado del índice en la base de producción.
3. **Nivel de seguridad sólido, pero no bancario.** La sección 2 explica qué lo separa de ese nivel y por qué equipararlo sería inexacto.

**Lo que se cerró a partir de esta auditoría:**

| Brecha detectada | Cómo se cerró |
|---|---|
| 53 pruebas de integración omitidas en CI | MongoDB en contenedor efímero del runner |
| A08 de OWASP parcial (sin firma de imagen) | Firma y verificación con cosign por digest, sin llaves |
| `get-nearby` en 500 por índice no creado | Índices separados en dos operaciones; el geoespacial ya no depende del único |

---

## 8. Brecha abierta: aprobación humana

Se documenta por separado porque **es la única brecha de proceso que queda**, y porque la primera versión de esta auditoría la describió mal.

**Lo que se dijo antes, y era inexacto:** que la falta de aprobación obligatoria era una limitación del plan gratuito de GitHub, y que el control existente era «protección de rama más revisión por PR».

**Lo que se verificó después:**

```
$ gh api repos/.../rulesets/<id>
{"nombre":"proteger-main","aplicacion":"active",
 "reglas":["deletion","non_fast_forward","required_status_checks","pull_request"],
 "revisiones":[0]}
```

El ruleset `proteger-main` **sí existe y está activo**: impide borrar la rama, impide reescribir historia y exige que los checks pasen. Pero `required_approving_review_count` está en **0**, así que exige abrir un PR sin exigir que alguien lo apruebe. En la práctica, quien abre el PR lo fusiona solo.

Que esto importa no es teórico: en el repositorio de la app móvil, un PR de 6,464 líneas se fusionó con cero revisiones e introdujo un fallo de configuración que dejó el CI en rojo. Una aprobación obligatoria lo habría detenido.

**Además**, el repositorio es público y pertenece a una organización, así que los *environments* con revisores requeridos **están disponibles**. La afirmación de que el plan no los permitía era incorrecta.

**Cómo se cierra:** subir `required_approving_review_count` a 1 en el ruleset. Es configuración del repositorio, no código, y requiere decisión del responsable porque cambia cómo trabaja el equipo. Un *environment* con revisor añadiría una segunda puerta antes del despliegue a Azure.
