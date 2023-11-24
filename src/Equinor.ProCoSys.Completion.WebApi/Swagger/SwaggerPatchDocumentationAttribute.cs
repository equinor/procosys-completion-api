using System;
using System.Linq;
using Equinor.ProCoSys.Completion.WebApi.Controllers;
using Equinor.ProCoSys.Completion.WebApi.Misc;

namespace Equinor.ProCoSys.Completion.WebApi.Swagger;

[AttributeUsage(AttributeTargets.Method)]
public class SwaggerPatchDocumentationAttribute : Attribute
{
    public Type Type { get; }

    public SwaggerPatchDocumentationAttribute(Type type)
    {
        if (!type.HasBaseClassOfType(typeof(PatchDto<>)))
        {
            throw new Exception($"Input type need to inherit {typeof(PatchDto<>)}");
        }

        var genericType = type.BaseType!.GetGenericArguments().First();
        Type = genericType;
    }
}
