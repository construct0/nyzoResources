using System;

namespace Nyzo.CL.Tests.@internal;

/// <summary>
/// Inspired by <see cref="InlineDataAttribute"/> but allowing for non-const arguments to be provided.
/// Still requires you to use <see cref="FactAttribute"/>, classes are not constants and thus can not be referenced within a InlineData attribute, they do however enable less cluttered tests to be written.
/// </summary>
internal record CryptographicInlineData
{
    public byte[] PrivateSeedBytes { get; init; }
    public byte[] PublicSeedBytes { get; init; }
    public byte[] MessageBytes { get; init; }
    public byte[] ExpectedSignatureBytes { get; init; }

	#region NyzoString 
	// Any and all NyzoString properties' values set in the test project were obtained by converting the IETF test vectors to NyzoStrings using the NyzoSpace tool
	// https://angainordev.github.io/NyzoSpace/js/dist/convert.html
	// https://github.com/AngainorDev/NyzoSpace
	public string? NyzoStringPublicIdentifier { get; init; }
    public string? NyzoStringPrivateKey { get; init; }

    public CryptographicInlineData(string privateSeed, string nyzoStringPrivateKey, string publicSeed, string nyzoStringPublicIdentifier, string message, string expectedSignature) : this(privateSeed, publicSeed, message, expectedSignature) {
        this.NyzoStringPrivateKey = nyzoStringPrivateKey;
        this.NyzoStringPublicIdentifier = nyzoStringPublicIdentifier;
    }
	#endregion

	public CryptographicInlineData(string privateSeed, string publicSeed, string message, string expectedSignature)
    {
        Func<string, string> cleanHex = (i) =>
        {
            return i.Replace("\r", "")
                    .Replace("\n", "")
                    .Replace(" ", "")
                    .Trim()
                    ;
        };

        PrivateSeedBytes = cleanHex(privateSeed).HexToByteArray();
        PublicSeedBytes = cleanHex(publicSeed).HexToByteArray();
        MessageBytes = cleanHex(message).HexToByteArray();
        ExpectedSignatureBytes = cleanHex(expectedSignature).HexToByteArray();
    }
}
