namespace Nyzo.CL.Tests;

// References in NyzoConstants are assumed to be tested separately, this ensures the logic referencing them is also valid
// Only properties which use external references are tested
public class NyzoConstantsTests {
	[Fact]
	public void GenesisBlockHash_ShouldConvertBackToSameHexString() {
		// Arrange
		byte[] genesisBlockHash = NyzoConstants.GenesisBlockHash;
		char[] genesisBlockHashArray = NyzoConstants.GenesisBlockHashHexString.Replace("-", "").ToCharArray();

		// Act
		string genesisBlockHashString = BitConverter.ToString(genesisBlockHash);

		// Assert
		for (var i = 0; i<genesisBlockHashArray.Length; i++) {
			Assert.Equal(genesisBlockHashArray[i], genesisBlockHashString[i]);
		}
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