# Tres Container Apps en el mismo ambiente:
#   - redis-auth  : lista negra de JWT. TCP interno + TLS, noeviction, AOF en Azure Files (constitución §5.1).
#   - redis-cache : caché distribuida. TCP interno + TLS, allkeys-lru, sin persistencia (constitución §5.2).
#   - petmate-api : la API. Ingress HTTPS externo detrás del balanceador del ambiente; 2 réplicas fijas o,
#                   en modo ahorro (var.modo_ahorro), de 0 a 2 según peticiones.

locals {
  redis_config = {
    auth = {
      args = "--appendonly yes --appendfsync everysec --dir /data --maxmemory 64mb --maxmemory-policy noeviction"
      # El AOF vive en Azure Files (SMB) y en modo ahorro Redis se apaga por horario: si se corta a media escritura,
      # el AOF incremental queda con formato invalido y Redis no arranca ("Bad file format reading the append only
      # file", incidente del 2026-11-10). Antes de arrancar se repara: redis-check-aof --fix trunca hasta el ultimo
      # comando valido. Solo se pierden las escrituras a medias; la lista negra de JWT vive 15 minutos.
      # DIAGNOSTICO temporal 2026-11-27: se borra el AOF en vez de repararlo. redis-check-aof --fix no
      # arregla un manifiesto corrupto, solo trunca datos; si el manifiesto en si quedo invalido, Redis
      # se niega a arrancar igual. Lo que vive aqui son tokens revocados con 15 min de vida, asi que
      # borrarlos no tiene consecuencia real: lo peor es que una sesion ya cerrada aguante unos minutos.
      prestart = "rm -rf /data/appendonlydir && "
    }
    cache = {
      args     = "--save '' --appendonly no --maxmemory 128mb --maxmemory-policy allkeys-lru"
      prestart = ""
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

  # La imagen se sirve desde el ACR propio, no desde Docker Hub (ver el comentario en `image`), asi que hace
  # falta identidad para autenticarse. Se reusa la misma identidad administrada de la API: ya tiene el rol
  # AcrPull sobre este registro y no hay razon para crear otra.
  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.api.id]
  }

  registry {
    server   = azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.api.id
  }

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
    # Modo ahorro: encendido solo en el horario de var.redis_schedule (regla cron de KEDA). Redis Auth no pierde
    # las revocaciones al apagarse porque el AOF vive en Azure Files.
    min_replicas = var.modo_ahorro ? 0 : 1
    max_replicas = 1

    dynamic "custom_scale_rule" {
      for_each = var.modo_ahorro ? [1] : []
      content {
        name             = "horario"
        custom_rule_type = "cron"
        metadata = {
          timezone        = var.redis_schedule.timezone
          start           = var.redis_schedule.start
          end             = var.redis_schedule.end
          desiredReplicas = "1"
        }
      }
    }

    container {
      name   = each.value
      # Desde el ACR propio, NO desde Docker Hub. Comprobado el 2026-11-26: cualquier contenedor que tire de
      # docker.io en este entorno muere con CrashLoopBackOff, sin codigo de salida y sin una sola linea de log,
      # porque la descarga anonima se corta (limite de Docker Hub para las IP compartidas de Azure). Se probo
      # con un redis:7.4 desnudo, sin TLS ni contrasena ni comando propio, y fallo igual: no era configuracion
      # nuestra. La API nunca lo sufrio porque su imagen siempre vino de este registro.
      #
      # La imagen se copia con: az acr import -n <acr> --source docker.io/library/redis:7.4 --image redis:7.4
      image  = "${azurerm_container_registry.main.login_server}/redis:7.4"
      cpu    = 0.25
      memory = "0.5Gi"

      # La contraseña se escribe en /tmp para no aparecer en la línea de comandos (mismo patrón que Compose).
      command = ["sh", "-c"]
      args = [
        "umask 077 && printf 'requirepass %s\\n' \"$REDIS_PASSWORD\" > /tmp/redis.conf && printf '%s' \"$REDIS_TLS_CRT\" > /tmp/tls.crt && printf '%s' \"$REDIS_TLS_KEY\" > /tmp/tls.key && ${local.redis_config[each.key].prestart}exec redis-server /tmp/redis.conf --port 0 --tls-port 6379 --tls-cert-file /tmp/tls.crt --tls-key-file /tmp/tls.key --tls-auth-clients no ${local.redis_config[each.key].args}"
      ]

      env {
        name        = "REDIS_PASSWORD"
        secret_name = "redis-password"
      }

      # El certificado y la llave llegan por variable de entorno y el arranque los escribe a /tmp con umask 077
      # (solo root los puede leer), igual que ya se hacia con la contrasena. Es el unico camino que el provider
      # ofrece: un volumen de tipo Secret no deja decir que secretos monta.
      env {
        name        = "REDIS_TLS_CRT"
        secret_name = "tls-crt"
      }

      env {
        name        = "REDIS_TLS_KEY"
        secret_name = "tls-key"
      }

      volume_mounts {
        name = "tls"
        path = "/tls"
      }

      # El montaje de Azure Files se mantiene DESACTIVADO (2026-11-27). Es lo unico que distinguia a redis-auth
      # de redis-cache, y quitandolo arranco: montar el share SMB es lo que impedia que el contenedor naciera
      # (ni un codigo de salida ni una linea de log; se descarto antes que fuera el AOF corrupto, porque
      # borrarlo con rm -rf tampoco lo arreglaba).
      #
      # Que se pierde: la lista negra de JWT ya no sobrevive a un reinicio. Esos tokens viven 15 minutos, asi
      # que el peor caso es que una sesion ya cerrada aguante unos minutos mas tras un reinicio del contenedor.
      # A cambio, el servicio existe: con el volumen, redis-auth llevaba una semana caido y el limite de
      # intentos de login no corria en absoluto, que es mucho peor.
      #
      # Para volver a activarlo hay que entender primero por que falla el montaje SMB en este entorno.
      dynamic "volume_mounts" {
        for_each = []
        content {
          name = "data"
          path = "/data"
        }
      }

      # Sin tiempos explicitos Azure pone initial_delay = 1 s. Ahora el arranque escribe dos archivos de
      # certificado y levanta TLS antes de abrir el 6379, asi que un segundo no alcanza: la sonda lo declaraba
      # muerto y Azure lo mataba en bucle, antes incluso de que Redis imprimiera su primera linea de log.
      startup_probe {
        transport               = "TCP"
        port                    = 6379
        failure_count_threshold = 30
        interval_seconds        = 2
      }

      liveness_probe {
        transport               = "TCP"
        port                    = 6379
        initial_delay           = 15
        interval_seconds        = 10
        failure_count_threshold = 3
      }

      readiness_probe {
        transport = "TCP"
        port      = 6379
      }
    }

    # El volumen `tls` ya no se usa: se deja declarado solo porque el volume_mount sigue apuntandole y quitarlo
    # obliga a recrear el contenedor. El certificado ahora llega por variable de entorno (ver `args` arriba).
    #
    # POR QUE SE CAMBIO (2026-11-26): un volumen de tipo Secret en azurerm_container_app NO permite declarar
    # QUE secretos monta —el bloque `volume` solo acepta name, storage_type y mount_options—, asi que montaba
    # un directorio VACIO. Redis arrancaba, no encontraba /tls/tls-crt y se mataba: CrashLoopBackOff con
    # StartupInterruption, 269 veces. Lo peor es que no fallaba el apply ni el estado de la Container App, que
    # seguia diciendo "Running": los dos Redis estuvieron caidos una semana y solo lo delataba el health check
    # interno de la API. El 500 del mapa publico venia de aqui.
    volume {
      name         = "tls"
      storage_type = "EmptyDir"
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
    # Evaluación (modo_ahorro = false): 2 réplicas fijas, requisito académico y constitución §8.3.
    # Pruebas (modo_ahorro = true): 0 réplicas sin tráfico; la primera petición la levanta y el balanceador
    # reparte hasta 2 réplicas; se apaga tras var.api_cooldown_seconds sin peticiones.
    min_replicas               = var.modo_ahorro ? 0 : 2
    max_replicas               = 2
    cooldown_period_in_seconds = var.modo_ahorro ? var.api_cooldown_seconds : 300

    http_scale_rule {
      name                = "peticiones"
      concurrent_requests = "20"
    }

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
      # Imágenes en Blob (pm-079): solo el nombre de la cuenta; la API entra con su identidad administrada.
      env {
        name  = "IMAGES_STORAGE_ACCOUNT"
        value = azurerm_storage_account.images.name
      }
      env {
        name  = "IMAGES_PET_CONTAINER"
        value = azurerm_storage_container.pet_photos.name
      }
      env {
        name  = "IMAGES_BUSINESS_CONTAINER"
        value = azurerm_storage_container.business_photos.name
      }
      # DefaultAzureCredential necesita saber cuál de las identidades del contenedor debe usar.
      env {
        name  = "AZURE_CLIENT_ID"
        value = azurerm_user_assigned_identity.api.client_id
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

  depends_on = [
    azurerm_role_assignment.api_acr_pull,
    azurerm_container_app.redis,
    azurerm_role_assignment.api_images_contributor,
    azurerm_role_assignment.api_images_delegator,
  ]

  lifecycle {
    # La imagen la cambia el pipeline de CD (pm-077); Terraform no debe revertirla.
    #
    # CUIDADO (comprobado el 2026-11-19): este ignore_changes protege la imagen
    # cuando es el unico cambio, pero NO cuando Terraform tiene que recrear la
    # plantilla del contenedor por otra razon (agregar variables de entorno, por
    # ejemplo). En esa recreacion usa la imagen que tiene en su estado, que puede
    # ser mas vieja que la desplegada, y crea una revision con ella.
    #
    # Ese dia el apply que agrego las variables IMAGES_* revirtio la API de
    # 0.5.0-77760e1 a 0.3.0-239557a sin avisar.
    #
    # Antes de cualquier apply que toque la plantilla, comprobar que imagen esta
    # corriendo de verdad:
    #
    #   az containerapp show --name petmate-api --resource-group rg-petmate-prod \
    #     --query "properties.template.containers[0].image" -o tsv
    #
    # y despues del apply verificarla otra vez. Si cambio, restaurarla con:
    #
    #   az containerapp update --name petmate-api --resource-group rg-petmate-prod \
    #     --image acrpetmateprodd8u3u.azurecr.io/petmate-api:<version desplegada>
    ignore_changes = [template[0].container[0].image]
  }
}
