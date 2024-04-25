using System;

namespace NyzoCL;

// Untested. Use at your own risk.
public class NyzoTransaction {
    public DateTime Timestamp {get;private set;}
    public short Type {get;private set;}
    public long Amount {get;private set;}
    public byte[] RecipientIdentifier {get;private set;}
    public long PreviousHashHeight {get;private set;}
    public byte[] PreviousBlockHash {get;private set;}
    public byte[] SenderIdentifier {get;private set;}
    public byte[] SenderData {get;private set;}
    public byte[] Signature {get;private set;}

    public NyzoTransaction(){
        this.Timestamp = DateTime.Now;
        this.Type = 2;
        this.Amount = 0L;
        this.RecipientIdentifier = new byte[32];
        this.PreviousHashHeight = 0;
        this.PreviousBlockHash = new byte[32];
        this.SenderIdentifier = new byte[32];
        this.SenderData = new byte[0];
        this.Signature = new byte[64];
    }

    public void SetTimestamp(DateTime timestamp){
        this.Timestamp = timestamp;
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

        for (var i = 0; i < 32; i++) {
            this.SenderIdentifier[i] = keyPair.PublicKey[i];
        }

        var signature = Sodium.PublicKeyAuth.Sign(this.GetBytes(false)!, keyPair.PrivateKey);

        for (var i = 0; i < 64; i++) {
            this.Signature[i] = signature[i];
        }
    }

    public byte[]? GetBytes(bool includeSignature){
        var forSigning = !includeSignature;
        var buffer = new ByteBuffer(1000);

        buffer.PutByte(2); // transaction type = 2 (standard)
        buffer.PutLong(this.Timestamp.ToFileTimeUtc());
        buffer.PutLong(this.Amount);
        buffer.PutBytes(this.RecipientIdentifier);

        if(forSigning){
            buffer.PutBytes(this.PreviousBlockHash);
        } else {
            buffer.PutLong(this.PreviousHashHeight);
        }

        buffer.PutBytes(this.SenderIdentifier);

        if(forSigning){
            buffer.PutBytes(
                NyzoUtil.DoubleSha256(
                    this.SenderData
                )
            );
        } else {
            buffer.PutInt(this.SenderData.Length);
            buffer.PutBytes(this.SenderData);
        }

        if(!forSigning){
            buffer.PutBytes(this.Signature);
        }

        return buffer.ReadBytes();
    }

    public static NyzoTransaction FromBytes(byte[] array){
        var buffer = new ByteBuffer();
        buffer.PutBytes(array);

        var transaction = new NyzoTransaction();
        transaction.Type = buffer.ReadByte();
        transaction.SetTimestamp(DateTime.FromFileTimeUtc(buffer.ReadLong()));
        transaction.SetAmount(buffer.ReadLong());
        transaction.SetRecipientIdentifier(buffer.ReadBytes(32));
        transaction.SetPreviousHashHeight(buffer.ReadLong());
        transaction.SetSenderIdentifier(buffer.ReadBytes(32));

        var dataLength = buffer.ReadByte();
        transaction.SetSenderData(buffer.ReadBytes(dataLength));
        transaction.SetSignature(buffer.ReadBytes(64));

        return transaction;
    }
}

