namespace Nyzo.CL;

using System;
using System.Linq;
using System.Text;

public class ByteBuffer {
	public byte[] Buffer { get; private set; }
	public int Size => this.Buffer.Length;
	public int Length => this.Size;
	public int Position { get; set; } = 0;

	public ByteBuffer(byte[] buffer) {
		Buffer = buffer;
	}

	public ByteBuffer(int size) {
		Buffer = new byte[size];
	}

	// Compare
	public bool IsEqualTo(ByteBuffer byteBuffer) {
		if(this == byteBuffer) {
			throw new InvalidOperationException("Provide a unique ByteBuffer instance to compare with");
		}

		var thisString = BitConverter.ToString(this.Buffer).Replace("-", "");
		var inputString = BitConverter.ToString(byteBuffer.Buffer).Replace("-", "");

		return thisString == inputString;
	}

	public override bool Equals(object? obj) {
		if(obj is null || obj.GetType() != this.GetType()) {
			return false;
		}

		return this.IsEqualTo((ByteBuffer)obj);
	}

	public override int GetHashCode() {
		throw new NotImplementedException();
	}

	// Position
	public void SetPosition(int position) {
		if(position < 0 || position >= this.Size) {
			throw new ArgumentOutOfRangeException($"Position must be a valid index");
		}

		this.Position = position;
	}

	public void ResetPosition() {
		this.SetPosition(0);
	}

	// Reads
	public byte ReadByte() {
		var value = Buffer[Position];
		Position++;

		return value;
	}

	public sbyte ReadSByte() {
		var value = (sbyte)Buffer[Position];
		Position++;

		return value;
	}

	public byte[] ReadBytes() {
		byte[] bytes = [.. this.Buffer];
		Position = this.Buffer.Length; // this Position does not exist & is intentional to avoid accidental overwrites

		return bytes;
	}

	public byte[] ReadBytes(int amount) {
		byte[] result = new byte[amount];
		Array.Copy(Buffer, Position, result, 0, amount);
		Position += amount;

		return result;
	}

	public sbyte[] ReadSBytes() {
		byte[] bytes = [.. this.Buffer];
		sbyte[] sbytes = [.. bytes.ToList().ConvertAll(x => (sbyte)x)];
		Position = this.Buffer.Length; // this Position does not exist & is intentional to avoid accidental overwrites

		return sbytes;
	}

	public sbyte[] ReadSBytes(int amount) {
		byte[] bytes = new byte[amount];
		Array.Copy(Buffer, Position, bytes, 0, amount);
		sbyte[] sbytes = [.. bytes.ToList().ConvertAll(x => (sbyte)x)];

		Position += amount;
		return sbytes;
	}
	
	public ushort ReadUInt16() {
		var value = BitConverter.ToUInt16(Buffer, Position);
		Position += 2;

		return value;
	}

	public short ReadInt16() {
		var value = BitConverter.ToInt16(Buffer, Position);
		Position += 2;

		return value;
	}

	public uint ReadUInt32() {
		var value = BitConverter.ToUInt32(Buffer, Position);
		Position += 4;

		return value;
	}

	public int ReadInt32() {
		var value = BitConverter.ToInt32(Buffer, Position);
		Position += 4;

		return value;
	}

	public ulong ReadUInt64() {
		var value = BitConverter.ToUInt64(Buffer, Position);
		Position += 8;

		return value;
	}

	public long ReadInt64() {
		var value = BitConverter.ToInt64(Buffer, Position);
		Position += 8;

		return value;
	}

	public float ReadFloat() {
		var value = BitConverter.ToSingle(Buffer, Position);
		Position += 4;

		return value;
	}

	public double ReadDouble() {
		var value = BitConverter.ToDouble(Buffer, Position);
		Position += 8;

		return value;
	}

	public bool ReadBoolean() {
		var value = BitConverter.ToBoolean(Buffer, Position);
		Position++;

		return value;
	}

	public string ReadString() {
		int stringEnd = Array.IndexOf(Buffer, (byte)'\0', Position) + 1;
		var value = Encoding.UTF8.GetString(Buffer, Position, stringEnd - Position);
		Position += value.Length;

		return value;
	}

	// Writes
	public void PutByte(byte value) {
		Buffer[Position] = value;
		Position++;
	}

	public void PutSByte(sbyte value) {
		Buffer[Position] = (byte)value;
		Position++;
	}

	public void PutBytes(byte[] bytes) {
		Array.Copy(bytes, 0, Buffer, Position, bytes.Length);
		Position += bytes.Length;
	}

	public void PutSBytes(sbyte[] sbytes) {
		byte[] bytes = sbytes.ToList()
							 .ConvertAll(x => (byte)x)
							 .ToArray()
							 ;

		this.PutBytes(bytes);
	}

	public void PutUInt16(ushort value) {
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Copy(bytes, 0, Buffer, Position, 2);
		Position += 2;
	}

	public void PutInt16(short value) {
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Copy(bytes, 0, Buffer, Position, 2);
		Position += 2;
	}

	public void PutUInt32(uint value) {
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Copy(bytes, 0, Buffer, Position, 4);
		Position += 4;
	}

	public void PutInt32(int value) {
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Copy(bytes, 0, Buffer, Position, 4);
		Position += 4;
	}

	public void PutUInt64(ulong value) {
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Copy(bytes, 0, Buffer, Position, 8);
		Position += 8;
	}

	public void PutInt64(long value) {
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Copy(bytes, 0, Buffer, Position, 8);
		Position += 8;
	}

	public void PutFloat(float value) {
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Copy(bytes, 0, Buffer, Position, 4);
		Position += 4;
	}

	public void PutDouble(double value) {
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Copy(bytes, 0, Buffer, Position, 8);
		Position += 8;
	}

	public void PutBoolean(bool value) {
		byte[] bytes = BitConverter.GetBytes(value);
		Array.Copy(bytes, 0, Buffer, Position, 1);
		Position += 1;
	}

	public void PutString(string value) {
		value = value.Replace("\0", "\\0");

		byte[] bytes = Encoding.UTF8.GetBytes(value + '\0');
		Array.Copy(bytes, 0, Buffer, Position, bytes.Length);
		Position += bytes.Length;
	}
}
