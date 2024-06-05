using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public class MainApiCheckListService : ICheckListApiService
{
    private readonly string _apiVersion;
    private readonly Uri _baseAddress;
    private readonly IMainApiClient _mainApiClient;

    public MainApiCheckListService(
        IMainApiClient mainApiClient,
        IOptionsSnapshot<MainApiOptions> options)
    {
        _mainApiClient = mainApiClient;
        _apiVersion = options.Value.ApiVersion;
        _baseAddress = new Uri(options.Value.BaseAddress);
    }

    public async Task<ProCoSys4CheckList?> GetCheckListAsync(string plant, Guid checkListGuid)
    {
        var url = $"{_baseAddress}CheckList/ForProCoSys5" +
                  $"?plantId={plant}" +
                  $"&proCoSysGuid={checkListGuid:N}" +
                  $"&api-version={_apiVersion}";

        return await _mainApiClient.TryQueryAndDeserializeAsync<ProCoSys4CheckList?>(url);
    }

    public async Task RecalculateCheckListStatus(string plant, Guid checkListGuid, CancellationToken cancellationToken)
    {
        var url = $"{_baseAddress}CheckList/ForProCoSys5" +
                  $"?plantId={plant}" +
                  $"&api-version={_apiVersion}";

        var requestBody = JsonConvert.SerializeObject(new CheckListGuidDto { ProCoSysGuid = checkListGuid });
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        await _mainApiClient.PostAsync(url, content, cancellationToken);
    }
}
