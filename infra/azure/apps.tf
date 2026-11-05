# Tres Container Apps en el mismo ambiente:
#   - redis-auth  : lista negra de JWT. TCP interno + TLS, noeviction, AOF en Azure Files (constitución §5.1).
#   - redis-cache : caché distribuida. TCP interno + TLS, allkeys-lru, sin persistencia (constitución §5.2).
#   - petmate-api : la API. Ingress HTTPS externo con 2 réplicas fijas detrás del balanceador del ambiente.

locals {
  redis_config = {
    auth = {
      args = "--appendonly yes --appendfsync everysec --dir /data --maxmemory 64mb --maxmemory-policy noeviction"
    }
    cache = {
      args = "--save '' --appendonly no --maxmemory 128mb --maxmemory-policy allkeys-lru"
    }
  }

  # StackExchange.Redis valida el certificado contra la CA (horneada en la imagen) y el nombre sslHost.
  redis_connection = {
    for key, name in local.redis_services :
    key => "${name}:6379,password=${random_password.redis[key].result},ssl=True,sslHost=${name},abortConnect=False"
  }

  api_fqdn = "petmate-api.${azurerm_container_app_environment.main.default_domain}"
}

resource "azurerm_container_app" "redis" {
  for_each                     = local.redis_services
  name                         = each.value
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = data.azurerm_resource_group.app.name
  revision_mode                = "Single"
  workload_profile_name        = "Consumption"
  tags                         = local.tags

  secret {
    name  = "redis-password"
    value = random_password.redis[each.key].result
  }

  secret {
    name  = "tls-crt"
    value = tls_locally_signed_cert.redis[each.key].cert_pem
  }

  secret {
    name  = "tls-key"
    value = tls_private_key.redis[each.key].private_key_pem
  }

  ingress {
    external_enabled = false
    transport        = "tcp"
    target_port      = 6379
    exposed_port     = 6379

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  template {
    # Redis es estado compartido: una sola réplica por servicio (las 2 instancias son de la API).
    min_replicas = 1
    max_replicas = 1

    container {
      name   = each.value
      image  = "docker.io/library/redis:7.4"
      cpu    = 0.25
      memory = "0.5Gi"

      # La contraseña se escribe en /tmp para no aparecer en la línea de comandos (mismo patrón que Compose).
      command = ["sh", "-c"]
      args = [
        "umask 077 && printf 'requirepass %s\\n' \"$REDIS_PASSWORD\" > /tmp/redis.conf && exec redis-server /tmp/redis.conf --port 0 --tls-port 6379 --tls-cert-file /tls/tls-crt --tls-key-file /tls/tls-key --tls-auth-clients no ${local.redis_config[each.key].args}"
      ]

      env {
        name        = "REDIS_PASSWORD"
        secret_name = "redis-password"
      }

      volume_mounts {
        name = "tls"
        path = "/tls"
      }

      dynamic "volume_mounts" {
        for_each = each.key == "auth" ? [1] : []
        content {
          name = "data"
          path = "/data"
        }
      }

      liveness_probe {
        transport = "TCP"
        port      = 6379
      }

      readiness_probe {
        transport = "TCP"
        port      = 6379
      }
    }

    volume {
      name         = "tls"
      storage_type = "Secret"
    }

    dynamic "volume" {
      for_each = each.key == "auth" ? [1] : []
      content {
        name         = "data"
        storage_type = "AzureFile"
        storage_name = azurerm_container_app_environment_storage.redis_auth.name
      }
    }
  }
}

resource "azurerm_container_app" "api" {
  name                         = "petmate-api"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = data.azurerm_resource_group.app.name
  revision_mode                = "Single"
  workload_profile_name        = "Consumption"
  tags                         = local.tags

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.api.id]
  }

  registry {
    server   = azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.api.id
  }

  secret {
    name  = "mongodb-uri"
    value = var.mongodb_uri
  }

  secret {
    name  = "jwt-signing-key"
    value = var.jwt_signing_key
  }

  secret {
    name  = "redis-auth-connection"
    value = local.redis_connection.auth
  }

  secret {
    name  = "redis-cache-connection"
    value = local.redis_connection.cache
  }

  # Balanceador + HTTPS: el ingress del ambiente termina TLS con certificado válido de *.azurecontainerapps.io,
  # reparte entre las réplicas y redirige HTTP a HTTPS (allow_insecure_connections = false).
  ingress {
    external_enabled           = true
    transport                  = "http"
    target_port                = 8080
    allow_insecure_connections = false

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  template {
    # Requisito académico y constitución §8.3: al menos dos instancias, sin escalar a cero.
    min_replicas = 2
    max_replicas = 2

    container {
      name   = "api"
      image  = var.api_image
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }
      env {
        name  = "PETMATE_VERSION"
        value = var.api_version
      }
      env {
        name        = "MONGODB_URI"
        secret_name = "mongodb-uri"
      }
      env {
        name  = "MONGODB_DATABASE"
        value = var.mongodb_database
      }
      env {
        name        = "REDIS_AUTH_CONNECTION"
        secret_name = "redis-auth-connection"
      }
      env {
        name        = "REDIS_CACHE_CONNECTION"
        secret_name = "redis-cache-connection"
      }
      env {
        name  = "JWT_ISSUER"
        value = "https://${local.api_fqdn}"
      }
      env {
        name  = "JWT_AUDIENCE"
        value = var.jwt_audience
      }
      env {
        name        = "JWT_SIGNING_KEY"
        secret_name = "jwt-signing-key"
      }
      dynamic "env" {
        for_each = var.trusted_proxies == "" ? [] : [var.trusted_proxies]
        content {
          name  = "TRUSTED_PROXIES"
          value = env.value
        }
      }

      liveness_probe {
        transport = "HTTP"
        port      = 8080
        path      = "/api/v1/health/live"
      }

      readiness_probe {
        transport = "HTTP"
        port      = 8080
        path      = "/api/v1/health/ready"
      }
    }
  }

  depends_on = [azurerm_role_assignment.api_acr_pull, azurerm_container_app.redis]

  lifecycle {
    # La imagen la cambia el pipeline de CD (pm-077); Terraform no debe revertirla.
    ignore_changes = [template[0].container[0].image]
  }
}
