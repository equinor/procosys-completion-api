using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.Completion.WebApi.DIModules;

public static class HttpConfig
{
    private static readonly string AllowAllOriginsCorsPolicy = "AllowAllOrigins";
    
    public static WebApplicationBuilder ConfigureHttp(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            // Case-insensitive property names
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;

            // Serialize enums as strings
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

            // Ignore null values during serialization
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        builder.Services.AddCors(options => //TODO: #104225 "CORS - Use a list of clients, not AllowAll"
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

        builder.Services.AddMvc(config =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            config.Filters.Add(new AuthorizeFilter(policy));
        }).AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        return builder;
    }
}
