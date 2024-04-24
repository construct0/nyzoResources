'use strict';

class NyzoUtil {
    static HexStringAsUint8Array(identifier) {
        identifier = identifier.split('-').join('');
        let array = new Uint8Array(identifier.length / 2);

        for (let i = 0; i < array.length; i++) {
            array[i] = parseInt(identifier.substring(i * 2, i * 2 + 2), 16);
        }
    
        return array;
    }

    static Sha256Uint8(array) {
        let ascii = '';

        for (let i = 0; i < array.length; i++) {
            ascii += String.fromCharCode(array[i]);
        }
    
        return NyzoUtil.HexStringAsUint8Array(sha256(ascii));
    }

    static DoubleSha256(array) {
        return NyzoUtil.Sha256Uint8(NyzoUtil.Sha256Uint8(array));
    }

    static SenderDataAsUint8Array(senderData) {
        // Process normalized sender-data strings.
        let array = null;

        if (senderData.length == 67) {
            let lowercase = senderData.toLowerCase();

            if (lowercase[0] == 'x' && lowercase[1] == '(' && lowercase[66] == ')') {

                // Get the underscore index to determine the length of the data.
                let underscoreIndex = lowercase.indexOf('_');

                if (underscoreIndex < 0) {
                    underscoreIndex = NyzoConstants.GetMaximumSenderDataLength() * 2 + 2;
                } 

                let dataLength = underscoreIndex / 2 - 1;
    
                // Ensure that all characters in the data field are correct. The left section must be all alphanumeric, and
                // the right section must be underscores. The string was converted to lowercase.
                let allAreCorrect = true;

                for (let i = 2; i < 66 && allAreCorrect; i++) {
                    // This could be written more succinctly, but it would be more difficult to read.
                    if (i < underscoreIndex) {
                        allAreCorrect = (lowercase[i] >= '0' && lowercase[i] <= '9') || (lowercase[i] >= 'a' && lowercase[i] <= 'f');
                    } else {
                        allAreCorrect = lowercase[i] == '_';
                    }
                }
    
                // If all characters are correct, decode the data. Otherwise, leave the result null to indicate that the
                // input is not a valid sender-data string.
                if (allAreCorrect) {
                    array = NyzoUtil.hexStringAsUint8Array(senderData.substring(2, 2 + dataLength * 2));
                }
            }
        }
    
        // If processing of a normalized sender-data string did not produce a result, process as a plain-text string.
        if (array == null) {
            array = new Uint8Array(Math.min(senderData.length, 32));
            for (let i = 0; i < array.length; i++) {
                array[i] = senderData.charCodeAt(i);
            }
        }
    
        return array;
    }

    static PrintAmount(amountMicronyzos) {
        return '&cap;' + (amountMicronyzos / 1000000).toFixed(6);
    }

    static IsValidAmountOfMicroNyzos(value){
        let parsedValue = parseFloat(value);

        if(CommonUtil.IsUndefined(parsedValue) || Number.isNaN(parsedValue) || parsedValue < NyzoConstants.GetMinimumTransactionAmount()){
            return false;
        }

        return true;
    }

    static IsValidPrivateKey(keyString){
        let isValid = false;
        if (typeof keyString === 'string') {
            keyString = keyString.trim();
            let key = NyzoStringEncoder.Decode(keyString);
            isValid = key != null && typeof key.getSeed() !== 'undefined';
        }
    
        return isValid;
    }

    static IsValidPublicIdentifier(identifierString){
        let isValid = false;
        if (typeof identifierString === 'string') {
            identifierString = identifierString.trim();
            let identifier = NyzoStringEncoder.Decode(identifierString);
            isValid = identifier != null && typeof identifier.getIdentifier() !== 'undefined';
        }

        return isValid;
    }

    static IsValidClientURL(clientUrl){
        // This is not a robust check for valid/invalid URLs. It is just a check to ensure that the provided URL is somewhat
        // reasonable for use as a client URL.
        let isValid = false;
        if (typeof clientUrl === 'string') {
            clientUrl = clientUrl.trim();
            isValid = (clientUrl.startsWith('http://') || clientUrl.startsWith('https://')) && !clientUrl.includes('<') &&
                !clientUrl.includes('>') && !clientUrl.includes('?') && !clientUrl.includes(' ') &&
                !clientUrl.includes('%');
        }

        return isValid;
    }
}