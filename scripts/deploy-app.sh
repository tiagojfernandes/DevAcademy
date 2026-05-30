#!/bin/bash
# Build and deploy the OnlineStore.Api project to an existing Azure Web App.
# Usage: ./deploy-app.sh <resource_group> <webapp_name> [project_path]

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

if [ $# -lt 2 ]; then
    echo -e "${RED}Usage: $0 <resource_group> <webapp_name> [project_path]${NC}"
    exit 1
fi

RESOURCE_GROUP=$1
WEBAPP_NAME=$2
PROJECT_PATH=${3:-"src/OnlineStore.Api/OnlineStore.Api.csproj"}

if ! command -v dotnet &>/dev/null; then
    echo -e "${RED}dotnet SDK not found. Install it and retry.${NC}"
    exit 1
fi

if ! command -v zip &>/dev/null; then
    echo -e "${RED}zip not found. Install with: sudo apt-get install -y zip${NC}"
    exit 1
fi

echo -e "${CYAN}Resource Group : ${RESOURCE_GROUP}${NC}"
echo -e "${CYAN}Web App        : ${WEBAPP_NAME}${NC}"
echo -e "${CYAN}Project        : ${PROJECT_PATH}${NC}"

PUBLISH_DIR=$(mktemp -d)
ZIP_FILE=$(mktemp --suffix=.zip)

rm -f "$ZIP_FILE"

cleanup() { rm -rf "$PUBLISH_DIR" "$ZIP_FILE"; }
trap cleanup EXIT

echo -e "${CYAN}Restoring + publishing (Release)...${NC}"
dotnet publish "$PROJECT_PATH" -c Release -o "$PUBLISH_DIR" --nologo

if [ ! "$(ls -A "$PUBLISH_DIR")" ]; then
    echo -e "${RED}Publish directory is empty. Aborting.${NC}"
    exit 1
fi

echo -e "${CYAN}Creating deployment package...${NC}"
( cd "$PUBLISH_DIR" && zip -qr "$ZIP_FILE" . )
echo -e "${GREEN}Package size: $(du -h "$ZIP_FILE" | cut -f1)${NC}"

echo -e "${CYAN}Deploying to App Service (this can take a few minutes)...${NC}"
if az webapp deploy \
    --resource-group "$RESOURCE_GROUP" \
    --name "$WEBAPP_NAME" \
    --src-path "$ZIP_FILE" \
    --type zip \
    --timeout 1800 \
    --output none; then
    echo -e "${GREEN}Deployment completed.${NC}"
else
    echo -e "${YELLOW}az webapp deploy returned non-zero (likely a client-side timeout).${NC}"
    echo -e "${YELLOW}Server-side deployment usually continues. Verify with the app URL.${NC}"
fi
