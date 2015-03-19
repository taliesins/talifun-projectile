using System;
using System.IO;
using Talifun.Projectile.Protocol;

namespace Talifun.Projectile.Command
{
    public class SendFileRequestHandler 
    {
        public Reply<SendFileReply> Execute(SendFileRequest command, Stream stream = null)
        {
            return Execute(command.FilePath, stream);
        }

        public Reply<SendFileReply> Execute(string filePath, Stream stream = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException("filePath", "No file path was specified");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found: " + filePath);
            }

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var message = new SendFileReply();
            var reply = new Reply<SendFileReply>
            {
                Message = message,
                Stream = fileStream
            };

            return reply;
        }
    }
}
