# Prerequisites: run as root
# this is a new addition, but ensures that time is synced properly (see MeshListener.response with its replay protection)
systemctl enable systemd-timesyncd
systemctl start systemd-timesyncd

apt update -y
apt install haveged openjdk-8-jdk supervisor certbot -y
ufw allow 9444/tcp
ufw allow 9446/udp
ufw allow 22/tcp
ufw allow 22/udp
ufw enable

mkdir -p /home/ubuntu && cd /home/ubuntu
git clone https://github.com/construct0/nyzoVerifier.git

cd nyzoVerifier
./gradlew build

mkdir -p /var/lib/nyzo/production
cp trusted_entry_points /var/lib/nyzo/production

chmod +x nyzoVerifier.sh
sed -i -e 's/startretries=20/startretries=99999/g' nyzoVerifier.sh
./nyzoVerifier.sh
cp nyzoVerifier.conf /etc/supervisor/conf.d

# adjust these to your liking
echo -e "
always_track_blockchain=1
verifier_add_metadata_transactions=1
enable_consensus_tracker=1
create_and_score_test_new_verifier_block=1
enable_console_color=1
log_timestamps=1" >> /var/lib/nyzo/production/preferences
echo "C0" > /var/lib/nyzo/production/nickname

echo ":::fetching genesis block"
mkdir -p /var/lib/nyzo/production/blocks/individual
curl -X GET https://seed.nyzo.org/genesis/i_000000000.nyzoblock > /var/lib/nyzo/production/blocks/individual/i_000000000.nyzoblock

echo ":::validating genesis block"
if [ $(sha256sum /var/lib/nyzo/production/blocks/individual/i_000000000.nyzoblock | grep -c 0a83f9aa933a406e3065c4d2e87867ecbad32bc5ecc794e4d795e2755e4f46c7) -eq 1 ]
then
    echo ":::fetched genesis block is VALID"
else
    echo ":::fetched genesis block is INVALID" && exit 1;
fi

mkdir -p /var/lib/nyzo/production/seed_transactions
cd /var/lib/nyzo/production/seed_transactions

echo ":::fetching seed transactions"
seed_url="https://seed.nyzo.org"
id=2
while [ $id -lt 2704  ]; do
	if  [ $id -lt 2704 ]; then
		formatted_id=$(printf "%06d" $id)
	fi

	tx_file="$formatted_id.nyzotransaction"

	if [ -e "$tx_file" ]; then
        	echo ":::$tx_file already exists. Skipping ID $id."
        	((id++))
        	continue
    	fi

	url="$seed_url/$tx_file"
	echo ":::fetching $url"

	temp_file=$(mktemp)

    	response=$(curl -s -o "$temp_file" -w "%{http_code}" "$url")

    	http_status="${response:0:1}"

    	if [[ $http_status -eq 2 ]]; then
        	echo ":::200 for $tx_file, writing to file"
        	mv "$temp_file" "$tx_file"
        	((id++))
        	sleep 0.2
   	else
        	echo ":::received non-2xx HTTP status code - retrying in 20 seconds..."
        	rm -f "$temp_file"
        	sleep 20
    	fi
done

echo ":::done fetching seed txs"

echo ":::backing up current crontab"
cd /home/ubuntu
crontab -l > crontab.backup

echo ":::adding reboot cronjob"
echo "@reboot sudo supervisorctl reload" >> rebootcronjob
crontab rebootcronjob
rm rebootcronjob

echo ":::starting verifier"

reload_instance(){
    sudo supervisorctl reload

    while ! (supervisorctl status | grep --line-buffered RUNNING); do
        :
    done

    echo ":::verifier started"

    sudo supervisorctl tail -f nyzo_verifier
}

reload_instance