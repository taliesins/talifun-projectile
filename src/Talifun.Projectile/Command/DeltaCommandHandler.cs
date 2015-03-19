using System;
using System.Collections.Generic;
using System.IO;
using Talifun.Projectile.Core;
using Talifun.Projectile.Protocol;

namespace Talifun.Projectile.Command
{
    public class DeltaCommandHandler 
    {
        private readonly List<Action<DeltaBuilder>> _configuration = new List<Action<DeltaBuilder>>();

        public Reply Execute(DeltaCommand command, Stream stream = null)
        {
            return Execute(command.NewFilePath, command.SignatureFilePath, command.DeltaFilePath, stream);
        }

        public Reply Execute(string newFilePath, string signatureFilePath, string deltaFilePath, Stream stream = null)
        {
            if (string.IsNullOrWhiteSpace(signatureFilePath))
                throw new ArgumentNullException("signatureFilePath", "No signature file was specified");
            if (string.IsNullOrWhiteSpace(newFilePath))
                throw new ArgumentNullException("newFilePath", "No new file was specified");

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

            return null;
        }
    }
}
