using ProtoBuf;

namespace Talifun.Projectile.Command
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class SendFileRequest
    {
        public string FilePath { get; set; }
    }
}
