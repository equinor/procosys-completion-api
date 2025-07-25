using Azure.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Monitor.OpenTelemetry.Exporter;
using Equinor.ProCoSys.Completion.WebApi.Misc;

namespace Equinor.ProCoSys.Completion.WebApi.DIModules;

public static class TelemetryConfig
{
    public static WebApplicationBuilder ConfigureTelemetry(this WebApplicationBuilder builder, TokenCredential credential, bool devOnLocalhost)
    {
        if (!devOnLocalhost)
        {
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .AddAspNetCoreInstrumentation(o =>
                        {
                            o.EnrichWithHttpRequest = (activity, httpRequest) =>
                            {
                                SetAuthTags(httpRequest, activity);
                                SetPlantTag(httpRequest, activity);
                            };
                        })
                        .AddAzureMonitorTraceExporter(o =>
                        {
                            o.Credential = credential; // Set the TokenCredential for authentication
                            o.ConnectionString = builder.Configuration["AzureMonitor:ConnectionString"];
                        });
                }).UseAzureMonitor();
        }
        return builder;
    }

    private static void SetPlantTag(HttpRequest request, Activity activity)
    {
        var plant = request.Headers.GetPlant();
        if (plant is not null)
        {
            activity.SetTag("plant", plant.Substring(4));
        }
    }

    private static void SetAuthTags(HttpRequest request, Activity activity)
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
