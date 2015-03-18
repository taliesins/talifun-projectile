using System;
using System.IO;

namespace Talifun.Projectile.Core
{
    public class BinaryDeltaWriter : IDeltaWriter
    {
        private readonly BinaryWriter _writer;

        public BinaryDeltaWriter(Stream stream)
        {
            _writer = new BinaryWriter(stream);
        }

        public void WriteMetadata(IHashAlgorithm hashAlgorithm, byte[] expectedNewFileHash)
        {
            _writer.Write(BinaryFormat.DeltaHeader);
            _writer.Write(BinaryFormat.Version);
            _writer.Write(hashAlgorithm.Name);
            _writer.Write(expectedNewFileHash.Length);
            _writer.Write(expectedNewFileHash);
            _writer.Write(BinaryFormat.EndOfMetadata);
        }

        public void WriteCopyCommand(DataRange segment)
        {
            _writer.Write(BinaryFormat.CopyCommand);
            _writer.Write(segment.StartOffset);
            _writer.Write(segment.Length);
        }

        public void WriteDataCommand(Stream source, long offset, long length)
        {
            _writer.Write(BinaryFormat.DataCommand);
            _writer.Write(length);

            var originalPosition = source.Position;
            try
            {
                source.Seek(offset, SeekOrigin.Begin);

                var buffer = new byte[Math.Min((int)length, 1024 * 1024)];

                int read;
                long soFar = 0;
                while ((read = source.Read(buffer, 0, (int)Math.Min(length - soFar, buffer.Length))) > 0)
                {
                    soFar += read;

                    _writer.Write(buffer, 0, read);
                }
            }
            finally
            {
                source.Seek(originalPosition, SeekOrigin.Begin);
            }
        }

        public void Finish()
        {
        }
    }
}