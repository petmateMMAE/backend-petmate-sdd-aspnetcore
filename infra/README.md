# Infraestructura de PetMate Backend (Terraform)

| Carpeta | Qué hace | Cuándo |
|---|---|---|
| `bootstrap/` | Script de Azure CLI: verifica Azure for Students, crea `rg-petmate-prod` (la app) y `rg-petmate-tfstate` (estado), la cuenta del estado y la identidad OIDC de GitHub Actions | Una sola vez |
| `azure/` | Todo lo de la app dentro de `rg-petmate-prod`: Log Analytics, VNet, ACR, ambiente de Container Apps, `redis-auth`, `redis-cache`, `petmate-api` (2 réplicas), alertas y presupuesto | `apply` / `destroy` desde *Actions → Infraestructura* |
| `atlas/` | Lista de acceso de MongoDB Atlas con las IPs de salida de la API | Después de `azure/`; cada 6 h por *Sincronizar IPs de Atlas* |

Guía completa, costos y pasos: [`docs/infrastructure/azure-deployment.md`](../docs/infrastructure/azure-deployment.md).

Reglas (constitución §8.6 y §8.15):

- No se ejecuta `apply` ni `destroy` sin autorización humana (environment `production` con revisores).
- Ningún secreto en archivos `.tf`/`.tfvars` versionados; llegan como `TF_VAR_*` desde secretos de GitHub.
- `destroy` no borra datos de Atlas, ni el grupo de recursos, ni el estado de Terraform.
- El primer `apply` crea la API con una imagen de arranque; la imagen real llega con el primer despliegue (`api-v0.1.0`). Mientras tanto la API aparece como no lista, es esperado.

Ejecución local (solo `plan`):

```powershell
az login
cd infra/azure
terraform init -backend-config="resource_group_name=rg-petmate-tfstate" -backend-config="storage_account_name=<cuenta>" -backend-config="container_name=tfstate"
$env:TF_VAR_mongodb_uri = '<uri>'; $env:TF_VAR_jwt_signing_key = '<clave>'; $env:TF_VAR_alert_email = '<correo>'; $env:TF_VAR_subscription_id = '<id>'
terraform plan
```
