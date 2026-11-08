# ADR-010 — Gestión de secretos

- **Estado:** Propuesto (pendiente de aprobación humana)
- **Fecha:** 2026-11-08
- **Specs:** pm-063 (principal), pm-068
- **Constitución:** §8.11
- **Costo:** clasificación y estimados en el [estudio de costos](../infrastructure/azure-cost-study.md) (pm-062). Ningún servicio se selecciona por haber tenido históricamente un nivel gratuito (pm-063 CA-04): cada decisión cita la clasificación vigente.

## Contexto
Los secretos (URI de Atlas, clave JWT, contraseñas y CA de Redis) nunca viven en el repositorio ni en la imagen; llegan por configuración segura del entorno.

## Decisión
**Secretos de Azure Container Apps**, cargados por Terraform desde secretos de GitHub (`TF_*`). Sin Azure Key Vault. Clasificación: **Incluido** (estudio, fila 12, a reconfirmar). El estado de Terraform contiene secretos generados y se protege con RBAC, cifrado y bloqueo.

## Alternativas dentro de Azure
| Alternativa | Clasificación y motivo de descarte |
|---|---|
| Azure Key Vault | Posible cargo por operaciones y complejidad adicional; pm-068 CA-04 solo lo permite si su modalidad no genera cargos |
| Variables de entorno en claro | Prohibido: expone secretos en la plantilla y en el estado sin protección |

## Consecuencias
- Rotación documentada en pm-068 (T080); `.env` nunca se despliega.
- Si en el futuro se usa Key Vault, debe confirmarse que su modalidad es $0 con el precio vigente.
