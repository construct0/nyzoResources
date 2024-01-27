#!/bin/bash
base_url="https://nyzo.co/blockPlain"
id=0

while true; do
    output_file="/var/www/scraped/$id.html"

    if [ -e "$output_file" ]; then
        echo "File $output_file already exists. Skipping ID $id."
        ((id++))
        continue
    fi

    url="$base_url/$id"
    echo "fetching $url"

    temp_file=$(mktemp)

    response=$(curl -s -o "$temp_file" -w "%{http_code}" "$url")

    http_status="${response:0:1}" 
    expected_content="Nyzo block $id"
    unavailable_content="Block not yet available on this server"

    if [[ $http_status -eq 2 ]]; then
	actual_content=$(cat "$temp_file")

	if { echo "$actual_content" | grep -q "$unavailable_content"; }; then
		echo "200 - block not yet available, sleeping for 10 seconds"
		rm -f "$temp_file"
		sleep 10
		continue
	fi

	if { echo "$actual_content" | grep -q "$expected_content"; }; then
		echo "200 for $output_file, writing to file"
		mv "$temp_file" "$output_file"
		((id++))
		sleep 0.01
	else 
		echo "200 - error reading file content, does not contain expected string, sleeping for 10 seconds"
		rm -f "$temp_file"
		sleep 10
		continue
	fi
    else
        echo "received non-2xx HTTP status code - retrying in 20 seconds..."
        rm -f "$temp_file"
        sleep 20
    fi
done