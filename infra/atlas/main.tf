# Acceso de red de MongoDB Atlas para la API desplegada (pm-069).
# Lee las IPs de salida desde el estado de infra/azure y las agrega a la lista de acceso del proyecto Atlas.
# No toca clústeres, bases ni usuarios: destruir este módulo solo quita las IPs, nunca datos (pm-083#CA-03).
# Credenciales: MONGODB_ATLAS_PUBLIC_KEY y MONGODB_ATLAS_PRIVATE_KEY como variables de entorno (secretos de GitHub).

terraform {
  required_version = ">= 1.9.0"

  required_providers {
    mongodbatlas = {
      source  = "mongodb/mongodbatlas"
      version = ">= 1.21.0"
    }
  }

  backend "azurerm" {
    key              = "petmate-backend/atlas.tfstate"
    use_azuread_auth = true
  }
}

provider "mongodbatlas" {}

variable "atlas_project_id" {
  description = "ID del proyecto de Atlas que contiene el clúster de PetMate."
  type        = string
}

variable "state_resource_group_name" {
  type = string
}

variable "state_storage_account_name" {
  type = string
}

variable "state_container_name" {
  type    = string
  default = "tfstate"
}

data "terraform_remote_state" "azure" {
  backend = "azurerm"
  config = {
    resource_group_name  = var.state_resource_group_name
    storage_account_name = var.state_storage_account_name
    container_name       = var.state_container_name
    key                  = "petmate-backend/azure.tfstate"
    use_azuread_auth     = true
  }
}

resource "mongodbatlas_project_ip_access_list" "api" {
  for_each   = toset(data.terraform_remote_state.azure.outputs.api_outbound_ip_addresses)
  project_id = var.atlas_project_id
  ip_address = each.value
  comment    = "PetMate API Azure Container Apps (Terraform)"
}

output "atlas_allowed_ips" {
  value = sort([for entry in mongodbatlas_project_ip_access_list.api : entry.ip_address])
}
