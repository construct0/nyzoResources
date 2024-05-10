using Nyzo.CL.Tests.@internal;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Nyzo.CL.Tests;

public class NyzoUtilTests {
	[Fact]
	public void ConvertingHexStringToByteArray_ShouldBeAccurate() {
		Assert.Equal("6efd383745a964768989b9df420811abc6e5873f874fc22a76fe9258e020c2e1".HexToByteArray(), NyzoUtil.HexStringAsByteArray("6efd383745a964768989b9df420811abc6e5873f874fc22a76fe9258e020c2e1"));
	}

	// While the tests within this region may seem useless, it is still important to validate that these wrappers produce an accurate result
	#region sha256
	// This test relies on the method HexStringAsByteArray functioning properly, which is being tested in the test method above
	// The "native JS output" part can be generated using the example provided here:
	// https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/digest#converting_a_digest_to_a_hex_string
	// Mirror with intructions:
	// https://pastebin.com/MxJeKqhR
	[Theory]
	[InlineData("An obscure body in the S-K System, your majesty. The inhabitants refer to it as the planet Earth.", "6efd383745a964768989b9df420811abc6e5873f874fc22a76fe9258e020c2e1")]
	public void ConvertingByteArrayToSha256ByteArray_ShouldMatchNativeJSOutput(string text, string expectedSha256String) {
		var text_AsByteArray = Encoding.UTF8.GetBytes(text);

		// Act
		var result_byteArray = NyzoUtil.ByteArrayAsSha256ByteArray(text_AsByteArray);

		// The resulting byte array is converted back to a hex string using an internal & independent extension method
		var result_hexString = result_byteArray.ByteArrayToHex();

		// Assert
		Assert.Equal(expectedSha256String, result_hexString);
	}

	// The content of the pastebin mirror for obtaining a single sha256 has been modified to provide you with a double sha256 in JS format:
	// https://pastebin.com/7v1tuAWG
	[Theory]
	[InlineData("An obscure body in the S-K System, your majesty. The inhabitants refer to it as the planet Earth.", "6efd383745a964768989b9df420811abc6e5873f874fc22a76fe9258e020c2e1", "255c3a66cd15953821c8c3a04dc22d10a826209a705d13f8072f674c2bf3294c")]
	public void ConvertingByteArrayToDoubleSha256ByteArray_ShouldBeAccurate(string text, string expectedSha256String, string expectedSha256OfSha256String) {
		var text_AsByteArray = Encoding.UTF8.GetBytes(text);

		// Act
		var result_byteArray = NyzoUtil.ByteArrayAsDoubleSha256ByteArray(text_AsByteArray);


		// The resulting byte array is converted back to a hex string using an internal & independent extension method
		var result_hexString = result_byteArray.ByteArrayToHex();

		// Assert
		Assert.Equal(expectedSha256OfSha256String, result_hexString);
	}
	#endregion

	// Tests handling of normalized sender data strings
	// https://tech.nyzo.org/dataFormats/normalizedSenderDataString
	[Theory]
	[InlineData("X(4e5454502d323a20297632ff2f9c810c2cca1f3a2b3ac320aea04ac9________)")]
	[InlineData("X(________________________________________________________________)")]
	[InlineData("X(4e797a6f________________________________________________________)")]
	public void ConvertingSenderDataToByteArray_ShouldBeAccurate(string input) {
		var lower = input.ToLower();
		var upper = input.ToUpper();
		var lowerResult = NyzoUtil.SenderDataAsByteArray(lower);
		var upperResult = NyzoUtil.SenderDataAsByteArray(upper);

		var x = lowerResult.ByteArrayToHex();
		var y = upperResult.ByteArrayToHex();

		// case insensitive
		Assert.Equal(lowerResult, upperResult);

		// non-normalized output
		Func<string, byte[]> nonNormalizedOutput = (string x) => {
			return Encoding.UTF8.GetBytes(x, 0, Math.Min(x.Length, 32));
		};

		// -1 char
		var charMissing = new StringBuilder(lower).Length--.ToString();
		Assert.Equal(nonNormalizedOutput(charMissing), NyzoUtil.SenderDataAsByteArray(charMissing));

		// first char != x 
		var firstCharWrong = lower.Replace("x", "-");
		Assert.Equal(nonNormalizedOutput(firstCharWrong), NyzoUtil.SenderDataAsByteArray(firstCharWrong));

		// second char != (
		var secondCharWrong = lower.Replace("(", "-");
		Assert.Equal(nonNormalizedOutput(secondCharWrong), NyzoUtil.SenderDataAsByteArray(secondCharWrong));

		// last char != )
		var lastCharWrong = lower.Replace(")", "-");
		Assert.Equal(nonNormalizedOutput(lastCharWrong), NyzoUtil.SenderDataAsByteArray(lastCharWrong));

		// left and right side reversed
		var amtUnderScores = lower.ToList().Where(x => x == '_').Count();
		var underScorePart = new StringBuilder();
		for(int i=0; i < amtUnderScores; i++) {
			underScorePart.Append("_");
		}
		var underScorePartString = underScorePart.ToString();
		var reversed = lower.Replace("_", "");
		reversed = reversed.Replace("x(", "x(" + underScorePartString);

		// This test is not performed for a normalized sender data string of which the content between the brackets consists out of only underscores
		if(amtUnderScores < 64) {
			Assert.Equal(nonNormalizedOutput(reversed), NyzoUtil.SenderDataAsByteArray(reversed));
		}
		
		// 2-step increment is wrong
		var firstUnderscoreIndex = lower.IndexOf("_");
		var extraHexChar = new StringBuilder(lower);
		extraHexChar[firstUnderscoreIndex] = 'a';
		var extraHexCharString = extraHexChar.ToString();

		// This test is not performed for a normalized sender data string of which the content between the brackets consists out of only underscores
		if (amtUnderScores < 64) {
			Assert.Equal(nonNormalizedOutput(extraHexCharString), NyzoUtil.SenderDataAsByteArray(extraHexCharString));
		}
	

	}

	[Theory]
	[InlineData(null)]
	[InlineData(0)]
	[InlineData("0")]
	[InlineData("0.000000")]
	[InlineData(100_000_000.000001)]
	[InlineData("100000000.000001")]
	public void DeterminingIfAmountOfNyzosIsValid_ShouldReturnFalse(object? input) {
		Assert.False(NyzoUtil.IsValidAmountOfNyzos(input));
	}

	[Theory]
	[InlineData(0.000001)]
	[InlineData("0.000001")]
	[InlineData(100_000_000)]
	[InlineData("100000000")]
	public void DeterminingIfAmountOfNyzosIsValid_ShouldReturnTrue(object? input) {
		Assert.True(NyzoUtil.IsValidAmountOfNyzos(input));
	}


	[Theory]
	[InlineData(null)]
	[InlineData(0)]
	[InlineData("0")]
	[InlineData(100_000_000_000_001)]
	[InlineData("100000000000001")]
	public void DeterminingIfAmountOfMicroNyzosIsValid_ShouldReturnFalse(object? input) {
		Assert.False(NyzoUtil.IsValidAmountOfMicroNyzos(input));
	}

	[Theory]
	[InlineData(1)]
	[InlineData("1")]
	[InlineData(100_000_000_000_000)]
	[InlineData("100000000000000")]
	public void DeterminingIfAmountOfMicroNyzosIsValid_ShouldReturnTrue(object? input) {
		Assert.True(NyzoUtil.IsValidAmountOfMicroNyzos(input));
	}

	// This merely tests if the value is trimmed, everything else is up to NyzoStringEncoder
	[Fact]
	public void DeterminingIfIsValidPrivateSeed_ShouldTrim() {
		foreach(var vector in new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors) {
			Assert.True(NyzoUtil.IsValidPrivateSeed("  " + vector.NyzoStringPrivateKey + "  \n\r"));
		}
	}

	// This merely tests if the value is trimmed, everything else is up to NyzoStringEncoder
	[Fact]
	public void DeterminingIfIsValidPublicIdentifier_ShouldTrim() {
		foreach (var vector in new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors) {
			Assert.True(NyzoUtil.IsValidPublicIdentifier("  " + vector.NyzoStringPublicIdentifier + "  \n\r"));
		}
	}

	[Fact]
	public void DeterminingIfIsValidSignedMessage_ShouldTrimPublicIdentifier() {
		foreach (var vector in new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors) {
			var sodiumKeyPair = Sodium.PublicKeyAuth.GenerateKeyPair(vector.PrivateSeedBytes);
			var signedMessage = Sodium.PublicKeyAuth.Sign(vector.MessageBytes, sodiumKeyPair.PrivateKey);

			Assert.True(NyzoUtil.IsValidSignedMessage(signedMessage.ByteArrayToHex(), "   " + vector.NyzoStringPublicIdentifier + "   \r\n"));

			Assert.True(NyzoUtil.IsValidSignedMessage(signedMessage.ByteArrayToHex(), "  " + sodiumKeyPair.PublicKey.ByteArrayToHex()  + "   \r\n"));
			Assert.True(NyzoUtil.IsValidSignedMessage(signedMessage.ByteArrayToHex(), "   " + vector.PublicSeedBytes.ByteArrayToHex() + "   \r\n"));

			Assert.True(NyzoUtil.IsValidSignedMessage(signedMessage, sodiumKeyPair.PublicKey));
			Assert.True(NyzoUtil.IsValidSignedMessage(signedMessage, vector.PublicSeedBytes));
		}
	}

	[Fact]
	public void GetSignedMessageContent_ShouldTrimPublicIdentifier() {
		foreach (var vector in new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors) {
			var sodiumKeyPair = Sodium.PublicKeyAuth.GenerateKeyPair(vector.PrivateSeedBytes);
			var signedMessage = Sodium.PublicKeyAuth.Sign(vector.MessageBytes, sodiumKeyPair.PrivateKey);

			// No asserts are needed, an exception fails the test
			NyzoUtil.GetSignedMessageContent(signedMessage.ByteArrayToHex(), "   " + vector.NyzoStringPublicIdentifier + "   \r\n");

			NyzoUtil.GetSignedMessageContent(signedMessage.ByteArrayToHex(), "  " + sodiumKeyPair.PublicKey.ByteArrayToHex()  + "   \r\n");
			NyzoUtil.GetSignedMessageContent(signedMessage.ByteArrayToHex(), "   " + vector.PublicSeedBytes.ByteArrayToHex() + "   \r\n");

			NyzoUtil.GetSignedMessageContent(signedMessage, sodiumKeyPair.PublicKey);
			NyzoUtil.GetSignedMessageContent(signedMessage, vector.PublicSeedBytes);
		}
	}
}
