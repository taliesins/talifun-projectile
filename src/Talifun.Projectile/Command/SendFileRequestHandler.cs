using System;
using System.IO;
using ProtoBuf;

namespace Talifun.Projectile.Command
{
    public class SendFileRequestHandler 
    {
        public int Execute(Stream stream, long metaDataLength)
        {
            if (metaDataLength != stream.Length)
            {
                throw new Exception("Unexpected stream attached");
            }
            var command = Serializer.Deserialize<SendFileRequest>(stream);
            return Execute(command);
        }

        public int Execute(SendFileRequest command)
        {
            return Execute(command.FilePath);
        }

        public int Execute(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException("No file path was specified", "filePath");

            return 0;
        }
    }
}
