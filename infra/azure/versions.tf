# Primer despliegue del backend en Azure for Students (pm-066, pm-067).
# El estado vive en una cuenta de almacenamiento fuera del grupo de la app (ver infra/bootstrap).
# La configuración del backend se pasa en `terraform init -backend-config=...`; aquí no hay secretos.

terraform {
  required_version = ">= 1.9.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 4.20.0, < 5.0.0"
    }
    random = {
      source  = "hashicorp/random"
      version = ">= 3.6.0, < 4.0.0"
    }
    tls = {
      source  = "hashicorp/tls"
      version = ">= 4.0.5, < 5.0.0"
    }
  }

  backend "azurerm" {
    key              = "petmate-backend/azure.tfstate"
    use_azuread_auth = true
  }
}

provider "azurerm" {
  features {
    resource_group {
      # El grupo de la app no lo gestiona Terraform (lo crea infra/bootstrap); nunca se borra desde aquí.
      prevent_deletion_if_contains_resources = true
    }
  }

  subscription_id     = var.subscription_id
  storage_use_azuread = true
}
