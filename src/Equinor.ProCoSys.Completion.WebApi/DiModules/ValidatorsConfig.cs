using System.Collections.Generic;
using System.Reflection;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Query;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;

namespace Equinor.ProCoSys.Completion.WebApi.DIModules;

public static class ValidatorsConfig
{
    public static WebApplicationBuilder ConfigureValidators(this WebApplicationBuilder builder)
    {
        builder.Services.AddFluentValidationAutoValidation(fv =>
        {
            fv.DisableDataAnnotationsValidation = true;
        });
        builder.Services.AddValidatorsFromAssemblies(new List<Assembly>
        {
            typeof(IQueryMarker).GetTypeInfo().Assembly,
            typeof(ICommandMarker).GetTypeInfo().Assembly,
            typeof(Program).GetTypeInfo().Assembly
        });
        return builder;
    }
}
