using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Middleware;

public class CurrentBearerTokenMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentBearerTokenMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        IHttpContextAccessor httpContextAccessor,
        IBearerTokenSetterForAll bearerTokenSetterForAll,
        ILogger<CurrentBearerTokenMiddleware> logger)
    {
        logger.LogDebug("----- {MiddlewareName} start", GetType().Name);
        if (httpContextAccessor.HttpContext is not null)
        {
            var authorizationHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            var tokens = authorizationHeader.ToString().Split(' ');

            if (tokens.Length > 1)
            {
                var token = tokens[1];
                bearerTokenSetterForAll.SetBearerToken(token);
            }
        }

        logger.LogDebug("----- {MiddlewareName} complete", GetType().Name);
        // Call the next delegate/middleware in the pipeline
        await _next(context);
    }
}
