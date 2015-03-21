using System.IO;
using Udt;

namespace Talifun.Projectile.Protocol
{
    public class WriteOnlyUdtSocketStream : Stream
    {
        private readonly Socket _socket;

        public WriteOnlyUdtSocketStream(Udt.Socket socket)
        {
            _socket = socket;
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
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _socket.Send(buffer, offset, count);
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw new System.NotImplementedException(); }
        }

        public override long Position { get; set; }
    }
}
