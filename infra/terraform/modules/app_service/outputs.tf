output "app_name" {
  value = azurerm_windows_web_app.this.name
}

output "default_hostname" {
  value = azurerm_windows_web_app.this.default_hostname
}

output "principal_id" {
  value = azurerm_windows_web_app.this.identity[0].principal_id
}