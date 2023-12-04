using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Equinor.ProCoSys.Completion.WebApi.Misc;

namespace Equinor.ProCoSys.Completion.WebApi.Swagger;

public class SwaggerPatchDocumentation : IOperationFilter
{
    private readonly string _generalPatchInfo =
        "Patching can be done on zero properties, one property only, all properties in once, or any combination of properties.<br>" +
        "If a required property is patched, a (new) value must be given.<br>" +
        "If an optional property is patched, both a (new) value or null can be given. Empty string can also be used for string properties.<br>" + 
        "<br>" +
        "Only replace operation are supported (\"op\": \"replace\").<br>";
        
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var patchDocumentationAttribute = GetSwaggerPatchDocumentationAttribute(context.ApiDescription);
        if (patchDocumentationAttribute is null)
        {
            return;
        }

        var descriptionToAdd = _generalPatchInfo;
        var requiredProperties = FindRequiredProperties(patchDocumentationAttribute.Type).ToList();
        if (requiredProperties.Any())
        {
            var requiredPropertiesList = string.Join(", ", requiredProperties);
            descriptionToAdd = $"{descriptionToAdd}<br>These properties are required: {requiredPropertiesList}";
        }

        if (string.IsNullOrWhiteSpace(operation.Description))
        {
            operation.Description = descriptionToAdd;
        }
        else
        {
            operation.Description = $"{operation.Description.Trim('.')}. {descriptionToAdd}";
        }
    }

    private IEnumerable<string> FindRequiredProperties(Type type)
    {
        var reqAttrType = typeof(RequiredAttribute);
        var requiredProperties = type.GetPropertiesWithAttribute(reqAttrType);

        return requiredProperties.Select(a => a.Name);
    }

    private SwaggerPatchDocumentationAttribute? GetSwaggerPatchDocumentationAttribute(ApiDescription apiDescription)
    {
        var attribute = apiDescription.ActionDescriptor.EndpointMetadata
            .SingleOrDefault(i => i.GetType() == typeof(SwaggerPatchDocumentationAttribute));

        return attribute as SwaggerPatchDocumentationAttribute;
    }
}
