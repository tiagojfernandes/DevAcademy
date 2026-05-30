output "app_url" {
  value = "https://${module.app_service.default_hostname}"
}

output "webapp_name" {
  value = module.app_service.app_name
}

output "resource_group_name" {
  value = module.rg.name
}

output "sql_server_fqdn" {
  value = "${var.sql_server_name}.database.windows.net"
}

output "sql_database_name" {
  value = module.sql.database_name
}