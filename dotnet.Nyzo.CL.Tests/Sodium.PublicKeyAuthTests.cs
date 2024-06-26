﻿using System.Collections.Generic;
using System.Text;
using Nyzo.CL.Tests.@internal;

namespace Nyzo.CL.Tests;

/// <summary>
/// <para>Only Sodium.PublicKeyAuth methods used in the Nyzo.CL library are covered by this test class.</para>
/// <para>https://asecuritysite.com/signatures/eddsa4</para>
/// <para>https://datatracker.ietf.org/doc/html/rfc8032#section-7.1</para>
/// <para>For Sodium.Core unit tests refer to:</para>
/// <para>https://github.com/ektrah/libsodium-core/blob/master/test/Sodium.Tests/PublicKeyAuthTests.cs</para>
/// </summary>
[Collection("SodiumCore.PublicKeyAuth.Tests.Sequential")]
public class SodiumPublicKeyAuthTests {
	internal List<CryptographicInlineData> Ed25519_RFC8032_TestVectors { get; init; }

	public SodiumPublicKeyAuthTests() {
		// https://datatracker.ietf.org/doc/html/rfc8032#section-7.1
		Ed25519_RFC8032_TestVectors = new() {
			new(
				"9d61b19deffd5a60ba844af492ec2cc44449c5697b326919703bac031cae7f60",
				"key_89TyJqVM_mGxLFha.9bJbch4itmGvR9G6o0ZI0ctIE.x4IUN1UTB",
				"d75a980182b10ab7d54bfed3c964073a0ee172f3daa62325af021a68f707511a",
				"id__8durD062JgHVTkM~S-CB1RFeWobRUHpA9r-26DAV1T4rm386SsPW",
				"",
				"e5564300c360ac729086e2cc806e828a84877f1eb8e5d974d873e065224901555fb8821590a33bacc61e39701cf9b46bd25bf5f0595bbe24655141438e7a100b"
			),
			new(
				"4ccd089b28ff96da9db6c346ec114e0f5b8a319f35aba624da8cf6ed4fb8a6fb",
				"key_84Rd29JF_XsrEss3hLNhjx.szA6wdrLD9dHc.LTfLasZJ_iYjqYT",
				"3d4017c3e843895a92b70aa74d1b7ebc9c982ccf2ec4968cc0cd55f12af4660c",
				"id__83T05-fFgWCrBItaGSSswIQtD2RfbJinAc3dmw4H.6pcpinVaiNb",
				"72",
				"92a009a9f0d4cab8720e820b5f642540a2b27b5416503f8fb3762223ebdb69da085ac1e43e15996e458f3613d0f11d8c387b2eaeb4302aeeb00d291612bb0c00"
			),
			new(
				"c5aa8df43f9f837bedb7442f31dcb7b166d38535076f094b85ce3a2e0b4458f7",
				"key_8cnHAwg_EWdZZsu4bR7tKZ5DSWkT1U-9iWoeezWbh5AVVeU7ohj-",
				"fc51cd8e6218a1a38da47ed00230f0580816ed13ba3303ac5deb911548908025",
				"id__8fPhRpXz6a6AArh~S08N-5x85LSjLAc3I5VIBhm8B80Chs7DZt1B",
				"af82",
				"6291d657deec24024827e69c3abe01a30ce548a284743a445e3680d7db5ac3ac18ff9b538d16f290ae67f760984dc6594a7c15e9716ed28dc027beceea1ec40a"
			),
			// The \r\n and spaces are filtered out on instantiation
			new(
				"f5e5767cf153319517630f226876b86c8160cc583bc013744c6bf255f5cc0ee5",
				"key_8foCuERPkR6m5Ucf8DyUL6Q1pcPpe-0ju4PI-CoTR0ZCDPznAWi0",
				"278117fc144c72340f67d0f2316e8386ceffbf2b2428c9c51fef7c597f1d426e",
				"id__82v15_Nkj78S3Uwg-A5LxWse_Z-I92A9Ph_Mw5C_7k9LhnCmHTB3",
				"08b8b2b733424243760fe426a4b54908\r\n   632110a66c2f6591eabd3345e3e4eb98\r\n   fa6e264bf09efe12ee50f8f54e9f77b1\r\n   e355f6c50544e23fb1433ddf73be84d8\r\n   79de7c0046dc4996d9e773f4bc9efe57\r\n   38829adb26c81b37c93a1b270b20329d\r\n   658675fc6ea534e0810a4432826bf58c\r\n   941efb65d57a338bbd2e26640f89ffbc\r\n   1a858efcb8550ee3a5e1998bd177e93a\r\n   7363c344fe6b199ee5d02e82d522c4fe\r\n   ba15452f80288a821a579116ec6dad2b\r\n   3b310da903401aa62100ab5d1a36553e\r\n   06203b33890cc9b832f79ef80560ccb9\r\n   a39ce767967ed628c6ad573cb116dbef\r\n   efd75499da96bd68a8a97b928a8bbc10\r\n   3b6621fcde2beca1231d206be6cd9ec7\r\n   aff6f6c94fcd7204ed3455c68c83f4a4\r\n   1da4af2b74ef5c53f1d8ac70bdcb7ed1\r\n   85ce81bd84359d44254d95629e9855a9\r\n   4a7c1958d1f8ada5d0532ed8a5aa3fb2\r\n   d17ba70eb6248e594e1a2297acbbb39d\r\n   502f1a8c6eb6f1ce22b3de1a1f40cc24\r\n   554119a831a9aad6079cad88425de6bd\r\n   e1a9187ebb6092cf67bf2b13fd65f270\r\n   88d78b7e883c8759d2c4f5c65adb7553\r\n   878ad575f9fad878e80a0c9ba63bcbcc\r\n   2732e69485bbc9c90bfbd62481d9089b\r\n   eccf80cfe2df16a2cf65bd92dd597b07\r\n   07e0917af48bbb75fed413d238f5555a\r\n   7a569d80c3414a8d0859dc65a46128ba\r\n   b27af87a71314f318c782b23ebfe808b\r\n   82b0ce26401d2e22f04d83d1255dc51a\r\n   ddd3b75a2b1ae0784504df543af8969b\r\n   e3ea7082ff7fc9888c144da2af58429e\r\n   c96031dbcad3dad9af0dcbaaaf268cb8\r\n   fcffead94f3c7ca495e056a9b47acdb7\r\n   51fb73e666c6c655ade8297297d07ad1\r\n   ba5e43f1bca32301651339e22904cc8c\r\n   42f58c30c04aafdb038dda0847dd988d\r\n   cda6f3bfd15c4b4c4525004aa06eeff8\r\n   ca61783aacec57fb3d1f92b0fe2fd1a8\r\n   5f6724517b65e614ad6808d6f6ee34df\r\n   f7310fdc82aebfd904b01e1dc54b2927\r\n   094b2db68d6f903b68401adebf5a7e08\r\n   d78ff4ef5d63653a65040cf9bfd4aca7\r\n   984a74d37145986780fc0b16ac451649\r\n   de6188a7dbdf191f64b5fc5e2ab47b57\r\n   f7f7276cd419c17a3ca8e1b939ae49e4\r\n   88acba6b965610b5480109c8b17b80e1\r\n   b7b750dfc7598d5d5011fd2dcc5600a3\r\n   2ef5b52a1ecc820e308aa342721aac09\r\n   43bf6686b64b2579376504ccc493d97e\r\n   6aed3fb0f9cd71a43dd497f01f17c0e2\r\n   cb3797aa2a2f256656168e6c496afc5f\r\n   b93246f6b1116398a346f1a641f3b041\r\n   e989f7914f90cc2c7fff357876e506b5\r\n   0d334ba77c225bc307ba537152f3f161\r\n   0e4eafe595f6d9d90d11faa933a15ef1\r\n   369546868a7f3a45a96768d40fd9d034\r\n   12c091c6315cf4fde7cb68606937380d\r\n   b2eaaa707b4c4185c32eddcdd306705e\r\n   4dc1ffc872eeee475a64dfac86aba41c\r\n   0618983f8741c5ef68d3a101e8a3b8ca\r\n   c60c905c15fc910840b94c00a0b9d0",
				"0aab4c900501b3e24d7cdf4663326a3a87df5e4843b2cbdb67cbf6e460fec350aa5371b1508f9f4528ecea23c436d94b5e8fcd4f681e30a6ac00a9704a188a03"
			),
			new(
				"833fe62409237b9d62ec77587520911e9a759cec1d19755b7da901b96dca3d42",
				"key_88c_Xzg98VLupLPVn7kxBhYruqRJ7hCTnVUG0sCKQAT29KvDTqPt",
				"ec172b93ad5e563bf4932c70e1245034c35467ef2efd4d64ebf819683467e2bf",
				"id__8eNoaXeKoCpZ.9cJte4Bk3j3m6wMbMTdqeMW6nxSq~a_BBr~d15y",
				"ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f",
				"dc2a4459e7369633a52b1bf277839a00201009a3efbf3ecb69bea2186c26b58909351fc9ac90b3ecfdfbc7c66431e0303dca179c138ac17ad9bef1177331a704"
			),
		};
	}

	/// <summary>
	/// <see cref="NyzoConverter.PublicIdentifierForPrivateKey(string)"/> <see cref="NyzoTransaction.Sign(byte[])"/>
	/// </summary>
	[Fact]
	public void GenerateKeyPair_FromPrivateSeedBytes_ShouldResultInCorrectKeyPair_AndPublicKeyBe32Chars() {
		foreach(var vector in this.Ed25519_RFC8032_TestVectors) {
			var sodiumKeyPair = Sodium.PublicKeyAuth.GenerateKeyPair(vector.PrivateSeedBytes);

			// An additional ByteArrayToHex call is used to assure that an eventual conversion to hex string is also covered
			Assert.Equal(vector.PublicSeedBytes.ByteArrayToHex(), sodiumKeyPair.PublicKey.ByteArrayToHex());
			Assert.Equal(32, sodiumKeyPair.PublicKey.Length);
			Assert.Equal(64, sodiumKeyPair.PrivateKey.Length);
		}
	}

	/// <summary>
	/// <see cref="NyzoTransaction.Sign(byte[])"/>
	/// </summary>
	[Fact]
	public void SigningByteArray_UsingPrivateKeyFromKeyPair_ShouldResultInCorrectSignature_AndSignatureBe64Chars() {
		foreach(var vector in this.Ed25519_RFC8032_TestVectors) {
			var sodiumKeyPair = Sodium.PublicKeyAuth.GenerateKeyPair(vector.PrivateSeedBytes);
			var signedMessage = Sodium.PublicKeyAuth.Sign(vector.MessageBytes, sodiumKeyPair.PrivateKey);
			var signature = signedMessage[..64];

			// An additional ByteArrayToHex call is used to assure that an eventual conversion to hex string is also covered
			Assert.Equal(vector.ExpectedSignatureBytes.ByteArrayToHex(), signature.ByteArrayToHex());
		}
	}

	/// <summary>
	/// <see cref="NyzoUtil.IsValidSignedMessage(string, string)"/> <see cref="NyzoUtil.GetSignedMessageContent(string, string)"/>
	/// </summary>
	[Fact]
	public void VerifyingByteArray_UsingPublicKeyFromKeyPair_ShouldReturnByteArrayMessage_OrThrowWhenInvalid() {
		foreach(var vector in this.Ed25519_RFC8032_TestVectors) {
			var sodiumKeyPair = Sodium.PublicKeyAuth.GenerateKeyPair(vector.PrivateSeedBytes);
			var signedMessage = Sodium.PublicKeyAuth.Sign(vector.MessageBytes, sodiumKeyPair.PrivateKey);
			var signature = signedMessage[..64];

			// No Assert needed, a throw here from within Sodium.Core would cause the test to fail
			Sodium.PublicKeyAuth.Verify(signedMessage, sodiumKeyPair.PublicKey);

			Assert.Equal(vector.ExpectedSignatureBytes.ByteArrayToHex(), signature.ByteArrayToHex());
		}
	}
}
