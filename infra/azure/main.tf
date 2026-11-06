# Infraestructura del primer despliegue (alcance: los 7 endpoints actuales).
# Ver docs/infrastructure/azure-deployment.md y docs/decisions/ADR-003-azure-container-apps.md.

locals {
  tags = merge(var.tags, { version = var.api_version })
}

# El grupo de recursos lo crea infra/bootstrap; Terraform solo lo lee y nunca lo destruye.
data "azurerm_resource_group" "app" {
  name = var.resource_group_name
}

resource "random_string" "global" {
  length  = 5
  upper   = false
  special = false
}

# ---------------------------------------------------------------- observabilidad (pm-078)
resource "azurerm_log_analytics_workspace" "main" {
  name                = "log-petmate-${var.name_suffix}"
  location            = var.location
  resource_group_name = data.azurerm_resource_group.app.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  # Tope diario de ingesta para no salir del costo previsto (pm-065).
  daily_quota_gb = 0.15
  tags           = local.tags
}

# ---------------------------------------------------------------- red privada (pm-072)
resource "azurerm_virtual_network" "main" {
  name                = "vnet-petmate-${var.name_suffix}"
  location            = var.location
  resource_group_name = data.azurerm_resource_group.app.name
  address_space       = ["10.40.0.0/16"]
  tags                = local.tags
}

resource "azurerm_subnet" "aca" {
  name                 = "snet-aca"
  resource_group_name  = data.azurerm_resource_group.app.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.40.0.0/24"]
  service_endpoints    = ["Microsoft.Storage"]

  delegation {
    name = "aca"
    service_delegation {
      name    = "Microsoft.App/environments"
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
    }
  }
}

# ---------------------------------------------------------------- registro de imágenes (pm-074)
resource "azurerm_container_registry" "main" {
  name                = "acrpetmate${var.name_suffix}${random_string.global.result}"
  location            = var.location
  resource_group_name = data.azurerm_resource_group.app.name
  sku                 = "Basic"
  admin_enabled       = false
  tags                = local.tags
}

resource "azurerm_user_assigned_identity" "api" {
  name                = "id-petmate-api-${var.name_suffix}"
  location            = var.location
  resource_group_name = data.azurerm_resource_group.app.name
  tags                = local.tags
}

resource "azurerm_role_assignment" "api_acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.api.principal_id
}

# ---------------------------------------------------------------- persistencia de Redis Auth (AOF)
resource "azurerm_storage_account" "redis" {
  name                            = "stpetmate${var.name_suffix}${random_string.global.result}"
  location                        = var.location
  resource_group_name             = data.azurerm_resource_group.app.name
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  min_tls_version                 = "TLS1_2"
  allow_nested_items_to_be_public = false
  https_traffic_only_enabled      = true
  tags                            = local.tags

  # Solo la subred de Container Apps llega al share (constitución §8.8).
  network_rules {
    default_action             = "Deny"
    bypass                     = ["AzureServices"]
    virtual_network_subnet_ids = [azurerm_subnet.aca.id]
  }
}

resource "azurerm_storage_share" "redis_auth" {
  name               = "redis-auth-data"
  storage_account_id = azurerm_storage_account.redis.id
  quota              = 1
}

# ---------------------------------------------------------------- ambiente de Container Apps (pm-075, pm-076)
resource "azurerm_container_app_environment" "main" {
  name                       = "cae-petmate-${var.name_suffix}"
  location                   = var.location
  resource_group_name        = data.azurerm_resource_group.app.name
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
  infrastructure_subnet_id   = azurerm_subnet.aca.id
  # Ingress público para la API; los Redis solo tienen ingress interno.
  internal_load_balancer_enabled = false
  tags                           = local.tags

  workload_profile {
    name                  = "Consumption"
    workload_profile_type = "Consumption"
  }

  # Con VNet propia, Azure puede tardar más de 30 min en crear el ambiente (primer apply en mexicocentral).
  timeouts {
    create = "90m"
    update = "60m"
  }
}

resource "azurerm_container_app_environment_storage" "redis_auth" {
  name                         = "redis-auth-data"
  container_app_environment_id = azurerm_container_app_environment.main.id
  account_name                 = azurerm_storage_account.redis.name
  share_name                   = azurerm_storage_share.redis_auth.name
  access_key                   = azurerm_storage_account.redis.primary_access_key
  access_mode                  = "ReadWrite"
}

# ---------------------------------------------------------------- TLS interno de Redis (CA propia)
# La CA es pública por naturaleza (se hornea en la imagen de la API); las llaves privadas viven solo en
# el estado cifrado de Terraform y en secretos de Container Apps.
resource "tls_private_key" "redis_ca" {
  algorithm   = "ECDSA"
  ecdsa_curve = "P256"
}

resource "tls_self_signed_cert" "redis_ca" {
  private_key_pem       = tls_private_key.redis_ca.private_key_pem
  is_ca_certificate     = true
  validity_period_hours = 24 * 365
  allowed_uses          = ["cert_signing", "crl_signing"]

  subject {
    common_name  = "PetMate Redis CA"
    organization = "PetMate"
  }
}

locals {
  redis_services = {
    auth  = "redis-auth"
    cache = "redis-cache"
  }
}

resource "tls_private_key" "redis" {
  for_each    = local.redis_services
  algorithm   = "ECDSA"
  ecdsa_curve = "P256"
}

resource "tls_cert_request" "redis" {
  for_each        = local.redis_services
  private_key_pem = tls_private_key.redis[each.key].private_key_pem
  dns_names       = [each.value, "${each.value}.internal.${azurerm_container_app_environment.main.default_domain}"]

  subject {
    common_name  = each.value
    organization = "PetMate"
  }
}

resource "tls_locally_signed_cert" "redis" {
  for_each              = local.redis_services
  cert_request_pem      = tls_cert_request.redis[each.key].cert_request_pem
  ca_private_key_pem    = tls_private_key.redis_ca.private_key_pem
  ca_cert_pem           = tls_self_signed_cert.redis_ca.cert_pem
  validity_period_hours = 24 * 365
  allowed_uses          = ["digital_signature", "key_encipherment", "server_auth"]
}

resource "random_password" "redis" {
  for_each = local.redis_services
  length   = 48
  special  = false
}
