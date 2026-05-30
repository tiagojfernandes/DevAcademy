#!/bin/bash
# One-shot deployment for OnlineStore.Api.
# Designed for Azure Cloud Shell (Bash). Always deploys a fresh stack
# (unique resource names) and seeds the database.
#
# Usage:
#   ./deploy.sh
#
# Hardcoded for this lab:
#   resource group : rg-devacademy-tf
#   region         : uksouth
#   sql server     : sql-tf-devacademy-<NNNN>   (random 4-digit suffix)
#   app service    : app-devacademy-tf-<NNNN>   (same suffix as sql server)
#
# Requirements (Cloud Shell has all of these out of the box):
#   az, terraform, dotnet, zip, openssl, curl, git

set -euo pipefail

REPO_URL="https://github.com/tiagojfernandes/DevAcademy.git"

# ---------- Colours ----------
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

# ---------- Helpers ----------
prompt_input() {
    local prompt_msg=$1
    local var_name=$2
    local current_value="${!var_name:-}"
    if [ -n "$current_value" ]; then
        read -rp "$(echo -e "${CYAN}${prompt_msg} ${YELLOW}[${current_value}]${CYAN}: ${NC}")" input
        if [ -n "$input" ]; then eval "$var_name=\"\$input\""; fi
    else
        while [ -z "${!var_name:-}" ]; do
            read -rp "$(echo -e "${CYAN}${prompt_msg}: ${NC}")" "$var_name"
        done
    fi
}

register_provider() {
    local ns=$1
    local status
    status=$(az provider show --namespace "$ns" --query "registrationState" -o tsv 2>/dev/null || echo "NotRegistered")
    if [ "$status" != "Registered" ]; then
        echo -e "${CYAN}Registering provider ${YELLOW}${ns}${CYAN}...${NC}"
        az provider register --namespace "$ns" --wait
    fi
}

require_cmd() {
    if ! command -v "$1" &>/dev/null; then
        echo -e "${RED}Required command not found: $1${NC}"
        exit 1
    fi
}

# ---------- Pre-flight ----------
for c in az terraform dotnet zip openssl curl git; do require_cmd "$c"; done

if ! az account show &>/dev/null; then
    echo -e "${RED}You are not signed in to az. Run 'az login' first.${NC}"
    exit 1
fi

SUBSCRIPTION_ID=$(az account show --query id -o tsv)
export ARM_SUBSCRIPTION_ID="$SUBSCRIPTION_ID"
echo -e "${CYAN}Using subscription: ${YELLOW}${SUBSCRIPTION_ID}${NC}"

# ---------- Clone repo if running standalone ----------
if [ -d "infra/terraform" ]; then
    REPO_DIR="$(pwd)"
else
    REPO_DIR=$(mktemp -d)/DevAcademy
    echo -e "${CYAN}Cloning repo into ${REPO_DIR}...${NC}"
    git clone --depth 1 "$REPO_URL" "$REPO_DIR"
fi

TF_DIR="$REPO_DIR/infra/terraform"
SCHEMA_FILE="$REPO_DIR/infra/sql/schema.sql"
SEED_FILE="$REPO_DIR/infra/sql/seed.sql"

# ---------- Hardcoded names (this is a lab; one stack per subscription) ----------
# App Service and SQL Server names must be globally unique, so we append a
# random 4-digit suffix. The resource group does not need to be unique.
SUFFIX=$(printf "%04d" $((RANDOM % 10000)))
RESOURCE_GROUP="rg-devacademy-tf"
SQL_SERVER_NAME="sql-tf-devacademy-${SUFFIX}"
APP_NAME="app-devacademy-tf-${SUFFIX}"
LOCATION="uksouth"

echo -e "${GREEN}Resource Group : ${RESOURCE_GROUP}${NC}"
echo -e "${GREEN}Region         : ${LOCATION}${NC}"
echo -e "${GREEN}SQL Server     : ${SQL_SERVER_NAME}${NC}"
echo -e "${GREEN}App name       : ${APP_NAME}${NC}"

# ---------- Providers ----------
echo ""
echo -e "${CYAN}Ensuring required resource providers are registered...${NC}"
for ns in Microsoft.Web Microsoft.Sql Microsoft.Insights Microsoft.OperationalInsights; do
    register_provider "$ns"
done

# ---------- Secrets ----------
echo ""
echo -e "${CYAN}Collecting AAD admin and generating secrets...${NC}"
AAD_ADMIN_NAME=$(az ad signed-in-user show --query displayName -o tsv)
AAD_ADMIN_OBJECT_ID=$(az ad signed-in-user show --query id -o tsv)
JWT_SECRET=$(openssl rand -base64 48)
echo -e "${GREEN}AAD admin: ${AAD_ADMIN_NAME}${NC}"

# ---------- Terraform ----------
echo ""
echo -e "${CYAN}Terraform init...${NC}"
terraform -chdir="$TF_DIR" init -upgrade

echo -e "${CYAN}Terraform apply (auto-approve)...${NC}"
terraform -chdir="$TF_DIR" apply -auto-approve \
    -var="resource_group_name=${RESOURCE_GROUP}" \
    -var="app_name=${APP_NAME}" \
    -var="sql_server_name=${SQL_SERVER_NAME}" \
    -var="location=${LOCATION}" \
    -var="aad_admin_name=${AAD_ADMIN_NAME}" \
    -var="aad_admin_object_id=${AAD_ADMIN_OBJECT_ID}" \
    -var="jwt_secret=${JWT_SECRET}"

# ---------- Read outputs ----------
DEPLOYED_RG=$(terraform -chdir="$TF_DIR" output -raw resource_group_name)
WEBAPP_NAME=$(terraform -chdir="$TF_DIR" output -raw webapp_name)
APP_URL=$(terraform -chdir="$TF_DIR" output -raw app_url)
SQL_SERVER_FQDN=$(terraform -chdir="$TF_DIR" output -raw sql_server_fqdn)
SQL_DB_NAME=$(terraform -chdir="$TF_DIR" output -raw sql_database_name)

# ---------- Deploy the .NET app ----------
echo ""
echo -e "${CYAN}Waiting for App Service to be ready...${NC}"
sleep 30

echo -e "${CYAN}Deploying OnlineStore.Api to ${WEBAPP_NAME}...${NC}"
chmod +x "$REPO_DIR/scripts/deploy-app.sh"
"$REPO_DIR/scripts/deploy-app.sh" "$DEPLOYED_RG" "$WEBAPP_NAME" "$REPO_DIR/src/OnlineStore.Api/OnlineStore.Api.csproj"

# ---------- SQL: grant MSI + apply schema + seed ----------
echo ""
echo -e "${CYAN}Preparing go-sqlcmd (supports AAD auth)...${NC}"
if command -v sqlcmd &>/dev/null && sqlcmd --version 2>&1 | grep -qi "go-sqlcmd\|Version: 1"; then
    SQLCMD="sqlcmd"
else
    curl -sL https://github.com/microsoft/go-sqlcmd/releases/latest/download/sqlcmd-linux-amd64.tar.bz2 | tar xj -C /tmp
    SQLCMD="/tmp/sqlcmd"
fi

echo -e "${CYAN}Granting the Web App's managed identity access to SQL...${NC}"
$SQLCMD -S "$SQL_SERVER_FQDN" -d "$SQL_DB_NAME" --authentication-method=ActiveDirectoryDefault -Q "
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = '${APP_NAME}')
BEGIN
    CREATE USER [${APP_NAME}] FROM EXTERNAL PROVIDER;
    ALTER ROLE db_datareader ADD MEMBER [${APP_NAME}];
    ALTER ROLE db_datawriter ADD MEMBER [${APP_NAME}];
END"

if [ -f "$SCHEMA_FILE" ]; then
    echo -e "${CYAN}Applying schema.sql...${NC}"
    $SQLCMD -S "$SQL_SERVER_FQDN" -d "$SQL_DB_NAME" --authentication-method=ActiveDirectoryDefault -i "$SCHEMA_FILE"
fi

if [ -f "$SEED_FILE" ]; then
    echo -e "${CYAN}Applying seed.sql...${NC}"
    $SQLCMD -S "$SQL_SERVER_FQDN" -d "$SQL_DB_NAME" --authentication-method=ActiveDirectoryDefault -i "$SEED_FILE"
fi

# ---------- Summary ----------
echo ""
echo -e "${CYAN}===============================================${NC}"
echo -e "${CYAN}            DEPLOYMENT SUMMARY${NC}"
echo -e "${CYAN}===============================================${NC}"
echo -e "${GREEN}Subscription   : ${SUBSCRIPTION_ID}${NC}"
echo -e "${GREEN}Resource Group : ${DEPLOYED_RG}${NC}"
echo -e "${GREEN}Region         : ${LOCATION}${NC}"
echo -e "${GREEN}Web App        : ${WEBAPP_NAME}${NC}"
echo -e "${GREEN}App URL        : ${APP_URL}${NC}"
echo -e "${GREEN}Swagger        : ${APP_URL}/swagger${NC}"
echo -e "${GREEN}SQL Server     : ${SQL_SERVER_FQDN}${NC}"
echo -e "${GREEN}SQL Database   : ${SQL_DB_NAME}${NC}"
echo -e "${CYAN}===============================================${NC}"
echo -e "${YELLOW}Note: a new random JWT signing key was generated for this deployment.${NC}"
echo -e "${YELLOW}      Old tokens from previous deployments will not validate.${NC}"

