using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Misc;

public static class TypeExtension
{
    public static IEnumerable<PropertyInfo> CollectPropertiesWithAttribute(this Type type, Type attributeType)
    {
        var props = GetWritableProperties(type);
        return CollectPropertiesWithAttribute(props, attributeType);
    }

    public static IEnumerable<PropertyInfo> GetWritableProperties(this Type type)
        => type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite);

    public static IEnumerable<PropertyInfo> CollectPropertiesWithStringLengthAttribute(this Type type)
    {
        var writableStringProperties = type.GetWritableProperties().Where(p => p.PropertyType == typeof(string));
        return CollectPropertiesWithAttribute(writableStringProperties, typeof(StringLengthAttribute)).ToList();
    }

    public static bool HasBaseClassOfType(this Type type, Type baseType)
    {
        var baseTypeFullName = baseType.FullName!;
        var hasBaseClassOfType =
            type.BaseType?.FullName != null &&
            type.BaseType.FullName.StartsWith(baseTypeFullName);

        return hasBaseClassOfType;
    }

    private static IEnumerable<PropertyInfo> CollectPropertiesWithAttribute(IEnumerable<PropertyInfo> props, Type attributeType)
        => props.Where(prop => prop.IsDefined(attributeType, false));
}
