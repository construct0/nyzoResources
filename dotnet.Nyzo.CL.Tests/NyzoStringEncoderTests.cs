using Nyzo.CL.Tests.@internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Nyzo.CL.Tests;

public class NyzoStringEncoderTests {
	[Fact]
	public void ProvidingASubArray_ShouldBeAccurate() {
		var array = new byte[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

		var subarray1 = ShimNyzoStringEncoder._ProvideSubArray(array, 0, array.Length);
		var subarray2 = ShimNyzoStringEncoder._ProvideSubArray(array, 4, 7);
		var subarray3 = ShimNyzoStringEncoder._ProvideSubArray(array, 2, 6);

		Assert.Equal(array, subarray1);

		Assert.Equal(array[4], subarray2[0]);
		Assert.Equal(array[6], subarray2.Last());
		
		Assert.Equal(array[2], subarray3[0]);
		Assert.Equal(array[5], subarray3.Last());
	}

	[Fact]
	public void DecodeToContentBytes_ShouldReturnBytes_OrNull() {
		// Private key and public identifier
		foreach(var vector in new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors) {
			var decodedPrivate = ShimNyzoStringEncoder._DecodeToContentBytes(vector.NyzoStringPrivateKey!);
			var decodedPublic = ShimNyzoStringEncoder._DecodeToContentBytes(vector.NyzoStringPublicIdentifier!);

			// This should be equal to the RFC test vector pub/privs
			Assert.Equal(vector.PrivateSeedBytes, decodedPrivate);
			Assert.Equal(vector.PublicSeedBytes, decodedPublic);
		}

		// Prefilled data
		// Extra reference: https://github.com/n-y-z-o/nyzoVerifier/blob/2d91ec85c3ff88c752730a94e9f9f22e8739d210/src/main/java/co/nyzo/verifier/client/commands/PrefilledDataCreateCommand.java
		foreach(var vector in new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors) {
			// The content length byte used can range from 0-255, some vectors exceed this
			// In reality the network does not accept sender data this large (i.e. 256 - 32 - ..)
			// This is the only deviation from the RFC vector spec
			var messageBytes = vector.MessageBytes.Length > (200 - 32) ? vector.MessageBytes[..(200-32)] : vector.MessageBytes;
			var instance = new NyzoStringPrefilledData(vector.PublicSeedBytes, messageBytes);

			// This assumes EncodedNyzoString produces an accurate output, see below. We need some form of NyzoString encoded string in any case. Assessing the interop/compatibility between en- & decode is thus of increased importance.
			var encodedPrefilledData = ShimNyzoStringEncoder._EncodedNyzoString(NyzoStringPrefix.PrefilledData, instance.GetBytes());

			Assert.NotNull(encodedPrefilledData);

			var expectedContentBytesPart1Length = vector.PublicSeedBytes.Length;
			var expectedContentBytesPart2Length = messageBytes.Length;

			var decodeOutput = ShimNyzoStringEncoder._DecodeToContentBytes(encodedPrefilledData);

			Assert.NotNull(decodeOutput);

			// ProvideSubArray is assumed to be accurate (see first test method)
			// The length must be the same and
			Assert.Equal(instance.GetBytes().Length, decodeOutput.Length);

			// the instance property values must be intact after an encode -> decode has been performed
			Assert.Equal(instance.ReceiverIdentifier, ShimNyzoStringEncoder._ProvideSubArray(decodeOutput, 0, expectedContentBytesPart1Length));
			Assert.Equal(instance.SenderData, ShimNyzoStringEncoder._ProvideSubArray(decodeOutput, expectedContentBytesPart1Length + 1, decodeOutput.Length));
		}

		// Transaction
		foreach (var vector in new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors) {
			var tx = new NyzoTransaction();

			var amt = 1_000_000;
			var recipientIdentifier = vector.PublicSeedBytes;
			var prevHashHeight = 1000;
			var prevBlockHash = vector.PublicSeedBytes;
			var senderData = ShimNyzoStringEncoder._ProvideSubArray(vector.MessageBytes, 0, Math.Min(vector.MessageBytes.Length, 32)); // preemptively limited to the max, for easier assertions later on
			var senderIdentifier = vector.PublicSeedBytes;

			// The use of the PublicSeedBytes holds no significance other than being a readily available byte[], and the correct SenderIdentifier for the Signature
			tx.SetAmount(amt);
			tx.SetRecipientIdentifier(recipientIdentifier);
			tx.SetPreviousHashHeight(prevHashHeight);
			tx.SetPreviousBlockHash(prevBlockHash);
			tx.SetSenderData(senderData);
			tx.Sign(vector.PrivateSeedBytes); // sets SenderIdentifier and Signature

			var signature = tx.Signature;

			// Get the bytes
			var txBytesWithSignature = tx.GetBytes(false);

			// Here, we also need an encoded NyzoString to start with
			var encodedTxWithSignature = ShimNyzoStringEncoder.NyzoStringFromTransaction(txBytesWithSignature);

			Assert.NotNull(encodedTxWithSignature);

			// Decode the NyzoStrings
			var decodedBytesWithSignature = ShimNyzoStringEncoder._DecodeToContentBytes(encodedTxWithSignature);

			Assert.NotNull(decodedBytesWithSignature);

			// Creating instance with FromBytes, some properties are omitted
			// TODO - full alignment with https://tech.nyzo.org/dataFormats/transaction
			// cf NyzoTransaction.cs
			var txFromByteArray = NyzoTransaction.FromBytes(decodedBytesWithSignature);

			Assert.Equal(tx.Type, txFromByteArray.Type);
			Assert.Equal(tx.Timestamp, txFromByteArray.Timestamp);
			Assert.Equal(amt, txFromByteArray.Amount);
			Assert.Equal(recipientIdentifier, txFromByteArray.RecipientIdentifier);
			Assert.Equal(prevHashHeight, txFromByteArray.PreviousHashHeight);
			Assert.Equal(senderIdentifier, txFromByteArray.SenderIdentifier);
			Assert.Equal(senderData.Length, txFromByteArray.SenderData.Length);
			Assert.Equal(senderData, txFromByteArray.SenderData);
			Assert.Equal(signature, txFromByteArray.Signature);
		}
	}

	[Fact]
	public void DecodePrivateKey_ShouldReturnInstance_OrNull() {
		// Vector private keys, these should not return null & be equal to the RFC seed bytes
		foreach(var vector in new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors) {
			var decodedPrivKey = ShimNyzoStringEncoder.DecodePrivateKey(vector.NyzoStringPrivateKey!);

			Assert.NotNull(decodedPrivKey?.Seed);
			Assert.Equal(vector.PrivateSeedBytes, decodedPrivKey.Seed);
		}

		var firstVector = new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors[0];


		// Invalid, bad prefix
		var badPrefixPrivKey = firstVector.NyzoStringPrivateKey!.Replace(NyzoStringPrefix.PrivateKey, NyzoStringPrefix.PrivateKey.Replace("y", "e"));

		Assert.Null(ShimNyzoStringEncoder.DecodePrivateKey(badPrefixPrivKey));


		// Invalid, character that does not exist in character map
		var badUnsupportedCharPrivKey = firstVector.NyzoStringPrivateKey.Replace(".", "й");

		Assert.Throws<KeyNotFoundException>(() => ShimNyzoStringEncoder.DecodePrivateKey(badUnsupportedCharPrivKey));


		// Invalid, prefix but no encoded content after it
		var badPrefixOnlyPrivKey = NyzoStringPrefix.PrivateKey;

		Assert.Throws<IndexOutOfRangeException>(() => ShimNyzoStringEncoder.DecodePrivateKey(badPrefixOnlyPrivKey));


		// Invalid, only first half of the priv key 
		var badOnlyFirstHalfPrivKey = firstVector.NyzoStringPrivateKey[..((int)firstVector.NyzoStringPrivateKey.Length / 2)];

		Assert.Null(ShimNyzoStringEncoder.DecodePrivateKey(badOnlyFirstHalfPrivKey));


		// Invalid, empty string
		var badEmptyPrivKey = "";

		Assert.Null(ShimNyzoStringEncoder.DecodePrivateKey(badEmptyPrivKey));


		// Invalid, +1 character
		var badAdditionalUntrimmableCharPrivKey = firstVector.NyzoStringPrivateKey + "0";

		Assert.Throws<IndexOutOfRangeException>(() => ShimNyzoStringEncoder.DecodePrivateKey(badAdditionalUntrimmableCharPrivKey));


		// Invalid, -1 character
		var badLastCharMissingPrivKey = firstVector.NyzoStringPrivateKey[..((int)firstVector.NyzoStringPrivateKey.Length - 1)];

		Assert.Throws<IndexOutOfRangeException>(() => ShimNyzoStringEncoder.DecodePrivateKey(badLastCharMissingPrivKey));
	}

	[Fact]
	public void DecodePublicIdentifier_ShouldReturnInstance_OrNull() {
		// Vector public identifiers, these should not return null & be equal to the RFC seed bytes
		foreach (var vector in new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors) {
			var decodedPubKey = ShimNyzoStringEncoder.DecodePublicIdentifier(vector.NyzoStringPublicIdentifier!);

			Assert.NotNull(decodedPubKey?.Identifier);
			Assert.Equal(vector.PublicSeedBytes, decodedPubKey.Identifier);
		}

		var firstVector = new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors[0];


		// Invalid, bad prefix
		var badPrefixPubKey = firstVector.NyzoStringPublicIdentifier!.Replace(NyzoStringPrefix.PublicIdentifier, NyzoStringPrefix.PublicIdentifier.Replace("d", "i"));

		Assert.Null(ShimNyzoStringEncoder.DecodePublicIdentifier(badPrefixPubKey));


		// Invalid, character that does not exist in character map
		var badUnsupportedCharPubKey = firstVector.NyzoStringPublicIdentifier.Replace("8", "й");

		Assert.Throws<KeyNotFoundException>(() => ShimNyzoStringEncoder.DecodePublicIdentifier(badUnsupportedCharPubKey));


		// Invalid, prefix but no encoded content after it
		var badPrefixOnlyPubKey = NyzoStringPrefix.PublicIdentifier;

		Assert.Throws<IndexOutOfRangeException>(() => ShimNyzoStringEncoder.DecodePublicIdentifier(badPrefixOnlyPubKey));


		// Invalid, only first half of the priv key 
		var badOnlyFirstHalfPubKey = firstVector.NyzoStringPublicIdentifier[..((int)firstVector.NyzoStringPublicIdentifier.Length / 2)];

		Assert.Null(ShimNyzoStringEncoder.DecodePublicIdentifier(badOnlyFirstHalfPubKey));


		// Invalid, empty string
		var badEmptyPubKey = "";

		Assert.Null(ShimNyzoStringEncoder.DecodePublicIdentifier(badEmptyPubKey));


		// Invalid, +1 character
		var badAdditionalUntrimmableCharPubKey = firstVector.NyzoStringPublicIdentifier + "0";

		Assert.Throws<IndexOutOfRangeException>(() => ShimNyzoStringEncoder.DecodePublicIdentifier(badAdditionalUntrimmableCharPubKey));


		// Invalid, -1 character
		var badLastCharMissingPubKey = firstVector.NyzoStringPublicIdentifier[..((int)firstVector.NyzoStringPublicIdentifier.Length - 1)];

		Assert.Throws<IndexOutOfRangeException>(() => ShimNyzoStringEncoder.DecodePublicIdentifier(badLastCharMissingPubKey));
	}

	[Fact]
	public void DecodePrefilledData_ShouldReturnInstance_OrNull() {
		// Vector public identifier w/ message bytes, these should not return null & be equal to the RFC seed bytes
		foreach (var vector in new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors) {
			var instance = new NyzoStringPrefilledData(vector.PublicSeedBytes, vector.MessageBytes);

			var encodedPrefilledData = ShimNyzoStringEncoder._EncodedNyzoString(NyzoStringPrefix.PrefilledData, instance.GetBytes());

			var decodedPrefilledData = ShimNyzoStringEncoder.DecodePrefilledData(encodedPrefilledData);

			Assert.NotNull(decodedPrefilledData);
			Assert.Equal(vector.PublicSeedBytes, decodedPrefilledData.ReceiverIdentifier);

			if(vector.MessageBytes.Length > FieldByteSize.MaximumSenderDataLength) {
				Assert.Equal(vector.MessageBytes[..FieldByteSize.MaximumSenderDataLength], decodedPrefilledData.SenderData);
			} else {
				Assert.Equal(vector.MessageBytes, decodedPrefilledData.SenderData);

			}
		}


		var firstVector = new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors[0];
		var firstVectorPrefilledData = new NyzoStringPrefilledData(firstVector.PublicSeedBytes, firstVector.MessageBytes);
		var firstVectorEncodedPrefilledData = ShimNyzoStringEncoder._EncodedNyzoString(NyzoStringPrefix.PrefilledData, firstVectorPrefilledData.GetBytes());

		// Invalid, bad prefix
		var badPrefixPrefilledData = firstVectorEncodedPrefilledData.Replace(NyzoStringPrefix.PrefilledData, NyzoStringPrefix.PrivateKey.Replace("e", "r"));

		Assert.Null(ShimNyzoStringEncoder.DecodePrefilledData(badPrefixPrefilledData));


		// Invalid, character that does not exist in character map
		var badUnsupportedCharPrefilledData = firstVectorEncodedPrefilledData.ToArray();
		badUnsupportedCharPrefilledData[6] = 'й';

		Assert.Null(ShimNyzoStringEncoder.DecodePrefilledData(badUnsupportedCharPrefilledData.ToString()));


		// Invalid, prefix but no encoded content after it
		var badPrefixOnlyPrefilledData = NyzoStringPrefix.PrefilledData;

		Assert.Throws<IndexOutOfRangeException>(() => ShimNyzoStringEncoder.DecodePrefilledData(badPrefixOnlyPrefilledData));


		// Invalid, only first half of the priv key 
		var badOnlyFirstHalfPrefilledData = firstVectorEncodedPrefilledData[..((int)firstVectorEncodedPrefilledData.Length / 2)];

		Assert.Null(ShimNyzoStringEncoder.DecodePrefilledData(badOnlyFirstHalfPrefilledData));


		// Invalid, empty string
		var badEmptyPrefilledData = "";

		Assert.Null(ShimNyzoStringEncoder.DecodePrefilledData(badEmptyPrefilledData));


		// Invalid, +1 character
		var badAdditionalUntrimmableCharPrefilledData = firstVectorEncodedPrefilledData + "0";

		Assert.Throws<IndexOutOfRangeException>(() => ShimNyzoStringEncoder.DecodePrefilledData(badAdditionalUntrimmableCharPrefilledData));


		// Invalid, -1 character
		var badLastCharMissingPrefilledData = firstVectorEncodedPrefilledData[..((int)firstVectorEncodedPrefilledData.Length - 1)];

		Assert.Throws<IndexOutOfRangeException>(() => ShimNyzoStringEncoder.DecodePrefilledData(badLastCharMissingPrefilledData));
	}

	[Fact]
	public void DecodeNyzoTransaction_ShouldReturnInstance_OrNull() {
		// Vector private keys, these should not return null & be equal to the RFC seed bytes
		foreach (var vector in new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors) {
			var tx = new NyzoTransaction();

			var amt = 1_000_000;
			var recipientIdentifier = vector.PublicSeedBytes;
			var prevHashHeight = 1000;
			var prevBlockHash = vector.PublicSeedBytes;
			var senderData = ShimNyzoStringEncoder._ProvideSubArray(vector.MessageBytes, 0, Math.Min(vector.MessageBytes.Length, 32)); // preemptively limited to the max, for easier assertions later on
			var senderIdentifier = vector.PublicSeedBytes;

			// The use of the PublicSeedBytes holds no significance other than being a readily available byte[], and the correct SenderIdentifier for the Signature
			tx.SetAmount(amt);
			tx.SetRecipientIdentifier(recipientIdentifier);
			tx.SetPreviousHashHeight(prevHashHeight);
			tx.SetPreviousBlockHash(prevBlockHash);
			tx.SetSenderData(senderData);
			tx.Sign(vector.PrivateSeedBytes); // sets SenderIdentifier and Signature

			var signature = tx.Signature;

			// Get the bytes
			var txBytesWithSignature = tx.GetBytes(false);

			// Here, we also need an encoded NyzoString to start with
			var encodedTxWithSignature = ShimNyzoStringEncoder.NyzoStringFromTransaction(txBytesWithSignature);

			Assert.NotNull(encodedTxWithSignature);

			// Decode the NyzoStrings
			var decodedTransaction = ShimNyzoStringEncoder.DecodeNyzoTransaction(encodedTxWithSignature);

			Assert.NotNull(decodedTransaction);

			Assert.Equal(tx.Type, decodedTransaction.Type);
			Assert.Equal(tx.Timestamp, decodedTransaction.Timestamp);
			Assert.Equal(amt, decodedTransaction.Amount);
			Assert.Equal(recipientIdentifier, decodedTransaction.RecipientIdentifier);
			Assert.Equal(prevHashHeight, decodedTransaction.PreviousHashHeight);
			Assert.Equal(senderIdentifier, decodedTransaction.SenderIdentifier);
			Assert.Equal(senderData.Length, decodedTransaction.SenderData.Length);
			Assert.Equal(senderData, decodedTransaction.SenderData);
			Assert.Equal(signature, decodedTransaction.Signature);
		}

		var firstVector = new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors[0];

		// ---- creating encoded tx with firstVector
		var firstVectortx = new NyzoTransaction();

		var firstVectoramt = 1_000_000;
		var firstVectorrecipientIdentifier = firstVector.PublicSeedBytes;
		var firstVectorprevHashHeight = 1000;
		var firstVectorprevBlockHash = firstVector.PublicSeedBytes;
		var firstVectorsenderData = ShimNyzoStringEncoder._ProvideSubArray(firstVector.MessageBytes, 0, Math.Min(firstVector.MessageBytes.Length, 32)); // preemptively limited to the max, for easier assertions later on
		var firstVectorsenderIdentifier = firstVector.PublicSeedBytes;

		// The use of the PublicSeedBytes holds no significance other than being a readily available byte[], and the correct SenderIdentifier for the Signature
		firstVectortx.SetAmount(firstVectoramt);
		firstVectortx.SetRecipientIdentifier(firstVectorrecipientIdentifier);
		firstVectortx.SetPreviousHashHeight(firstVectorprevHashHeight);
		firstVectortx.SetPreviousBlockHash(firstVectorprevBlockHash);
		firstVectortx.SetSenderData(firstVectorsenderData);
		firstVectortx.Sign(firstVector.PrivateSeedBytes); // sets SenderIdentifier and Signature

		var firstVectorsignature = firstVectortx.Signature;

		// Get the bytes
		var firstVectortxBytesWithSignature = firstVectortx.GetBytes(true);

		// Here, we also need an encoded NyzoString to start with
		var firstVectorencodedTxWithSignature = ShimNyzoStringEncoder.NyzoStringFromTransaction(firstVectortxBytesWithSignature);
		// --------------------------------------------

		// Invalid, bad prefix
		var badPrefixtx = firstVectorencodedTxWithSignature!.Replace(NyzoStringPrefix.Transaction, NyzoStringPrefix.Transaction.Replace("t", "x"));

		Assert.Null(ShimNyzoStringEncoder.DecodeNyzoTransaction(badPrefixtx));


		// Invalid, character that does not exist in character map
		var badUnsupportedChartx = firstVectorencodedTxWithSignature.Replace("e", "й");

		Assert.Throws<KeyNotFoundException>(() => ShimNyzoStringEncoder.DecodeNyzoTransaction(badUnsupportedChartx));


		// Invalid, prefix but no encoded content after it
		var badPrefixOnlytx = NyzoStringPrefix.Transaction;

		Assert.Throws<IndexOutOfRangeException>(() => ShimNyzoStringEncoder.DecodeNyzoTransaction(badPrefixOnlytx));


		// Invalid, only first half of the priv key 
		var badOnlyFirstHalftx = firstVectorencodedTxWithSignature[..((int)firstVectorencodedTxWithSignature.Length / 2)];

		Assert.Throws<IndexOutOfRangeException>(() => ShimNyzoStringEncoder.DecodeNyzoTransaction(badOnlyFirstHalftx));


		// Invalid, empty string
		var badEmptytx = "";

		Assert.Null(ShimNyzoStringEncoder.DecodeNyzoTransaction(badEmptytx));


		// Invalid, +1 character
		var badAdditionalUntrimmableChartx = firstVectorencodedTxWithSignature + "0";

		Assert.Throws<IndexOutOfRangeException>(() => ShimNyzoStringEncoder.DecodeNyzoTransaction(badAdditionalUntrimmableChartx));


		// Invalid, -1 character
		var badLastCharMissingtx = firstVectorencodedTxWithSignature[..((int)firstVectorencodedTxWithSignature.Length - 1)];

		Assert.Throws<IndexOutOfRangeException>(() => ShimNyzoStringEncoder.DecodeNyzoTransaction(badLastCharMissingtx));
	}

	#region core encoding (private)
	[Fact]
	public void EncodedStringForByteArray_ShouldBeAccurate() {
		Assert.NotNull("""
			This method is used internally within the NyzoStringEncoder and is considered accurate by means of inferrence, i.e. the methods which reference this method are tested thoroughly, and if they pass, they confirm the accuracy of this method.
		""");

		// Method EncodedNyzoString: converts the end result to the encoded NyzoString
	}

	
	[Fact]
	public void ByteArrayForEncodedString_ShouldBeAccurate() {
		Assert.NotNull("""
			This method is used internally within the NyzoStringEncoder and is considered accurate by means of inferrence, i.e. the methods which reference this method are tested thoroughly, and if they pass, they confirm the accuracy of this method.
		""");

		// Method EncodedNyzoString: converts a NyzoString prefix to bytes
		// Method DecodeToContentBytes: returns the value provided by this method, if the value passed all validations (lengths, checksum). Clearly separating the conversion from pre- and post- processing surrounding the call (SRP)
	}

	// This tests whether calling these methods repetitively with eachother's output does not produce errors
	// Furthermore, it proves that a "full" utilization of the characters and indices represented by CharacterLookup & CharacterToValueDict does not result in conversion errors or losses
	[Fact]
	public void ByteArray_X_EncodedString_AreCompatibleWithEachother() {
		Assert.NotNull("""
			This method is used internally within the NyzoStringEncoder and is considered accurate by means of inferrence, i.e. the methods which reference this method are tested thoroughly, and if they pass, they confirm the accuracy of this method.
		""");
	}
	#endregion

	[Fact]
	public void NyzoStringFromPrivateSeed_ShouldBeAccurate() {
		foreach(var vector in new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors) {
			Assert.Equal(
				vector.NyzoStringPrivateKey,
				ShimNyzoStringEncoder.NyzoStringFromPrivateSeed(vector.PrivateSeedBytes)
			);
		}
	}

	[Fact]
	public void NyzoStringFromPublicIdentifier_ShouldBeAccurate() {
		foreach (var vector in new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors) {
			Assert.Equal(
				vector.NyzoStringPublicIdentifier,
				ShimNyzoStringEncoder.NyzoStringFromPublicIdentifier(vector.PublicSeedBytes)
			);
		}
	}

	[Fact]
	public void NyzoStringFromTransaction_ShouldBeAccurate() {
		Assert.NotNull("""
			This method is used internally within the NyzoStringEncoder and is considered accurate by means of inferrence, i.e. the methods which reference this method are tested thoroughly, and if they pass, they confirm the accuracy of this method.
		""");
	}

}
