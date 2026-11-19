# ADR-006 — Almacenamiento de imágenes de mascotas y negocios

- **Estado:** Aceptado (aprobación humana de Miguel, 2026-11-11), enmendado el 2026-11-11 para registrar el contenedor público de negocios (pm-111#CA-04)
- **Fecha:** 2026-11-08
- **Specs:** pm-063 (principal), pm-079, pm-080, pm-081, pm-111
- **Constitución:** §8
- **Costo:** clasificación y estimados en el [estudio de costos](../infrastructure/azure-cost-study.md) (pm-062). Ningún servicio se selecciona por haber tenido históricamente un nivel gratuito (pm-063 CA-04): cada decisión cita la clasificación vigente.

## Contexto
Las fotos de mascotas hoy se guardan como URL HTTPS (el servidor nunca las descarga). La carga de archivos llega con pm-079 a pm-082 y exige almacenamiento privado, validación y acceso controlado.

## Decisión
**Azure Blob Storage** privado (sin acceso anónimo, acceso por identidad administrada y URLs temporales), cuando se implementen los endpoints de imágenes. Clasificación: **Posible cargo — diferido** (estudio, fila 7): no se crea ningún recurso hasta que el spec esté aprobado y se confirme el precio vigente y el crédito restante.

## Alternativas dentro de Azure
| Alternativa | Clasificación y motivo de descarte |
|---|---|
| Azure Files | Pensado para compartir sistemas de archivos; sin URLs temporales ni integración con CDN |
| Disco o volumen del contenedor | No es persistente ni compartido entre réplicas (rompe el requisito de API sin estado) |
| Azure Data Lake Storage | Orientado a analítica; sin ventaja para imágenes de usuarios |

## Enmienda del 2026-11-11 — contenedor público para las fotos de negocios (pm-111)

La decisión original cubría solo fotos privadas. La foto de un negocio tiene que verse en el mapa **sin cuenta**, así que firmar cada lectura no sirve: serían miles de firmas para algo que es público a propósito.

- **Una sola cuenta de almacenamiento, dos contenedores.** `pet-photos` privado y `business-photos` con acceso público de solo lectura al blob (nunca al listado del contenedor). Separar por contenedor y no por cuenta cumple pm-111#CA-01 sin crear un segundo recurso que cobrar.
- **Cuenta aparte de la de Redis.** La de Redis niega todo lo que no venga de la subred de Container Apps (§8.8) y así se queda: abrirla para servir fotos sería debilitar algo que hoy está cerrado.
- **Sin llave de cuenta.** `shared_access_key_enabled = false`: la API entra con su identidad administrada y firma las URLs temporales con una clave de delegación, que caduca sola. No hay secreto que rotar ni que se pueda filtrar.
- **El CDN sigue fuera** (pm-082, ADR-007): las fotos públicas se sirven directo del Blob.

### Costo

Clasificación: **posible cargo, bajo**. Blob Storage cobra por almacenamiento, operaciones y salida de datos; no hay cuota fija. Con el tope de 2 MB por mascota y 1 MB por negocio, y el tamaño de la demostración, el volumen es de megabytes.

El precio vigente se confirma en la [calculadora de Azure](https://azure.microsoft.com/pricing/details/storage/blobs/) **antes del `apply`**, que ejecuta una persona autorizada (`AGENTS.md` §4, Constitución §8.6): el agente no crea recursos. Hasta que ese `apply` corra, la fila 7 del [estudio de costos](../infrastructure/azure-cost-study.md) sigue como «diferido» y la API arranca con el almacenamiento deshabilitado, que es un estado válido.

## Consecuencias
- El diseño debe limitar tamaño, tipo de contenido y cuota por propietario (revisión de seguridad pm-045).
- `photoUrl` escrita a mano se sigue validando (HTTPS, sin credenciales) y nunca se resuelve en el servidor; convive con las fotos subidas, que mandan sobre ella.
- Sin configuración de almacenamiento, los endpoints de foto responden 503 y el resto de la API funciona igual: ningún entorno queda obligado a tener Blob.
