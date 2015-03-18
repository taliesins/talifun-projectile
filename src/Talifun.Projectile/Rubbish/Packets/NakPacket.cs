using System;

namespace Talifun.Projectile.Rubbish.Packets
{
    public struct NakPacket
    {
        public Header h;

        /// <summary>
        /// integer array of compressed loss information
        /// </summary>
        public UInt32 cmpLossInfo;
    }
}
