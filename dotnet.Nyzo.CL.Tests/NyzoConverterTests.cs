using Nyzo.CL.Tests.@internal;
using System.Collections.Generic;

namespace Nyzo.CL.Tests;


public class NyzoConverterTests {
	private List<CryptographicInlineData> _vectors { get; init; } = new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors;

	[Fact]
	public void PublicIdentifierForPrivateKey_ShouldReturnInstance() {
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

	[Fact]
	public void GetAmountOfMicroNyzos_ShouldBeAccurate() {
		Assert.Equal(0, NyzoConverter.GetAmountOfMicroNyzos("0.0000005"));
		Assert.Equal(1, NyzoConverter.GetAmountOfMicroNyzos("0.000001"));
		Assert.Equal(100_000_000__000_000, NyzoConverter.GetAmountOfMicroNyzos("100000000"));
	}
}
