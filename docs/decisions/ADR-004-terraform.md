# ADR-004 — Terraform como herramienta de infraestructura como código

- **Estado:** Propuesto (pendiente de aprobación humana en el PR)
- **Fecha:** 2026-11-05
- **Specs:** pm-066 (principal), pm-067, pm-083
- **Constitución:** §8.15

## Decisión

Se usa **Terraform** (licencia BUSL 1.1, uso gratuito para este proyecto) con los proveedores `azurerm`, `tls`, `random` y `mongodbatlas`.

| Tema | Decisión |
|---|---|
| Estructura | `infra/bootstrap` (script único con Azure CLI: grupos de recursos, estado, identidad OIDC), `infra/azure` (todo lo de la app), `infra/atlas` (lista de acceso de Atlas) |
| Estado remoto (pm-067) | Blob `tfstate` en una cuenta de almacenamiento del grupo `rg-petmate-tfstate`: cifrado, versionado, sin acceso público, sin llaves compartidas (solo Entra ID), bloqueo por *lease* del backend `azurerm` |
| Secretos | Nunca en `.tf` ni `.tfvars` versionados; llegan como `TF_VAR_*` desde secretos de GitHub. El estado contiene secretos generados (contraseñas de Redis, llaves TLS), por eso su acceso se limita por RBAC |
| plan / apply | Separados (pm-066#CA-02): `infra.yml` con `plan`, `apply` o `destroy`, solo manual y con aprobación en el environment `production` |
| Destrucción (pm-083) | `destroy` exige escribir `destruir-petmate`; quita primero las IPs de Atlas y luego Azure. No borra datos de Atlas, el grupo de recursos (lo creó bootstrap) ni el estado |
| Autenticación de GitHub | OIDC federado (sin secretos de cliente) con roles *Contributor* y *Role Based Access Control Administrator* sobre el grupo de la app y *Storage Blob Data Contributor* sobre el estado |

## Alternativas

- **Bicep**: nativo de Azure, pero no gestiona Atlas ni es la herramienta recomendada en los lineamientos.
- **Azure CLI en scripts**: no es declarativo ni reproducible para crear y destruir.
