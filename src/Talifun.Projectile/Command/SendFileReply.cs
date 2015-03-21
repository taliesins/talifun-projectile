using ProtoBuf;

namespace Talifun.Projectile.Command
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class SendFileReply
    {
        public string RemoteFilePath { get; set; }
        public string LocalFilePath { get; set; }
    }
}
