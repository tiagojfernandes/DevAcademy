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
  sku                 = "B1"

  app_settings = {
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = module.appi.connection_string
    "SQL_CONNECTION_STRING"                 = "Server=tcp:${var.app_name}-sql.database.windows.net,1433;Database=${var.app_name}-db;Authentication=Active Directory Managed Identity;Encrypt=True;TrustServerCertificate=False;"
  }
}

module "sql" {
  source               = "./modules/sql_database"
  server_name          = "${var.app_name}-sql"
  db_name              = "${var.app_name}-db"
  location             = module.rg.location
  resource_group_name  = module.rg.name
  admin_login          = var.sql_admin
  admin_password       = var.sql_password
  aad_admin_name       = var.aad_admin_name
  aad_admin_object_id  = var.aad_admin_object_id
}