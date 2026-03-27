variable "plan_name" {}
variable "app_name" {}
variable "location" {}
variable "resource_group_name" {}
variable "sku" {}

variable "app_settings" {
  type = map(string)
}