using System;
using System.Linq;

namespace Nyzo.CL.Tests.@internal;

internal static class StringExtensions
{
	// The functionality of this should be identical to NyzoUtil.HexStringAsUint8Array but the performance may be considerably worse due to using LINQ here,
	// and dependant on the .NET version
    internal static byte[] HexToByteArray(this string hex)
    {
		return Enumerable.Range(0, hex.Length)
						 .Where(x => x % 2 == 0)
						 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
						 .ToArray()
						 ;
	}
}
