using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Swagger;
using Equinor.ProCoSys.Completion.WebApi.Swagger;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Equinor.ProCoSys.Completion.WebApi.DIModules;

public static class SwaggerConfig
{
    public static WebApplicationBuilder ConfigureSwagger(this WebApplicationBuilder builder)
    {
        var scopes = builder.Configuration.GetSection("Swagger:Scopes").Get<Dictionary<string, string>>() ??
                     new Dictionary<string, string>();

        builder.Services.AddSwaggerExamplesFromAssemblyOf<Startup>();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProCoSys Completion API", Version = "v1" });
            var authorizationUrl = builder.Configuration.GetRequiredConfiguration("Swagger:AuthorizationUrl");

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

        builder.Services.ConfigureSwaggerGen(options =>
        {
            options.CustomSchemaIds(x => x.FullName);
        });

        builder.Services.AddFluentValidationRulesToSwagger();
        
        return builder;
    }
    
    public static IApplicationBuilder UseCompletionSwagger(this IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProCoSys Completion API V1");
            c.DocExpansion(DocExpansion.List);
            c.DisplayRequestDuration();

            c.OAuthClientId(configuration["Swagger:ClientId"]);
            c.OAuthAppName("ProCoSys Completion API V1");
            c.OAuthScopeSeparator(" ");
            var audience = configuration.GetRequiredConfiguration("AzureAd:Audience");
            c.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { { "resource", audience } });
        });
        
        return app;
    }
}
