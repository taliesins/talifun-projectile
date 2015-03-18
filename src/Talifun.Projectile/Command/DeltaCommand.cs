using ProtoBuf;

namespace Talifun.Projectile.Command
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DeltaCommand
    {
        public string NewFilePath { get; set; }
        public string SignatureFilePath { get; set; }
        public string DeltaFilePath { get; set; }
    }
}
