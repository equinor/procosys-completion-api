using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Azure.Core;
using Azure.Identity;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Swagger;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Query;
using Equinor.ProCoSys.Completion.WebApi.DIModules;
using Equinor.ProCoSys.Completion.WebApi.HostedServices;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Equinor.ProCoSys.Completion.WebApi.Seeding;
using Equinor.ProCoSys.Completion.WebApi.Swagger;
using FluentValidation;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Equinor.ProCoSys.Completion.WebApi;

public class Startup
{
    private static readonly string AllowAllOriginsCorsPolicy = "AllowAllOrigins";
    private readonly IWebHostEnvironment _environment;

    public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        Configuration = configuration;
        _environment = webHostEnvironment;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services, TokenCredential credential, bool devOnLocalhost)
    {
        //TODO: PBI #104224 "Ensure using Auth Code Grant flow and add token validation"
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                Configuration.Bind("AzureAd", options);
            });

        services.AddFluentValidationAutoValidation(fv =>
        {
            fv.DisableDataAnnotationsValidation = true;
        });
        services.AddValidatorsFromAssemblies(new List<Assembly>
        {
            typeof(IQueryMarker).GetTypeInfo().Assembly,
            typeof(ICommandMarker).GetTypeInfo().Assembly,
            typeof(Startup).Assembly
        });
        
        services.AddPcsAuthIntegration();

        if (!devOnLocalhost)
        {
            services.Configure<TelemetryConfiguration>(config =>
            {
                config.SetAzureTokenCredential(credential);
            });
        }

        services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = Configuration.GetRequiredConfiguration("ApplicationInsights:ConnectionString");
        });
        services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
        {
            module.EnableSqlCommandTextInstrumentation = true;
        });

        services.AddMediatrModules();
        services.AddApplicationModules(Configuration);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (Configuration.GetValue<bool>("UseAzureAppConfiguration"))
        {
            app.UseAzureAppConfiguration();
        }

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseGlobalExceptionHandling();

        app.UseCors(AllowAllOriginsCorsPolicy); //TODO: CORS, dont allow all. Se better comment above

        app.UseCompletionSwagger(Configuration);

        app.UseHttpsRedirection();

        app.UseRouting();

        // order of adding middleware are crucial. Some depend that other has been run in advance
        app.UseCurrentPlant();
        app.UseCurrentBearerToken();
        app.UseAuthentication();
        app.UseCurrentUser();
        app.UsePersonValidator();
        app.UsePlantValidator();
        app.UseVerifyOidInDb();
        app.UseAuthorization();

        app.UseResponseCompression();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
