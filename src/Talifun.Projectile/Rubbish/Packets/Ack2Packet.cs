using System;

namespace Talifun.Projectile.Rubbish.Packets
{
    public struct Ack2Packet
    {
        public Header h;

        /// <summary>
        /// ACK sequence number
        /// </summary>
        public UInt32 ackSeqNo;
    }
}
