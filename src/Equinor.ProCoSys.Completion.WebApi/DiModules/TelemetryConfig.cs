using Azure.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Trace;
using OpenTelemetry;
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
            var serviceProvider = builder.Services.BuildServiceProvider();
            builder.Services.ConfigureOpenTelemetryTracerProvider((sp, tracerProviderBuilder) 
                => tracerProviderBuilder.AddProcessor(new ActivityEnrichingProcessor(serviceProvider.GetRequiredService<IHttpContextAccessor>())));
            builder.Services.AddOpenTelemetry().UseAzureMonitor();
        }
        
        return builder;
    }

    public class ActivityEnrichingProcessor : BaseProcessor<Activity>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ActivityEnrichingProcessor(IHttpContextAccessor httpContextAccessor) =>
            _httpContextAccessor = httpContextAccessor;

        // The OnStart method is called when an activity is started. This is the ideal place to filter activities.
        public override void OnStart(Activity activity)
        {
            // prevents all exporters from exporting internal activities
            if (activity.Kind == ActivityKind.Internal)
            {
                activity.IsAllDataRequested = false;
            }
        }

        public override void OnEnd(Activity activity)
        {
            if (activity.Kind != ActivityKind.Server)
            {
                return;
            }

            if (null != _httpContextAccessor.HttpContext)
            {
                var authHeader = _httpContextAccessor.HttpContext.Request.Headers.Authorization.ToString();

                if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();

                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(token);
                        var audience = jwtToken.Audiences.FirstOrDefault();
                        var appId = jwtToken.Claims.FirstOrDefault(x => x.Type == "appid")?.Value ?? "N/A";
                        var oid = jwtToken.Claims.FirstOrDefault(x => x.Type == "oid")?.Value ?? "N/A";

                        if (!string.IsNullOrEmpty(audience))
                        {
                            //activity.SetTag("Audience", audience);
                            activity.SetTag("AppId", appId);
                            activity.SetTag("enduser.id", oid);
                        }
                    }
                    catch (Exception e)
                    {
                        activity.SetStatus(ActivityStatusCode.Error);
                        activity.RecordException(e);
                    }
                }

                foreach (var header in _httpContextAccessor.HttpContext.Request.Headers)
                {
                    if (header.Key == "x-plant")
                    {
                        activity.SetTag("plant", header.Value.ToString().Substring(4));
                    } 
                }
            }
        }
    }
}
