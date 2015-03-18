using System.Collections.Generic;

namespace Talifun.Projectile.Core
{
    public class Signature
    {
        public Signature(IHashAlgorithm hashAlgorithm, IRollingChecksum rollingChecksumAlgorithm)
        {
            HashAlgorithm = hashAlgorithm;
            RollingChecksumAlgorithm = rollingChecksumAlgorithm;
            Chunks = new List<ChunkSignature>();
        }

        public IHashAlgorithm HashAlgorithm { get; private set; }
        public IRollingChecksum RollingChecksumAlgorithm { get; private set; }
        public List<ChunkSignature> Chunks { get; private set; } 
    }
}