# Estrategia de entornos (pm-064 / T076)

> Define los entornos, qué cambia entre ellos y las convenciones de nombres y etiquetas de recursos Azure. `EnvironmentStrategyTests` (SecurityTests) comprueba por código las partes verificables de este documento.

## 1. Entornos y diferencias (CA-01)

| Aspecto | Desarrollo | Pruebas | Demo (Azure) |
|---|---|---|---|
| Propósito | Trabajo local con `docker compose` | Suites de integración y CI | Despliegue público de evaluación |
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Development` (pruebas) | `Production` |
| Dónde corre | Equipo local | Contenedor del SDK o runner de GitHub | Azure Container Apps (`rg-petmate-prod`) |
| Base de Atlas | `MONGODB_DATABASE` (`petmate_dev`) | `MONGODB_TEST_DATABASE` (`petmate_test`) | `petmate_prod` |
| Credencial de Atlas | `MONGODB_URI` del `.env` local | `MONGODB_TEST_URI` aparte | Secreto `TF_MONGODB_URI` con `readWrite` solo en `petmate_prod` |
| Redis | Dos contenedores de Compose, texto plano en red interna | Redis de Compose o temporales | Dos Container Apps con TLS y CA propia |
| `.env` | Se lee | Se lee (local) / variables de CI | **Nunca** (Production no lo carga) |
| OpenAPI y Scalar | Habilitados | Habilitados | Deshabilitados |
| Secretos | `.env` ignorado por Git | Secretos de GitHub (`MONGODB_TEST_*`) | Secretos de Container Apps desde GitHub vía Terraform |
| Datos | Descartables | Colecciones con prefijo único que se borran al terminar | Persistentes (Atlas conserva los datos al destruir Azure) |
| Costo | $0 | $0 | Crédito de Azure for Students (ver `azure-cost-study.md`) |

El entorno Azure se nombra con el sufijo **`prod`** en recursos y etiquetas (heredado del primer despliegue) y funciona como **demo** del proyecto: no hay un cuarto entorno de producción real. Si algún día se separan, el sufijo y la base se cambian por variable (`name_suffix`, `mongodb_database`) sin tocar el código.

## 2. Una base de datos de Atlas distinta por entorno (CA-02)

- Desarrollo, pruebas y demo usan bases con nombres distintos (`petmate_dev`, `petmate_test`, `petmate_prod`).
- **Las pruebas no pueden escribir en la base de desarrollo:** `MongoTestGuard` exige que `MONGODB_TEST_DATABASE` contenga «test» y sea distinta de `MONGODB_DATABASE`, y nunca usa la URI de desarrollo (`AuthPersistenceIntegrationTests.Guard_rejects_unsafe_test_database_names`).
- El usuario de Atlas de la demo tiene privilegios solo sobre `petmate_prod` y la lista de acceso se limita a las IPs de salida de la API (pm-069), sin `0.0.0.0/0`.
- Los valores de ejemplo de `.env.example` (`petmate_dev_ficticio` y `petmate_test_ficticio`) son marcadores; los reales los define cada persona.

## 3. Convenciones de nombres de recursos Azure (CA-03)

Patrón general: `<tipo>-petmate-<sufijo>`, en minúsculas; el sufijo sale de `name_suffix` (2 a 8 caracteres, minúsculas o números). Para nombres globales (ACR y cuentas de almacenamiento, que no admiten guiones) se concatena `<tipo>petmate<sufijo><aleatorio>`.

| Recurso | Patrón | Ejemplo con sufijo `prod` |
|---|---|---|
| Grupo de recursos de la app | `rg-petmate-<sufijo>` | `rg-petmate-prod` |
| Grupo del estado de IaC | `rg-petmate-tfstate` | `rg-petmate-tfstate` |
| Red virtual | `vnet-petmate-<sufijo>` | `vnet-petmate-prod` |
| Ambiente de Container Apps | `cae-petmate-<sufijo>` | `cae-petmate-prod` |
| Espacio de Log Analytics | `log-petmate-<sufijo>` | `log-petmate-prod` |
| Identidad administrada | `id-petmate-api-<sufijo>` | `id-petmate-api-prod` |
| Registro de contenedores (global) | `acrpetmate<sufijo><aleatorio>` | `acrpetmateprod…` |
| Cuenta de almacenamiento (global) | `stpetmate<sufijo><aleatorio>` | `stpetmateprod…` |
| Aplicaciones | `petmate-api`, `redis-auth`, `redis-cache` | — |

### Etiquetas obligatorias (seguimiento de costo, pm-065)

| Etiqueta | Valor | Uso |
|---|---|---|
| `proyecto` | `petmate` | Agrupar el costo del proyecto |
| `componente` | `backend` | Separar componentes cuando haya más de uno |
| `entorno` | `prod` (demo) | Separar costos por entorno |
| `costo` | `azure-for-students` | Indicar el origen del presupuesto |
| `version` | versión SemVer desplegada (la añade Terraform) | Trazabilidad con la imagen y la etiqueta Git |

Un recurso nuevo debe usar el patrón de nombre y `local.tags`; añadir un entorno nuevo implica cambiar `name_suffix` y `entorno`, nunca reutilizar los de otro entorno.
