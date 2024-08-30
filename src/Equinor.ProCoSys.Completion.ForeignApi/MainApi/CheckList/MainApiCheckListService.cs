using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Imports;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;

public class MainApiCheckListService(
    IMainApiClientForApplication mainApiClientForApplication,
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

        // Execute as application. The recalc endpoint in Main Api requires
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

    // public async Task<ChecklistsByPunchGuidInstance> GetByPunchItemGuidAsync(string plant, Guid punchItemGuid, CancellationToken cancellationToken)
    // {
    //     var url = $"{_baseAddress}CheckList/ByPunchItemGuid" +
    //               $"?plantId={plant}" +
    //               $"&proCoSysGuid={punchItemGuid:N}" +
    //               $"&api-version={_apiVersion}";
    //
    //     return await mainApiClientForUser.TryQueryAndDeserializeAsync<ChecklistsByPunchGuidInstance>(url, cancellationToken);
    // }

    public async Task<TagCheckList[]> GetCheckListsByTagIdAndPlantAsync(int tagId, string plant, CancellationToken cancellationToken)
    {
        var url = $"{_baseAddress}Tag/CheckLists" +
                  $"?plantId={plant}" +
                  $"&tagId={tagId}" +
                  $"&api-version={_apiVersion}";

        var checkLists = await mainApiClientForApplication.TryQueryAndDeserializeAsync<TagCheckList[]>(url, cancellationToken);
        return checkLists
            .Select(x => x with {Plant = plant})
            .ToArray();
    }
}
