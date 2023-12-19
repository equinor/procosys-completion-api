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
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
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
using Equinor.ProCoSys.PcsServiceBus;
using Equinor.ProCoSys.PcsServiceBus.Sender.Interfaces;
using Swashbuckle.AspNetCore.Filters;
using System.IO;

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
        if (_environment.IsDevelopment() || _environment.IsTest())
        {
            var migrateDatabase = Configuration.GetValue<bool>("MigrateDatabase");
            if (migrateDatabase)
            {
                services.AddHostedService<DatabaseMigrator>();
            }
        }
        if (_environment.IsDevelopment())
        {
            DebugOptions.DebugEntityFrameworkInDevelopment = Configuration.GetValue<bool>("DebugEntityFrameworkInDevelopment");

            if (Configuration.GetValue<bool>("SeedDummyData"))
            {
                services.AddHostedService<Seeder>();
            }
        }

        services.AddControllers().AddNewtonsoftJson();

        //TODO: PBI #104224 "Ensure using Auth Code Grant flow and add token validation"
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                Configuration.Bind("API", options); //TODO #104226 "Used standardized config section names for Azure Ad config"
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

        var scopes = Configuration.GetSection("Swagger:Scopes").Get<Dictionary<string, string>>() ?? new Dictionary<string, string>();
        
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

        services.AddApplicationInsightsTelemetry(options =>
        {
            options.ConnectionString = Configuration.GetRequiredConfiguration("ApplicationInsights:ConnectionString");
        });
        services.AddMediatrModules();
        services.AddApplicationModules(Configuration);

        services.AddHostedService<VerifyApplicationExistsAsPerson>();
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
            var audience = Configuration.GetRequiredConfiguration("API:Audience");
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
