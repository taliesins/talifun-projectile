using System;
using System.IO;
using ProtoBuf;

namespace Talifun.Projectile.Protocol
{
    public abstract class Reply
    {
        internal abstract Action<Stream> AddMessageToStream();
        public Stream Stream { get; set; }
    }

    public class Reply<T> : Reply where T : class
    {
        public T Message { get; set; }

        internal override Action<Stream> AddMessageToStream()
        {
            if (Message == null) return (stream) => { };
            return (stream)=>Serializer.Serialize<T>(stream, Message);
        }
    }
}
