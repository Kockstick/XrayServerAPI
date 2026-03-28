#!/bin/bash

set -e

DOMAIN=$1
ORIGIN="/home/XrayServerAPI/"
CADDY="${ORIGIN}/caddy"
EMAIL="app.service@gmail.com"
APP_PORT="5000"

if [ -z "$DOMAIN" ]; then
  echo "Usage: ./setup_caddy.sh your-domain.com"
  exit 1
fi

echo "== Creating directories =="
mkdir -p "${CADDY}/caddy_data"
mkdir -p "${CADDY}/caddy_config"

echo "== Creating Caddyfile =="

cat > "${CADDY}/Caddyfile" <<EOF
$DOMAIN {
    encode zstd gzip
    reverse_proxy 127.0.0.1:$APP_PORT

    tls $EMAIL

    log {
        output stdout
        format console
    }
}
EOF

echo "== Opening ports (if firewall exists) =="

ufw allow 80/tcp || true
ufw allow 443/tcp || true

echo "== Checking Caddy container =="

if [ "$(docker ps -q -f name=caddy)" ]; then
    echo "Caddy is already running. Skipping запуск."
    exit 0
fi

if [ "$(docker ps -aq -f name=caddy)" ]; then
    echo "Caddy container exists but stopped. Removing..."
    docker rm caddy
fi

echo "== Freeing ports 80 and 443 =="

# Убиваем nginx, если он есть
if systemctl is-active --quiet nginx; then
  echo "Stopping nginx..."
  sudo systemctl stop nginx
fi

# На всякий случай прибиваем всё, что держит 80/443
sudo fuser -k 80/tcp || true
sudo fuser -k 443/tcp || true

# Проверка (fail-fast, как ты любишь)
if sudo ss -tulpn | grep -q ':80 '; then
  echo "Port 80 is still in use. Exiting."
  exit 1
fi

if sudo ss -tulpn | grep -q ':443 '; then
  echo "Port 443 is still in use. Exiting."
  exit 1
fi

echo "== Starting Caddy =="

docker run -d \
  --name caddy \
  --network host \
  -v ${CADDY}/Caddyfile:/etc/caddy/Caddyfile:ro \
  -v ${CADDY}/caddy_data:/data \
  -v ${CADDY}/caddy_config:/config \
  caddy

echo "== DONE =="
echo "Your site should be available at: https://$DOMAIN"