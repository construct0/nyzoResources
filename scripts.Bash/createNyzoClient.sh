echo "Refer to tech.nyzo.org setup instructions and use the enableWebListenerHTTPS.sh script (runmode agnostic)"
exit 1;

#!/bin/bash
# Tested on ubuntu 22.04, certbot v1.21.0, nyzoVerifier v642

# e.g. client.nyzo.org or nyzo.org
CLIENT_DOMAIN=""
# e.g. mail@domain.com 
SSL_CERT_EMAIL=""
# Your export password, minimum 16 chars
SSL_EXPORT_PASSWORD=""
 
# Update the server
# Upgrade the server if necessary, remove the hashtag on the next line to do so
sudo apt update -y

# Ensures that system time is synced properly
sudo systemctl enable systemd-timesyncd
sudo systemctl start systemd-timesyncd
 
# Install dependencies, things the application relies on to function properly
sudo apt install haveged -y
sudo apt install openjdk-8-jdk -y
sudo apt install supervisor -y
sudo apt install certbot -y
 
# Create a special directory for your client
mkdir /home/ubuntu && cd /home/ubuntu
 
# Allow network connectivity on the server level, check your virtual private server provider if this does not work. (todo add a page with overview comparing port blocks)
sudo ufw allow 9444/tcp
sudo ufw allow 9446/udp
sudo ufw allow 80/tcp 
sudo ufw allow 80/udp 
sudo ufw allow 443/tcp 
sudo ufw allow 443/udp 
sudo ufw allow 8080/tcp 
sudo ufw allow 8080/udp
sudo ufw allow 593/tcp 
sudo ufw allow 593/udp
sudo ufw allow 22/tcp 
sudo ufw allow 22/udp
sudo ufw enable 
 
# Copy the source code to your machine
git clone https://github.com/construct0/nyzoVerifier.git
 
# Java Gradlew converts this source code to machine readable binaries
cd nyzoVerifier
./gradlew build
 
# Create a folder for the client you're going to put into production, ie. activating and enabling your client
sudo mkdir -p /var/lib/nyzo/production
 
# The client will contact trusted verifiers first, if you provide none, your client will not work
# The default trusted entry points are by default "nyzo.co" controlled, "nyzo.co" scans the mesh of active verifiers and couples ipv4 addresses to specific subdomains when nodes have proven to behave correctly
# In addition to that a nyzo.org controlled node has been added
# You can replace the domain or ipv4 addresses in this file at your discretion, when running multiple verifiers it is recommended to trust node(s) you're already the owner of 
sudo cp trusted_entry_points /var/lib/nyzo/production
echo "verifier0.nyzo.org:9444" | sudo tee -a /var/lib/nyzo/production/trusted_entry_points > /dev/null
 
# Grant special rights to the script in order to be able to execute it
chmod +x nyzoClient.sh
 
# Change some values, alternatively update the file itself
sed -i -e 's/startretries=20/startretries=99999/g' nyzoClient.sh
 
# Run the script, it generates a supervisor configuration file for you
./nyzoClient.sh
 
# Copy this configuration file to the supervisor's production folders
sudo cp nyzoClient.conf /etc/supervisor/conf.d
 
# Enter your client preferences, these get stored in the preferences file, adjusting the web ports should be sufficient for most users
sudo echo -e "block_file_consolidator=disable
start_historical_block_manager=1
transaction_indexer_active=1
start_web_listener=1
web_port=80
web_port_https=443
" >> /var/lib/nyzo/production/preferences

# Create a temporary web forwarding proxy
sudo mkdir /var/lib/nyzo/production/webTemp
 
reload_instance(){
    # If trying to install multiple times, the log output is removed so the grep (searching the output file) will work 
    sudo rm /var/log/nyzo-client.log && sudo touch /var/log/nyzo-client.log
 
    # Reload the progress supervisor, awaits a clean output by status call
    sudo supervisorctl reload
 
    while ! (supervisorctl status | grep --line-buffered RUNNING); do
        :
    done
 
    echo "Client started, pending launch end"
 
    # Await a successful client launch
    while ! (supervisorctl tail -9 nyzo_client | grep --line-buffered command); do 
        :
    done
 
    echo "Client launched successfully, network status: $(supervisorctl tail -100 nyzo_client | grep "frozen edge")"
 
    # Create SSL certificate, fetch using HTTP to be aware of liveliness of the web listener content 
    while ! (curl $CLIENT_DOMAIN | grep --line-buffered frozenEdge); do 
        :
    done 
 
    echo "Web listener live as indicated by HTTP fetch"
}
 
#reload_instance
 
getfullchainpem() { [[ $1 =~ ^/etc/letsencrypt/live/(.*fullchain\.pem)$ ]] && echo "${BASH_REMATCH[1]}"; }
getprivkeypem() { [[ $1 =~ ^/etc/letsencrypt/live/(.*privkey\.pem)$ ]] && echo "${BASH_REMATCH[1]}"; }
 
 
if [[ ! (-z $CLIENT_DOMAIN || -z $SSL_CERT_EMAIL || -z $SSL_EXPORT_PASSWORD) ]]; then
    echo "Setting up your SSL certificate"
    
    CERTBOT_RESULT=$(sudo certbot certonly -n -m $SSL_CERT_EMAIL --agree-tos --webroot --webroot-map '{"'$CLIENT_DOMAIN'":"/var/lib/nyzo/production/webTemp"}' -d $CLIENT_DOMAIN --webroot-path '/var/lib/nyzo/production/webTemp')
    
    if [[ "$CERTBOT_RESULT" == *Successfully* ]]; then 
        echo "Successfully generated SSL certificate"
        
        echo -e -n "$CERTBOT_RESULT"
        
        CERT_PATH=$(getfullchainpem "$CERTBOT_RESULT")
        KEY_PATH=$(getprivkeypem "$CERTBOT_RESULT")
        
        echo "$CERT_PATH"
        echo "$KEY_PATH"
        
        sudo openssl pkcs12 -export -inkey "$KEY_PATH" -in "$CERT_PATH" generatedcert -out /var/lib/nyzo/production/ssl-keystore.p12 
        
        echo "web_listener_keystore_path=/var/lib/nyzo/production/ssl-keystore.p12
web_listener_keystore_password=" "$SSL_EXPORT_PASSWORD" | sudo tee -a /var/lib/nyzo/production/preferences > /dev/null
        
        reload_instance
    else
        echo "Failed to generate an SSL certificate, check your DNS records"
    fi
else
    echo "Invalid domain, email or password length"
fi 
 

sudo rmdir /var/lib/nyzo/production/webTemp 

cd /home/ubuntu
crontab -l > crontab.backup

# When the server reboots, this will ensure the client starts as well
echo "@reboot sudo supervisorctl reload" >> rebootcronjob
crontab rebootcronjob
rm rebootcronjob
 
 
 