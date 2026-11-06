output "api_url" {
  description = "URL pública HTTPS de la API."
  value       = "https://${azurerm_container_app.api.ingress[0].fqdn}"
}

output "api_app_name" {
  value = azurerm_container_app.api.name
}

output "resource_group_name" {
  value = data.azurerm_resource_group.app.name
}

output "acr_login_server" {
  value = azurerm_container_registry.main.login_server
}

output "acr_name" {
  value = azurerm_container_registry.main.name
}

# Público por naturaleza: el pipeline lo hornea en la imagen para confiar en el TLS de Redis.
output "redis_ca_cert_pem" {
  value = tls_self_signed_cert.redis_ca.cert_pem
}

# IPs de salida de la API: infra/atlas las agrega a la lista de acceso de Atlas.
output "api_outbound_ip_addresses" {
  value = azurerm_container_app.api.outbound_ip_addresses
}

output "log_analytics_workspace_id" {
  value = azurerm_log_analytics_workspace.main.workspace_id
}
