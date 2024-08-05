using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Microsoft.Extensions.Options;

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

    public async Task<ProCoSys4CheckList?> GetCheckListAsync(string plant, Guid checkListGuid,
        CancellationToken cancellationToken)
    {
        var oldAuthenticationType = mainApiAuthenticator.AuthenticationType;
        try
        {
            mainApiAuthenticator.AuthenticationType = AuthenticationType.AsApplication;
            var url = $"{_baseAddress}CheckList/ForProCoSys5" +
                      $"?plantId={plant}" +
                      $"&proCoSysGuid={checkListGuid:N}" +
                      $"&api-version={_apiVersion}";

            return await mainApiClient.TryQueryAndDeserializeAsync<ProCoSys4CheckList?>(url, default, cancellationToken);
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

            var requestBody = JsonSerializer.Serialize(new CheckListGuidDto { ProCoSysGuid = checkListGuid });
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            await mainApiClient.PostAsync(url, content, cancellationToken);
        }
        finally
        {
            mainApiAuthenticator.AuthenticationType = oldAuthenticationType;
        }
    }

    public async Task<ChecklistsByPunchGuidInstance> GetByPunchItemGuidAsync(string plant, Guid punchItemGuid, CancellationToken cancellationToken)
    {
        var url = $"{_baseAddress}CheckList/ByPunchItemGuid" +
                  $"?plantId={plant}" +
                  $"&proCoSysGuid={punchItemGuid:N}" +
                  $"&api-version={_apiVersion}";

        return await mainApiClient.TryQueryAndDeserializeAsync<ChecklistsByPunchGuidInstance>(url, null, cancellationToken);
    }

    public async Task<TagCheckList[]> GetCheckListsByTagIdAndPlantAsync(int tagId, string plant, CancellationToken cancellationToken)
    {
        var oldAuthenticationType = mainApiAuthenticator.AuthenticationType;
        try
        {
            mainApiAuthenticator.AuthenticationType = AuthenticationType.AsApplication;
            var url = $"{_baseAddress}Tag/CheckLists" +
                      $"?plantId={plant}" +
                      $"&tagId={tagId}" +
                      $"&api-version={_apiVersion}";

            var checkLists = await mainApiClient.TryQueryAndDeserializeAsync<TagCheckList[]>(url, null, cancellationToken);
            return checkLists
                .Select(x => x with {Plant = plant})
                .ToArray();
        }
        finally
        {
            mainApiAuthenticator.AuthenticationType = oldAuthenticationType;
        }
    }
}
