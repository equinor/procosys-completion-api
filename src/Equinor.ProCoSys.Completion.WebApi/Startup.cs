using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Query;
using Equinor.ProCoSys.Completion.WebApi.DIModules;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using Equinor.ProCoSys.Completion.WebApi.Seeding;
using FluentValidation;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Swagger;
using Equinor.ProCoSys.Completion.WebApi.Swagger;
using Swashbuckle.AspNetCore.Filters;
using System.IO;
using Equinor.ProCoSys.Completion.WebApi.HostedServices;
using Azure.Core;
using Azure.Identity;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.ApplicationInsights.Extensibility;

namespace Equinor.ProCoSys.Completion.WebApi;

public class Startup
{
    private readonly string AllowAllOriginsCorsPolicy = "AllowAllOrigins";
    private readonly IWebHostEnvironment _environment;

    public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        Configuration = configuration;
        _environment = webHostEnvironment;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        var devOnLocalhost = Configuration.IsDevOnLocalhost();

        if (devOnLocalhost && Configuration.GetValue<bool>("MigrateDatabase"))
        {
            services.AddHostedService<DatabaseMigrator>();

            DebugOptions.DebugEntityFrameworkInDevelopment = Configuration.GetValue<bool>("DebugEntityFrameworkInDevelopment");
        }

        if (!_environment.IsProduction() && Configuration.GetValue<bool>("SeedDummyData"))
        {
            services.AddHostedService<Seeder>();
        }

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
        services.AddSingleton(credential);

        services.AddControllers().AddNewtonsoftJson();

        //TODO: PBI #104224 "Ensure using Auth Code Grant flow and add token validation"
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                Configuration.Bind("AzureAd", options);
            });

        services.AddCors(options => //TODO: #104225 "CORS - Use a list of clients, not AllowAll"
        {
            options.AddPolicy(AllowAllOriginsCorsPolicy,
                builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        services.AddMvc(config =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            config.Filters.Add(new AuthorizeFilter(policy));
        }).AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        if (Configuration.GetValue<bool>("UseAzureAppConfiguration"))
        {
            services.AddAzureAppConfiguration();
        }

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

        var scopes = Configuration.GetSection("Swagger:Scopes").Get<Dictionary<string, string>>() ??
                     new Dictionary<string, string>();

        services.AddSwaggerExamplesFromAssemblyOf<Startup>();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProCoSys Completion API", Version = "v1" });
            var authorizationUrl = Configuration.GetRequiredConfiguration("Swagger:AuthorizationUrl");

            //Define the OAuth2.0 scheme that's in use (i.e. Implicit Flow)
            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(authorizationUrl),
                        Scopes = scopes
                    }
                }
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                    },
                    scopes.Keys.ToArray()
                }
            });

            c.ExampleFilters();
            c.OperationFilter<AddRoleDocumentation>();
            c.OperationFilter<SwaggerPatchDocumentation>();
            var filePath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            c.IncludeXmlComments(filePath);
        });

        services.ConfigureSwaggerGen(options =>
        {
            options.CustomSchemaIds(x => x.FullName);
        });

        services.AddFluentValidationRulesToSwagger();

        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
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
        services.AddMediatrModules();
        services.AddApplicationModules(Configuration);

        services.AddHostedService<VerifyApplicationExistsAsPerson>();
        // VerifyLabelEntitiesExists need to come after VerifyApplicationExistsAsPerson!
        services.AddHostedService<ConfigureRequiredLabels>();
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

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProCoSys Completion API V1");
            c.DocExpansion(DocExpansion.List);
            c.DisplayRequestDuration();

            c.OAuthClientId(Configuration["Swagger:ClientId"]);
            c.OAuthAppName("ProCoSys Completion API V1");
            c.OAuthScopeSeparator(" ");
            var audience = Configuration.GetRequiredConfiguration("AzureAd:Audience");
            c.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { { "resource", audience } });
        });

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
