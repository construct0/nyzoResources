using System;

namespace Nyzo.CL;

// Untested. Use at your own risk.
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

        var signature = Sodium.PublicKeyAuth.Sign(this.GetBytes(false)!, keyPair.PrivateKey);

        this.SetSignature(signature);
    }

    public byte[] GetBytes(bool includeSignature){
        var forSigning = !includeSignature;
        var buffer = new ByteBuffer(1000);

        // Not sure why this was hardcoded while the Type property exists, replaced with reference
        // buffer.PutByte(2); // transaction type = 2 (standard)
        buffer.PutByte((byte)this.Type);
        
        buffer.PutInt64(this.Timestamp.ToFileTimeUtc());
        buffer.PutInt64(this.Amount);
        buffer.PutBytes(this.RecipientIdentifier);

        if(forSigning){
            buffer.PutBytes(this.PreviousBlockHash);
        } else {
            buffer.PutInt64(this.PreviousHashHeight);
        }

        buffer.PutBytes(this.SenderIdentifier);

        if(forSigning){
            var doubleShaSenderDataBytes = NyzoUtil.ByteArrayAsDoubleSha256ByteArray(this.SenderData);

            buffer.PutBytes(doubleShaSenderDataBytes);
        } else {
            buffer.PutByte((byte)this.SenderData.Length);
            buffer.PutBytes(this.SenderData);
        }

        if(!forSigning){
            buffer.PutBytes(this.Signature);
        }

        return buffer.ReadBytes();
    }

    /// <summary>
    /// Using content from GetBytes output as an argument here does not mean you will have an identical object, refer to Nyzo.CL.Tests.NyzoTransactionTests
    /// </summary>
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

