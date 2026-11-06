variable "subscription_id" {
  description = "Suscripción Azure for Students."
  type        = string
}

variable "resource_group_name" {
  description = "Grupo de recursos de la app, creado previamente por infra/bootstrap."
  type        = string
  default     = "rg-petmate-prod"
}

variable "location" {
  description = "Región. Debe estar permitida por la política de Azure for Students (verificar con infra/bootstrap)."
  type        = string
  default     = "chilecentral"
}

variable "name_suffix" {
  description = "Sufijo corto para nombres globales (ACR, storage). Solo minúsculas y números."
  type        = string
  default     = "prod"

  validation {
    condition     = can(regex("^[a-z0-9]{2,8}$", var.name_suffix))
    error_message = "name_suffix: 2 a 8 caracteres, minúsculas o números."
  }
}

variable "api_version" {
  description = "Versión SemVer desplegada (docs/versioning/version-matrix.md)."
  type        = string
  default     = "0.1.0"
}

variable "api_image" {
  description = "Imagen inicial de la API. El pipeline de CD la reemplaza; Terraform no revierte el cambio."
  type        = string
  default     = "mcr.microsoft.com/k8se/quickstart:latest"
}

variable "mongodb_uri" {
  description = "URI de MongoDB Atlas del usuario de producción (solo readWrite sobre la base de producción)."
  type        = string
  sensitive   = true
}

variable "mongodb_database" {
  description = "Base de datos de producción en Atlas."
  type        = string
  default     = "petmate_prod"
}

variable "jwt_signing_key" {
  description = "Clave HS256 aleatoria de al menos 32 bytes (openssl rand -base64 48). Nunca una plantilla."
  type        = string
  sensitive   = true

  validation {
    condition     = length(var.jwt_signing_key) >= 44
    error_message = "jwt_signing_key debe ser una clave aleatoria larga (>= 32 bytes en base64)."
  }
}

variable "jwt_audience" {
  description = "Audiencia del JWT."
  type        = string
  default     = "petmate-api"
}

variable "trusted_proxies" {
  description = "IPs del ingress de Container Apps separadas por coma. Vacío hasta medirlas en el primer despliegue."
  type        = string
  default     = ""
}

variable "alert_email" {
  description = "Correo del proyecto para alertas de errores y de presupuesto."
  type        = string
}

variable "monthly_budget_usd" {
  description = "Presupuesto mensual del grupo de recursos en USD (alertas al 50, 80 y 100 %)."
  type        = number
  default     = 40
}

variable "budget_start_date" {
  description = "Primer día del mes en que empieza el presupuesto (formato AAAA-MM-01T00:00:00Z)."
  type        = string
  default     = "2026-11-01T00:00:00Z"
}

variable "enable_budget" {
  description = "Crea el presupuesto de costos. Desactivar solo si la oferta de la suscripción no lo admite."
  type        = bool
  default     = true
}

variable "tags" {
  description = "Etiquetas de costo (pm-065)."
  type        = map(string)
  default = {
    proyecto   = "petmate"
    componente = "backend"
    entorno    = "prod"
    costo      = "azure-for-students"
  }
}

# Modo ahorro (aprobado por el equipo para la fase de pruebas, constitución §8.16):
#   true  -> la API escala a 0 sin peticiones, se levanta sola con la primera petición (máx. 2 réplicas
#            con balanceo) y se apaga tras api_cooldown_seconds; Redis solo de 7:00 a 23:00 (hora de México).
#   false -> configuración de evaluación: 2 réplicas fijas de la API y Redis las 24 h.
variable "modo_ahorro" {
  description = "true: escala a cero y Redis con horario (pruebas). false: 2 réplicas fijas y Redis 24 h (evaluación)."
  type        = bool
  default     = true
}

variable "api_cooldown_seconds" {
  description = "Segundos sin peticiones antes de apagar la API en modo ahorro."
  type        = number
  default     = 900
}

variable "redis_schedule" {
  description = "Horario en que Redis está encendido en modo ahorro (cron de inicio y fin)."
  type = object({
    timezone = string
    start    = string
    end      = string
  })
  default = {
    timezone = "America/Mexico_City"
    start    = "0 7 * * *"
    end      = "0 23 * * *"
  }
}
