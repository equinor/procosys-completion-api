using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

/**
 * This class provide an interface to the operations for synchronizing data to ProCoSys 4. 
 * The class should only be used as a singleton (through dependency injection). 
 */
public class SyncToPCS4Service : ISyncToPCS4Service
{
    private readonly IOptionsMonitor<SyncToPCS4Options> _options;
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SyncToPCS4Service> _logger;

    public SyncToPCS4Service(IOptionsMonitor<SyncToPCS4Options> options, IHttpContextAccessor httpContextAccessor, ILogger<SyncToPCS4Service> logger)
    {
        _options = options;

        var baseUrl = options.CurrentValue.Endpoint;
        var client = new HttpClient()
        {
            BaseAddress = new Uri(baseUrl)
        };
        _httpClient = client;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }
    public async Task SyncNewPunchListItemAsync(object updateEvent, CancellationToken cancellationToken)
    {
        if (!_options.CurrentValue.Enabled)
        {
            return;
        }

        var requestBody = JsonConvert.SerializeObject(updateEvent);
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(new HttpMethod("POST"), SyncToPCS4Constants.PunchListItemInsertEndpoint) { Content = content };
        await SendRequest(request, "Insert", "PunchListItem", cancellationToken);
    }

    public async Task SyncPunchListItemUpdateAsync(object updateEvent, CancellationToken cancellationToken)
    {
        if (!_options.CurrentValue.Enabled)
        {
            return;
        }

        var requestBody = JsonConvert.SerializeObject(updateEvent);
        _logger.LogDebug(@"Serialized Request: {requestBody}",requestBody);
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(new HttpMethod("PUT"), SyncToPCS4Constants.PunchListItemUpdateEndpoint) { Content = content };
        await SendRequest(request, "Update", "PunchListItem", cancellationToken);
    }

    public async Task SyncPunchListItemDeleteAsync(object deleteEvent, CancellationToken cancellationToken)
    {
        if (!_options.CurrentValue.Enabled)
        {
            return;
        }

        var requestBody = JsonConvert.SerializeObject(deleteEvent);
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(new HttpMethod("DELETE"), SyncToPCS4Constants.PunchListItemDeleteEndpoint) { Content = content };
        await SendRequest(request, "Delete", "PunchListItem", cancellationToken);
    }

    /**
     * Insert a new row in the PCS 4 database based on the sourceObject.
     * Note: The endpoint must have support for the given sourceObjectName.
     */
    public async Task SyncNewObjectAsync(string sourceObjectName, object sourceObject, string plant, CancellationToken cancellationToken)
    {
        if (!_options.CurrentValue.Enabled)
        {
            return;
        }

        var request = CreateRequest(SyncToPCS4Constants.InsertEndpoint, "POST", sourceObjectName, sourceObject, plant);
        await SendRequest(request, "Insert", sourceObjectName, cancellationToken);
    }

    /**
     * Updates the PCS 4 database with changes provided by the sourceObject. 
     * Note: The endpoint must have support for the given sourceObjectName.
     */
    public async Task SyncObjectUpdateAsync(string sourceObjectName, object sourceObject, string plant, CancellationToken cancellationToken)
    {
        if (!_options.CurrentValue.Enabled)
        {
            return;
        }

        var request = CreateRequest(SyncToPCS4Constants.UpdateEndpoint, "PUT", sourceObjectName, sourceObject, plant);
        await SendRequest(request, "Update", sourceObjectName, cancellationToken);
    }

    /**
     * Deletes the row in the PCS 4 database that correspond to the given source object. 
     * Note: The endpoint must have support for the given sourceObjectName.
     */
    public async Task SyncObjectDeletionAsync(string sourceObjectName, object sourceObject, string plant, CancellationToken cancellationToken)
    {
        if (!_options.CurrentValue.Enabled)
        {
            return;
        }

        var request = CreateRequest(SyncToPCS4Constants.DeleteEndpoint, "DELETE", sourceObjectName, sourceObject, plant);
        await SendRequest(request, "Delete", sourceObjectName, cancellationToken);
    }

    private static HttpRequestMessage CreateRequest(string url, string method, string sourceObjectName, object sourceObject, string plant)
    {
        var bodyObject = new SyncObjectDto { SyncObjectName = sourceObjectName, SyncObject = sourceObject, SyncPlant = plant };

        var requestBody = JsonConvert.SerializeObject(bodyObject);
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(new HttpMethod(method), url) { Content = content };

        return request;
    }

    private async Task SendRequest(HttpRequestMessage request, string method, string sourceObjectName, CancellationToken cancellationToken)
    {
        AddBearerToken(ref request);
        var result = await _httpClient.SendAsync(request, cancellationToken);

        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"Error occured when trying to execute a ({method}) sync statement to PCS4 for data of type ({sourceObjectName}).");
        }
    }

    private void AddBearerToken(ref HttpRequestMessage request)
    {
        var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString().Split(' ').Last();

        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}


