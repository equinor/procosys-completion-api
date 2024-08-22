using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.Completion.Domain;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public class MainApiCheckListService(
    IMainApiClientForApplication mainApiClientForApplication,
    IMainApiClientForUser mainApiClientForUser,
    IOptionsMonitor<MainApiOptions> mainApiOptions,
    IOptionsMonitor<ApplicationOptions> applicationOptions)
    : ICheckListApiService
{
    private readonly string _apiVersion = mainApiOptions.CurrentValue.ApiVersion;
    private readonly Uri _baseAddress = new(mainApiOptions.CurrentValue.BaseAddress);
    private readonly bool _recalculateStatusInPcs4 = applicationOptions.CurrentValue.RecalculateStatusInPcs4;

    // Do not pass plant to the GET endpoint for checklist in Main API due to performance. The endpoint has m2m auth, hence it doesn't require plant specific permissions
    public async Task<ProCoSys4CheckList?> GetCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken)
    {
        var url = $"{_baseAddress}CheckList/ForProCoSys5" +
                  $"?proCoSysGuid={checkListGuid:N}" +
                  $"&api-version={_apiVersion}";

        // Execute as application. The get checklist endpoint in Main Api requires
        // a special role "Checklist.RecalcStatus", which the Azure application registration has
        return await mainApiClientForApplication.TryQueryAndDeserializeAsync<ProCoSys4CheckList?>(url, cancellationToken);
    }

    public async Task RecalculateCheckListStatus(string plant, Guid checkListGuid, CancellationToken cancellationToken)
    {
        if (!_recalculateStatusInPcs4)
        {
            return;
        }

        var url = $"{_baseAddress}CheckList/ForProCoSys5" +
                  $"?plantId={plant}" +
                  $"&api-version={_apiVersion}";

        var requestBody = JsonSerializer.Serialize(new CheckListGuidDto { ProCoSysGuid = checkListGuid });
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        // Execute as application. The recalc endpoint in Main Api requires
        // a special role "Checklist.RecalcStatus", which the Azure application registration has
        await mainApiClientForApplication.PostAsync(url, content, cancellationToken);
    }

    public async Task<ChecklistsByPunchGuidInstance> GetByPunchItemGuidAsync(string plant, Guid punchItemGuid, CancellationToken cancellationToken)
    {
        var url = $"{_baseAddress}CheckList/ByPunchItemGuid" +
                  $"?plantId={plant}" +
                  $"&proCoSysGuid={punchItemGuid:N}" +
                  $"&api-version={_apiVersion}";

        return await mainApiClientForUser.TryQueryAndDeserializeAsync<ChecklistsByPunchGuidInstance>(url, cancellationToken);
    }

    // Do not pass plant to the GET endpoint for many checklists in Main API due to performance. The endpoint has m2m auth, hence it doesn't require plant specific permissions
    public async Task<List<ProCoSys4CheckList>> GetManyCheckListsAsync(List<Guid> checkListGuids, CancellationToken cancellationToken)
    {
        var url = $"{_baseAddress}CheckLists/ForProCoSys5" +
                  $"?api-version={_apiVersion}";

        foreach (var checkListGuid in checkListGuids)
        {
            url += $"&proCoSysGuids={checkListGuid:N}";
        }

        // Execute as application. The get checklists endpoint in Main Api requires
        // a special role "Checklist.RecalcStatus", which the Azure application registration has
        return await mainApiClientForApplication.TryQueryAndDeserializeAsync<List<ProCoSys4CheckList>>(url, cancellationToken);
    }
}
