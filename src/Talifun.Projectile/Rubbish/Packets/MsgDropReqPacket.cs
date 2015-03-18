using System;

namespace Talifun.Projectile.Rubbish.Packets
{
    public struct MsgDropReqPacket
    {
        public Header h;

        /// <summary>
        /// Message ID
        /// </summary>
        public UInt32 msgId;

        /// <summary>
        /// First sequence number in the message
        /// </summary>
        public UInt32 firstSeq;

        /// <summary>
        /// Last sequence number in the message
        /// </summary>
        public UInt32 lastSeq;
    }
}
