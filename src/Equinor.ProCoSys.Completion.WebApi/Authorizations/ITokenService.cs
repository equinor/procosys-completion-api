using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public interface ITokenService
{
    Task<string> AcquireTokenAsync(CancellationToken cancellationToken);
}
