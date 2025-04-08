using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.TieImport.Authorizations;

public class BearerTokenHandler : DelegatingHandler
{
    private readonly ITieTokenService _tokenService;
    private readonly ILogger<BearerTokenHandler> _logger;

    public BearerTokenHandler(ITieTokenService tokenService, ILogger<BearerTokenHandler> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Requesting token for:: " + request.RequestUri);
            var token = await _tokenService.AcquireTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        catch (Exception ex)
        {
            // Log the error and rethrow or handle it as needed
            _logger.LogError(ex, "Failed to acquire token for request to {Url}", request.RequestUri);
            throw; // Consider rethrowing or handling the exception based on your needs
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

