namespace Talifun.Projectile.Rubbish.Packets
{
    public enum PacketTypeEnum
    {
        Handshake = 0,
        KeepAlive = 1,
        Ack = 2,
        Nak = 3,
        Unused = 4,
        Shutdown = 5,
        Ack2 = 6,
        MsgDropReq = 7
    }
}
