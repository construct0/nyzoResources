export default class NyzoStringEncoder {
    static GetCharacterLookup() {
        return ('0123456789' + 'abcdefghijkmnopqrstuvwxyz' + 'ABCDEFGHIJKLMNPQRSTUVWXYZ' + '-.~_').split('');
    }

    static GetCharacterToValueMap() {
        let characterLookup = NyzoStringEncoder.GetCharacterLookup();
        let characterToValueMap = [];

        for (let i = 0; i < characterLookup.length; i++) {
            characterToValueMap[characterLookup[i]] = i;
        }

        return characterToValueMap;
    }

    static ArraysAreEqual(array1, array2) {
        let arraysAreEqual;
    
        if (array1 == null || array2 == null) {
            arraysAreEqual = array1 == null && array2 == null;
        } else {
            arraysAreEqual = array1.length == array2.length;
            for (let i = 0; i < array1.length && arraysAreEqual; i++) {
                if (array1[i] != array2[i]) {
                    arraysAreEqual = false;
                }
            }
        }
    
        return arraysAreEqual;
    }

    static Decode(encodedString) {
        let result = null;
    
        // Trim the string.
        encodedString = encodedString.trim();
    
        // Map characters from the old encoding to the new encoding. A few characters were changed to make Nyzo strings more
        // URL-friendly.
        encodedString = encodedString.replace(/\*/g, '-').replace(/\+/g, '.').replace(/=/g, '~');
    
        // Map characters that may be mistyped. Nyzo strings contain neither 'l' nor 'O'.
        encodedString = encodedString.replace(/l/g, '1').replace(/O/g, '0');
    
        // Get the type from the prefix.
        let prefix = encodedString.substring(0, 4);
    
        // Get the array representation of the encoded string.
        let expandedArray = NyzoStringEncoder.ByteArrayForEncodedString(encodedString);
    
        // Get the content length from the next byte and calculate the checksum length.
        let contentLength = expandedArray[3] & 0xff;
        let checksumLength = expandedArray.length - contentLength - 4;
    
        // Only continue if the checksum length is valid.
        if (checksumLength >= 4 && checksumLength <= 6) {
            // Calculate the checksum and compare it to the provided checksum. Only create the result array if the checksums
            // match.
            let headerLength = 4;
            let calculatedChecksum = NyzoUtil.DoubleSha256(expandedArray.subarray(0, headerLength + contentLength)).subarray(0, checksumLength);
            let providedChecksum = expandedArray.subarray(expandedArray.length - checksumLength, expandedArray.length);
    
            if (NyzoStringEncoder.ArraysAreEqual(calculatedChecksum, providedChecksum)) {
                // Get the content array. This is the encoded object with the prefix, length byte, and checksum removed.
                let contentBytes = expandedArray.subarray(headerLength, expandedArray.length - checksumLength);
    
                // Make the object from the content array.
                if (prefix === 'key_') {
                    result = new NyzoStringPrivateSeed(contentBytes);
                } else if (prefix === 'id__') {
                    result = new NyzoStringPublicIdentifier(contentBytes);
                } else if (prefix === 'pre_') {
                    result = new NyzoStringPrefilledData(contentBytes.subarray(0, 32), contentBytes.subarray(33, contentBytes.length));
                } else if (prefix === 'tx__') {
                    result = NyzoTransaction.FromBytes(contentBytes);
                }
            }
        }
    
        return result;
    }

    static ByteArrayForEncodedString(encodedString) {
        let characterToValueMap = NyzoStringEncoder.GetCharacterToValueMap();

        let arrayLength = (encodedString.length * 6 + 7) / 8;
        let array = new Uint8Array(arrayLength);

        for (let i = 0; i < arrayLength; i++) {
            let leftCharacter = encodedString.charAt(i * 8 / 6);
            let rightCharacter = encodedString.charAt(i * 8 / 6 + 1);
    
            let leftValue = characterToValueMap[leftCharacter];
            let rightValue = characterToValueMap[rightCharacter];
            let bitOffset = (i * 2) % 6;

            array[i] = ((((leftValue << 6) + rightValue) >> 4 - bitOffset) & 0xff);
        }
    
        return array;
    }

    static EncodedStringForByteArray(array) {
        let characterLookup = NyzoStringEncoder.GetCharacterLookup();

        let index = 0;
        let bitOffset = 0;
        let encodedString = "";
        
        while (index < array.length) {
            // Get the current and next byte.
            let leftByte = array[index] & 0xff;
            let rightByte = index < array.length - 1 ? array[index + 1] & 0xff : 0;
    
            // Append the character for the next 6 bits in the array.
            let lookupIndex = (((leftByte << 8) + rightByte) >> (10 - bitOffset)) & 0x3f;
            encodedString += characterLookup[lookupIndex];
    
            // Advance forward 6 bits.
            if (bitOffset == 0) {
                bitOffset = 6;
            } else {
                index++;
                bitOffset -= 2;
            }
        }
    
        return encodedString;
    }

    static EncodedNyzoString(prefix, contentBytes) {
        // Get the prefix array from the type.
        let prefixBytes = NyzoStringEncoder.ByteArrayForEncodedString(prefix);
    
        // Determine the length of the expanded array with the header and the checksum. The header is the type-specific
        // prefix in characters followed by a single byte that indicates the length of the content array (four bytes
        // total). The checksum is a minimum of 4 bytes and a maximum of 6 bytes, widening the expanded array so that
        // its length is divisible by 3.
        let checksumLength = 4 + (3 - (contentBytes.length + 2) % 3) % 3;
        let expandedLength = 4 + contentBytes.length + checksumLength;
    
        // Create the array and add the header and the content. The first three bytes turn into the user-readable
        // prefix in the encoded string. The next byte specifies the length of the content array, and it is immediately
        // followed by the content array.
        let expandedArray = new Uint8Array(expandedLength);

        for (let i = 0; i < prefixBytes.length; i++) {
            expandedArray[i] = prefixBytes[i];
        }
        
        expandedArray[3] = contentBytes.length;
        for (let i = 0; i < contentBytes.length; i++) {
            expandedArray[i + 4] = contentBytes[i];
        }
    
        // Compute the checksum and add the appropriate number of bytes to the end of the array.
        let checksum = NyzoUtil.DoubleSha256(expandedArray.subarray(0, 4 + contentBytes.length));

        for (let i = 0; i < checksumLength; i++) {
            expandedArray[expandedArray.length - checksumLength + i] = checksum[i];
        }
    
        // Build and return the encoded string from the expanded array.
        return NyzoStringEncoder.EncodedStringForByteArray(expandedArray);
    }

    static NyzoStringFromPrivateKey(byteArray) {
        return NyzoStringEncoder.EncodedNyzoString('key_', byteArray);
    }

    static NyzoStringFromPublicIdentifier(byteArray) {
        return NyzoStringEncoder.EncodedNyzoString('id__', byteArray);
    }

    static NyzoStringFromTransaction(byteArray) {
        return NyzoStringEncoder.EncodedNyzoString('tx__', byteArray);
    }
}