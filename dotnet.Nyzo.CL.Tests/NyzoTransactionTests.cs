using System;
using System.Linq;
using System.Security.Cryptography;

namespace Nyzo.CL.Tests;

public class NyzoTransactionTests {
	[Fact]
	public void Constructing_ShouldResultInCorrectDefaultPropertyValues() {
		// Arrange
		// This is nothing more than a regurgitation of current values but acts as an extra barrier for changes
		var defaultTxType = 2;
		var defaultTxAmount = 0;
		var defaultRecipientIdentifier = new byte[32];
		var defaultPreviousHashHeight = 0;
		var defaultPreviousBlockHash = new byte[32];
		var defaultSenderIdentifier = new byte[32];
		var defaultSenderData = new byte[0];
		var defaultSignature = new byte[64];

		// Seconds will be stripped to be able to check whether both this and the timestamp assigned during init are reasonably equal
		// The while() halt assures that both the defaultTimestamp and tx are created in the same minute, to avoid the rare possibility that this test fails due to the DateTimes being created with a different Minute
		while (DateTime.Now.Second < 1 || DateTime.Now.Second > 58) { }
		var defaultTimestamp = DateTime.Now;

		// Act
		var tx = new NyzoTransaction();
		var txTimestampStripped = new DateTime(tx.Timestamp.Year, tx.Timestamp.Month, tx.Timestamp.Day, tx.Timestamp.Hour, tx.Timestamp.Minute, tx.Timestamp.Second);

		var defaultTimestampStripped = new DateTime(defaultTimestamp.Year, defaultTimestamp.Month, defaultTimestamp.Day, defaultTimestamp.Hour, defaultTimestamp.Minute, defaultTimestamp.Second);

		// Assert
		Assert.Equal(defaultTxType, tx.Type);
		Assert.Equal(defaultTxAmount, tx.Amount);
		Assert.True(defaultRecipientIdentifier.SequenceEqual(tx.RecipientIdentifier));
		Assert.Equal(defaultPreviousHashHeight, tx.PreviousHashHeight);
		Assert.True(defaultPreviousBlockHash.SequenceEqual(tx.PreviousBlockHash));
		Assert.True(defaultSenderIdentifier.SequenceEqual(tx.SenderIdentifier));
		Assert.True(defaultSenderData.SequenceEqual(tx.SenderData));
		Assert.True(defaultSignature.SequenceEqual(tx.Signature));

		
		Assert.Equal(defaultTimestampStripped.ToUniversalTime(), txTimestampStripped);
	}

	[Fact]
	public void SetTimestamp_ShouldResultInCorrectTimestamp() {
		// Arrange
		var dt1 = DateTime.Now.AddMinutes(-10);
		var dt2 = DateTime.Now.AddMinutes(10);

		//
		var tx = new NyzoTransaction();
		tx.SetTimestamp(dt1);
		Assert.Equal(dt1.ToUniversalTime(), tx.Timestamp);

		tx.SetTimestamp(dt2);
		Assert.Equal(dt2.ToUniversalTime(), tx.Timestamp);
	}

	[Fact]
	public void SetAmount_ShouldResultInCorrectAmount() {
		// Arrange
		var amount = (long)(NyzoConstants.MinimumTransactionAmount * NyzoConstants.MicroNyzosPerNyzo);
		var amount2 = long.MaxValue;

		// 
		var tx = new NyzoTransaction();

		tx.SetAmount(amount);
		Assert.Equal(amount, tx.Amount);

		tx.SetAmount(amount2);
		Assert.Equal(amount2, tx.Amount);
	}

	[Fact]
	public void SetRecipientIdentifier_ShouldResultInFirst32BytesAsPropertyValue() {
		// Arrange
		// (!) this is not a valid identifier, merely a test that only 32 bytes make it into the byte[]
		byte[] array = new byte[64];

		for(var i=1; i < array.Length + 1; i++) {
			array[i - 1] = (byte)i;
		}

		// Act
		var tx = new NyzoTransaction();
		tx.SetRecipientIdentifier(array);

		// Assert
		Assert.Equal(32, tx.RecipientIdentifier.Length);
		for(var i=0; i < 32; i++) {
			Assert.Equal(array[i], tx.RecipientIdentifier[i]);
		}

	}

	[Fact]
	public void SetPreviousHashHeight_ShouldResultInCorrectHashHeight() {
		// Arrange
		var height1 = 1;
		var height2 = long.MaxValue;

		// 
		var tx = new NyzoTransaction();

		tx.SetPreviousHashHeight(height1);
		Assert.Equal(height1, tx.PreviousHashHeight);

		tx.SetPreviousHashHeight(height2);
		Assert.Equal(height2, tx.PreviousHashHeight);
	}

	[Fact]
	public void SetPreviousBlockHash_ShouldResultInFirst32BytesAsPropertyValue() {
		// Arrange
		// (!) this is not a valid block hash, merely a test that only 32 bytes make it into the byte[]
		byte[] array = new byte[64];

		for (var i = 1; i < array.Length + 1; i++) {
			array[i-1] = (byte)i;
		}

		// Act
		var tx = new NyzoTransaction();
		tx.SetPreviousBlockHash(array);

		// Assert
		Assert.Equal(32, tx.PreviousBlockHash.Length);
		for (var i = 0; i < 32; i++) {
			Assert.Equal(array[i], tx.PreviousBlockHash[i]);
		}
	}

	[Fact]
	public void SetSenderIdentifier_ShouldResultInFirst32BytesAsPropertyValue() {
		// Arrange
		// (!) this is not a valid public identifier, merely a test that only 32 bytes make it into the byte[]
		byte[] array = new byte[64];

		for (var i = 1; i < array.Length + 1; i++) {
			array[i-1] = (byte)i;
		}

		// Act
		var tx = new NyzoTransaction();
		tx.SetSenderIdentifier(array);

		// Assert
		Assert.Equal(32, tx.SenderIdentifier.Length);
		for (var i = 0; i < 32; i++) {
			Assert.Equal(array[i], tx.SenderIdentifier[i]);
		}
	}

	[InlineData()]
	public void SetSenderData_ShouldResultInLessThanOrMax32BytesAsPropertyValue() {
		// Arrange
		// (!) this is not validated sender data, merely a test that only 32 bytes or less make it into the byte[]
		byte[] array = new byte[64];

		for (var i = 1; i < array.Length + 1; i++) {
			array[i-1] = (byte)i;
		}

		byte[] array2 = new byte[31];

		for (var i = 1; i < array2.Length + 1; i++) {
			array2[i-1] = (byte)i;
		}

		// 
		var tx = new NyzoTransaction();
		tx.SetSenderData(array);

		Assert.Equal(32, tx.SenderData.Length);
		for (var i = 0; i < 32; i++) {
			Assert.Equal(array[i], tx.SenderData[i]);
		}

		tx.SetSenderData(array2);
		Assert.Equal(31, tx.SenderData.Length);
		for (var i = 0; i < 31; i++) {
			Assert.Equal(array[i], tx.SenderData[i]);
		}
	}

	[Fact]
	public void SetSignature_ShouldResultInFirst64BytesAsPropertyValue() {
		// Arrange
		// (!) this is not a valid signature, merely a test that only 64 bytes make it into the byte[]
		byte[] array = new byte[128];

		for (var i = 1; i < array.Length + 1; i++) {
			array[i-1] = (byte)i;
		}

		// Act
		var tx = new NyzoTransaction();
		tx.SetSignature(array);

		// Assert
		Assert.Equal(64, tx.Signature.Length);
		for (var i = 0; i < 64; i++) {
			Assert.Equal(array[i], tx.Signature[i]);
		}
	}

	/// <summary>
	/// <see cref="SodiumPublicKeyAuthTests"/>
	/// </summary>
	[Fact]
	public void SignWithSeedBytes_ShouldResultInSenderIdentifierAndSignatureBeingSet() {
		// This does not validate the sender identifier and signature, it merely checks whether these properties are set after Sign is called
		// For validation please refer to SodiumPublicKeyAuthTests
		foreach(var vector in new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors) {
			var seedBytes = vector.PrivateSeedBytes;
			
			var tx = new NyzoTransaction();
			tx.Sign(seedBytes);

			Assert.Equal(32, tx.SenderIdentifier.Length);
			Assert.Equal(64, tx.Signature.Length);
		}
	}

	[Fact]
	public void GetBytes_ShouldResultInAccurateProperties_AndBeCyclableWithFromBytes() {
		var dummytx = new NyzoTransaction();
		NyzoTransaction? txFromBytes = null;

		var vector = new SodiumPublicKeyAuthTests().Ed25519_RFC8032_TestVectors[0];
		var dummyBytes = vector.PublicSeedBytes; //  this holds no significance other than being a readily available byte[]

		var txTimestamp = DateTime.Now.AddMinutes(5);
		var txAmount = 1000;
		var txRecipientIdentifier = dummyBytes;
		var txPreviousBlockHash = dummyBytes;
		var txPreviousHashHeight = 100000L;
		var txSenderIdentifier = dummyBytes;
		var txSenderData = dummyBytes;
		var txPrivateSeedBytes = vector.PrivateSeedBytes;

		dummytx.SetTimestamp(txTimestamp);
		dummytx.SetAmount(txAmount);
		dummytx.SetRecipientIdentifier(txRecipientIdentifier);
		dummytx.SetPreviousBlockHash(txPreviousBlockHash);
		dummytx.SetPreviousHashHeight(txPreviousHashHeight);
		dummytx.SetSenderIdentifier(txSenderIdentifier);
		dummytx.SetSenderData(txSenderData);

		dummytx.Sign(txPrivateSeedBytes);

		var dummytxGetBytes = dummytx.GetBytes(true);

		// Using content from GetBytes output as an argument here does not mean you will have an identical object
		txFromBytes = NyzoTransaction.FromBytes(dummytxGetBytes);

		// But we do check that the data that should be there, is there
		Assert.Equal(dummytx.Type, txFromBytes.Type);
		Assert.Equal(dummytx.Timestamp, txFromBytes.Timestamp);
		Assert.Equal(dummytx.Amount, txFromBytes.Amount);
		Assert.Equal(dummytx.RecipientIdentifier, txFromBytes.RecipientIdentifier);
		Assert.Equal(dummytx.PreviousHashHeight, txFromBytes.PreviousHashHeight);
		Assert.Equal(dummytx.SenderIdentifier, txFromBytes.SenderIdentifier);

		Assert.Equal(dummytx.SenderData, txFromBytes.SenderData);
		Assert.Equal(dummytx.Signature, txFromBytes.Signature);
	}
}