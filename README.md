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

## Deploy

From Azure Cloud Shell

```bash
bash <(curl -s https://raw.githubusercontent.com/tiagojfernandes/DevAcademy/refs/heads/main/deploy.sh)
```

## Database Schema
```mermaid
erDiagram
    Users ||--o{ UserRoles            : has
    Roles ||--o{ UserRoles            : assigned_to
    Users ||--o{ ShoppingCart         : owns
    Users ||--o{ Orders               : places
    Categories ||--o{ Products        : contains
    ShoppingCart ||--o{ ShoppingCartProducts : "contains items"
    Products ||--o{ ShoppingCartProducts     : "appears in"
    Orders ||--o{ OrderItems          : "contains lines"
    Products ||--o{ OrderItems        : "ordered as"

    Users {
        int Id PK
        nvarchar Username UK
        nvarchar Password
        datetime2 LastModified
        bit IsDeleted
    }
    Roles {
        int Id PK
        nvarchar Name UK
    }
    UserRoles {
        int UserId PK,FK
        int RoleId PK,FK
    }
    Categories {
        int Id PK
        nvarchar Name UK
    }
    Products {
        int Id PK
        nvarchar Name
        nvarchar Description
        decimal Price
        nvarchar SKU UK
        int StockQuantity
        int CategoryId FK
        bit IsDeleted
        datetime2 LastModified
    }
    ShoppingCart {
        int Id PK
        int UserId FK
        datetime2 CreatedAt
    }
    ShoppingCartProducts {
        int ShoppingCartId PK,FK
        int ProductId PK,FK
        int Quantity
        decimal UnitPrice
    }
    Orders {
        int Id PK
        int UserId FK
        nvarchar Status
        decimal TotalPrice
        datetime2 CreatedAt
    }
    OrderItems {
        int Id PK
        int OrderId FK
        int ProductId FK
        decimal UnitPrice
        int Quantity
        decimal LineTotal "computed, persisted"
    }
```