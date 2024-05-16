echo ":::validating genesis block"
if [ $(sha256sum /var/lib/nyzo/production/blocks/individual/i_000000000.nyzoblock | grep -c 0a83f9aa933a406e3065c4d2e87867ecbad32bc5ecc794e4d795e2755e4f46c7) -eq 1 ]
then
    echo ":::genesis block is VALID"
else
    echo ":::genesis block is INVALID" && exit 1;
fi