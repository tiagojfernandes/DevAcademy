# ARM requires a SQL admin login/password to be set on server creation even when
# azuread_authentication_only = true. We never use these, so generate them here
# and throw the password away rather than asking the operator for it.
resource "random_password" "sql_admin" {
  length  = 32
  special = true
}

resource "azurerm_mssql_server" "this" {
  name                         = var.server_name
  resource_group_name          = var.resource_group_name
  location                     = var.location
  version                      = "12.0"
  administrator_login          = "sqladminuser"
  administrator_login_password = random_password.sql_admin.result

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

resource "azurerm_mssql_firewall_rule" "allow_azure" {
  name             = "AllowAzure"
  server_id        = azurerm_mssql_server.this.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

