using ProtoBuf;

namespace Talifun.Projectile.Command
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class ErrorCommand
    {
        public string Exception { get; set; }
    }
}
