using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;

/**
 * This class provide an interface to the operations for synchronizing data to ProCoSys 4. 
 * The class should only be used as a singleton (through dependency injection). 
 */
public class SyncToPCS4Service : ISyncToPCS4Service
{
    public static string ClientName = "SyncHttpClient";

    private readonly IOptionsMonitor<SyncToPCS4Options> _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SyncToPCS4Service> _logger;

    public SyncToPCS4Service(IHttpClientFactory httpClientFactory, IOptionsMonitor<SyncToPCS4Options> options, ILogger<SyncToPCS4Service> logger)
    {
        _options = options;
        _logger = logger;

        _httpClient = httpClientFactory.CreateClient(ClientName);
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

        var requestBody = JsonSerializer.Serialize(syncEvent);
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(new HttpMethod(method), endpoint) { Content = content };
        _logger.LogInformation("Sending a request for a {method} method for an object of type {objectName}", method, objectName);
        await SendRequest(request, method, objectName, cancellationToken);
    }

    private async Task SendRequest(HttpRequestMessage request, string method, string sourceObjectName, CancellationToken cancellationToken)
    {
        var result = await _httpClient.SendAsync(request, cancellationToken);

        if (!result.IsSuccessStatusCode)
        {
            var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Error occurred when trying to execute a {Method} sync statement to PCS4 for data of type {SourceObjectName}. Status code: {StatusCode}, Response: {ResponseContent}", method, sourceObjectName, result.StatusCode, responseContent);
            throw new Exception($"Error occurred when trying to execute a ({method}) sync statement to PCS4 for data of type ({sourceObjectName}). Status code: {result.StatusCode}, Response: {responseContent}");
        }
    }
}
