#!/bin/bash

DOMAIN=$1
MAIN="/home/XrayServerAPI/out"
WEBROOT="${MAIN}/wwwroot"
CERT_DIR="$HOME/.acme.sh/${DOMAIN}_ecc"

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

if [ -z "$DOMAIN" ]; then
  echo -e "${RED}Domain not provided${NC}"
  exit 1
fi

echo -e "${YELLOW}Checking certificate for $DOMAIN...${NC}"

# Проверка наличия сертификата
if [ -f "$CERT_DIR/fullchain.cer" ] && [ -f "$CERT_DIR/${DOMAIN}.key" ]; then
  echo -e "${GREEN}Certificate already exists. Skipping issue.${NC}"
  exit 0
fi

echo -e "${YELLOW}Installing acme.sh if not installed...${NC}"

# Установка acme.sh если нет
if [ ! -d "$HOME/.acme.sh" ]; then
  wget -O - https://get.acme.sh | sh
  source "$HOME/.bashrc"
fi

# Обновление
~/.acme.sh/acme.sh --upgrade --auto-upgrade

echo -e "${YELLOW}Issuing certificate for $DOMAIN...${NC}"

~/.acme.sh/acme.sh --set-default-ca --server letsencrypt

~/.acme.sh/acme.sh --issue \
  -d "$DOMAIN" \
  -w "$WEBROOT" \
  --keylength ec-256

if [ $? -ne 0 ]; then
  echo -e "${RED}Certificate issue failed${NC}"
  exit 1
fi

echo -e "${GREEN}Certificate successfully issued for $DOMAIN${NC}"