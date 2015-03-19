using System;
using System.Collections.Generic;
using System.IO;
using Talifun.Projectile.Core;
using Talifun.Projectile.Protocol;

namespace Talifun.Projectile.Command
{
    public class SignatureCommandHander
    {
        private readonly List<Action<SignatureBuilder>> _configuration = new List<Action<SignatureBuilder>>();

        public Reply Execute(SignatureCommand command, Stream stream = null)
        {
            return Execute(command.BasisFilePath, command.SignatureFilePath, stream);
        }

        public Reply Execute(string basisFilePath, string signatureFilePath, Stream stream = null)
        {
            if (string.IsNullOrWhiteSpace(basisFilePath))
                throw new ArgumentNullException("basisFilePath", "No basis file was specified");

            basisFilePath = Path.GetFullPath(basisFilePath);

            var signatureBuilder = new SignatureBuilder();
            foreach (var config in _configuration) config(signatureBuilder);

            if (!File.Exists(basisFilePath))
            {
                throw new FileNotFoundException("File not found: " + basisFilePath, basisFilePath);
            }

            if (string.IsNullOrWhiteSpace(signatureFilePath))
            {
                signatureFilePath = basisFilePath + ".octosig";
            }
            else
            {
                signatureFilePath = Path.GetFullPath(signatureFilePath);
                var sigDirectory = Path.GetDirectoryName(signatureFilePath);
                if (sigDirectory != null && !Directory.Exists(sigDirectory))
                {
                    Directory.CreateDirectory(sigDirectory);
                }
            }

            using (var basisStream = new FileStream(basisFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var signatureStream = new FileStream(signatureFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                signatureBuilder.Build(basisStream, new SignatureWriter(signatureStream));
            }

            return null;
        }
    }
}
