using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using Equinor.ProCoSys.Completion.WebApi.Misc;

namespace Equinor.ProCoSys.Completion.Test.Common;

public static class TestHelper
{
    public static List<Type> GetClassesImplementingInterface(string assemblyName, Type interfaceType)
    {
        var assembly =
            AppDomain.CurrentDomain.GetAssemblies()
                .Single(a => a.FullName is not null &&
                             a.FullName.Contains(assemblyName) &&
                             !a.FullName.Contains(".Test"));

        var classes =
            assembly.GetTypes()
                .Where(t => interfaceType.IsAssignableFrom(t) && 
                            t.IsClass && 
                            !t.IsAbstract)
                .ToList();
        return classes;
    }
    public static List<Type> GetInterfacesImplementingInterface(string assemblyName, Type interfaceType)
    {
        var assembly =
            AppDomain.CurrentDomain.GetAssemblies()
                .Single(a => a.FullName is not null &&
                             a.FullName.Contains(assemblyName) &&
                             !a.FullName.Contains(".Test"));

        var interfaces =
            assembly.GetTypes()
                .Where(t => interfaceType.IsAssignableFrom(t) && 
                            t.IsInterface &&
                            t != interfaceType)
                .ToList();
        return interfaces;
    }

    public static List<Type> GetGenericArgumentsOfBaseClasses(List<Type> testClassList) =>
        testClassList
            .Where(a => a.BaseType is not null)
            .SelectMany(a => a.BaseType.GetGenericArguments()).ToList();

    public static List<Type> GetInterfaces(List<Type> testClassList) =>
        testClassList
            .SelectMany(a => a.GetInterfaces())
            .Where(IsAEquinorType)
            .ToList();

    public static List<Type> GetTestsWhichInheritsBaseClass(Assembly assembly, Type baseClass)
    {
        var accessValidatorForIPunchItemQueryTestClasses =
            assembly.GetTypes()
                .Where(t =>
                    IsAEquinorType(t) &&
                    IsATestClass(t) &&
                    t.HasBaseClassOfType(baseClass))
                .ToList();
        return accessValidatorForIPunchItemQueryTestClasses;
    }

    public static bool IsAEquinorType(Type type)
    {
        var isAEquinorClass =
            type.FullName is not null &&
            type.FullName.Contains("Equinor.");
        return isAEquinorClass;
    }

    public static bool IsATestClass(Type type)
    {
        var customAttributes = type.CustomAttributes;
        var isATestClass =
            customAttributes.Any(a => a.AttributeType == typeof(TestClassAttribute)) &&
            type.IsClass &&
            !type.IsAbstract;
        return isATestClass;
    }

    public static void AssertPropertiesNotChanged(
        Dictionary<string, Type> expectedProperties,
        Dictionary<string, Type> actualProperties)
    {
        CollectionAssert.AreEquivalent(expectedProperties.Keys, actualProperties.Keys,
            "The number expected properties does not match number of interface properties, " +
            "test needs to be updated if the change is non breaking(non required property added)");

        foreach (var expectedProperty in expectedProperties)
        {
            Assert.AreEqual(expectedProperty.Value, actualProperties[expectedProperty.Key],
                $"Property type mismatch for {expectedProperty.Key}. " +
                "Consider creating a new version instead of modifying the existing one.");
        }
    }
}
