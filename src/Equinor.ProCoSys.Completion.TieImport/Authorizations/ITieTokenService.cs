using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.TieImport.Authorizations;

public interface ITieTokenService
{
    Task<string> AcquireTokenAsync(CancellationToken cancellationToken);
}
