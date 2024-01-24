using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Command.Email;

public interface ICompletionMailService
{
    Task SendEmailAsync(dynamic context, string eMailCode, List<string> eMailAddresses, CancellationToken cancellationToken);
}
