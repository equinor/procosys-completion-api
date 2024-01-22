using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Command.Email;

public interface ICompletionMailService
{
    Task SendEMailAsync(string eMailCode, List<string> eMailAddresses, dynamic context, CancellationToken cancellationToken);
}
