using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.Tags;

public interface ITagService
{
    Task<ProCoSys4Tag[]> GetTagsByTagNosAsync(string plant, string projectName, string[] tagNos, CancellationToken cancellationToken);
}

public sealed class TagService(
    IMainApiClient mainApiClient,
    IMainApiAuthenticator mainApiAuthenticator,
    IOptionsMonitor<MainApiOptions> mainApiOptions) : ITagService
{
    private readonly string _apiVersion = mainApiOptions.CurrentValue.ApiVersion;
    private readonly Uri _baseAddress = new(mainApiOptions.CurrentValue.BaseAddress);
    
    public async Task<ProCoSys4Tag[]> GetTagsByTagNosAsync(string plant, string projectName, string[] tagNos,
        CancellationToken cancellationToken)
    {
        var oldAuthenticationType = mainApiAuthenticator.AuthenticationType;
        mainApiAuthenticator.AuthenticationType = AuthenticationType.AsApplication;
        try
        {
            var url = $"{_baseAddress}Tag/ByTagNos" +
                         $"?plantId={plant}" +
                         string.Join(string.Empty, tagNos.Select(x => $"&tagNos={x}")) +
                         $"&projectName={projectName}" +
                         $"&api-version={_apiVersion}";

            return await mainApiClient.TryQueryAndDeserializeAsync<ProCoSys4Tag[]>(url, null, cancellationToken);
        }
        finally
        {
            mainApiAuthenticator.AuthenticationType = oldAuthenticationType;
        }
    }
}