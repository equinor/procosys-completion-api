using System;
using System.Collections.Generic;
using System.Linq;
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
    IOptionsMonitor<MainApiOptions> mainApiOptions,
    IOptionsMonitor<ApplicationOptions> applicationOptions)
    : ICheckListApiService
{
    private readonly string _apiVersion = mainApiOptions.CurrentValue.ApiVersion;
    private readonly Uri _baseAddress = new(mainApiOptions.CurrentValue.BaseAddress);
    private readonly bool _recalculateStatusInPcs4 = applicationOptions.CurrentValue.RecalculateStatusInPcs4;

    // Do not pass plant to the GET endpoint for checklist in Main API due to performance.
    // The endpoint has m2m auth, hence it doesn't require plant specific permissions
    public async Task<ProCoSys4CheckList?> GetCheckListAsync(Guid checkListGuid, CancellationToken cancellationToken)
    {
        var url = $"{_baseAddress}CheckList/ForProCoSys5" +
                  $"?proCoSysGuid={checkListGuid:N}" +
                  $"&api-version={_apiVersion}";

        // Execute as application. The get checklist endpoint in Main Api requires
        // a special role "Checklist.RecalcStatus", which the Azure application registration has
        return await mainApiClientForApplication.TryQueryAndDeserializeAsync<ProCoSys4CheckList?>(url, cancellationToken);
    }

    public async Task RecalculateCheckListStatusAsync(string plant, Guid checkListGuid, CancellationToken cancellationToken)
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

    // Do not pass plant to the GET endpoint for many checklists in Main API due to performance.
    // The endpoint has m2m auth, hence it doesn't require plant specific permissions
    public async Task<List<ProCoSys4CheckList>> GetManyCheckListsAsync(List<Guid> checkListGuids, CancellationToken cancellationToken)
    {
        var baseUrl = $"{_baseAddress}CheckLists/ForProCoSys5" +
                  $"?api-version={_apiVersion}";

        var checkLists = new List<ProCoSys4CheckList>();

        var page = 0;
        // Use relative small page size since checkListGuids are added to querystring of url and maxlength of an url is 2000
        // length of url with baseAddress + _apiVersion is approx 105 chars.
        // Each checkListGuid will add "&proCoSysGuids=EB386A05B827044DE0532910000AFBB1" to url. Length of each is 47
        // Number of checkLists to add is approx ( (2000-105) / 47) = 40
        var pageSize = 40;
        IEnumerable<Guid> pageCheckListGuids;
        do
        {
            pageCheckListGuids = checkListGuids.Skip(pageSize * page).Take(pageSize).ToList();

            if (pageCheckListGuids.Any())
            {
                var url = baseUrl;
                foreach (var checkListGuid in pageCheckListGuids)
                {
                    url += $"&proCoSysGuids={checkListGuid:N}";
                }

                // Execute as application. The get checklists endpoint in Main Api requires
                // a special role "Checklist.RecalcStatus", which the Azure application registration has
                var checkListsPage
                    = await mainApiClientForApplication.TryQueryAndDeserializeAsync<List<ProCoSys4CheckList>>(url, cancellationToken);
                checkLists.AddRange(checkListsPage);
            }

            page++;

        } while (pageCheckListGuids.Count() == pageSize);

        return checkLists;
    }

    public async Task RecalculateCheckListStatusForManyAsync(string plant, List<Guid> checkListGuids,
        CancellationToken cancellationToken)
    {
        if (!_recalculateStatusInPcs4)
        {
            return;
        }

        var url = $"{_baseAddress}CheckLists/ForProCoSys5" +
                  $"?plantId={plant}" +
                  $"&api-version={_apiVersion}";

        var requestBody = JsonSerializer.Serialize(new CheckListGuidsDto(checkListGuids));
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        // Execute as application. The recalc endpoint in Main Api requires
        // a special role "Checklist.RecalcStatus", which the Azure application registration has
        await mainApiClientForApplication.PostAsync(url, content, cancellationToken);
    }

    public async Task<ProCoSys4CheckListSearchResult> SearchCheckListsAsync(
        Guid projectGuid,
        string? tagNoContains,
        string? responsibleCode,
        string? tagRegisterCode,
        string? tagFunctionCode,
        string? formularType,
        int? currentPage,
        int? itemsPerPage,
        CancellationToken cancellationToken)
    {
        var url = $"{_baseAddress}CheckList/ForProCoSys5/Search" +
                  $"?projectGuid={projectGuid:N}" +
                  $"&api-version={_apiVersion}";
        if (tagNoContains is not null)
        {
            url += $"&tagNoContains={tagNoContains}";
        }
        if (responsibleCode is not null)
        {
            url += $"&responsibleCode={responsibleCode}";
        }
        if (tagRegisterCode is not null)
        {
            url += $"&tagRegisterCode={tagRegisterCode}";
        }
        if (tagFunctionCode is not null)
        {
            url += $"&tagFunctionCode={tagFunctionCode}";
        }
        if (formularType is not null)
        {
            url += $"&formularType={formularType}";
        }
        if (currentPage.HasValue)
        {
            url += $"&currentPage={currentPage.Value}";
        }
        if (itemsPerPage.HasValue)
        {
            url += $"&itemsPerPage={itemsPerPage.Value}";
        }

        // Execute as application. The search endpoint in Main Api requires
        // a special role "Checklist.RecalcStatus", which the Azure application registration has
        return await mainApiClientForApplication
            .QueryAndDeserializeAsync<ProCoSys4CheckListSearchResult>(url, cancellationToken);
    }
}
