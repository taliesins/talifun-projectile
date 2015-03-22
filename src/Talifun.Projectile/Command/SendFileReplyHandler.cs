using System;
using System.IO;
using Talifun.Projectile.Protocol;

namespace Talifun.Projectile.Command
{
    public class SendFileReplyHandler 
    {
        public Reply Execute(SendFileReply command, Stream stream = null)
        {
            return Execute(command.LocalFilePath, command.RemoteFilePath, stream);
        }

        public Reply Execute(string localFilePath, string remoteFilePath = null, Stream stream = null)
        {
            if (string.IsNullOrWhiteSpace(localFilePath))
                throw new ArgumentNullException("localFilePath", "No file path was specified");

            if (stream == null)
                throw new ArgumentNullException("stream", "No file stream");

            const int bufferSize = 4096;
            using (var fileStream = File.Create(localFilePath, bufferSize, FileOptions.SequentialScan | FileOptions.Asynchronous  ))
            {
                stream.CopyTo(fileStream);
            }
         
            return null;
        }
    }
}
