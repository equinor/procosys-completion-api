using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.Tags;

public interface ITagService
{
    Task<ProCoSys4Tag[]> GetTagsByTagNosAsync(string plant, string projectName, string[] tagNos, CancellationToken cancellationToken);
}

public sealed class TagService(
    IMainApiClientForApplication mainApiClient,
    IOptionsMonitor<MainApiOptions> mainApiOptions) : ITagService
{
    private readonly string _apiVersion = mainApiOptions.CurrentValue.ApiVersion;
    private readonly Uri _baseAddress = new(mainApiOptions.CurrentValue.BaseAddress);
    
    public async Task<ProCoSys4Tag[]> GetTagsByTagNosAsync(string plant, string projectName, string[] tagNos,
        CancellationToken cancellationToken)
    {
        var encodedTagNos = tagNos.Select(HttpUtility.UrlEncode);
      
        var url = $"{_baseAddress}Tag/ByTagNos" 
                  + $"?plantId={plant}" 
                  + string.Join(string.Empty, encodedTagNos.Select(x => $"&tagNos={x}"))
                  + $"&projectName={projectName}" + $"&api-version={_apiVersion}";

            return await mainApiClient.TryQueryAndDeserializeAsync<ProCoSys4Tag[]>(url, cancellationToken)
                ?? [];
    }
}
