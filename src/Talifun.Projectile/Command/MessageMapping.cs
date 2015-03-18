using System;
using System.IO;

namespace Talifun.Projectile.Command
{
    public class MessageMapping
    {
        public MessageType MessageType { get; set; }
        public Type Type { get; set; }
        public Func<Stream, long, int> Handler { get; set; }
    }
}