using System;

namespace Nyzo.CL.Tests.@internal;

internal static class ByteArrayExtensions
{
    internal static string ByteArrayToHex(this byte[] array)
    {
        return BitConverter.ToString(array)
                           .Replace("-", "")
                           .ToLower()
                           ;
    }
}
