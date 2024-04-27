export default class ByteBuffer {
    constructor(maximumSize) {
        this.index = 0;
        this.array = new Uint8Array(Math.max(maximumSize, 1));
    }

    InitializeWithArray(bytes) {
        this.index = 0;
        this.array = bytes;
    }

    PutBytes(bytes) {
        for (let i = 0; i < bytes.length; i++) {
            this.array[this.index++] = bytes[i];
        }
    }

    PutByte(byte) {
        this.array[this.index++] = byte;
    }

    PutIntegerValue(value, length) {
        value = Math.floor(value);
        for (let i = 0; i < length; i++) {
            this.array[this.index + length - 1 - i] = value % 256;
            value = Math.floor(value / 256);
        }
        this.index += length;
    }

    PutShort(value) {
        this.PutIntegerValue(value, 2);
    }

    PutInt(value) {
        this.PutIntegerValue(value, 4);
    }

    PutLong(value) {
        this.PutIntegerValue(value, 8);
    }

    ToArray() {
        let result = new Uint8Array(this.index);
        for (let i = 0; i < this.index; i++) {
            result[i] = this.array[i];
        }

        return result;
    }

    ReadBytes(length) {
        const result = new Uint8Array(length);
        for (let i = 0; i < length; i++) {
            result[i] = this.array[this.index++];
        }

        return result;
    }

    ReadByte() {
        return this.array[this.index++];
    }

    ReadIntegerValue(length) {
        let result = 0;
        for (let i = 0; i < length; i++) {
            result *= 256;
            result += this.array[this.index + i];
        }
        this.index += length;

        return result;
    }

    ReadShort() {
        return this.ReadIntegerValue(2);
    }

    ReadInt() {
        return this.ReadIntegerValue(4);
    }

    ReadLong() {
        return this.ReadIntegerValue(8);
    }
}