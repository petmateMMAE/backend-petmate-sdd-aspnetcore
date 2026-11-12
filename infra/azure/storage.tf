# Almacenamiento de imágenes (pm-079, pm-080, pm-081, pm-111).
#
# Cuenta aparte de la de Redis: aquella niega todo el tráfico que no venga de la subred de Container Apps, y las
# fotos de los negocios tienen que poder leerse desde cualquier teléfono. Separar las dos cuentas deja a cada una
# con la red que le toca, en vez de abrir la que hoy está cerrada.
#
# Dos contenedores con responsabilidades distintas (pm-079#CA-01, pm-111#CA-01):
#   - pet-photos:      privado. Solo se lee con una URL temporal firmada (pm-081).
#   - business-photos: público de solo lectura. La foto del local sale en el mapa y en el perfil.
#
# El CDN (pm-082) no está aprobado: las imágenes públicas se sirven directo del Blob, como previó ADR-007.
resource "azurerm_storage_account" "images" {
  name                       = "stpetmateimg${var.name_suffix}${random_string.global.result}"
  location                   = var.location
  resource_group_name        = data.azurerm_resource_group.app.name
  account_tier               = "Standard"
  account_replication_type   = "LRS"
  min_tls_version            = "TLS1_2"
  https_traffic_only_enabled = true
  # Necesario para que el contenedor de negocios pueda ser público; el de mascotas sigue privado.
  allow_nested_items_to_be_public = true
  shared_access_key_enabled       = false
  tags                            = local.tags

  blob_properties {
    # Una foto borrada se puede recuperar durante una semana si alguien se equivoca.
    delete_retention_policy {
      days = 7
    }
  }
}

resource "azurerm_storage_container" "pet_photos" {
  name                  = "pet-photos"
  storage_account_id    = azurerm_storage_account.images.id
  container_access_type = "private"
}

resource "azurerm_storage_container" "business_photos" {
  name                  = "business-photos"
  storage_account_id    = azurerm_storage_account.images.id
  container_access_type = "blob"
}

# La API entra con su identidad administrada: no hay llave de cuenta que guardar en ningún lado
# (shared_access_key_enabled = false).
resource "azurerm_role_assignment" "api_images_contributor" {
  scope                = azurerm_storage_account.images.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.api.principal_id
}

# Para firmar las URLs temporales de las fotos privadas con una clave de delegación (pm-081).
resource "azurerm_role_assignment" "api_images_delegator" {
  scope                = azurerm_storage_account.images.id
  role_definition_name = "Storage Blob Delegator"
  principal_id         = azurerm_user_assigned_identity.api.principal_id
}
