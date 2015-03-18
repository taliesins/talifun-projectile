using System;
using System.IO;
using ProtoBuf;
using Talifun.Projectile.Core;
using Talifun.Projectile.Diagnostics;

namespace Talifun.Projectile.Command
{
    public class PatchCommandHandler 
    {
        private IProgressReporter _progressReporter;

        public int Execute(Stream stream, long metaDataLength)
        {
            if (metaDataLength != stream.Length)
            {
                throw new Exception("Unexpected stream attached");
            }

            var command = Serializer.Deserialize<PatchCommand>(stream);
            return Execute(command);
        }

        public int Execute(PatchCommand command)
        {
            return Execute(command.BasisFilePath, command.DeltaFilePath, command.NewFilePath, command.SkipHashCheck);
        }

        public int Execute(string basisFilePath, string deltaFilePath, string newFilePath, bool skipHashCheck)
        {
            if (string.IsNullOrWhiteSpace(basisFilePath))
                throw new ArgumentNullException("No basis file was specified", "basis-file");
            if (string.IsNullOrWhiteSpace(deltaFilePath))
                throw new ArgumentNullException("No delta file was specified", "delta-file");
            if (string.IsNullOrWhiteSpace(newFilePath))
                throw new ArgumentNullException("No new file was specified", "new-file");

            basisFilePath = Path.GetFullPath(basisFilePath);
            deltaFilePath = Path.GetFullPath(deltaFilePath);
            newFilePath = Path.GetFullPath(newFilePath);

            if (!File.Exists(basisFilePath)) throw new FileNotFoundException("File not found: " + basisFilePath, basisFilePath);
            if (!File.Exists(deltaFilePath)) throw new FileNotFoundException("File not found: " + deltaFilePath, deltaFilePath);

            var directory = Path.GetDirectoryName(newFilePath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var delta = new DeltaApplier
            {
                SkipHashCheck = skipHashCheck
            };

            using (var basisStream = new FileStream(basisFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var deltaStream = new FileStream(deltaFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var newFileStream = new FileStream(newFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                delta.Apply(basisStream, new BinaryDeltaReader(deltaStream, _progressReporter), newFileStream);
            }

            return 0;
        }
    }
}
