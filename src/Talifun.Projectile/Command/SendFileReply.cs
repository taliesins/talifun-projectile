using ProtoBuf;

namespace Talifun.Projectile.Command
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class SendFileReply
    {
        public string FileName { get; set; }
    }
}
