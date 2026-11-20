# Reglas de red de la subred de Container Apps (pm-072).
#
# Hasta hoy la subred no tenía grupo de seguridad: la salida estaba abierta a
# todo Internet (pm-072#CA-02). Estas reglas dejan solo lo que el backend
# necesita de verdad.
#
# Container Apps exige cierto tráfico de infraestructura para arrancar.
# Bloquearlo deja el ambiente inservible, así que las reglas de abajo lo
# permiten de forma explícita y dicen por qué, en vez de abrir todo por si acaso.

locals {
  aca_subnet_cidr = "10.40.0.0/24"
}

resource "azurerm_network_security_group" "aca" {
  name                = "nsg-snet-aca-${var.name_suffix}"
  location            = var.location
  resource_group_name = data.azurerm_resource_group.app.name
  tags                = local.tags
}

resource "azurerm_subnet_network_security_group_association" "aca" {
  subnet_id                 = azurerm_subnet.aca.id
  network_security_group_id = azurerm_network_security_group.aca.id
}

# ---------------------------------------------------------------------------
# Entrada
# ---------------------------------------------------------------------------

# El ingress del ambiente termina TLS y reparte entre las réplicas. El tráfico
# llega desde el balanceador de Azure, no directo de Internet: por eso el origen
# es AzureLoadBalancer y no "*".
resource "azurerm_network_security_rule" "entrada_balanceador" {
  name                        = "permitir-balanceador-azure"
  priority                    = 100
  direction                   = "Inbound"
  access                      = "Allow"
  protocol                    = "*"
  source_port_range           = "*"
  destination_port_range      = "*"
  source_address_prefix       = "AzureLoadBalancer"
  destination_address_prefix  = "*"
  resource_group_name         = data.azurerm_resource_group.app.name
  network_security_group_name = azurerm_network_security_group.aca.name
}

# Las réplicas de la API hablan con Redis dentro de la misma subred (6379 y
# 6380). Sin esta regla, denegar-resto-entrada las separaría.
resource "azurerm_network_security_rule" "entrada_interna" {
  name                        = "permitir-trafico-de-la-subred"
  priority                    = 110
  direction                   = "Inbound"
  access                      = "Allow"
  protocol                    = "*"
  source_port_range           = "*"
  destination_port_range      = "*"
  source_address_prefix       = local.aca_subnet_cidr
  destination_address_prefix  = local.aca_subnet_cidr
  resource_group_name         = data.azurerm_resource_group.app.name
  network_security_group_name = azurerm_network_security_group.aca.name
}

# Todo lo que no sea el balanceador o la propia subred se rechaza (pm-072#CA-01).
resource "azurerm_network_security_rule" "entrada_denegar_resto" {
  name                        = "denegar-resto-entrada"
  priority                    = 4096
  direction                   = "Inbound"
  access                      = "Deny"
  protocol                    = "*"
  source_port_range           = "*"
  destination_port_range      = "*"
  source_address_prefix       = "*"
  destination_address_prefix  = "*"
  resource_group_name         = data.azurerm_resource_group.app.name
  network_security_group_name = azurerm_network_security_group.aca.name
}

# ---------------------------------------------------------------------------
# Salida (pm-072#CA-02)
# ---------------------------------------------------------------------------

# MongoDB Atlas vive fuera de Azure y escucha en 27017 sobre TLS.
resource "azurerm_network_security_rule" "salida_atlas" {
  name                        = "permitir-mongodb-atlas"
  priority                    = 100
  direction                   = "Outbound"
  access                      = "Allow"
  protocol                    = "Tcp"
  source_port_range           = "*"
  destination_port_range      = "27017"
  source_address_prefix       = local.aca_subnet_cidr
  destination_address_prefix  = "Internet"
  resource_group_name         = data.azurerm_resource_group.app.name
  network_security_group_name = azurerm_network_security_group.aca.name
}

# Blob Storage (fotos), por el service endpoint de la subred. La etiqueta
# Storage cubre solo las direcciones de ese servicio, no Internet entera.
resource "azurerm_network_security_rule" "salida_storage" {
  name                        = "permitir-azure-storage"
  priority                    = 110
  direction                   = "Outbound"
  access                      = "Allow"
  protocol                    = "Tcp"
  source_port_range           = "*"
  destination_port_range      = "443"
  source_address_prefix       = local.aca_subnet_cidr
  destination_address_prefix  = "Storage"
  resource_group_name         = data.azurerm_resource_group.app.name
  network_security_group_name = azurerm_network_security_group.aca.name
}

# Container Apps necesita el plano de control de Azure: descargar la imagen del
# registro, publicar registros en Log Analytics y renovar los tokens de la
# identidad administrada. Sin esto el ambiente no arranca.
resource "azurerm_network_security_rule" "salida_azure_cloud" {
  name                        = "permitir-plano-de-control-de-azure"
  priority                    = 120
  direction                   = "Outbound"
  access                      = "Allow"
  protocol                    = "Tcp"
  source_port_range           = "*"
  destination_port_range      = "443"
  source_address_prefix       = local.aca_subnet_cidr
  destination_address_prefix  = "AzureCloud"
  resource_group_name         = data.azurerm_resource_group.app.name
  network_security_group_name = azurerm_network_security_group.aca.name
}

# Tráfico entre réplicas dentro de la subred (la API con Redis).
resource "azurerm_network_security_rule" "salida_interna" {
  name                        = "permitir-trafico-a-la-subred"
  priority                    = 130
  direction                   = "Outbound"
  access                      = "Allow"
  protocol                    = "*"
  source_port_range           = "*"
  destination_port_range      = "*"
  source_address_prefix       = local.aca_subnet_cidr
  destination_address_prefix  = local.aca_subnet_cidr
  resource_group_name         = data.azurerm_resource_group.app.name
  network_security_group_name = azurerm_network_security_group.aca.name
}

# DNS: sin resolución de nombres la API no encuentra Atlas ni el Blob.
resource "azurerm_network_security_rule" "salida_dns" {
  name                        = "permitir-dns"
  priority                    = 140
  direction                   = "Outbound"
  access                      = "Allow"
  protocol                    = "*"
  source_port_range           = "*"
  destination_port_range      = "53"
  source_address_prefix       = local.aca_subnet_cidr
  destination_address_prefix  = "*"
  resource_group_name         = data.azurerm_resource_group.app.name
  network_security_group_name = azurerm_network_security_group.aca.name
}

# Cualquier otra salida se rechaza: si mañana alguien agrega una dependencia
# nueva, falla aquí y queda registrado, en vez de salir sin que nadie lo note
# (pm-072#CA-02).
resource "azurerm_network_security_rule" "salida_denegar_resto" {
  name                        = "denegar-resto-salida"
  priority                    = 4096
  direction                   = "Outbound"
  access                      = "Deny"
  protocol                    = "*"
  source_port_range           = "*"
  destination_port_range      = "*"
  source_address_prefix       = "*"
  destination_address_prefix  = "*"
  resource_group_name         = data.azurerm_resource_group.app.name
  network_security_group_name = azurerm_network_security_group.aca.name
}
