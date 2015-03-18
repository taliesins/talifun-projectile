using ProtoBuf;

namespace Talifun.Projectile.Command
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class PatchCommand
    {
        public string BasisFilePath { get; set; }
        public string DeltaFilePath { get; set; }
        public string NewFilePath { get; set; }
        public bool SkipHashCheck { get; set; }
    }
}
