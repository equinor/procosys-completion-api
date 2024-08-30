using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.TagFunctions;

public class MainApiTagFunctionService(
    IMainApiClientForUser mainApiClientForUser,
    IOptionsMonitor<MainApiOptions> mainApiOptions)
    : ITagFunctionApiService
{
    private readonly string _apiVersion = mainApiOptions.CurrentValue.ApiVersion;
    private readonly Uri _baseAddress = new(mainApiOptions.CurrentValue.BaseAddress);

    public async Task<List<ProCoSys4TagFunction>> GetAllAsync(string plant, CancellationToken cancellationToken)
    {
        var url = $"{_baseAddress}Library/TagFunctions" +
                  $"?plantId={plant}" +
                  $"&api-version={_apiVersion}";

        return await mainApiClientForUser.QueryAndDeserializeAsync<List<ProCoSys4TagFunction>>(url, cancellationToken);
    }
}
