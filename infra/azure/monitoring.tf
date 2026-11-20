# Alertas mínimas (pm-065 y pm-078). Las alertas por correo y el presupuesto no tienen costo.

resource "azurerm_monitor_action_group" "email" {
  name                = "ag-petmate-${var.name_suffix}"
  resource_group_name = data.azurerm_resource_group.app.name
  short_name          = "petmate"
  tags                = local.tags

  email_receiver {
    name          = "proyecto"
    email_address = var.alert_email
  }
}

# Errores 5xx de la API en 5 minutos.
resource "azurerm_monitor_metric_alert" "api_5xx" {
  name                = "alert-petmate-api-5xx"
  resource_group_name = data.azurerm_resource_group.app.name
  scopes              = [azurerm_container_app.api.id]
  description         = "La API devolvió errores 5xx."
  severity            = 2
  frequency           = "PT5M"
  window_size         = "PT5M"
  tags                = local.tags

  criteria {
    metric_namespace = "Microsoft.App/containerApps"
    metric_name      = "Requests"
    aggregation      = "Total"
    operator         = "GreaterThan"
    threshold        = 5

    dimension {
      name     = "statusCodeCategory"
      operator = "Include"
      values   = ["5xx"]
    }
  }

  action {
    action_group_id = azurerm_monitor_action_group.email.id
  }
}

# Indisponibilidad de la API (pm-078#CA-02).
#
# La alerta de 5xx solo se dispara si la API responde y responde mal. Si se cae
# del todo deja de haber peticiones, y entonces nadie se entera: sin esta alerta
# el servicio puede estar caído horas sin aviso.
#
# Se mide sobre las réplicas en ejecución, no sobre el tráfico: con
# modo_ahorro = false el mínimo son 2, así que menos de 2 durante 15 minutos
# significa que algo no está levantando.
#
# La ventana de 15 minutos evita falsos positivos durante un despliegue, cuando
# una revisión nueva reemplaza a la anterior y el conteo baja un momento.
resource "azurerm_monitor_metric_alert" "api_sin_replicas" {
  count               = var.modo_ahorro ? 0 : 1
  name                = "alert-petmate-api-sin-replicas"
  resource_group_name = data.azurerm_resource_group.app.name
  scopes              = [azurerm_container_app.api.id]
  description         = "La API tiene menos réplicas de las esperadas: puede estar caída."
  severity            = 1
  frequency           = "PT5M"
  window_size         = "PT15M"
  tags                = local.tags

  criteria {
    metric_namespace = "Microsoft.App/containerApps"
    metric_name      = "Replicas"
    aggregation      = "Average"
    operator         = "LessThan"
    threshold        = 2
  }

  action {
    action_group_id = azurerm_monitor_action_group.email.id
  }
}

# Presupuesto del grupo de recursos: avisa al 50, 80 y 100 % (pm-065).
resource "azurerm_consumption_budget_resource_group" "app" {
  count             = var.enable_budget ? 1 : 0
  name              = "budget-petmate-${var.name_suffix}"
  resource_group_id = data.azurerm_resource_group.app.id
  amount            = var.monthly_budget_usd
  time_grain        = "Monthly"

  time_period {
    start_date = var.budget_start_date
  }

  dynamic "notification" {
    for_each = [50, 80, 100]
    content {
      enabled        = true
      threshold      = notification.value
      operator       = "GreaterThanOrEqualTo"
      threshold_type = "Actual"
      contact_emails = [var.alert_email]
    }
  }

  lifecycle {
    ignore_changes = [time_period]
  }
}
