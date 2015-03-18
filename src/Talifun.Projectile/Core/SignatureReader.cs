using System.Collections;
using System.IO;
using Talifun.Projectile.Diagnostics;

namespace Talifun.Projectile.Core
{
    public class SignatureReader : ISignatureReader
    {
        private readonly IProgressReporter _reporter;
        private readonly BinaryReader _reader;

        public SignatureReader(Stream stream, IProgressReporter reporter)
        {
            this._reporter = reporter;
            this._reader = new BinaryReader(stream);
        }

        public Signature ReadSignature()
        {
            Progress();
            var header = _reader.ReadBytes(BinaryFormat.SignatureHeader.Length);
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(BinaryFormat.SignatureHeader, header)) 
                throw new CorruptFileFormatException("The signature file appears to be corrupt.");

            var version = _reader.ReadByte();
            if (version != BinaryFormat.Version)
                throw new CorruptFileFormatException("The signature file uses a newer file format than this program can handle.");

            var hashAlgorithm = _reader.ReadString();
            var rollingChecksumAlgorithm = _reader.ReadString();

            var endOfMeta = _reader.ReadBytes(BinaryFormat.EndOfMetadata.Length);
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(BinaryFormat.EndOfMetadata, endOfMeta)) 
                throw new CorruptFileFormatException("The signature file appears to be corrupt.");

            Progress();

            var hashAlgo = SupportedAlgorithms.Hashing.Create(hashAlgorithm);
            var signature = new Signature(
                hashAlgo,
                SupportedAlgorithms.Checksum.Create(rollingChecksumAlgorithm));

            var expectedHashLength = hashAlgo.HashLength;
            long start = 0;

            var fileLength = _reader.BaseStream.Length;
            var remainingBytes = fileLength - _reader.BaseStream.Position;
            var signatureSize = sizeof (ushort) + sizeof (uint) + expectedHashLength;
            if (remainingBytes % signatureSize != 0)
                throw new CorruptFileFormatException("The signature file appears to be corrupt; at least one chunk has data missing.");

            while (_reader.BaseStream.Position < fileLength - 1)
            {
                var length = _reader.ReadInt16();
                var checksum = _reader.ReadUInt32();
                var chunkHash = _reader.ReadBytes(expectedHashLength);

                signature.Chunks.Add(new ChunkSignature
                {
                    StartOffset = start,
                    Length = length,
                    RollingChecksum = checksum,
                    Hash = chunkHash
                });

                start += length;

                Progress();
            }

            return signature;
        }

        void Progress()
        {
            _reporter.ReportProgress("Reading signature", _reader.BaseStream.Position, _reader.BaseStream.Length);
        }
    }
}