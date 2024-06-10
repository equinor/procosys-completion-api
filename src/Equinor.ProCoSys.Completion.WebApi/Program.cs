using System;
using System.Collections.Generic;
using Azure.Core;
using Azure.Identity;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.WebApi;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

var devOnLocalhost = builder.Configuration.IsDevOnLocalhost();

// ChainedTokenCredential iterates through each credential passed to it in order, when running locally
// DefaultAzureCredential will probably fail locally, so if an instance of Azure Cli is logged in, those credentials will be used
// If those credentials fail, the next credentials will be those of the current user logged into the local Visual Studio Instance
// which is also the most likely case
TokenCredential credential = devOnLocalhost switch
{
    true
        => new ChainedTokenCredential(
            new AzureCliCredential(),
            new VisualStudioCredential(),
            new DefaultAzureCredential()
        ),
    false => new DefaultAzureCredential()
};

if (!builder.Environment.IsIntegrationTest())
{
    var azConfig = builder.Configuration.GetValue<bool>("UseAzureAppConfiguration");
    if (azConfig)
    {
        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            var connectionString = builder.Configuration["ConnectionStrings:AppConfig"];
            options.Connect(connectionString)
                .ConfigureKeyVault(kv =>
                {
                    if (builder.Configuration.IsDevOnLocalhost())
                    {
                        kv.SetCredential(new DefaultAzureCredential());
                    }
                    else
                    {
                        kv.SetCredential(new ManagedIdentityCredential());
                    }
                })
                .Select(KeyFilter.Any)
                .Select(KeyFilter.Any, builder.Environment.EnvironmentName)
                .ConfigureRefresh(refreshOptions =>
                {
                    refreshOptions.Register("Sentinel", true);
                    refreshOptions.SetCacheExpiration(TimeSpan.FromSeconds(30));
                });
        });
    }
}

builder.WebHost.UseKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.MaxRequestBodySize = null;
});

var startup = new Startup(builder.Configuration, builder.Environment);
startup.ConfigureServices(builder.Services, credential, devOnLocalhost);


var app = builder.Build();

startup.Configure(app, builder.Environment);

app.Run();
