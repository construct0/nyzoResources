using System;
using System.Text;

namespace Nyzo.CL;

public static class FieldByteSize {
	public const int BalanceListLength = 4;
	public const int BlockHeight = 8;
	public const int BlocksUntilFee = 2;
	public const int BooleanField = 1;
	public const int CycleLength = 4;
	public const int CombinedVersionAndHeight = 8;
	public const int FrozenBlockListLength = 2;
	public const int Hash = 32;
	public const int HashListLength = 1;
	public const int IpAddress = 4;
	public const int MaximumSenderDataLength = 32;
	public const int MessageLength = 4;
	public const int Port = 4;
	public const int RolloverTransactionFees = 1;
	public const int Seed = 32;
	public const int Timestamp = 8;
	public const int TransactionAmount = 8;
	public const int TransactionType = 1;
	public const int MessageType = 2;
	public const int Identifier = 32;
	public const int NodeListLength = 4;
	public const int Signature = 64;
	public const int StringLength = 2;
	public const int UnfrozenBlockPoolLength = 2;
	public const int UnnamedByte = 1;
	public const int UnnamedDouble = 8;
	public const int UnnamedInteger = 4;
	public const int UnnamedShort = 2;
	public const int VoteListLength = 1;

	public static int ForString(string? value) {
		return 
			FieldByteSize.StringLength 
			+ (value is null ? 0 : Encoding.UTF8.GetBytes(value).Length);
	}

	public static int ForString(string? value, int maximumStringByteLength) {
		return
			FieldByteSize.StringLength
			+ (
				value is null ? 0 
				: Math.Max(0,
					Math.Min(
						Encoding.UTF8.GetBytes(value).Length,
						maximumStringByteLength
					)
				)
			);
	}

}
