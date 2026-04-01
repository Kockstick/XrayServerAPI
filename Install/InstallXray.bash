#!/bin/bash

DOMAIN=$1

MAIN="/home/XrayServerAPI/out"
XRAY_CONFIG="${MAIN}/xrayconf.json"

CERT_DIR="${MAIN}/cert"
LOG_DIR="${MAIN}/logs"
DATA_FILE="${MAIN}/data/xray_data.json"
TMP_CONFIG="${MAIN}/xrayconf.tmp.json"

mkdir -p "$CERT_DIR"
mkdir -p "$LOG_DIR"
mkdir -p "${MAIN}/data"

GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

if [ -z "$DOMAIN" ]; then
  echo -e "${RED}Domain not provided${NC}"
  exit 1
fi

echo -e "${YELLOW}Installing Xray if not installed...${NC}"

if ! command -v xray &> /dev/null; then
  wget https://github.com/XTLS/Xray-install/raw/main/install-release.sh
  sudo bash install-release.sh
  rm install-release.sh
fi

echo -e "${YELLOW}Configuring systemd service...${NC}"

ufw allow 8443/tcp

sudo tee /etc/systemd/system/xray.service > /dev/null <<EOF
[Unit]
Description=Xray Service
Documentation=https://github.com/xtls
After=network.target nss-lookup.target

[Service]
User=root
ExecStart=/usr/local/bin/xray run -config ${XRAY_CONFIG}
Restart=on-failure
LimitNPROC=10000
LimitNOFILE=1000000

[Install]
WantedBy=multi-user.target
EOF

sudo bash -c cat > /etc/systemd/system/xray.service.d/10-donot_touch_single_conf.conf <<EOF
[Service]
ExecStart=
ExecStart=/usr/local/bin/xray run -config /home/XrayServerAPI/out/xrayconf.json
EOF

echo -e "${YELLOW}Preparing directories...${NC}"

touch "$LOG_DIR/access.log" "$LOG_DIR/error.log"
chmod a+w "$LOG_DIR/"*.log

echo -e "${YELLOW}Installing certificate...${NC}"

~/.acme.sh/acme.sh --install-cert -d "$DOMAIN" --ecc \
  --fullchain-file "$CERT_DIR/xray.crt" \
  --key-file "$CERT_DIR/xray.key"

if [ $? -ne 0 ]; then
  echo -e "${RED}Certificate install failed${NC}"
  exit 1
fi

echo -e "${YELLOW}Generating ShortId...${NC}"
SHORT_ID=$(openssl rand -hex 8)

TEMP_UUID=$(xray uuid)

echo -e "${YELLOW}Generating Reality keys...${NC}"
KEYS=$(/usr/local/bin/xray x25519)
PRIVATE_KEY=$(echo "$KEYS" | grep "PrivateKey" | cut -d':' -f2- | xargs)
PASSWORD=$(echo "$KEYS" | grep "Password" | cut -d':' -f2- | xargs)
HASH32=$(echo "$KEYS" | grep "Hash32" | cut -d':' -f2- | xargs)

echo -e "${YELLOW}Saving credentials...${NC}"

cat > "$DATA_FILE" <<EOF
{
  "domain": "$DOMAIN",
  "privateKey": "$PRIVATE_KEY",
  "password": "$PASSWORD",
  "hash32": "$HASH32",
  "shortId": "$SHORT_ID"
}
EOF

echo -e "${YELLOW}Preparing config...${NC}"

cp "$XRAY_CONFIG" "$TMP_CONFIG"

sed -i "s|SHORT_ID|$SHORT_ID|g" "$TMP_CONFIG"
sed -i "s|PRIVATE_KEY|$PRIVATE_KEY|g" "$TMP_CONFIG"
sed -i "s|CLIENT_UUID|$TEMP_UUID|g" "$TMP_CONFIG"

sudo mv "$TMP_CONFIG" "$XRAY_CONFIG"

echo -e "${YELLOW}Restarting Xray...${NC}"
sudo systemctl daemon-reload
sudo systemctl restart xray

echo -e "${GREEN}=== XRAY INSTALLED ===${NC}"