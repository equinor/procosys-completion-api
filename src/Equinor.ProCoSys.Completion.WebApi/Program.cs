using System.Diagnostics;
using Azure.Core;
using Azure.Identity;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.WebApi.DIModules;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;

const string AllowAllOriginsCorsPolicy = "AllowAllOrigins";

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

builder.Services.AddSingleton(credential);
builder.ConfigureAzureAppConfig(credential);
builder.Services.AddHealthChecks();

// Don't use Redis for integration test. Use the memory implementation
if (!builder.Environment.IsIntegrationTest())
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("RedisCache");
    });
} else if (builder.Environment.IsIntegrationTest())
{
    builder.Services.AddDistributedMemoryCache();
}


builder.WebHost.UseKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.MaxRequestBodySize = null;
});

builder.ConfigureHostedJobs(devOnLocalhost);
builder.ConfigureSwagger();
builder.ConfigureHttp();

//TODO: PBI #104224 "Ensure using Auth Code Grant flow and add token validation"
builder.Services
    .AddMicrosoftIdentityWebApiAuthentication(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddDistributedTokenCaches();

builder.Services.AddPcsAuthIntegration();

builder.ConfigureValidators();
builder.ConfigureTelemetry(credential, devOnLocalhost);

builder.Services.AddMediatrModules();
builder.Services.AddApplicationModules(builder.Configuration, credential);

if (builder.Configuration.GetValue<bool>("TieImport:Enable"))
{
    builder.Services.AddTieImportModule(builder);
}

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("UseAzureAppConfiguration"))
{
    app.UseAzureAppConfiguration();
}

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseGlobalExceptionHandling();

app.UseCors(AllowAllOriginsCorsPolicy); //TODO: #104225 "CORS - Use a list of clients, not AllowAll"

app.UseCompletionSwagger(builder.Configuration);

app.UseHttpsRedirection();

app.UseRouting();

// order of adding middleware are crucial. Some depend that other has been run in advance
app.UseCurrentPlant();
app.UseAuthentication();
app.UseCurrentUser();
app.UsePersonValidator();
app.UsePlantValidator();
app.UseVerifyOidInDb();
app.UseAuthorization();

app.UseResponseCompression();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
