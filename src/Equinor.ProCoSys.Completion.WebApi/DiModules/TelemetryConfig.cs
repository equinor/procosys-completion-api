using System.Configuration;
using Azure.Core;
using Equinor.ProCoSys.Common.Misc;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Azure.Monitor.OpenTelemetry.AspNetCore;

namespace Equinor.ProCoSys.Completion.WebApi.DIModules;

public static class TelemetryConfig
{
    public static WebApplicationBuilder ConfigureTelemetry(this WebApplicationBuilder builder, TokenCredential credential, bool devOnLocalhost)
    {
        // if (!devOnLocalhost)
        // {
        //     builder.Services.Configure<TelemetryConfiguration>(config =>
        //     {
        //         config.SetAzureTokenCredential(credential);
        //     });
        // }
        //
        // builder.Services.AddApplicationInsightsTelemetry(options =>
        // {
        //     var optionsConnectionString = builder.Configuration.GetRequiredConfiguration("ApplicationInsights:ConnectionString");
        //     options.ConnectionString = optionsConnectionString;
        // });
        // builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
        // {
        //     module.EnableSqlCommandTextInstrumentation = builder.Configuration.GetValue("EnableSqlCommandTextInstrumentation", false);
        // });
        
        builder.Services.AddOpenTelemetry().UseAzureMonitor();
        
        return builder;
    }
}
