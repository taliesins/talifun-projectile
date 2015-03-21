using ProtoBuf;

namespace Talifun.Projectile.Command
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class SendFileRequest
    {
        public string RemoteFilePath { get; set; }
        public string LocalFilePath { get; set; }
    }
}
