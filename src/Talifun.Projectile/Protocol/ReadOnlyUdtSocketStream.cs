using System;
using System.IO;
using System.Threading;
using Udt;

namespace Talifun.Projectile.Protocol
{
    public class ReadOnlyUdtSocketStream : Stream
    {
        private readonly Socket _socket;
        private readonly long _length;
        private readonly TimeSpan _timeout = new TimeSpan(0,0,30);

        public ReadOnlyUdtSocketStream(Udt.Socket socket, long length)
        {
            _socket = socket;
            _length = length;
        }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Position + count > Length)
            {
                count = (int)(Length - Position);
            }

            if (count > 0)
            {
                var readLength = 0;
                while ((readLength = _socket.Receive(buffer, offset, count)) < 1)
                {
                    Thread.Yield();
                }

                Position += readLength;

                return readLength;
            }
            else
            {
                return 0;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position { get; set; }
    }
}
