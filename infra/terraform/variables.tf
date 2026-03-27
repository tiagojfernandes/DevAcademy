variable "location" {
  default = "westeurope"
}

variable "resource_group_name" {}

variable "app_name" {}

variable "sql_admin" {}

variable "sql_password" {
  sensitive = true
}

variable "aad_admin_name" {
  description = "Display name of the AAD admin for SQL Server"
}

variable "aad_admin_object_id" {
  description = "Object ID of the AAD admin for SQL Server"
}