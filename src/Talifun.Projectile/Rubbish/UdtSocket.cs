using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Talifun.Projectile.Rubbish.Structures;

namespace Talifun.Projectile.Rubbish
{
    //////////////////////////////////////////////////////////////////////////////
    //    0                   1                   2                   3
    //    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //   |                        Packet Header                          |
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //   |                                                               |
    //   ~              Data / Control Information Field                 ~
    //   |                                                               |
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //
    //    0                   1                   2                   3
    //    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //   |0|                        Sequence Number                      |
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //   |ff |o|                     Message Number                      |
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //   |                          Time Stamp                           |
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //   |                     Destination Socket ID                     |
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //
    //   bit 0:
    //      0: Data Packet
    //      1: Control Packet
    //   bit ff:
    //      11: solo message packet
    //      10: first packet of a message
    //      01: last packet of a message
    //   bit o:
    //      0: in order delivery not required
    //      1: in order delivery required
    //
    //    0                   1                   2                   3
    //    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //   |1|            Type             |             Reserved          |
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //   |                       Additional Info                         |
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //   |                          Time Stamp                           |
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //   |                     Destination Socket ID                     |
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //
    //   bit 1-15:
    //      0: Protocol Connection Handshake
    //              Add. Info:    Undefined
    //              Control Info: Handshake information (see CHandShake)
    //      1: Keep-alive
    //              Add. Info:    Undefined
    //              Control Info: None
    //      2: Acknowledgement (ACK)
    //              Add. Info:    The ACK sequence number
    //              Control Info: The sequence number to which (but not include) all the previous packets have beed received
    //              Optional:     RTT
    //                            RTT Variance
    //                            available receiver buffer size (in bytes)
    //                            advertised flow window size (number of packets)
    //                            estimated bandwidth (number of packets per second)
    //      3: Negative Acknowledgement (NAK)
    //              Add. Info:    Undefined
    //              Control Info: Loss list (see loss list coding below)
    //      4: Congestion/Delay Warning
    //              Add. Info:    Undefined
    //              Control Info: None
    //      5: Shutdown
    //              Add. Info:    Undefined
    //              Control Info: None
    //      6: Acknowledgement of Acknowledement (ACK-square)
    //              Add. Info:    The ACK sequence number
    //              Control Info: None
    //      7: Message Drop Request
    //              Add. Info:    Message ID
    //              Control Info: first sequence number of the message
    //                            last seqeunce number of the message
    //      8: Error Signal from the Peer Side
    //              Add. Info:    Error code
    //              Control Info: None
    //      0x7FFF: Explained by bits 16 - 31
    //              
    //   bit 16 - 31:
    //      This space is used for future expansion or user defined control packets. 
    //
    //    0                   1                   2                   3
    //    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //   |1|                 Sequence Number a (first)                   |
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //   |0|                 Sequence Number b (last)                    |
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //   |0|                 Sequence Number (single)                    |
    //   +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //
    //   Loss List Field Coding:
    //      For any consectutive lost seqeunce numbers that the differnece between
    //      the last and first is more than 1, only record the first (a) and the
    //      the last (b) sequence numbers in the loss list field, and modify the
    //      the first bit of a to 1.
    //      For any single loss or consectutive loss less than 2 packets, use
    //      the original sequence numbers in the field.

    public class UdtSocket
    {
        private readonly SocketAwaitablePool _socketAwaitablePool;
        private readonly BlockingBufferManager _blockingBufferManager;
        private readonly int _receiveTimeout;
        private readonly int _sendTimeout;
        private readonly int _receiveBufferSize;
        private readonly int _sendBufferSize;
        private readonly int _socketConnectAttempts;
        private readonly DnsEndPoint _endPoint;

        private Socket _socket;
        private DateTime _lastActivity;

        public UdtSocket(string server, int port, SocketAwaitablePool socketAwaitablePool, BlockingBufferManager blockingBufferManager,
            int receiveTimeout, int sendTimeout,
            int receiveBufferSize = 65536, int sendBufferSize = 65536, int socketConnectAttempts = 3)
        {
            _socketAwaitablePool = socketAwaitablePool;
            _blockingBufferManager = blockingBufferManager;
            _receiveTimeout = receiveTimeout;
            _sendTimeout = sendTimeout;
            _receiveBufferSize = receiveBufferSize;
            _sendBufferSize = sendBufferSize;
            _socketConnectAttempts = socketConnectAttempts;
            _endPoint = new DnsEndPoint(server, port);
        }

        private async Task ConnectAsync(EndPoint endPoint)
        {
            var result = SocketError.Fault;
            for (var i = 0; i < _socketConnectAttempts; i++)
            {
                if (_socket != null)
                {
                    _socket.Close();
                    _socket = null;
                }
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                {
                    UseOnlyOverlappedIO = true,
                    NoDelay = true,
                    ReceiveTimeout = _receiveTimeout,
                    SendTimeout = _sendTimeout,
                    ReceiveBufferSize = _receiveBufferSize,
                    SendBufferSize = _sendBufferSize
                };

                var awaitable = _socketAwaitablePool.Take();
                try
                {
                    awaitable.RemoteEndPoint = endPoint;

                    var socketAwaitable = _socket.ConnectAsync(awaitable);
                    _lastActivity = DateTime.UtcNow;

                    result = await socketAwaitable;

                    if (result == SocketError.Success)
                    {
                        break;
                    }
                }
                finally
                {
                    awaitable.Clear();
                    _socketAwaitablePool.Add(awaitable);
                }
            }

            if (result != SocketError.Success)
            {
                if (_socket != null)
                {
                    _socket.Close();
                    _socket = null;
                }
                throw new Exception(string.Format("Unable to connect to remote server: {0}:{1} error code {2}", _endPoint.Host, _endPoint.Port, result));
            }
        }

        private async Task ReceiveAsync(ArraySegment<byte> buffer)
        {
            var awaitable = _socketAwaitablePool.Take();
            awaitable.Buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset, buffer.Count);

            try
            {
                while (true)
                {
                    var result = await _socket.ReceiveAsync(awaitable);
                    _lastActivity = DateTime.UtcNow;

                    if (result != SocketError.Success)
                    {
                        throw new Exception(string.Format("Unable to read data from the source stream: {0}:{1} error code {2}",
                            _endPoint.Host, _endPoint.Port, result));
                    }

                    if (awaitable.Arguments.BytesTransferred == 0)
                    {
                        throw new Exception(string.Format("Unable to read data from the source stream: {0}:{1} remote server closed connection",
                            _endPoint.Host, _endPoint.Port));
                    }

                    if (awaitable.Arguments.Offset + awaitable.Arguments.BytesTransferred >= buffer.Offset + buffer.Count)
                    {
                        break;
                    }

                    awaitable.Buffer = new ArraySegment<byte>(
                        awaitable.Arguments.Buffer,
                        awaitable.Arguments.Offset + awaitable.Arguments.BytesTransferred,
                        awaitable.Arguments.Count - awaitable.Arguments.BytesTransferred);
                }
            }
            finally
            {
                awaitable.Clear();
                _socketAwaitablePool.Add(awaitable);
            }
        }

        private async Task SendAsync(ArraySegment<byte> buffer)
        {
            var awaitable = _socketAwaitablePool.Take();
            try
            {
                awaitable.Buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset, buffer.Count);

                while (true)
                {
                    var result = await _socket.SendAsync(awaitable);
                    _lastActivity = DateTime.UtcNow;

                    if (result != SocketError.Success)
                    {
                        throw new Exception(string.Format("Failed to send data to server - Timed Out: {0}:{1} error code {2}", _endPoint.Host, _endPoint.Port, result));
                    }

                    if (awaitable.Arguments.BytesTransferred == 0)
                    {
                        throw new Exception(string.Format("Failed to send data to server - Timed Out: {0}:{1}",_endPoint.Host, _endPoint.Port));
                    }

                    if (awaitable.Arguments.Offset + awaitable.Arguments.BytesTransferred >= buffer.Offset + buffer.Count)
                    {
                        break;
                    }
                    
                    // Set the buffer to send the remaining data.
                    awaitable.Buffer = new ArraySegment<byte>(
                        awaitable.Arguments.Buffer,
                        awaitable.Arguments.Offset + awaitable.Arguments.BytesTransferred,
                        awaitable.Arguments.Count - awaitable.Arguments.BytesTransferred);
                }
            }
            finally
            {
                awaitable.Clear();
                _socketAwaitablePool.Add(awaitable);
            }
        }
    }
}
