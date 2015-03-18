using System;

namespace Talifun.Projectile.Rubbish.Packets
{
    public struct DataPacket
    {
        public Header h;

        public UInt32 seq;
        public byte[] data;
    }
}
