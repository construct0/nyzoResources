const nacl = require("tweetnacl");

module.exports = class NyzoConverter {
    static PublicIdentifierForPrivateKey(keyString){
        let identifierString = "";

        if (typeof keyString === "string") {
            // Decode the key string.
            keyString = keyString.trim();
            let key = NyzoStringEncoder.Decode(keyString);
            if (key != null && !CommonUtil.IsUndefined(key.GetSeed())) {
                // Get the identifier for the key and make an identifier string.
                let keyPair = nacl.sign.keyPair.fromSeed(key.GetSeed());
                identifierString = NyzoStringEncoder.NyzoStringFromPublicIdentifier(keyPair.publicKey);
            }
        }

        return identifierString;
    }

    // This function was modified while porting to allow for both nyzo and micronyzo amounts to be garnered a human readable string of
    // The default is "true", if a developer forgets to set it to false it will display a lower amount than it actually is, not a higher amount
    static GetDisplayAmount(amount, isMicroNyzos=true) {
        let division = isMicroNyzos ? 1000000 : 1;
        return '&cap;' + (amount / division).toFixed(6);
    }

    static GetAmountOfMicroNyzos(valueString){
        return Math.floor(+valueString * NyzoConstants.GetMicroNyzosPerNyzo());
    }
}