using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.Responsibles;

public class MainApiResponsibleService(
    IMainApiClientForUser mainApiClientForUser,
    IOptionsMonitor<MainApiOptions> mainApiOptions)
    : IResponsibleApiService
{
    private readonly string _apiVersion = mainApiOptions.CurrentValue.ApiVersion;
    private readonly Uri _baseAddress = new(mainApiOptions.CurrentValue.BaseAddress);

    public async Task<List<ProCoSys4Responsible>> GetAllAsync(string plant, CancellationToken cancellationToken)
    {
        var url = $"{_baseAddress}Library/Responsibles" +
                  $"?plantId={plant}" +
                  $"&api-version={_apiVersion}";

        return await mainApiClientForUser.QueryAndDeserializeAsync<List<ProCoSys4Responsible>>(url, cancellationToken);
    }
}
