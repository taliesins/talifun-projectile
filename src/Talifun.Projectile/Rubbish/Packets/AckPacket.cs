using System;

namespace Talifun.Projectile.Rubbish.Packets
{
    public struct AckPacket
    {
        public Header h;

        /// <summary>
        /// ACK sequence number
        /// </summary>
        public UInt32 ackSeqNo;

        /// <summary>
        /// The packet sequence number to which all the previous packets have been received (excluding)
        /// </summary>
        public UInt32 pktSeqHi;

        // The below are optional
        /// <summary>
        /// RTT (in microseconds)
        /// </summary>
        public UInt32 rtt;
 
        /// <summary>
        /// RTT variance
        /// </summary>
        public UInt32 rttVar; 

        /// <summary>
        /// Available buffer size (in bytes)
        /// </summary>
        public UInt32 buffAvail;

        /// <summary>
        /// Packets receiving rate (in number of packets per second)
        /// </summary>
        public UInt32 pktRecvRate;

        /// <summary>
        /// Estimated link capacity (in number of packets per second)
        /// </summary>
        public UInt32 estLinkCap;
    }
}
