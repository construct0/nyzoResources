using Sodium;

namespace Nyzo.CL;

public class NyzoStringPrivateKey {
    public byte[] Seed {get;init;}
    public KeyPair KeyPair {get;init;}

    public NyzoStringPrivateKey(byte[] seed){
        this.Seed = seed;
        this.KeyPair = Sodium.PublicKeyAuth.GenerateKeyPair(seed);
    }
}

