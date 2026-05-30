variable "location" {
  default = "uksouth"
}

variable "resource_group_name" {}

variable "app_name" {}

variable "sql_server_name" {
  description = "Base name for the primary SQL logical server. The failover group is named <name>-fog and the secondary server <name>-secondary. Must be globally unique."
  type        = string
}

variable "aad_admin_name" {
  description = "Display name of the AAD admin for SQL Server"
}

variable "aad_admin_object_id" {
  description = "Object ID of the AAD admin for SQL Server"
}

variable "jwt_secret" {
  description = "HMAC-SHA256 key used to sign JWTs. Must be at least 32 bytes. Provide via TF_VAR_jwt_secret or a non-committed tfvars file."
  type        = string
  sensitive   = true
}

variable "jwt_issuer" {
  description = "JWT iss claim. Must match the value in appsettings.json."
  type        = string
  default     = "OnlineStore.Api"
}

variable "jwt_audience" {
  description = "JWT aud claim."
  type        = string
  default     = "OnlineStore.Client"
}

variable "jwt_access_token_minutes" {
  description = "Token lifetime in minutes."
  type        = number
  default     = 60
}