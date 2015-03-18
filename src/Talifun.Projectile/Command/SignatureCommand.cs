using ProtoBuf;

namespace Talifun.Projectile.Command
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class SignatureCommand
    {
        public string BasisFilePath { get; set; }
        public string SignatureFilePath { get; set; }
    }
}
