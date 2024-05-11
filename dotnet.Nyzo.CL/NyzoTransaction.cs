using System;
using System.Collections.Generic;
using System.Transactions;

namespace Nyzo.CL;

/// <summary>
/// As of now, only transaction type 2 is supported.
/// </summary>
public class NyzoTransaction {
    public DateTime Timestamp {get;private set;}
    public short Type {get;private set;}
    public long Amount {get;private set;} // micro nyzos
    public byte[] RecipientIdentifier {get;private set;}
    public long PreviousHashHeight {get;private set;}
    public byte[] PreviousBlockHash {get;private set;}
    public byte[] SenderIdentifier {get;private set;}
    public byte[] SenderData {get;private set;}
    public byte[] Signature {get;private set;}

    public NyzoTransaction(){
        this.Timestamp = DateTime.Now.ToUniversalTime();
        this.Type = 2; // transaction type = 2 (standard)
		this.Amount = 0L;
        this.RecipientIdentifier = new byte[32];
        this.PreviousHashHeight = 0;
        this.PreviousBlockHash = new byte[32];
        this.SenderIdentifier = new byte[32];
        this.SenderData = new byte[0];
        this.Signature = new byte[64];
    }

    public void SetTimestamp(DateTime timestamp){
        this.Timestamp = timestamp.ToUniversalTime();
    }

    public void SetAmount(long amount){
        this.Amount = amount;
    }

    public void SetRecipientIdentifier(byte[] recipientIdentifier){
        for (var i = 0; i < 32; i++) {
            this.RecipientIdentifier[i] = recipientIdentifier[i];
        }
    }

    public void SetPreviousHashHeight(long previousHashHeight){
        this.PreviousHashHeight = previousHashHeight;
    }

    public void SetPreviousBlockHash(byte[] previousBlockHash){
        for (var i = 0; i < 32; i++) {
            this.PreviousBlockHash[i] = previousBlockHash[i];
        }
    }

    public void SetSenderIdentifier(byte[] senderIdentifier){
        for (var i = 0; i < 32; i++) {
            this.SenderIdentifier[i] = senderIdentifier[i];
        }
    }

    public void SetSenderData(byte[] senderData){
        this.SenderData = new byte[Math.Min(senderData.Length, 32)];
        for (var i = 0; i < this.SenderData.Length; i++) {
            this.SenderData[i] = senderData[i];
        }
    }

    public void SetSignature(byte[] signature){
        for (var i = 0; i < 64; i++) {
            this.Signature[i] = signature[i];
        }
    }

    public void Sign(byte[] seedBytes){
        var keyPair = Sodium.PublicKeyAuth.GenerateKeyPair(seedBytes);

        this.SetSenderIdentifier(keyPair.PublicKey);

        var signature = Sodium.PublicKeyAuth.Sign(this.GetBytes(true)!, keyPair.PrivateKey);

        this.SetSignature(signature);
    }

    public int GetByteSize(bool forSigning = false) {
		// All transactions begin with a type and timestamp.
		int size = 
            FieldByteSize.TransactionType 
            + FieldByteSize.Timestamp
            ;

        //if (type == typeCycleSignature) {
        //	size += FieldByteSize.identifier +          // verifier (signer) identifier
        //			FieldByteSize.booleanField +        // yes/no
        //			FieldByteSize.signature;            // cycle transaction signature
        //	if (!forSigning) {
        //		size += FieldByteSize.signature;        // signature
        //	}
        //} else {

        size +=
            FieldByteSize.TransactionAmount
            + FieldByteSize.Identifier // receiver identifier
            ;
        //}

        //if (type == typeSeed || type == typeStandard || type == typeCycle) {

        if (forSigning) {
            size += FieldByteSize.Hash; // previous blockhash for signing
        } else {
            size += FieldByteSize.BlockHeight; // previous-hash height for storage and transmission
		}

        size += FieldByteSize.Identifier; // sender identifier

        if (forSigning) {
            size += FieldByteSize.Hash; // sender data hash for signing
        } else {
            size +=
                1 // length specifier
                + this.SenderData.Length // sender data
                + FieldByteSize.Signature // transaction signature
                ;

			//if (type == typeCycle) {
			//	// These are stored differently in the v1 and v2 blockchains. The cycleSignatures field is used for
			//	// the v1 blockchain, and the cycleSignatureTransactions field is used for the v2 blockchain.
			//	if (cycleSignatures != null && !cycleSignatures.isEmpty()) {
			//		// The v1 blockchain stores identifier and signature for each.
			//		size += FieldByteSize.unnamedInteger + cycleSignatures.size() * (FieldByteSize.identifier +
			//				FieldByteSize.signature);
			//	} else {
			//		// The v2 blockchain stores timestamp, identifier, vote, and signature for each.
			//		size += FieldByteSize.unnamedInteger + cycleSignatureTransactions.size() *
			//				(FieldByteSize.timestamp + FieldByteSize.identifier + FieldByteSize.booleanField +
			//						FieldByteSize.signature);

			//	}
			//}
		}

		return size;
	}

    public byte[] GetBytes(bool forSigning=false){
        var array = new byte[this.GetByteSize(forSigning)];
        var buffer = new ByteBuffer(array);

        buffer.PutByte((byte)this.Type);
        buffer.PutInt64(this.Timestamp.ToFileTimeUtc());

		//if (type == typeCoinGeneration || type == typeSeed || type == typeStandard || type == typeCycle) {
		buffer.PutInt64(this.Amount);
		buffer.PutBytes(this.RecipientIdentifier);
        //} else if (type == typeCycleSignature) {
        //	buffer.put(senderIdentifier);
        //	buffer.put(cycleTransactionVote);
        //	buffer.put(cycleTransactionSignature);
        //	if (!forSigning) {
        //		buffer.put(signature);
        //	}
        //}

        //--
        //if (type == typeSeed || type == typeStandard || type == typeCycle) {

        if (forSigning) {
            buffer.PutBytes(this.PreviousBlockHash);
        } else {
            buffer.PutInt64(this.PreviousHashHeight);
        }

        buffer.PutBytes(this.SenderIdentifier);

        // For serializing, we use the raw sender data with a length specifier. For signing, we use the double-SHA-256 of the user data. This will allow us to remove inappropriate or illegal metadata from the blockchain at a later date by replacing it with its double-SHA-256 without compromising the signature integrity.
        if (forSigning) {
            var doubleShaSenderDataBytes = NyzoUtil.ByteArrayAsDoubleSha256ByteArray(this.SenderData);

            buffer.PutBytes(doubleShaSenderDataBytes);
        } else {
            buffer.PutByte((byte)this.SenderData.Length);
            buffer.PutBytes(this.SenderData);
        }

        if (!forSigning) {
            buffer.PutBytes(this.Signature);

			//// For cycle transactions, order the signatures by verifier identifier. In the v1 blockchain, the
			//// cycleSignatures field is used. In the v2 blockchain, the cycleSignatureTransactions field is used.
			//if (type == typeCycle) {
			//	if (cycleSignatures != null && !cycleSignatures.isEmpty()) {
			//		List<ByteBuffer> signatureIdentifiers = new ArrayList<>(cycleSignatures.keySet());
			//		signatureIdentifiers.sort(identifierComparator);

			//		buffer.putInt(cycleSignatures.size());
			//		for (ByteBuffer identifier : signatureIdentifiers) {
			//			buffer.put(identifier.array());
			//			buffer.put(cycleSignatures.get(identifier));
			//		}
			//	} else {
			//		List<ByteBuffer> signatureIdentifiers = new ArrayList<>(cycleSignatureTransactions.keySet());
			//		signatureIdentifiers.sort(identifierComparator);

			//		buffer.putInt(cycleSignatureTransactions.size());
			//		for (ByteBuffer identifier : signatureIdentifiers) {
			//			Transaction signatureTransaction = cycleSignatureTransactions.get(identifier);
			//			buffer.putLong(signatureTransaction.timestamp);
			//			buffer.put(signatureTransaction.senderIdentifier);
			//			buffer.put(signatureTransaction.cycleTransactionVote);
			//			buffer.put(signatureTransaction.signature);
			//		}
			//	}
			//}
		}

        return buffer.ReadBytes();
    }

	/// <summary>
	/// <para>Only compatible with an output from GetBytes whereby includeSignature:true & ported with implementation from nyzoChromeExtension, not nyzoVerifier</para>
	/// <para>TODO - full alignment with <see href="https://tech.nyzo.org/dataFormats/transaction"></see></para>
	/// </summary>
	/// <param name="array"></param>
	/// <returns></returns>
	public static NyzoTransaction FromBytes(byte[] array){
        var buffer = new ByteBuffer(array);
        var transaction = new NyzoTransaction();

        transaction.Type = buffer.ReadByte();
        transaction.SetTimestamp(DateTime.FromFileTimeUtc(buffer.ReadInt64()));
        transaction.SetAmount(buffer.ReadInt64());
        transaction.SetRecipientIdentifier(buffer.ReadBytes(32));
		transaction.SetPreviousHashHeight(buffer.ReadInt64());
        transaction.SetSenderIdentifier(buffer.ReadBytes(32));

        var dataLength = buffer.ReadByte();
        transaction.SetSenderData(buffer.ReadBytes(dataLength));
        transaction.SetSignature(buffer.ReadBytes(64));

        return transaction;
    }
}

