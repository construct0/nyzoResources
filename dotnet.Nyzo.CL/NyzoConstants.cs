using System.Globalization;

namespace Nyzo.CL;

public static class NyzoConstants {
    public static string GenesisBlockHashString => "bc4cca2a2a50a229-256ae3f5b2b5cd49-aa1df1e2d0192726-c4bb41cdcea15364";

	public static byte[] GenesisBlockHash => NyzoUtil.HexStringAsByteArray(NyzoConstants.GenesisBlockHashString);

    public static int MicroNyzosPerNyzo => 1000000;
    public static long TotalNyzosAvailable => 100_000_000;
    public static long TotalMicroNyzosAvailable => NyzoConstants.MicroNyzosPerNyzo * NyzoConstants.TotalNyzosAvailable;

    public static int MaximumSenderDataLength => 32;

    public static double MinimumTransactionAmount {get {
        var microNyzosPerNyzo = NyzoConstants.MicroNyzosPerNyzo.ToString();
        var length = microNyzosPerNyzo.Length - 1;
        var result = "0.";

        for(var i=1; i <= length; i++){
            result += ((i == length) ? "1" : "0");
        }

        return double.Parse(result, CultureInfo.InvariantCulture);
    }}

}