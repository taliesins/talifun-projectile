using System;
using System.IO;
using Talifun.Projectile.Protocol;

namespace Talifun.Projectile.Command
{
    public class MessageMapping
    {
        public MessageType MessageType { get; set; }
        public Type Type { get; set; }
        public Func<Stream, Stream, Reply> Handler { get; set; }
    }
}