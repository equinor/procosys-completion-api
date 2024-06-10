using Azure.Core;
using Azure.Identity;
using Equinor.ProCoSys.Completion.WebApi;
using Equinor.ProCoSys.Completion.WebApi.DIModules;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

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

builder.ConfigureAppConfig();

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
