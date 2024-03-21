using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Client;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.ForeignApi.MainApi.Persons;

public class MainApiPersonService : IPersonApiService
{
    private readonly string _apiVersion;
    private readonly Uri _baseAddress;
    private readonly IMainApiClient _mainApiClient;

    public MainApiPersonService(
        IMainApiClient mainApiClient,
        IOptions<MainApiOptions> options)
    {
        _mainApiClient = mainApiClient;
        _apiVersion = options.Value.ApiVersion;
        _baseAddress = new Uri(options.Value.BaseAddress);
    }

    public async Task<List<ProCoSys4Person>> GetAllPersonsAsync(string plant, CancellationToken cancellationToken)
    {
        var url = $"{_baseAddress}Person/AllPersons" +
                  $"?plantId={plant}" +
                  $"&api-version={_apiVersion}";
        return await _mainApiClient.TryQueryAndDeserializeAsync<List<ProCoSys4Person>>(url);
    }
}
