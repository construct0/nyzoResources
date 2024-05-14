#!/bin/bash
# Prerequisites: web listener serves content on port 80
# Tested, runmode agnostic

# e.g. client.nyzo.org or nyzo.org
WEB_DOMAIN=""
# e.g. mail@domain.com 
SSL_CERT_EMAIL=""
# Your export password, minimum 16 chars
SSL_EXPORT_PASSWORD=""

echo ":::getting updates & installing certbot"
sudo apt update -y
sudo apt install certbot -y

echo ":::copying preferences to preferences.backup"
cp --backup /var/lib/nyzo/production/preferences /var/lib/nyzo/production/preferences.backup

echo ":::removing existing web_port and web_listener preferences"
sed -i '/^web_port=/d' /var/lib/nyzo/production/preferences
sed -i '/^web_port_https=/d' /var/lib/nyzo/production/preferences
sed -i '/^web_listener_keystore_path=/d' /var/lib/nyzo/production/preferences
sed -i '/^web_listener_keystore_password=/d' /var/lib/nyzo/production/preferences
sed -i '/^start_web_listener=/d' /var/lib/nyzo/production/preferences

echo ":::adding web_port preferences (8080,443)"
sudo echo -e "web_port=8080" >> /var/lib/nyzo/production/preferences
sudo echo -e "web_port_https=443" >> /var/lib/nyzo/production/preferences
sudo echo -e "start_web_listener=1" >> /var/lib/nyzo/production/preferences

echo ":::adding ufw rules"
sudo ufw allow 8080/tcp
sudo ufw allow 8080/udp
sudo ufw allow 443/tcp
sudo ufw allow 443/udp

# Create a temporary web forwarding proxy
sudo mkdir /var/lib/nyzo/production/webTemp 

reloadinstance(){
    sudo supervisorctl reload

    while ! (supervisorctl status | grep --line-buffered RUNNING); do
        :
    done

    echo ":::instance started"

    # read RUN_MODE _ <<< $( supervisorctl status )

    # until sudo supervisorctl tail -f $RUN_MODE | grep -m 1 "HTTP"; do : ; done
}
 
extract_fullchain_path() {
    local text="$1"
    local cert_path=$(echo "$text" | grep -oE '/etc/letsencrypt/live/[^/]+/fullchain.pem')
    echo "$cert_path"
}

extract_privkey_path() {
    local text="$1"
    local cert_path=$(echo "$text" | grep -oE '/etc/letsencrypt/live/[^/]+/privkey.pem')
    echo "$cert_path"
}


if [[ ! (-z $WEB_DOMAIN || -z $SSL_CERT_EMAIL || -z $SSL_EXPORT_PASSWORD) ]]; then
    echo ":::setting up your SSL certificate"
    
    # add --test-cert for testing, this avoids getting ratelimited by requesting too many certs
    # manually remove test cert folders from /etc/letsencrypt/live/[...] before trying again
    CERTBOT_RESULT=$(sudo certbot certonly -n -m $SSL_CERT_EMAIL --agree-tos --webroot --webroot-map '{"'$WEB_DOMAIN'":"/var/lib/nyzo/production/webTemp"}' -d $WEB_DOMAIN --webroot-path '/var/lib/nyzo/production/webTemp')
    
    echo ":::received certbot result"
    echo -e -n "$CERTBOT_RESULT"

    if [[ "$CERTBOT_RESULT" == *Successfully* ]]; then 
        echo ":::successfully generated SSL certificate"
                
        CERT_PATH=$(extract_fullchain_path "$CERTBOT_RESULT")
        KEY_PATH=$(extract_privkey_path "$CERTBOT_RESULT")
        
        echo ":::extracting paths"
        
        echo "$CERT_PATH"
        echo "$KEY_PATH"
        
        echo ":::writing to prod"

        sudo openssl pkcs12 -export -inkey "$KEY_PATH" -in "$CERT_PATH" -password "pass:$SSL_EXPORT_PASSWORD" -name ncert -out /var/lib/nyzo/production/ssl-keystore.p12 
        
        echo ":::adding web_listener_keystore prefs"
        echo "web_listener_keystore_path=/var/lib/nyzo/production/ssl-keystore.p12
web_listener_keystore_password=$SSL_EXPORT_PASSWORD" | sudo tee -a /var/lib/nyzo/production/preferences > /dev/null
        
		echo ":::done"

        reloadinstance
    else
        echo ":::failed to generate an SSL certificate, check your DNS records"
    fi
else
    echo ":::invalid domain, email or password length"
fi 
 

sudo rmdir /var/lib/nyzo/production/webTemp 





