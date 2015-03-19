using System;
using System.IO;
using Talifun.Projectile.Protocol;

namespace Talifun.Projectile.Command
{
    public class SendFileReplyHandler 
    {
        public Reply Execute(SendFileReply command, Stream stream = null)
        {
            return Execute(command.FileName, stream);
        }

        public Reply Execute(string filePath, Stream stream = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException("filePath", "No file path was specified");

            return null;
        }
    }
}
