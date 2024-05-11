using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nyzo.CL.Tests;

public class ByteBufferTests {
	[Fact]
	public void ConstructingWith_PresetCapacity_ShouldNotAllowInsertsBeyondCapacity() {
		// Arrange
		int maxSize = 10000;
		var buffer = new ByteBuffer(maxSize);

		// Act - fill the buffer to its max capacity
		for (var i = 0; i<maxSize; i++) {
			buffer.PutByte(0xf);
		}

		// Assert the buffer is at max capacity
		Assert.Equal(maxSize, buffer.Length);

		// Assert exception for one extra entry occurs
		Assert.Throws<IndexOutOfRangeException>(() => buffer.PutByte(0xf));
	}

	[Fact]
	public void ConstructingWith_ProvidedByteArray_ShouldAllowValuesToBeExtracted() {
		// Arrange
		var array = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
		var byteBuffer = new ByteBuffer(array);

		// Act
		for(var i=0; i < array.Length; i++) {
			_ = byteBuffer.ReadByte();
		}

		// Assert
		Assert.Throws<IndexOutOfRangeException>(() => byteBuffer.ReadByte());
	}

	[Fact]
	public void ConstructingWith_ProvidedByteArray_ShouldAdhereToWriteBoundsOfInitialArraySize() {
		// Arrange
		var array = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
		var byteBuffer = new ByteBuffer(array);

		// Act
		byteBuffer.SetPosition(byteBuffer.Length - 1);

		// Assert
		Assert.Throws<ArgumentOutOfRangeException>(() => byteBuffer.SetPosition(array.Length));
		Assert.Throws<ArgumentOutOfRangeException>(() => byteBuffer.SetPosition(-1));
		Assert.Throws<ArgumentException>(() => byteBuffer.PutBytes(new byte[2] { 10, 11 }));
		Assert.Throws<ArgumentException>(() => byteBuffer.PutBytes(array));
	}

	[Fact]
	public void ComparingByteBufferWith_DifferentByteBufferInstance_ShouldYieldCorrectResult() {
		// Arrange
		var array = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
		var array2 = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
		var byteBufferDuo1 = new ByteBuffer(array);
		var byteBufferDuo2 = new ByteBuffer(array2);

		// Act
		// Assert
		Assert.True(byteBufferDuo1.Equals(byteBufferDuo2));
		Assert.True(byteBufferDuo2.Equals(byteBufferDuo1));
	}

	[Fact]
	public void PuttingByte_ShouldAllowForRetrieval() {
		// Arrange
		var byteBuffer = new ByteBuffer(1);

		// Act
		byte input = 0xf;
		byteBuffer.PutByte(input);
		byteBuffer.ResetPosition();

		// Assert
		Assert.Equal(1, byteBuffer.Length);
		Assert.Equal(0, byteBuffer.Position);
		Assert.Equal(byteBuffer.Length, byteBuffer.Length - byteBuffer.Position);

		Assert.Equal(input, byteBuffer.ReadByte());
		Assert.Equal(1, byteBuffer.Position);
	}

	[Fact]
	public void Constructing_OrPuttingBytes_ShouldAllowForRetrieval_ByOne_ByGivenAmount_OrAllAtOnce() {
		// Arrange
		var array = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
		var array2 = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
		var array3 = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
		var array4 = new byte[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
		var byteBuffer1 = new ByteBuffer(array);
		var byteBuffer2 = new ByteBuffer(array2);
		var byteBuffer3 = new ByteBuffer(array3);
		var byteBuffer4 = new ByteBuffer(array4.Length);
		byteBuffer4.PutBytes(array4);

		// Act
		byte firstByte = byteBuffer1.ReadByte();
		byte[] firstHalfOfBytes = byteBuffer2.ReadBytes(array2.Length / 2);
		byte[] allBytes = byteBuffer3.ReadBytes();
		byte[] allBytesFromPut = byteBuffer4.ReadBytes();

		// Assert
		Assert.Equal(array[0], firstByte);
		
		Assert.Equal(array2.Length / 2, firstHalfOfBytes.Length);
		Assert.Equal(array2.Length / 2, byteBuffer2.Position);

		Assert.Equal(array3.Length, allBytes.Length);
		Assert.Equal(array3.Length, byteBuffer3.Position);

		Assert.Equal(array4.Length, allBytesFromPut.Length);
		Assert.Equal(array4.Length, byteBuffer4.Position);
	}

	[Fact]
	public void PuttingLong_ShouldAllowForRetrieval() {
		// Arrange
		var buffer1 = new ByteBuffer(8);
		var buffer2 = new ByteBuffer(8);
		var buffer3 = new ByteBuffer(8);

		// Act
		buffer1.PutInt64(long.MaxValue);
		buffer2.PutInt64(0);
		buffer3.PutInt64(long.MinValue);

		buffer1.ResetPosition();
		buffer2.ResetPosition();
		buffer3.ResetPosition();

		var read = buffer1.ReadInt64();
		var read2 = buffer2.ReadInt64();
		var read3 = buffer3.ReadInt64();

		// Assert
		Assert.Equal(long.MaxValue, read);
		Assert.Equal(0, read2);
		Assert.Equal(long.MinValue, read3);

		Assert.Equal(8, buffer1.Position);
		Assert.Equal(8, buffer2.Position);
		Assert.Equal(8, buffer3.Position);
	}

	[Fact]
	public void PuttingInt_ShouldAllowForRetrieval() {
		// Arrange
		var buffer1 = new ByteBuffer(4);
		var buffer2 = new ByteBuffer(4);
		var buffer3 = new ByteBuffer(4);

		// Act
		buffer1.PutInt32(int.MaxValue);
		buffer2.PutInt32(0);
		buffer3.PutInt32(int.MinValue);

		buffer1.ResetPosition();
		buffer2.ResetPosition();
		buffer3.ResetPosition();

		var read = buffer1.ReadInt32();
		var read2 = buffer2.ReadInt32();
		var read3 = buffer3.ReadInt32();

		// Assert
		Assert.Equal(int.MaxValue, read);
		Assert.Equal(0, read2);
		Assert.Equal(int.MinValue, read3);

		Assert.Equal(4, buffer1.Position);
		Assert.Equal(4, buffer2.Position);
		Assert.Equal(4, buffer3.Position);
	}

	[Fact]
	public void PuttingShort_ShouldAllowForRetrieval() {
		// Arrange
		var buffer1 = new ByteBuffer(2);
		var buffer2 = new ByteBuffer(2);
		var buffer3 = new ByteBuffer(2);

		// Act
		buffer1.PutInt16(short.MaxValue);
		buffer2.PutInt16(0);
		buffer3.PutInt16(short.MinValue);

		buffer1.ResetPosition();
		buffer2.ResetPosition();
		buffer3.ResetPosition();

		var read = buffer1.ReadInt16();
		var read2 = buffer2.ReadInt16();
		var read3 = buffer3.ReadInt16();

		// Assert
		Assert.Equal(short.MaxValue, read);
		Assert.Equal(0, read2);
		Assert.Equal(short.MinValue, read3);

		Assert.Equal(2, buffer1.Position);
		Assert.Equal(2, buffer2.Position);
		Assert.Equal(2, buffer3.Position);
	}

	[Fact]
	public void PuttingString_ShouldAllowForRetrieval() {
		var value = "This is a test string with \0 an erroneous null char";
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		byte[] procBytes = Encoding.UTF8.GetBytes(value.Replace("\0", "\\0") +  "\0");
		var buffer = new ByteBuffer(bytes.Length); // 51
		var buffer2 = new ByteBuffer(procBytes.Length); // 53

		buffer2.PutString(value);

		Assert.Throws<ArgumentException>(() => buffer.PutString(value));
		Assert.Equal(buffer2.Length, buffer2.Position);

		buffer2.ResetPosition();

		var valueRead = buffer2.ReadString();

		Assert.Equal(value, valueRead);

		buffer2.ResetPosition();

		var unsanitizedValueRead = buffer2.ReadString(false);

		Assert.EndsWith("\0", unsanitizedValueRead);
		Assert.Contains("\\0", unsanitizedValueRead);
	}

	[Fact]
	public void PuttingBool_ShouldAllowForRetrieval() {
		// Arrange
		var buffer = new ByteBuffer(1);

		// Act
		buffer.PutBoolean(true);
		buffer.ResetPosition();

		// Assert
		Assert.True(buffer.ReadBoolean());
		Assert.Throws<ArgumentOutOfRangeException>(() => buffer.ReadBoolean());
	}

	[Fact]
	public void PuttingAllSupported_ShouldAllowFor_AccurateRetrieval() {
		int capacity = 51;

		var buffer = new ByteBuffer(capacity);

		buffer.PutByte(1);
		buffer.PutSByte(2);
		
		buffer.PutBytes(new byte[2] {3,4});
		buffer.PutSBytes(new sbyte[2] {5,6});

		buffer.PutUInt16(7);
		buffer.PutInt16(8);

		buffer.PutUInt32((uint)short.MaxValue + 1);
		buffer.PutInt32((int)short.MaxValue + 2);

		buffer.PutUInt64((ulong)int.MaxValue + 1);
		buffer.PutInt64((long)int.MaxValue + 2);

		buffer.PutFloat(float.MaxValue);
		buffer.PutDouble(double.MaxValue);

		buffer.PutBoolean(true);
		buffer.PutString("abc");

		Assert.Equal(capacity, buffer.Length);
		Assert.Equal(capacity, buffer.Position);

		buffer.ResetPosition();

		Assert.Equal(1, buffer.ReadByte());
		Assert.Equal(2, buffer.ReadSByte());

		Assert.Contains((byte)3, buffer.ReadBytes(2));
		Assert.Contains((sbyte)5, buffer.ReadSBytes(2));

		Assert.Equal(7, buffer.ReadUInt16());
		Assert.Equal(8, buffer.ReadInt16());

		Assert.Equal((uint)short.MaxValue + 1, buffer.ReadUInt32());
		Assert.Equal((int)short.MaxValue + 2, buffer.ReadInt32());

		Assert.Equal((ulong)int.MaxValue + 1, buffer.ReadUInt64());
		Assert.Equal((long)int.MaxValue + 2, buffer.ReadInt64());

		Assert.Equal(float.MaxValue, buffer.ReadFloat());
		Assert.Equal(double.MaxValue, buffer.ReadDouble());

		Assert.True(buffer.ReadBoolean());
		Assert.Equal("abc", buffer.ReadString());

		Assert.Equal(capacity, buffer.Position);
		Assert.Throws<IndexOutOfRangeException>(() => buffer.ReadByte());
	}

	[Fact]
	public void Clearing_ShouldNotAllowSubsequentRetrieval() {
		// Arrange
		var buffer = new ByteBuffer(4);

		buffer.PutInt32(int.MaxValue);

		Assert.Equal(4, buffer.Position);
		Assert.Equal(4, buffer.Length);
		buffer.Clear();
		Assert.Equal(0, buffer.Position);
		Assert.Equal(0, buffer.Length);

		Assert.Throws<IndexOutOfRangeException>(() => buffer.PutByte(0));
	}
}
