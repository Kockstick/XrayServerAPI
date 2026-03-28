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

echo -e "${YELLOW}Setting up nginx...${NC}"

# Установка nginx

echo -e "${YELLOW}Freeing port 80...${NC}"
sudo fuser -k 80/tcp || true

sudo apt update && sudo apt install -y nginx
sudo systemctl start nginx

# Конфиг nginx
NGINX_CONF="/etc/nginx/sites-available/${DOMAIN}"

sudo bash -c "cat > $NGINX_CONF" <<EOF
server {
    listen 80;
    server_name $DOMAIN;
    root $WEBROOT;
    index index.html;
}
EOF

# Активируем конфиг
sudo ln -sf "$NGINX_CONF" "/etc/nginx/sites-enabled/${DOMAIN}"

# Проверка и перезапуск
sudo nginx -t
if [ $? -ne 0 ]; then
  echo -e "${RED}Nginx config test failed${NC}"
  exit 1
fi

sudo systemctl reload nginx

sudo systemctl is-active --quiet nginx
if [ $? -ne 0 ]; then
  echo -e "${RED}Nginx failed to start${NC}"
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

echo -e "${YELLOW}Running test (staging) certificate request for $DOMAIN...${NC}"

~/.acme.sh/acme.sh --issue \
  --server letsencrypt_test \
  -d "$DOMAIN" \
  -w "$WEBROOT" \
  --keylength ec-256

if [ $? -ne 0 ]; then
  echo -e "${RED}Test certificate issue failed. Exiting to avoid rate limits.${NC}"
  exit 1
fi

echo -e "${GREEN}Test certificate issued successfully. Proceeding with real certificate...${NC}"

echo -e "${YELLOW}Issuing certificate for $DOMAIN...${NC}"

~/.acme.sh/acme.sh --set-default-ca --server letsencrypt

~/.acme.sh/acme.sh --issue \
  -d "$DOMAIN" \
  -w "$WEBROOT" \
  --keylength ec-256
  --force

if [ $? -ne 0 ]; then
  echo -e "${RED}Certificate issue failed${NC}"
  exit 1
fi

echo -e "${GREEN}Certificate successfully issued for $DOMAIN${NC}"

echo -e "${YELLOW}Removing nginx (no longer needed)...${NC}"

# Останавливаем и удаляем nginx
sudo systemctl stop nginx
sudo apt remove -y nginx nginx-common
sudo apt purge -y nginx nginx-common
sudo apt autoremove -y

echo -e "${GREEN}Nginx removed${NC}"