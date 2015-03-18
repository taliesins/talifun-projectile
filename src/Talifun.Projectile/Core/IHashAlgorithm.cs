using System.IO;

namespace Talifun.Projectile.Core
{
    public interface IHashAlgorithm
    {
        string Name { get; }
        int HashLength { get; }
        byte[] ComputeHash(Stream stream);
        byte[] ComputeHash(byte[] buffer, int offset, int length);
    }
}