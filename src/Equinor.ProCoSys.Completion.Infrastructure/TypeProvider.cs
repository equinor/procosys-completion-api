using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Equinor.ProCoSys.Completion.Infrastructure;

public static class TypeProvider
{
    private static List<Type>? s_entityTypeCache;

    public static List<Type> GetEntityTypes(Assembly assembly, Type baseType)
    {
        if (s_entityTypeCache != null)
        {
            return s_entityTypeCache;
        }

        s_entityTypeCache = (from t in assembly.DefinedTypes
            where t.BaseType == baseType
            select t.AsType()).ToList();

        return s_entityTypeCache;
    }
}