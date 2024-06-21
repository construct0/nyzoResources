using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;

namespace Nyzo.CL;

public static class NyzoUtil {
    public static string ByteArrayAsHexString(byte[] array) {
        return BitConverter.ToString(array)
                           .Replace("-", "")
                           .ToLower()
                           ;
    }

    public static byte[] HexStringAsByteArray(string identifier){
        identifier = identifier.Replace("-", "");

        var array = new byte[identifier.Length / 2];

        for (var i = 0; i < array.Length; i++) {
            array[i] = Convert.ToByte(identifier.Substring(i * 2, 2), 16);
        }

        return array;
    }

	// Ignored: https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1850
	public static byte[] ByteArrayAsSha256ByteArray(byte[] array) {
		using (SHA256 sha256 = SHA256.Create()) {
			byte[] hashBytes = sha256.ComputeHash(array);

            return hashBytes;
		}
	}

    public static byte[] ByteArrayAsDoubleSha256ByteArray(byte[] array){
        return NyzoUtil.ByteArrayAsSha256ByteArray(
            NyzoUtil.ByteArrayAsSha256ByteArray(array)
        );
    }

	/// <summary>
	/// Argument should be a UTF-8 sender data string. Conversion is case-insensitive for a normalized sender data string.
	/// </summary>
	/// <param name="senderData">UTF-8 sender data string</param>
	/// <returns>A byte array, the content of the byte array depends on whether the sender data is normalized or not. (https://tech.nyzo.org/dataFormats/normalizedSenderDataString)</returns>
	public static byte[] SenderDataAsByteArray(string senderData){
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

                for(var i=2; i<66 && allAreCorrect; i++){
                    // This could be written more succinctly, but it would be more difficult to read
                    if(i < underscoreIndex){
                        allAreCorrect = (lowercase[i] >= '0' && lowercase[i] <= '9') || (lowercase[i] >= 'a' && lowercase[i] <= 'f');
                    } else {
                        allAreCorrect = lowercase[i] == '_';
                    }
                }

                if (allAreCorrect) {
                    var amtUnderScores = senderData.ToList().Where(x => x == '_').Count();

                    // The amount of characters on the left and right side (data and underscores) should be divisible by 2, the presence of 0 underscores indicates that the entire 64 character span has been utilized by hexadecimal characters
                    if(amtUnderScores > 0 && (amtUnderScores % 2) != 0) {
                        allAreCorrect = false;
                    }
				}

                // If all characters are correct, decode the data. Otherwise, leave the result null to indicate that the input is not a valid sender-data string.
                if(allAreCorrect){
                    array = NyzoUtil.HexStringAsByteArray(senderData.Substring(2, dataLength * 2));
                }
            }
        }

        // If processing of a normalized sender-data string did not produce a result, process as a plain-text string
        array ??= Encoding.UTF8.GetBytes(senderData, 0, Math.Min(senderData.Length, 32));

        return array;
    }

    public static bool IsValidAmountOfNyzos(object? value) {
		if (value is not null && value is IConvertible) {
			bool canParse = double.TryParse(value.ToString(), out _);

			if (canParse) {
				var parsedValue = double.Parse(value.ToString()!);

				if (!(parsedValue < NyzoConstants.MinimumTransactionAmount || parsedValue > NyzoConstants.TotalNyzosAvailable)) {
					return true;
				}
			}
		}

		return false;
	}

    public static bool IsValidAmountOfMicroNyzos(object? value){
        if(value is not null && value is IConvertible){
            bool canParse = long.TryParse(value.ToString(), out _);

            if(canParse){
                var parsedValue = long.Parse(value.ToString()!);

                if(!(parsedValue < 1 || parsedValue > NyzoConstants.TotalMicroNyzosAvailable)){
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsValidPrivateSeed(string seedString){
        var isValid = false;

        seedString = seedString.Trim();
        var key = NyzoStringEncoder.DecodePrivateKey(seedString);
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

    public static bool IsValidSignedMessage(string signedMessage, string publicIdentifier) {
        publicIdentifier = publicIdentifier.Trim();

        return NyzoUtil.IsValidSignedMessage(
            NyzoUtil.HexStringAsByteArray(signedMessage),
			publicIdentifier.StartsWith("id__")
            ? NyzoStringEncoder.DecodePublicIdentifier(publicIdentifier)?.Identifier ?? new byte[0]
            : NyzoUtil.HexStringAsByteArray(publicIdentifier)
        );
    }

    public static bool IsValidSignedMessage(byte[]? signedMessage, byte[] publicIdentifier){
        if(signedMessage is null){
            return false;
        }

        try {
            Sodium.PublicKeyAuth.Verify(signedMessage, publicIdentifier);
        } catch {
            return false;
        }

        return true;
    }

    // This assumes you called IsValidSignedMessage already
    public static byte[] GetSignedMessageContent(string signedMessage, string publicIdentifier){
		publicIdentifier = publicIdentifier.Trim();

		return NyzoUtil.GetSignedMessageContent(
			NyzoUtil.HexStringAsByteArray(signedMessage),
			publicIdentifier.StartsWith("id__")
			? NyzoStringEncoder.DecodePublicIdentifier(publicIdentifier)?.Identifier ?? new byte[0]
			: NyzoUtil.HexStringAsByteArray(publicIdentifier)
		);
	}

	public static byte[] GetSignedMessageContent(byte[] signedMessage, byte[] publicIdentifier) {
        try {
            return Sodium.PublicKeyAuth.Verify(signedMessage, publicIdentifier);
		} catch(CryptographicException e) {
            throw new InvalidOperationException("Validate the arguments with IsValidSignedMessage first", e);
        }
	}

    public static byte[] SignMessage(string message, NyzoStringPrivateKey nyzoStringPrivateKey) {
        return Sodium.PublicKeyAuth.Sign(message, nyzoStringPrivateKey.KeyPair.PrivateKey);
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