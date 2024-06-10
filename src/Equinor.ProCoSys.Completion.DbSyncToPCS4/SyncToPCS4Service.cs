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

    public async Task SyncNewPunchListItemAsync(object addEvent, CancellationToken cancellationToken) 
        => await SynchronizeEventAsync(addEvent, SyncToPCS4Constants.PunchListItemInsertEndpoint, SyncToPCS4Constants.PunchListItem, SyncToPCS4Constants.Post, cancellationToken);

    public async Task SyncPunchListItemUpdateAsync(object updateEvent, CancellationToken cancellationToken) 
        => await SynchronizeEventAsync(updateEvent, SyncToPCS4Constants.PunchListItemUpdateEndpoint, SyncToPCS4Constants.PunchListItem, SyncToPCS4Constants.Put, cancellationToken);

    public async Task SyncPunchListItemDeleteAsync(object deleteEvent, CancellationToken cancellationToken) 
        => await SynchronizeEventAsync(deleteEvent, SyncToPCS4Constants.PunchListItemDeleteEndpoint, SyncToPCS4Constants.PunchListItem, SyncToPCS4Constants.Delete, cancellationToken);

    public async Task SyncNewCommentAsync(object addEvent, CancellationToken cancellationToken)
        => await SynchronizeEventAsync(addEvent, SyncToPCS4Constants.CommentInsertEndpoint, SyncToPCS4Constants.Comment, SyncToPCS4Constants.Post, cancellationToken);

    public async Task SyncNewAttachmentAsync(object addEvent, CancellationToken cancellationToken) 
        => await SynchronizeEventAsync(addEvent, SyncToPCS4Constants.AttachmentInsertEndpoint, SyncToPCS4Constants.Attachment, SyncToPCS4Constants.Post, cancellationToken);

    public async Task SyncAttachmentUpdateAsync(object updateEvent, CancellationToken cancellationToken) 
        => await SynchronizeEventAsync(updateEvent, SyncToPCS4Constants.AttachmentUpdateEndpoint, SyncToPCS4Constants.Attachment, SyncToPCS4Constants.Put, cancellationToken);

    public async Task SyncAttachmentDeleteAsync(object deleteEvent, CancellationToken cancellationToken) 
        => await SynchronizeEventAsync(deleteEvent, SyncToPCS4Constants.AttachmentDeleteEndpoint, SyncToPCS4Constants.Attachment, SyncToPCS4Constants.Delete, cancellationToken);

    public async Task SyncNewLinkAsync(object addEvent, CancellationToken cancellationToken) 
        => await SynchronizeEventAsync(addEvent, SyncToPCS4Constants.LinkInsertEndpoint, SyncToPCS4Constants.Link, SyncToPCS4Constants.Post, cancellationToken);

    public async Task SyncLinkUpdateAsync(object updateEvent, CancellationToken cancellationToken) 
        => await SynchronizeEventAsync(updateEvent, SyncToPCS4Constants.LinkUpdateEndpoint, SyncToPCS4Constants.Link, SyncToPCS4Constants.Put, cancellationToken);

    public async Task SyncLinkDeleteAsync(object deleteEvent, CancellationToken cancellationToken) 
        => await SynchronizeEventAsync(deleteEvent, SyncToPCS4Constants.LinkDeleteEndpoint, SyncToPCS4Constants.Link, SyncToPCS4Constants.Delete, cancellationToken);

    private async Task SynchronizeEventAsync(object syncEvent, string endpoint, string objectName, string method, CancellationToken cancellationToken)
    {
        if (!_options.CurrentValue.Enabled)
        {
            return;
        }

        var requestBody = JsonConvert.SerializeObject(syncEvent);
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(new HttpMethod(method), endpoint) { Content = content };
        _logger.LogInformation("Sending a request for a {method} method for an object of type {objectName}", method, objectName);
        await SendRequest(request, method, objectName, cancellationToken);
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


