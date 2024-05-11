namespace Nyzo.CL;

public class NyzoStringPrivateKey {
    public byte[] Seed {get;init;}

    public NyzoStringPrivateKey(byte[] seed){
        this.Seed = seed;
    }
}

