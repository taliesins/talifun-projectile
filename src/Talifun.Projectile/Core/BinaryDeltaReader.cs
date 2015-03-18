using System;
using System.Collections;
using System.IO;
using Talifun.Projectile.Diagnostics;

namespace Talifun.Projectile.Core
{
    public class BinaryDeltaReader : IDeltaReader
    {
        private readonly BinaryReader _reader;
        private readonly IProgressReporter _progressReporter;
        private byte[] _expectedHash;
        private IHashAlgorithm _hashAlgorithm;
        private bool _hasReadMetadata;

        public BinaryDeltaReader(Stream stream, IProgressReporter progressReporter)
        {
            this._reader = new BinaryReader(stream);
            this._progressReporter = progressReporter ?? new NullProgressReporter();
        }

        public byte[] ExpectedHash
        {
            get
            {
                EnsureMetadata();
                return _expectedHash;
            }
        }

        public IHashAlgorithm HashAlgorithm
        {
            get
            {
                EnsureMetadata();
                return _hashAlgorithm;
            }
        }

        void EnsureMetadata()
        {
            if (_hasReadMetadata)
                return;

            _reader.BaseStream.Seek(0, SeekOrigin.Begin);

            var first = _reader.ReadBytes(BinaryFormat.DeltaHeader.Length);
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(first, BinaryFormat.DeltaHeader))
                throw new CorruptFileFormatException("The delta file appears to be corrupt.");

            var version = _reader.ReadByte();
            if (version != BinaryFormat.Version)
                throw new CorruptFileFormatException("The delta file uses a newer file format than this program can handle.");

            var hashAlgorithmName = _reader.ReadString();
            _hashAlgorithm = SupportedAlgorithms.Hashing.Create(hashAlgorithmName);

            var hashLength = _reader.ReadInt32();
            _expectedHash = _reader.ReadBytes(hashLength);
            var endOfMeta = _reader.ReadBytes(BinaryFormat.EndOfMetadata.Length);
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(BinaryFormat.EndOfMetadata, endOfMeta))
                throw new CorruptFileFormatException("The signature file appears to be corrupt.");

            _hasReadMetadata = true;
        }

        public void Apply(
            Action<byte[]> writeData, 
            Action<long, long> copy)
        {
            var fileLength = _reader.BaseStream.Length;

            EnsureMetadata();

            while (_reader.BaseStream.Position != fileLength)
            {
                var b = _reader.ReadByte();

                _progressReporter.ReportProgress("Applying delta", _reader.BaseStream.Position, fileLength);
                
                if (b == BinaryFormat.CopyCommand)
                {
                    var start = _reader.ReadInt64();
                    var length = _reader.ReadInt64();
                    copy(start, length);
                }
                else if (b == BinaryFormat.DataCommand)
                {
                    var length = _reader.ReadInt64();
                    long soFar = 0;
                    while (soFar < length)
                    {
                        var bytes = _reader.ReadBytes((int) Math.Min(length - soFar, 1024*1024*4));
                        soFar += bytes.Length;
                        writeData(bytes);
                    }
                }
            }
        }
    }
}