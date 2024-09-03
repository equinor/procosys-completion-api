using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.FormularTypes;

public class MainApiFormularTypeService(
    IMainApiClientForUser mainApiClientForUser,
    IOptionsMonitor<MainApiOptions> mainApiOptions)
    : IFormularTypeApiService
{
    private readonly string _apiVersion = mainApiOptions.CurrentValue.ApiVersion;
    private readonly Uri _baseAddress = new(mainApiOptions.CurrentValue.BaseAddress);

    public async Task<List<ProCoSys4FormularType>> GetAllAsync(string plant, CancellationToken cancellationToken)
    {
        var url = $"{_baseAddress}Library/FormularTypes" +
                  $"?plantId={plant}" +
                  $"&api-version={_apiVersion}";

        return await mainApiClientForUser.QueryAndDeserializeAsync<List<ProCoSys4FormularType>>(url, cancellationToken);
    }
}
