using System;

namespace Talifun.Projectile.Core
{
    public class CorruptFileFormatException : Exception
    {
        public CorruptFileFormatException(string message) : base(message)
        {
        }
    }
}