output "server_name" {
  value = azurerm_mssql_server.this.name
}

output "database_name" {
  value = azurerm_mssql_database.this.name
}

output "failover_group_endpoint" {
  value = "${azurerm_mssql_failover_group.this.name}.database.windows.net"
}