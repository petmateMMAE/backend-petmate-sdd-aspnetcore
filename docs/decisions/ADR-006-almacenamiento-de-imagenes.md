# ADR-006 — Almacenamiento de imágenes de mascotas

- **Estado:** Propuesto (pendiente de aprobación humana)
- **Fecha:** 2026-11-08
- **Specs:** pm-063 (principal), pm-079, pm-080, pm-081
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

## Consecuencias
- El diseño debe limitar tamaño, tipo de contenido y cuota por propietario (revisión de seguridad pm-045).
- Mientras tanto, `photoUrl` se valida (HTTPS, sin credenciales) y nunca se resuelve en el servidor.
