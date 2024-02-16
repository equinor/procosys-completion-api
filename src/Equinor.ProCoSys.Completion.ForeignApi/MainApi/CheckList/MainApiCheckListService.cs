using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

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
}
