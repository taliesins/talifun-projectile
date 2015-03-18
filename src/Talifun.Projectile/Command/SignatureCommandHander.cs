using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using Talifun.Projectile.Core;

namespace Talifun.Projectile.Command
{
    public class SignatureCommandHander
    {
        private readonly List<Action<SignatureBuilder>> _configuration = new List<Action<SignatureBuilder>>();

        public int Execute(Stream stream, long metaDataLength)
        {
            if (metaDataLength != stream.Length)
            {
                throw new Exception("Unexpected stream attached");
            }
            var command = Serializer.Deserialize<SignatureCommand>(stream);
            return Execute(command);
        }

        public int Execute(SignatureCommand command)
        {
            return Execute(command.BasisFilePath, command.SignatureFilePath);
        }

        public int Execute(string basisFilePath, string signatureFilePath)
        {
            if (string.IsNullOrWhiteSpace(basisFilePath))
                throw new ArgumentNullException("No basis file was specified", "basis-file");

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

            return 0;
        }
    }
}
