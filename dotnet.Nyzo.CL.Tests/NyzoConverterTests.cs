using Nyzo.CL.Tests.@internal;
using System.Collections.Generic;

namespace Nyzo.CL.Tests;


public class NyzoConverterTests {
	private List<CryptographicInlineData> _vectors { get; init; } = new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors;

	[Fact]
	public void PublicIdentifierForPrivateKey_ShouldReturnInstance_OrNull() {
		foreach(var vector in _vectors) {
			Assert.Equal(vector.NyzoStringPublicIdentifier, NyzoConverter.PublicIdentifierForPrivateKey(vector.NyzoStringPrivateKey!));
		}
	}

	[Fact]
	public void GetDisplayAmount_ShouldBeAccurate() {
		var amount = 1;

		Assert.EndsWith("0.000001", NyzoConverter.GetDisplayAmount(amount, true));
		Assert.EndsWith("1.000000", NyzoConverter.GetDisplayAmount(amount, false));
	}
}
