using System;
using System.Net;

namespace Talifun.Projectile.Rubbish.Packets
{
    public struct HandshakePacket
    {
        public Header h;

        /// <summary>
        /// UDT version
        /// </summary>
        public UInt32 udtVer;

        /// <summary>
        /// Socket Type (0 = STREAM or 1 = DGRAM)
        /// </summary>
        public UInt32 sockType;

        /// <summary>
        /// initial packet sequence number
        /// </summary>
        public UInt32 initPktSeq;

        /// <summary>
        /// maximum packet size (including UDP/IP headers)
        /// </summary>
        public UInt32 maxPktSize;

        /// <summary>
        /// maximum flow window size
        /// </summary>
        public UInt32 maxFlowWinSize;
 
        /// <summary>
        /// connection type (regular or rendezvous)
        /// </summary>
        public UInt32 connType;

        /// <summary>
        /// socket ID
        /// </summary>
        public UInt32 sockId;

        /// <summary>
        /// SYN cookie
        /// </summary>
        public UInt32 synCookie;

        /// <summary>
        /// the IP address of the UDP socket to which this packet is being sent
        /// </summary>
        public IPAddress sockAddr;
    }
}
