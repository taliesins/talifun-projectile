using System.IO;
using Udt;

namespace Talifun.Projectile
{
    public class ReadOnlySocketStream : Stream
    {
        private readonly Socket _socket;
        private readonly long _length;

        public ReadOnlySocketStream(Udt.Socket socket, long length)
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
            var maximumLength = count;

            if (Position + maximumLength > Length)
            {
                maximumLength = (int)(Length - Position);
            }

            if (maximumLength == 0)
            {
                return 0;
            }
            
            var readLength = _socket.Receive(buffer, offset, maximumLength);

            Position = Position + maximumLength;

            return readLength;
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
