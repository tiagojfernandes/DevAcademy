#!/bin/bash
set -euo pipefail

REPO_URL="https://github.com/tiagojfernandes/DevAcademy.git"

# If the repo isn't cloned locally, clone it to a temp directory
if [ -d "infra/terraform" ]; then
  REPO_DIR="$(pwd)"
else
  REPO_DIR=$(mktemp -d)
  echo "Cloning repo into $REPO_DIR..."
  git clone "$REPO_URL" "$REPO_DIR"
fi

TF_DIR="$REPO_DIR/infra/terraform"
ENV="${1:-dev}"
VAR_FILE="$TF_DIR/${ENV}.tfvars"

if [ ! -f "$VAR_FILE" ]; then
  echo "ERROR: '$VAR_FILE' not found. Usage: ./deploy.sh [env]"
  exit 1
fi

# Grab the signed-in user info and generate a throwaway SQL password
AAD_ADMIN_NAME=$(az ad signed-in-user show --query displayName -o tsv)
AAD_ADMIN_OBJECT_ID=$(az ad signed-in-user show --query id -o tsv)
SQL_PASSWORD=$(openssl rand -base64 24)

echo "AAD Admin: $AAD_ADMIN_NAME ($AAD_ADMIN_OBJECT_ID)"
echo "Deploying [$ENV] with $VAR_FILE"
echo ""

terraform -chdir="$TF_DIR" init
terraform -chdir="$TF_DIR" validate
terraform -chdir="$TF_DIR" plan \
  -var-file="$VAR_FILE" \
  -var="aad_admin_name=$AAD_ADMIN_NAME" \
  -var="aad_admin_object_id=$AAD_ADMIN_OBJECT_ID" \
  -var="sql_password=$SQL_PASSWORD" \
  -out=tfplan

echo ""
read -rp "Apply this plan? (yes/no): " CONFIRM
if [ "$CONFIRM" != "yes" ]; then
  echo "Aborted."
  exit 0
fi

terraform -chdir="$TF_DIR" apply tfplan

# Read app_name from the tfvars file for the SQL grant
APP_NAME=$(grep 'app_name' "$VAR_FILE" | sed 's/.*= *"\(.*\)"/\1/')
SQL_SERVER="${APP_NAME}-sql-fog.database.windows.net"
SQL_DB="${APP_NAME}-db"

echo ""
echo "Granting managed identity access to SQL..."

# Need the Go-based sqlcmd (supports AAD auth)
if command -v sqlcmd &>/dev/null && sqlcmd --version 2>&1 | grep -qi "go-sqlcmd\|Version: 1"; then
  SQLCMD="sqlcmd"
else
  echo "Installing go-sqlcmd..."
  curl -sL https://github.com/microsoft/go-sqlcmd/releases/latest/download/sqlcmd-linux-amd64.tar.bz2 | tar xj -C /tmp
  SQLCMD="/tmp/sqlcmd"
fi

if [ ! -x "$SQLCMD" ]; then
  echo "WARNING: Could not find go-sqlcmd. Run the SQL grant manually:"
  echo "  CREATE USER [${APP_NAME}] FROM EXTERNAL PROVIDER;"
  echo "  ALTER ROLE db_datareader ADD MEMBER [${APP_NAME}];"
  echo "  ALTER ROLE db_datawriter ADD MEMBER [${APP_NAME}];"
  echo "Done (SQL grant skipped)."
  exit 0
fi

$SQLCMD -S "$SQL_SERVER" -d "$SQL_DB" --authentication-method=ActiveDirectoryDefault -Q "
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = '${APP_NAME}')
BEGIN
    CREATE USER [${APP_NAME}] FROM EXTERNAL PROVIDER;
    ALTER ROLE db_datareader ADD MEMBER [${APP_NAME}];
    ALTER ROLE db_datawriter ADD MEMBER [${APP_NAME}];
END
"

echo "Done."
