using System.ComponentModel.DataAnnotations;

namespace Nyzo.CL;

public class NyzoStringPrefilledData {
    public byte[] ReceiverIdentifier {get;init;}
    public byte[] SenderData {get;init;}

	// todo: add Amount; the nyzoVerifier.NyzoStringPrefilledData class differs from the ported nyzoChromeExtension JS class
	// todo: add "Amount" to spec @ https://tech.nyzo.org/dataFormats/nyzoString

	public NyzoStringPrefilledData(byte[] receiverIdentifier, byte[] senderData){
        this.ReceiverIdentifier = receiverIdentifier;
        this.SenderData = senderData;
    }

    // TODO: FieldByteSize, cf NyzoTransaction decode test
    public byte[] GetBytes() {
        byte[] array = new byte[32 + 1 + this.SenderData.Length + (0)];
        var buffer = new ByteBuffer(array);

        buffer.PutBytes(this.ReceiverIdentifier);

        buffer.PutByte((byte)this.SenderData.Length);

        buffer.PutBytes(this.SenderData);

        return buffer.ReadBytes();
    }
}

