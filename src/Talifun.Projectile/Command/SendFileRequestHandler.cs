using System;
using System.IO;
using Talifun.Projectile.Protocol;

namespace Talifun.Projectile.Command
{
    public class SendFileRequestHandler 
    {
        public Reply<SendFileReply> Execute(SendFileRequest command, Stream stream = null)
        {
            return Execute(command.RemoteFilePath, command.LocalFilePath, stream);
        }

        public Reply<SendFileReply> Execute(string remoteFilePath, string localFilePath, Stream stream = null)
        {
            if (string.IsNullOrWhiteSpace(remoteFilePath))
                throw new ArgumentNullException("remoteFilePath", "No file path was specified");

            if (!File.Exists(remoteFilePath))
            {
                throw new FileNotFoundException("File not found: " + remoteFilePath);
            }

            const int bufferSize = 4096;
            var fileStream = new FileStream(remoteFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan | FileOptions.Asynchronous);

            var message = new SendFileReply
            {
                LocalFilePath = localFilePath,
                RemoteFilePath = remoteFilePath
            };

            var reply = new Reply<SendFileReply>
            {
                Message = message,
                Stream = fileStream
            };

            return reply;
        }
    }
}
