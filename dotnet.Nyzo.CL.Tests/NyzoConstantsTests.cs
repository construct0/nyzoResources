using Nyzo.CL.Tests.@internal;
using System;

namespace Nyzo.CL.Tests;

// References in NyzoConstants are assumed to be tested separately, this ensures the logic referencing them is also valid
// Only properties which use external references are tested
public class NyzoConstantsTests {
	[Fact]
	public void GenesisBlockHash_ShouldConvertBackToSameHexString() {
		// Arrange
		byte[] coreGenesisBlockHash = NyzoConstants.GenesisBlockHash;
		byte[] testGenesisBlockHash = NyzoConstants.GenesisBlockHashString.HexToByteArray();

		// Assert
		Assert.Equal(coreGenesisBlockHash.Length,testGenesisBlockHash.Length);

		for (var i = 0; i<testGenesisBlockHash.Length; i++) {
			Assert.Equal(testGenesisBlockHash[i], coreGenesisBlockHash[i]);
		}

		string coreGenesisBlockHashString = coreGenesisBlockHash.ByteArrayToHex();
		string testGenesisBlockHashString = testGenesisBlockHash.ByteArrayToHex();

		Assert.Equal(coreGenesisBlockHashString, testGenesisBlockHashString);
	}

	[Fact]
	public void MinimumTransactionAmount_ShouldConvertToAWhole_UsingMicroNyzosPerNyzo() {
		// Arrange
		int microNyzosPerNyzo = NyzoConstants.MicroNyzosPerNyzo;
		double minimumTransactionAmount = NyzoConstants.MinimumTransactionAmount;

		// Act
		double derivedWhole = minimumTransactionAmount * microNyzosPerNyzo;

		// Assert
		Assert.Equal(1, derivedWhole);
	}
}
