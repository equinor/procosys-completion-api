using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Completion.Domain.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi;

public sealed class DistributedCacheMainApiAuthenticator(
    ITokenAcquisition tokenAcquisition,
    IOptionsMonitor<AzureAdOptions> options) : IMainApiAuthenticator
{
    public ValueTask<string> GetBearerTokenAsync(CancellationToken cancellationToken = new())
        => AuthenticationType == AuthenticationType.AsApplication
            ? GetAppTokenAsync()
            : GetOnBehalfOfTokenAsync();

    private async ValueTask<string> GetOnBehalfOfTokenAsync() =>
        await tokenAcquisition.GetAccessTokenForUserAsync(new[] { options.CurrentValue.MainApiScope });

    private async ValueTask<string> GetAppTokenAsync() =>
        await tokenAcquisition.GetAccessTokenForAppAsync(options.CurrentValue.MainApiScope);

    public AuthenticationType AuthenticationType { get; set; }
}
