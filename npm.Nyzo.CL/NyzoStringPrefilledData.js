module.exports = class NyzoStringPrefilledData {
    constructor(receiverIdentifier, senderData) {
        this.receiverIdentifier = receiverIdentifier;
        this.senderData = senderData;
    }

    GetReceiverIdentifier() {
        return this.receiverIdentifier;
    }

    GetSenderData() {
        return this.senderData;
    }
}
