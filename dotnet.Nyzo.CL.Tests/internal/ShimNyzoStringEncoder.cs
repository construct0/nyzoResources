namespace Nyzo.CL.Tests.@internal;

/// <summary>
/// Methods in NyzoStringEncoder which aren't intended to be publicly accessible have been assigned the "protected" keyword, this shim class then makes the method accessible internally, i.e. within the Nyzo.CL.Tests namespace
/// </summary>
internal sealed class ShimNyzoStringEncoder : NyzoStringEncoder
{
    public static byte[] _ProvideSubArray(byte[] input, int inclusiveBegin, int exclusiveEnd)
    {
        return ProvideSubArray(input, inclusiveBegin, exclusiveEnd);
    }

    public static byte[]? _DecodeToContentBytes(string encodedString)
    {
        return DecodeToContentBytes(encodedString);
    }

    public static string _EncodedNyzoString(string prefix, byte[] contentBytes) {
        return EncodedNyzoString(prefix, contentBytes);
    }
}
