using System;

namespace NyzoCL;

public class NyzoConverter {
    public static string PublicIdentifierForPrivateKey(string keyString){
        var identifierString = "";

        // Decode the key string
        keyString = keyString.Trim();
        var key = NyzoStringEncoder.DecodePrivateSeed(keyString);

        if(key?.Seed is not null){
            // Get the identifier for the key and make an identifier string
            var keyPair = Sodium.PublicKeyAuth.GenerateKeyPair(key.Seed);
            identifierString = NyzoStringEncoder.NyzoStringFromPublicIdentifier(keyPair.PublicKey);
        }

        return identifierString;
    }

    public static string GetDisplayAmount(double amount, bool isMicroNyzos=true){
        var division = isMicroNyzos ? 1000000 : 1;
        return "&cap;" + (amount / division).ToString("N6");
    }

    public static double GetAmountOfMicroNyzos(string valueString){
        return Math.Floor(double.Parse(valueString) * NyzoConstants.MicroNyzosPerNyzo);
    }
}

