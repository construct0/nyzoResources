using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nyzo.CL;

public static class NyzoStringEncoder {
    public static char[] CharacterLookup => ("0123456789" + "abcdefghijkmnopqrstuvwxyz" + "ABCDEFGHIJKLMNPQRSTUVWXYZ" + "-.~_").ToArray();

    public static Dictionary<char, int> CharacterToValueDict {get {
        var characterLookup = NyzoStringEncoder.CharacterLookup;
        var characterToValueDict = new Dictionary<char, int>();

        for (var i = 0; i < characterLookup.Length; i++) {
            characterToValueDict[Convert.ToChar(characterLookup[i])] = i;
        }

        return characterToValueDict;
    }}

    // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/TypedArray/subarray
    private static byte[] ProvideSubArray(byte[] input, int inclusiveBegin, int exclusiveEnd){
        int subArrayLength = exclusiveEnd - inclusiveBegin;
        byte[] subArray = new byte[subArrayLength];
        Array.Copy(input, inclusiveBegin, subArray, 0, subArrayLength);

        return subArray;
    }

    // Null if this fails, after which the other Decode[..] functions below also return null
    private static byte[]? DecodeToContentBytes(string encodedString){
        byte[]? contentBytes = null;
        encodedString = encodedString.Trim();

        // Map characters from the old encoding to the new encoding. A few characters were changed to make Nyzo strings more URL-friendly
        encodedString = Regex.Replace(encodedString, "/\\*/g", "-");
        encodedString = Regex.Replace(encodedString, "/\\+/g", ".");
        encodedString = Regex.Replace(encodedString, "/=/g", "~");

        // Map characters that may be mistyped. Nyzo strings contain neither 'l' nor 'O'
        encodedString = Regex.Replace(encodedString, "/l/g", "1");
        encodedString = Regex.Replace(encodedString, "/O/g", "0");

        // Get the prefix
        var prefix = encodedString.Substring(0, 4);

        // Get the array representation of the encoded string
        byte[] expandedArray = NyzoStringEncoder.ByteArrayForEncodedString(encodedString);

        // Get the content length from the next byte and calculate the checksum length
        var contentLength = expandedArray[3] & 0xff;
        var checksumLength = expandedArray.Length - contentLength - 4;

        // Return null if the checksum length is invalid
        if(!(checksumLength >= 4 && checksumLength <= 6)){
            return null;
        }

        // The checksum length is valid
        // Calculate the checksum and compare it to the provided checksum. Only create the result array if the checksums match
        var headerLength = 4;

        var calculatedChecksum =
            NyzoStringEncoder.ProvideSubArray(
                NyzoUtil.DoubleSha256(
                    NyzoStringEncoder.ProvideSubArray(expandedArray, 0, headerLength + contentLength)
                ),
                0,
                checksumLength
            );

        var providedChecksum = NyzoStringEncoder.ProvideSubArray(expandedArray, expandedArray.Length - checksumLength, expandedArray.Length);

        if(calculatedChecksum.SequenceEqual(providedChecksum)){
            // Get the content array. This is the encoded object with the prefix, length byte, and checksum removed
            contentBytes = NyzoStringEncoder.ProvideSubArray(expandedArray, headerLength, expandedArray.Length - checksumLength);
        }

        return contentBytes;
    }

    public static NyzoStringPrivateSeed? DecodePrivateSeed(string encodedString){
        // An additional prefix validation is performed
        if(!(encodedString.Length >= 4 || encodedString[..4] == "key_")){
            return null;
        }

        var contentBytes = NyzoStringEncoder.DecodeToContentBytes(encodedString);

        return contentBytes is null ? null : new NyzoStringPrivateSeed(contentBytes);
    }

    public static NyzoStringPublicIdentifier? DecodePublicIdentifier(string encodedString){
        // An additional prefix validation is performed
        if(!(encodedString.Length >= 4 || encodedString[..4] == "id__")){
            return null;
        }

        var contentBytes = NyzoStringEncoder.DecodeToContentBytes(encodedString);

        return contentBytes is null ? null : new NyzoStringPublicIdentifier(contentBytes);
    }

    public static NyzoStringPrefilledData? DecodePrefilledData(string encodedString){
        // An additional prefix validation is performed
        if(!(encodedString.Length >= 4 || encodedString[..4] == "pre_")){
            return null;
        }

        var contentBytes = NyzoStringEncoder.DecodeToContentBytes(encodedString);

        return contentBytes is null ? null : new NyzoStringPrefilledData(
            NyzoStringEncoder.ProvideSubArray(contentBytes, 0, 32),
            NyzoStringEncoder.ProvideSubArray(contentBytes, 33, contentBytes.Length)
        );
    }

    public static NyzoTransaction? DecodeNyzoTransaction(string encodedString){
        // An additional prefix validation is performed
        if(!(encodedString.Length >= 4 || encodedString[..4] == "tx__")){
            return null;
        }

        var contentBytes = NyzoStringEncoder.DecodeToContentBytes(encodedString);

        return contentBytes is null ? null : NyzoTransaction.FromBytes(contentBytes);
    }

    public static byte[] ByteArrayForEncodedString(string encodedString){
        var characterToValueDict = NyzoStringEncoder.CharacterToValueDict;

        var arrayLength = (encodedString.Length * 6 + 7) / 8;
        var array = new byte[arrayLength];

        for(var i=0; i<arrayLength; i++){
            var leftCharacter = encodedString[i * 8 / 6];
            var rightCharacter = encodedString[i * 8 / 6 + 1];

            var leftValue = characterToValueDict[leftCharacter];
            var rightValue = characterToValueDict[rightCharacter];
            var bitOffset = (i * 2) % 6;

            array[i] = (byte)((((leftValue << 6) + rightValue) >> 4 - bitOffset) & 0xff);
        }

        return array;
    }

    public static string EncodedStringForByteArray(byte[] array){
        var characterLookup = NyzoStringEncoder.CharacterLookup;

        var index = 0;
        var bitOffset = 0;
        var encodedString = "";

        while(index < array.Length){
            // Get the current and next byte
            var leftByte = array[index] & 0xff;
            var rightByte = index < array.Length - 1 ? array[index + 1] & 0xff : 0;

            // Append the character for the next 6 bits in the array
            var lookupIndex = (((leftByte << 8) + rightByte) >> (10 - bitOffset)) & 0x3f;
            encodedString += characterLookup[lookupIndex];

            // Advance forward 6 bits
            if (bitOffset == 0) {
                bitOffset = 6;
            } else {
                index++;
                bitOffset -= 2;
            }
        }

        return encodedString;
    }

    private static string EncodedNyzoString(string prefix, byte[] contentBytes){
        // Get the prefix array from the type
        var prefixBytes = NyzoStringEncoder.ByteArrayForEncodedString(prefix);

        // Determine the length of the expanded array with the header and the checksum. The header is the type-specific prefix in characters followed by a single byte that indicates the length of the content array (four bytes total). The checksum is a minimum of 4 bytes and a maximum of 6 bytes, widening the expanded array so that its length is divisible by 3.
        var checksumLength = 4 + (3 - (contentBytes.Length + 2) % 3) % 3;
        var expandedLength = 4 + contentBytes.Length + checksumLength;

        // Create the array and add the header and the content. The first three bytes turn into the user-readable prefix in the encoded string. The next byte specifies the length of the content array, and it is immediately followed by the content array.
        var expandedArray = new byte[expandedLength];

        for (var i = 0; i < prefixBytes.Length; i++) {
            expandedArray[i] = prefixBytes[i];
        }
        
        expandedArray[3] = (byte)contentBytes.Length;
        for (var i = 0; i < contentBytes.Length; i++) {
            expandedArray[i + 4] = contentBytes[i];
        }

        // Compute the checksum and add the appropriate number of bytes to the end of the array
        var checksum = NyzoUtil.DoubleSha256(
            NyzoStringEncoder.ProvideSubArray(expandedArray, 0, 4 + contentBytes.Length)
        );

        for (var i = 0; i < checksumLength; i++) {
            expandedArray[expandedArray.Length - checksumLength + i] = checksum[i];
        }

        // Build and return the encoded string from the expanded array
        return NyzoStringEncoder.EncodedStringForByteArray(expandedArray);
    }

    public static string NyzoStringFromPrivateKey(byte[] array){
        return NyzoStringEncoder.EncodedNyzoString("key_", array);
    }

    public static string NyzoStringFromPublicIdentifier(byte[] array){
        return NyzoStringEncoder.EncodedNyzoString("id__", array);
    }

    public static string NyzoStringFromTransaction(byte[] array){
        return NyzoStringEncoder.EncodedNyzoString("tx__", array);
    }
    
}

