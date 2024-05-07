using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;

namespace Nyzo.CL;

// Untested - todo
public static class NyzoUtil {
    public static byte[] HexStringAsUint8Array(string identifier){
        identifier = identifier.Replace("-", "");

        var array = new byte[identifier.Length / 2];

        for (var i = 0; i < array.Length; i++) {
            array[i] = Convert.ToByte(identifier.Substring(i * 2, i * 2 + 2), 16);
        }

        return array;
    }

    // Ignored: https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1850
    public static byte[] Sha256Uint8(byte[] array){
        using(SHA256 sha256 = SHA256.Create()){
            byte[] hashBytes = sha256.ComputeHash(array);
            string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return NyzoUtil.HexStringAsUint8Array(hashString);
        }
    }

    public static byte[] DoubleSha256(byte[] array){
        return NyzoUtil.Sha256Uint8(
            NyzoUtil.Sha256Uint8(array)
        );
    }

    public static byte[] SenderDataAsUint8Array(string senderData){
        // Process normalized sender-data strings
        byte[]? array = null;

        if(senderData.Length == 67){
            var lowercase = senderData.ToLower();

            if(lowercase[0] == 'x' && lowercase[1] == '(' && lowercase[66] == ')'){
                // Get the underscore index to determine the length of the data
                var underscoreIndex = lowercase.IndexOf('_');

                if(underscoreIndex < 0){
                    underscoreIndex = NyzoConstants.MaximumSenderDataLength * 2 + 2;
                }

                var dataLength = underscoreIndex / 2 - 1;

                // Ensure that all characters in the data field are correct. The left section must be all alphanumeric, and the right section must be underscores. The string was converted to lowercase.
                var allAreCorrect = true;

                for(var i=0; i<66 && allAreCorrect; i++){
                    // This could be written more succinctly, but it would be more difficult to read
                    if(i < underscoreIndex){
                        allAreCorrect = (lowercase[i] >= '0' && lowercase[i] <= '9') || (lowercase[i] >= 'a' && lowercase[i] <= 'f');
                    } else {
                        allAreCorrect = lowercase[i] == '_';
                    }
                }

                // If all characters are correct, decode the data. Otherwise, leave the result null to indicate that the input is not a valid sender-data string.
                if(allAreCorrect){
                    array = NyzoUtil.HexStringAsUint8Array(senderData.Substring(2, 2 + dataLength * 2));
                }
            }
        }

        // If processing of a normalized sender-data string did not produce a result, process as a plain-text string
        array ??= Encoding.Unicode.GetBytes(senderData, 0, Math.Min(senderData.Length, 32));

        return array;
    }

    public static bool IsValidAmountOfMicroNyzos(object value){
        if(value is not null && value is IConvertible){
            bool canParse = double.TryParse(value.ToString(), out _);

            if(canParse){
                var parsedValue = double.Parse(value.ToString()!);

                if(!(parsedValue < NyzoConstants.MinimumTransactionAmount)){
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsValidPrivateKey(string keyString){
        var isValid = false;

        keyString = keyString.Trim();
        var key = NyzoStringEncoder.DecodePrivateSeed(keyString);
        isValid = key is not null;

        return isValid;
    }

    public static bool IsValidPublicIdentifier(string identifierString){
        var isValid = false;

        identifierString = identifierString.Trim();
        var identifier = NyzoStringEncoder.DecodePublicIdentifier(identifierString);
        isValid = identifier is not null;

        return isValid;
    }

    public static bool IsValidSignedMessage(string signedMessageString, string publicIdentifierString){
        publicIdentifierString = publicIdentifierString.Trim();
        var identifier = NyzoStringEncoder.DecodePublicIdentifier(publicIdentifierString);
        var signedMessage = NyzoStringEncoder.ByteArrayForEncodedString(signedMessageString);

        if(identifier?.Identifier is null || signedMessage is null){
            return false;
        }

        try {
            Sodium.PublicKeyAuth.Verify(signedMessage, identifier.Identifier);
        } catch {
            return false;
        }

        return true;
    }

    // This assumes you called IsValidSignedMessage already
    public static byte[] GetSignedMessageContent(string signedMessageString, string publicIdentifierString){
        publicIdentifierString = publicIdentifierString.Trim();
        var identifier = NyzoStringEncoder.DecodePublicIdentifier(publicIdentifierString);
        var signedMessage = NyzoStringEncoder.ByteArrayForEncodedString(signedMessageString);

        if(identifier?.Identifier is null || signedMessage is null){
            throw new ArgumentException("[0]: Could not get content, validate your arguments with NyzoUtil.IsValidSignedMessage first");
        }

        try {
            return Sodium.PublicKeyAuth.Verify(signedMessage, identifier.Identifier);
        } catch {
            throw new CryptographicException("[1]: Could not get content, validate your arguments with NyzoUtil.IsValidSignedMessage first");
        }
    }

    // This is not a robust check for valid/invalid URLs. It is just a check to ensure that the provided URL is somewhat reasonable for use as a client URL.
    public static bool IsValidClientURL(string clientUrl){
        var isValid = false;

        clientUrl = clientUrl.Trim();
        isValid = 
            (clientUrl.StartsWith("http://") || clientUrl.StartsWith("https://")) 
            && !clientUrl.Contains('<') 
            && !clientUrl.Contains('>') 
            && !clientUrl.Contains('?') 
            && !clientUrl.Contains(' ') 
            && !clientUrl.Contains('%')
            ;

        return isValid;
    }

}