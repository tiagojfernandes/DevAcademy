#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TF_DIR="$SCRIPT_DIR/infra/terraform"
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
echo "Done."
