#!/bin/bash

# crontab:
# @reboot /bin/bash /path/to/script/fetchConsolidatedBlockFiles.sh

# initial run:
# tmux
# cd /path/to/script
# chmod +x fetchConsolidatedBlockFiles.sh
# bash fetchConsolidatedBlockFiles.sh
# CTRL + b
# d
base_url="https://blocks.nyzo.co/blockFiles"
id=0

while true; do
    if [ $id -lt 1000000 ]; then
        formatted_id=$(printf "%06d" $id)
    else
        formatted_id=$id
    fi

    output_file="$formatted_id.nyzoblock"

    if [ -e "$output_file" ]; then
        echo "File $output_file already exists. Skipping ID $id."
        ((id++))
        continue
    fi

    url="$base_url/$formatted_id.nyzoblock"
    echo "fetching $url"

    temp_file=$(mktemp)

    response=$(curl -s -o "$temp_file" -w "%{http_code}" "$url")

    http_status="${response:0:1}"

    if [[ $http_status -eq 2 ]]; then
        if { c=$(strings -n 10 <"$temp_file") && echo "$c" | grep -q "error reading file"; }; then
            echo "200 - error reading file for $output_file - the consolidated block may not be available yet - retrying in 60 seconds..."
            rm -f "$temp_file"
            sleep 60
            continue
        fi

        echo "200 for $output_file, writing to file"
        mv "$temp_file" "$output_file"
        ((id++))
        sleep 0.2
    else
        echo "received non-2xx HTTP status code - retrying in 20 seconds..."
        rm -f "$temp_file"
        sleep 20
    fi
done
