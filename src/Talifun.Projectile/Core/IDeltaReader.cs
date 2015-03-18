using System;

namespace Talifun.Projectile.Core
{
    public interface IDeltaReader
    {
        byte[] ExpectedHash { get; }
        IHashAlgorithm HashAlgorithm { get; }
        void Apply(
            Action<byte[]> writeData,
            Action<long, long> copy
            );
    }
}