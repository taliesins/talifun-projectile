namespace Talifun.Projectile.Protocol
{
    public enum MessageType
    {
        Error,
        SendFileRequest,
        SendFileResponse,
        DeltaCommand,
        PatchCommand,
        SignatureCommand
    }
}