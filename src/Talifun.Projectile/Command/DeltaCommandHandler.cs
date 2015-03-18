using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using Talifun.Projectile.Core;

namespace Talifun.Projectile.Command
{
    public class DeltaCommandHandler 
    {
        private readonly List<Action<DeltaBuilder>> _configuration = new List<Action<DeltaBuilder>>();

        public int Execute(Stream stream, long metaDataLength)
        {
            if (metaDataLength != stream.Length)
            {
                throw new Exception("Unexpected stream attached");
            }
            var command = Serializer.Deserialize<DeltaCommand>(stream);
            return Execute(command);
        }

        public int Execute(DeltaCommand command)
        {
            return Execute(command.NewFilePath, command.SignatureFilePath, command.DeltaFilePath);
        }

        public int Execute(string newFilePath, string signatureFilePath, string deltaFilePath)
        {
            if (string.IsNullOrWhiteSpace(signatureFilePath))
                throw new ArgumentNullException("No signature file was specified", "new-file");
            if (string.IsNullOrWhiteSpace(newFilePath))
                throw new ArgumentNullException("No new file was specified", "new-file");

            newFilePath = Path.GetFullPath(newFilePath);
            signatureFilePath = Path.GetFullPath(signatureFilePath);

            var delta = new DeltaBuilder();
            foreach (var config in _configuration) config(delta);

            if (!File.Exists(signatureFilePath))
            {
                throw new FileNotFoundException("File not found: " + signatureFilePath, signatureFilePath);
            }

            if (!File.Exists(newFilePath))
            {
                throw new FileNotFoundException("File not found: " + newFilePath, newFilePath);
            }

            if (string.IsNullOrWhiteSpace(deltaFilePath))
            {
                deltaFilePath = newFilePath + ".octodelta";
            }
            else
            {
                deltaFilePath = Path.GetFullPath(deltaFilePath);
                var directory = Path.GetDirectoryName(deltaFilePath);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }

            using (var newFileStream = new FileStream(newFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var signatureStream = new FileStream(signatureFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var deltaStream = new FileStream(deltaFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                delta.BuildDelta(newFileStream, new SignatureReader(signatureStream, delta.ProgressReporter), new AggregateCopyOperationsDecorator(new BinaryDeltaWriter(deltaStream)));
            }

            return 0;
        }
    }
}
