# DevAcademy — Azure Infrastructure

Terraform project that deploys an Azure environment with:

- **Resource Group**
- **Log Analytics Workspace**
- **Application Insights**
- **Windows App Service** (with System Managed Identity)
- **Azure SQL Database** (AAD-only authentication)

The App Service connects to SQL using its managed identity — no passwords stored in app config.

## Project Structure

```
infra/terraform/
 ├── main.tf            # Module composition
 ├── variables.tf       # Root variables
 ├── outputs.tf         # Root outputs
 ├── providers.tf       # Provider config (azurerm ~> 3.100)
 ├── dev.tfvars         # Dev environment values
 └── modules/
      ├── resource_group/
      ├── log_analytics/
      ├── app_insights/
      ├── app_service/
      └── sql_database/
```

## Prerequisites

- An Azure subscription
- Azure CLI logged in (`az login`)
- Terraform >= 1.5.0

## Deploy

From Azure Cloud Shell (or any bash terminal with `az` and `terraform`):

```bash
git clone https://github.com/tiagojfernandes/DevAcademy.git
cd DevAcademy
chmod +x deploy.sh
./deploy.sh          # defaults to dev environment
```

The script automatically:
1. Detects your Azure AD identity for the SQL admin
2. Generates a throwaway SQL password (required by Azure API, blocked at runtime)
3. Runs `terraform init` → `validate` → `plan`
4. Asks for confirmation before applying
5. Grants the App Service managed identity `db_datareader` and `db_datawriter` roles on the SQL database
