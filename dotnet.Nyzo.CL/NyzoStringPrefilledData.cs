using System.ComponentModel.DataAnnotations;

namespace Nyzo.CL;

public class NyzoStringPrefilledData {
    public byte[] ReceiverIdentifier {get;init;}
    public byte[] SenderData {get;init;}
    public long Amount { get; init; }

	// todo: add "Amount" to spec @ https://tech.nyzo.org/dataFormats/nyzoString
	public NyzoStringPrefilledData(byte[] receiverIdentifier, byte[] senderData){
        this.ReceiverIdentifier = receiverIdentifier;
        
        if(senderData.Length <= FieldByteSize.MaximumSenderDataLength) {
            this.SenderData = senderData;
        } else {
            this.SenderData = senderData[..FieldByteSize.MaximumSenderDataLength];
		}

		this.Amount = 0;
    }

    public NyzoStringPrefilledData(byte[] receiverIdentifier, byte[] senderData, long amount): this(receiverIdentifier, senderData) {
        this.Amount = amount;
    }

    public byte[] GetBytes() {
        // Determine the length of the array. The single byte after the identifier encodes presence/absence of the amount in its most significant bit. The 6 least-significant bits of this byte are used for the sender-data length, which has a maximum value of 32.
        int arrayLength =
            FieldByteSize.Identifier
            + 1
            + SenderData.Length
            + (this.Amount > 0 ? FieldByteSize.TransactionAmount : 0)
            ;

		byte[] array = new byte[arrayLength];
        var buffer = new ByteBuffer(array);

        buffer.PutBytes(this.ReceiverIdentifier);
        buffer.PutByte((byte)((this.Amount == 0L ? 0 : 0b1000_0000) | this.SenderData.Length));
        buffer.PutBytes(this.SenderData);

        if(this.Amount > 0) {
            buffer.PutInt64(this.Amount);
        }

        return buffer.ReadBytes();
    }
}

