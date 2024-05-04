using System.IO;

namespace Nyzo.CL;

public class ByteBuffer {
    private readonly MemoryStream _memoryStream;
	private readonly BinaryWriter _binaryWriter;
	private readonly BinaryReader _binaryReader;

	public ByteBuffer(int? capacity=null) {
		_memoryStream = capacity is null ? new MemoryStream() : new MemoryStream((int)capacity);
		_binaryWriter = new BinaryWriter(_memoryStream);
		_binaryReader = new BinaryReader(_memoryStream);
	}

	// Akin to wrap
	public ByteBuffer(byte[] buffer) {
		_memoryStream = new MemoryStream(buffer);
		_binaryWriter = new BinaryWriter(_memoryStream);
		_binaryReader = new BinaryReader(_memoryStream);
	}

	public bool IsEqualToContentInByteBuffer(ByteBuffer byteBuffer) => byteBuffer.ReadBytes().ToString() == this.ReadBytes().ToString();

    public void PutByte(byte @byte){
        _binaryWriter.Write(@byte);
    }

    public void PutBytes(byte[] bytes){
        _binaryWriter.Write(bytes);
    }

    public long ReadLong(){
        return _binaryReader.ReadInt64();
    }

    public void PutLong(long value){
        _binaryWriter.Write(value);
    }

	public void PutInt(int value) {
		_binaryWriter.Write(value);
	}

	public int ReadInt() {
		return _binaryReader.ReadInt32();
	}

	public void PutString(string value) {
		_binaryWriter.Write(value);
	}

	public string ReadString() {
		return _binaryReader.ReadString();
	}

	public void PutBoolean(bool value) {
		_binaryWriter.Write(value);
	}

	public bool ReadBoolean() {
		return _binaryReader.ReadBoolean();
	}

    public byte ReadByte(){
        return _binaryReader.ReadByte();
    }

	public byte[] ReadBytes() {
		return _memoryStream.ToArray();
	}

    public byte[] ReadBytes(int amount) {
        return _binaryReader.ReadBytes(amount);
    }

	public void Clear() {
		_memoryStream.SetLength(0);
		_memoryStream.Position = 0;
	}

	public void Dispose() {
		_binaryWriter.Dispose();
		_binaryReader.Dispose();
		_memoryStream.Dispose();
	}
}

