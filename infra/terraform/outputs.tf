output "app_url" {
  value = "https://${module.app_service.default_hostname}"
}