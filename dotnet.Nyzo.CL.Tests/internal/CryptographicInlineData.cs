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
