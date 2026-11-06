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
