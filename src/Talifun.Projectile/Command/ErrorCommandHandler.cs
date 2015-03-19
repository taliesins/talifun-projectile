using System.IO;
using Talifun.Projectile.Protocol;

namespace Talifun.Projectile.Command
{
    public class ErrorCommandHandler
    {
        public Reply Execute(ErrorCommand command, Stream stream = null)
        {
            return Execute(command.Exception, stream);
        }

        public Reply Execute(string exception, Stream stream = null)
        {
            return null;
        }
    }
}
