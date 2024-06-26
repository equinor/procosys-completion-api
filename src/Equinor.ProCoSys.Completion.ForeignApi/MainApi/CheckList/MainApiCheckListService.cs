using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.Completion.Domain;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public class MainApiCheckListService(
    IMainApiClient mainApiClient,
    IMainApiAuthenticator mainApiAuthenticator,
    IOptionsMonitor<MainApiOptions> mainApiOptions,
    IOptionsMonitor<ApplicationOptions> applicationOptions)
    : ICheckListApiService
{
    private readonly string _apiVersion = mainApiOptions.CurrentValue.ApiVersion;
    private readonly Uri _baseAddress = new(mainApiOptions.CurrentValue.BaseAddress);
    private readonly bool _recalculateStatusInPcs4 = applicationOptions.CurrentValue.RecalculateStatusInPcs4;

    public async Task<ProCoSys4CheckList?> GetCheckListAsync(Guid checkListGuid)
    {
        var oldAuthenticationType = mainApiAuthenticator.AuthenticationType;
        try
        {
            mainApiAuthenticator.AuthenticationType = AuthenticationType.AsApplication;
            var url = $"{_baseAddress}CheckList/ForProCoSys5" +
                      $"?proCoSysGuid={checkListGuid:N}" +
                      $"&api-version={_apiVersion}";

            return await mainApiClient.TryQueryAndDeserializeAsync<ProCoSys4CheckList?>(url);
        }
        finally
        {
            mainApiAuthenticator.AuthenticationType = oldAuthenticationType;
        }
    }

    public async Task RecalculateCheckListStatus(string plant, Guid checkListGuid, CancellationToken cancellationToken)
    {
        if (!_recalculateStatusInPcs4)
        {
            return;
        }

        var oldAuthenticationType = mainApiAuthenticator.AuthenticationType;
        try
        {
            mainApiAuthenticator.AuthenticationType = AuthenticationType.AsApplication;
            var url = $"{_baseAddress}CheckList/ForProCoSys5" +
                      $"?plantId={plant}" +
                      $"&api-version={_apiVersion}";

            var requestBody = JsonConvert.SerializeObject(new CheckListGuidDto { ProCoSysGuid = checkListGuid });
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            await mainApiClient.PostAsync(url, content, cancellationToken);
        }
        finally
        {
            mainApiAuthenticator.AuthenticationType = oldAuthenticationType;
        }
    }

    public async Task<ChecklistsByPunchGuidInstance> GetByPunchItemGuidAsync(string plant, Guid punchItemGuid)
    {
        var url = $"{_baseAddress}CheckList/ByPunchItemGuid" +
                  $"?plantId={plant}" +
                  $"&proCoSysGuid={punchItemGuid:N}" +
                  $"&api-version={_apiVersion}";

        return await mainApiClient.TryQueryAndDeserializeAsync<ChecklistsByPunchGuidInstance>(url);
    }
}
