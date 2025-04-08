using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public class HttpClientBearerTokenHandler : DelegatingHandler
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<HttpClientBearerTokenHandler> _logger;

    public HttpClientBearerTokenHandler(ITokenService tokenService, ILogger<HttpClientBearerTokenHandler> logger)
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

