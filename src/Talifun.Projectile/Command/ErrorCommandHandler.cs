using System;
using System.IO;
using ProtoBuf;

namespace Talifun.Projectile.Command
{
    public class ErrorCommandHandler
    {
        public int Execute(Stream stream, long metaDataLength)
        {
            if (metaDataLength != stream.Length)
            {
                throw new Exception("Unexpected stream attached");
            }
            var command = Serializer.Deserialize<ErrorCommand>(stream);
            return Execute(command);
        }

        public int Execute(ErrorCommand command)
        {
            return Execute(command.Exception);
        }

        public int Execute(string exception)
        {
            return 0;
        }
    }
}
