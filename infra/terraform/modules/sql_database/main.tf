resource "azurerm_mssql_server" "this" {
  name                         = var.server_name
  resource_group_name          = var.resource_group_name
  location                     = var.location
  version                      = "12.0"
  administrator_login          = var.admin_login
  administrator_login_password = var.admin_password

  azuread_administrator {
    login_username              = var.aad_admin_name
    object_id                   = var.aad_admin_object_id
    azuread_authentication_only = true
  }
}

resource "azurerm_mssql_server" "secondary" {
  name                         = "${var.server_name}-secondary"
  resource_group_name          = var.resource_group_name
  location                     = var.failover_location
  version                      = "12.0"
  administrator_login          = var.admin_login
  administrator_login_password = var.admin_password

  azuread_administrator {
    login_username              = var.aad_admin_name
    object_id                   = var.aad_admin_object_id
    azuread_authentication_only = true
  }
}

resource "azurerm_mssql_database" "this" {
  name      = var.db_name
  server_id = azurerm_mssql_server.this.id
  sku_name  = "S0"
}

resource "azurerm_mssql_failover_group" "this" {
  name      = "${var.server_name}-fog"
  server_id = azurerm_mssql_server.this.id

  partner_server {
    id = azurerm_mssql_server.secondary.id
  }

  databases = [azurerm_mssql_database.this.id]

  read_write_endpoint_failover_policy {
    mode          = "Automatic"
    grace_minutes = 60
  }
}

resource "azurerm_mssql_firewall_rule" "allow_azure" {
  name             = "AllowAzure"
  server_id        = azurerm_mssql_server.this.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_mssql_firewall_rule" "allow_azure_secondary" {
  name             = "AllowAzure"
  server_id        = azurerm_mssql_server.secondary.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

