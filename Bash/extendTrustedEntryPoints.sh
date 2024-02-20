#!/bin/bash
cd $1
mv $2 "$2.backup"
rm $2
curl -X GET https://raw.githubusercontent.com/construct0/nyzoResources/main/Data/trusted_entry_points > $2
