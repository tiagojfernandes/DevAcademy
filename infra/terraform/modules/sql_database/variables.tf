variable "server_name" {}
variable "db_name" {}
variable "location" {}
variable "failover_location" {}
variable "resource_group_name" {}
variable "admin_login" {}
variable "admin_password" {
  sensitive = true
}
variable "aad_admin_name" {}
variable "aad_admin_object_id" {}