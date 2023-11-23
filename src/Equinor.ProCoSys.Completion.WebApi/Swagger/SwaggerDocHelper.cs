using Microsoft.AspNetCore.JsonPatch;
using System.Reflection;
using System;
using System.Linq;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Equinor.ProCoSys.Completion.WebApi.Swagger;

public class SwaggerDocHelper
{
    public static void FillPatchDocumentWithSampleData<T>(JsonPatchDocument<T> patchDocument) where T : class
    {
        var type = typeof(T);
        var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanWrite);
        //var requiredProps = props.Where(prop => prop.IsDefined(typeof(RequiredAttribute), false))
        //    .Select(prop => prop.Name).ToList();

        foreach (var prop in props)
        {
            object value;
            var propType = prop.PropertyType;
            if (propType == typeof(bool) || propType == typeof(bool?))
            {
                value = true;
            }
            else if (propType == typeof(int) || propType == typeof(int?))
            {
                value = 1;
            }
            else if (propType == typeof(double) || propType == typeof(double?))
            {
                value = 1.1;
            }
            else if (propType == typeof(Guid) || propType == typeof(Guid?))
            {
                value = Guid.Empty;
            }
            else if (propType == typeof(DateTime) || propType == typeof(DateTime?))
            {
                value = new DateTime(2023, 1, 2, 13, 1, 2, DateTimeKind.Utc);
            }
            else
            {
                value = "string";
            }
            patchDocument.Operations.Add(new Operation<T>("replace", $"/{prop.Name}", null, value));
        }
    }
}
