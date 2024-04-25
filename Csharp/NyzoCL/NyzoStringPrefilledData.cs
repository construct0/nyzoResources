namespace NyzoCL;

public class NyzoStringPrefilledData {
    public byte[] ReceiverIdentifier {get;init;}
    public byte[] SenderData {get;init;}

    public NyzoStringPrefilledData(byte[] receiverIdentifier, byte[] senderData){
        this.ReceiverIdentifier = receiverIdentifier;
        this.SenderData = senderData;
    }
}

