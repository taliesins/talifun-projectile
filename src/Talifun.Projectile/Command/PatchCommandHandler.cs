using System;
using System.IO;
using Talifun.Projectile.Core;
using Talifun.Projectile.Diagnostics;

namespace Talifun.Projectile.Command
{
    public class PatchCommandHandler 
    {
        private IProgressReporter _progressReporter;

        public Reply Execute(PatchCommand command, Stream stream = null)
        {
            return Execute(command.BasisFilePath, command.DeltaFilePath, command.NewFilePath, command.SkipHashCheck, stream);
        }

        public Reply Execute(string basisFilePath, string deltaFilePath, string newFilePath, bool skipHashCheck, Stream stream = null)
        {
            if (string.IsNullOrWhiteSpace(basisFilePath))
                throw new ArgumentNullException("basisFilePath", "No basis file was specified");
            if (string.IsNullOrWhiteSpace(deltaFilePath))
                throw new ArgumentNullException("deltaFilePath", "No delta file was specified");
            if (string.IsNullOrWhiteSpace(newFilePath))
                throw new ArgumentNullException("newFilePath", "No new file was specified");

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

            return null;
        }
    }
}
