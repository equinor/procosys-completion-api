using Azure.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Trace;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Equinor.ProCoSys.Completion.WebApi.DIModules;

public static class TelemetryConfig
{
    public static WebApplicationBuilder ConfigureTelemetry(this WebApplicationBuilder builder, TokenCredential credential, bool devOnLocalhost)
    {
        if (!devOnLocalhost)
        {
            builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder => tracerProviderBuilder
                .AddAspNetCoreInstrumentation(o =>
                {
                    o.EnrichWithHttpRequest = (activity, httpRequest) =>
                    {
                        SetAuthTags(httpRequest, activity);
                        SetPlantTag(httpRequest, activity);
                    };
                }));
            // by default, UseAzureMonitor look for config key "AzureMonitor:ConnectionString"
            builder.Services.AddOpenTelemetry().UseAzureMonitor();
        }

        return builder;
    }

    public static void SetPlantTag(HttpRequest request, Activity activity)
    {
        var plantHeader = request.Headers
            .SingleOrDefault(header => header.Key == "x-plant").Value;

        if (!string.IsNullOrEmpty(plantHeader))
        {
            activity.SetTag("plant", plantHeader.ToString().Substring(4));
        }
    }

    public static void SetAuthTags(HttpRequest request, Activity activity)
    {
        if (request.Headers.TryGetValue("Authorization", out var authHeader)
            && !string.IsNullOrEmpty(authHeader)
            && authHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.ToString().Substring("Bearer ".Length).Trim();
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var audience = jwtToken.Audiences.FirstOrDefault();
                var appId = jwtToken.Claims.FirstOrDefault(x => x.Type == "appid")?.Value ?? "N/A";
                var oid = jwtToken.Claims.FirstOrDefault(x => x.Type == "oid")?.Value ?? "N/A";

                activity.SetTag("AppId", appId);
                activity.SetTag("enduser.id", oid);
            }
            catch (Exception e)
            {
                activity.SetStatus(ActivityStatusCode.Error);
                activity.RecordException(e);
            }
        }
    }
}
