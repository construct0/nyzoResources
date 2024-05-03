const nacl = require("tweetnacl");

module.exports = class NyzoTransaction {
    constructor() {
        this.timestamp = Date.now();
        this.type = 2;
        this.amount = 0;
        this.recipientIdentifier = new Uint8Array(32);
        this.previousHashHeight = 0;
        this.previousBlockHash = new Uint8Array(32);
        this.senderIdentifier = new Uint8Array(32);
        this.senderData = new Uint8Array(0);
        this.signature = new Uint8Array(64);
    }

    SetTimestamp(timestamp) {
        this.timestamp = timestamp;
    }

    SetAmount(amount) {
        this.amount = amount;
    }

    SetRecipientIdentifier(recipientIdentifier) {
        for (let i = 0; i < 32; i++) {
            this.recipientIdentifier[i] = recipientIdentifier[i];
        }
    }

    SetPreviousHashHeight(previousHashHeight) {
        this.previousHashHeight = previousHashHeight;
    }

    SetPreviousBlockHash(previousBlockHash) {
        for (let i = 0; i < 32; i++) {
            this.previousBlockHash[i] = previousBlockHash[i];
        }
    }

    SetSenderIdentifier(senderIdentifier) {
        for (let i = 0; i < 32; i++) {
            this.senderIdentifier[i] = senderIdentifier[i];
        }
    }

    SetSenderData(senderData) {
        this.senderData = new Uint8Array(Math.min(senderData.length, 32));
        for (let i = 0; i < this.senderData.length; i++) {
            this.senderData[i] = senderData[i];
        }
    }

    SetSignature(signature) {
        for (let i = 0; i < 64; i++) {
            this.signature[i] = signature[i];
        }
    }

    Sign(seedBytes) {
        let keyPair = nacl.sign.keyPair.fromSeed(seedBytes);
        for (let i = 0; i < 32; i++) {
            this.senderIdentifier[i] = keyPair.publicKey[i];
        }

        let signature = nacl.sign.detached(this.GetBytes(false), keyPair.secretKey);
        for (let i = 0; i < 64; i++) {
            this.signature[i] = signature[i];
        }
    }

    GetBytes(includeSignature) {
        let forSigning = !includeSignature;
        let buffer = new ByteBuffer(1000);

        buffer.PutByte(2); // transaction type = 2 (standard)
        buffer.PutLong(this.timestamp);
        buffer.PutLong(this.amount);
        buffer.PutBytes(this.recipientIdentifier);

        if (forSigning) {
            buffer.PutBytes(this.previousBlockHash);
        } else {
            buffer.PutLong(this.previousHashHeight);
        }

        buffer.PutBytes(this.senderIdentifier);

        if (forSigning) {
            buffer.PutBytes(NyzoUtil.DoubleSha256(this.senderData));
        } else {
            buffer.PutByte(this.senderData.length);
            buffer.PutBytes(this.senderData);
        }

        if (!forSigning) {
            buffer.PutBytes(this.signature);
        }

        return buffer.ToArray();
    }

    static FromBytes(array) {
        const buffer = new ByteBuffer();
        buffer.InitializeWithArray(array);

        const transaction = new NyzoTransaction();
        transaction.type = buffer.ReadByte();
        transaction.SetTimestamp(buffer.ReadLong());
        transaction.SetAmount(buffer.ReadLong());
        transaction.SetRecipientIdentifier(buffer.ReadBytes(32));
        transaction.SetPreviousHashHeight(buffer.ReadLong());
        transaction.SetSenderIdentifier(buffer.ReadBytes(32));
        const dataLength = buffer.ReadByte();
        transaction.SetSenderData(buffer.ReadBytes(dataLength));
        transaction.SetSignature(buffer.ReadBytes(64));

        return transaction;
    }
}
