using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nyzo.CL;

/// <summary>
/// <para>Denotes the type of NyzoString & is hexadecimally represented by the first 3 bytes of a NyzoString encoded string.</para>
/// <para><see href="https://tech.nyzo.org/dataFormats/nyzoString">View specification.</see></para>
/// </summary>
public static class NyzoStringPrefix {
	/// <summary>
	/// The absolute length of a prefix.
	/// </summary>
	public const short Length = 4;

	/// <summary>
	/// <para>Not referenced within this CL, yet. Micropay JS classes were not ported due to uncertainty about their relevance within a predominantly backend oriented environment.</para>
	/// <para><c>60a87f</c>: several fields bundled to support 1st-generation Micropay</para>
	/// </summary>
	public const string Micropay = "pay_";

	/// <summary>
	/// <para><c>61a3bf</c>: receiver identifier and sender-data field</para>
	/// </summary>
	public const string PrefilledData = "pre_";

	/// <summary>
	/// <para><c>50e87f</c>: 32-byte (256-bit) seed for the private key for a Nyzo wallet</para>
	/// </summary>
	public const string PrivateKey = "key_";

	/// <summary>
	/// <para><c>48dfff</c>: 32-byte (256-bit) public identifier for a Nyzo wallet</para>
	/// </summary>
	public const string PublicIdentifier = "id__";

	// todo - reference within encoder
	/// <summary>
	/// <para><c>6d243f</c>: 64-byte (512-bit) Ed25519 signature</para>
	/// </summary>
	public const string Signature = "sig_";

	/// <summary>
	/// <para><c>720fff</c>: full Nyzo transaction; supports all types of transactions</para>
	/// </summary>
	public const string Transaction = "tx__";
	
}
