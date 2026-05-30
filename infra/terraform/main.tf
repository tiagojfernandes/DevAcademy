module "rg" {
  source   = "./modules/resource_group"
  name     = var.resource_group_name
  location = var.location
}

module "law" {
  source              = "./modules/log_analytics"
  name                = "${var.app_name}-law"
  location            = module.rg.location
  resource_group_name = module.rg.name
}

module "appi" {
  source              = "./modules/app_insights"
  name                = "${var.app_name}-appi"
  location            = module.rg.location
  resource_group_name = module.rg.name
  workspace_id        = module.law.id
}

module "app_service" {
  source              = "./modules/app_service"
  app_name            = var.app_name
  plan_name           = "${var.app_name}-plan"
  location            = module.rg.location
  resource_group_name = module.rg.name
  sku                 = "S1"

  app_settings = {
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = module.appi.connection_string
    "APPLICATIONINSIGHTS_ENABLE_AGENT"      = "false"

    # .NET maps nested config keys (Jwt:Secret) to env vars with '__' separators (Jwt__Secret).
    "ConnectionStrings__DefaultConnection" = "Server=tcp:${var.sql_server_name}.database.windows.net,1433;Database=OnlineStoreDb;Authentication=Active Directory Managed Identity;Encrypt=True;TrustServerCertificate=False;"
    "Jwt__Secret"                          = var.jwt_secret
    "Jwt__Issuer"                          = var.jwt_issuer
    "Jwt__Audience"                        = var.jwt_audience
    "Jwt__AccessTokenMinutes"              = tostring(var.jwt_access_token_minutes)
  }
}

module "sql" {
  source               = "./modules/sql_database"
  server_name          = var.sql_server_name
  db_name              = "OnlineStoreDb"
  location             = module.rg.location
  resource_group_name  = module.rg.name
  aad_admin_name       = var.aad_admin_name
  aad_admin_object_id  = var.aad_admin_object_id
}