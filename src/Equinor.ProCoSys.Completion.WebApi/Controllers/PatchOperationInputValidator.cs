﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Equinor.ProCoSys.Completion.WebApi.Misc;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

// Use Attribute on classes represented by the generic type T to get correct validation of T
// Following Attribute can be used:
//      * RequiredAttribute: Check if property is required or not (Can't just check if property is nullable or not, since property type of string and string? are both "System.String")
//      * StringLengthAttribute: check min / max length of values to set in string properties
//          NB: Using StringLengthAttribute on non-string properties has no affect
public class PatchOperationInputValidator : IPatchOperationInputValidator
{
    public bool HaveValidReplaceOperationsOnly<T>(List<Operation<T>> operations) where T : class
    {
        var illegalOperations = GetInvalidOperations(operations);
        return illegalOperations.Count == 0;
    }

    public string? GetMessageForInvalidReplaceOperations<T>(List<Operation<T>> operations) where T : class
    {
        var illegalOperations = GetInvalidOperations(operations);
        if (illegalOperations.Count == 0)
        {
            return null;
        }

        return string.Join('|', illegalOperations);
    }

    public bool HaveReplaceOperationsOnly<T>(List<Operation<T>> operations) where T : class
        => operations.All(o => o.OperationType == OperationType.Replace);

    public bool HaveUniqueReplaceOperations<T>(List<Operation<T>> operations) where T : class
    {
        var allPaths = operations.Select(o => o.path).ToList();
        return allPaths.Count == allPaths.Distinct().Count();
    }

    public bool AllRequiredFieldsHaveValue<T>(List<Operation<T>> operations) where T : class
    {
        var nullOperationsForRequiredProperties = GetNullOperationsForRequiredProperties(operations, out _);

        return nullOperationsForRequiredProperties.Count == 0;
    }

    public string? GetMessageForRequiredFields<T>(List<Operation<T>> operations) where T : class
    {
        var nullOperationsForRequiredProperties =
            GetNullOperationsForRequiredProperties(operations, out var requiredProperties);
        if (nullOperationsForRequiredProperties.Count == 0)
        {
            return null;
        }
        var message =
            $"{typeof(T).Name} has required properties: {string.Join(',', requiredProperties)}. " + 
            $"Can't set null for {string.Join(',', nullOperationsForRequiredProperties)}";
        return message;
    }

    public bool HaveValidLengthOfStrings<T>(List<Operation<T>> operations) where T : class
    {
        var illegalOperations = GetInvalidStringOperations(operations);

        return illegalOperations.Count == 0;
    }

    public string? GetMessageForInvalidLengthOfStrings<T>(List<Operation<T>> operations) where T : class
    {
        var illegalOperations = GetInvalidStringOperations(operations);
        if (illegalOperations.Count == 0)
        {
            return null;
        }
        return string.Join('|', illegalOperations);
    }

    private List<string> GetInvalidOperations<T>(List<Operation<T>> operations) where T : class
    {
        var replaceOperations = CollectReplaceOperations(operations);
        var writableProperties = CollectWritableProperties(typeof(T));

        var illegalOperations = GetInvalidOperations(typeof(T).Name, replaceOperations, writableProperties);
        return illegalOperations;
    }

    private List<string> GetInvalidOperations(
        string typeName,
        Dictionary<string, object?> replaceOperations, 
        Dictionary<string, Type> writableProperties)
    {
        var illegalOperations = new List<string>();
        foreach (var operation in replaceOperations)
        {
            if (!writableProperties.ContainsKey(operation.Key))
            {
                illegalOperations.Add($"{typeName} don't have writable property {operation.Key}");
                continue;
            }

            var propType = writableProperties[operation.Key];

            if (!CanAssignValueToProperty(operation.Value, propType))
            {
                if (operation.Value is null)
                {
                    illegalOperations.Add(
                        $"Can't assign null-value to property {operation.Key} of type {propType}");
                }
                else
                {
                    illegalOperations.Add(
                        $"Can't assign value value of type {operation.Value.GetType()} to property {operation.Key} of type {propType}");
                }
            }
        }

        return illegalOperations;
    }

    private static bool CanAssignValueToProperty(object? value, Type propType)
    {
        if (value is null)
        {
            // a null value can be assigned to propType if it is nullable
            return IsNullableType(propType);
        }

        var valueType = value.GetType();
        if (propType.IsAssignableFrom(valueType))
        {
            return true;
        }

        // special case of assigning an int to a double-property
        if ((propType == typeof(double) || propType == typeof(double?)) && value is int)
        {
            return true;
        }

        // special case of assigning an long to an int-property
        if ((propType == typeof(int) || propType == typeof(int?)) && value is long and < int.MaxValue)
        {
            return true;
        }

        if (value is string str)
        {
            return TryParse(str, propType);
        }

        return false;
    }

    private static bool TryParse(string str, Type propType)
    {
        if (propType == typeof(bool) || propType == typeof(bool?))
        {
            return bool.TryParse(str, out _);
        }
        if (propType == typeof(int) || propType == typeof(int?))
        {
            return int.TryParse(str, out _);
        }
        if (propType == typeof(double) || propType == typeof(double?))
        {
            return double.TryParse(str, CultureInfo.CurrentCulture, out _);
        }
        if (propType == typeof(Guid) || propType == typeof(Guid?))
        {
            return Guid.TryParse(str, out _);
        }
        if (propType == typeof(DateTime) || propType == typeof(DateTime?))
        {
            return DateTime.TryParse(str, CultureInfo.CurrentCulture, out _);
        }
        return false;
    }

    private static bool IsNullableType(Type type)
    {
        // special case for string: when using reflection to get type of a nullable string (string?),
        // propType.IsGenericType is false. The type for a string? is still "System.String"
        if (type == typeof(string))
        {
            return true;
        }
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    private Dictionary<string, Type> CollectWritableProperties(Type type)
        => type.GetWritableProperties().ToDictionary(p => p.Name, p => p.PropertyType);

    private Dictionary<string, object?> CollectReplaceOperations<T>(List<Operation<T>> operations) where T : class
        => operations
            .Where(op => op.OperationType == OperationType.Replace)
            .ToDictionary(op => op.path.TrimStart('/'), op => op.value is null ? null : op.value);

    private List<string> GetNullOperationsForRequiredProperties<T>(
        List<Operation<T>> operations,
        out List<string> requiredProperties) where T : class
    {
        var replaceOperationsToSetNull = CollectReplaceOperations(operations)
            .Where(op => op.Value is null)
            .Select(op => op.Key).ToList();
        var type = typeof(T);
        requiredProperties = type.GetPropertiesWithAttribute(typeof(RequiredAttribute))
            .Select(prop => prop.Name).ToList();

        return requiredProperties.Intersect(replaceOperationsToSetNull).ToList();
    }

    private List<string> GetInvalidStringOperations<T>(List<Operation<T>> operations) where T : class
    {
        var illegalOperations = new List<string>();
        var type = typeof(T);
        var propertiesWithLengthLimiting = type.GetWritablePropertiesWithStringLengthAttribute().ToList();

        if (!propertiesWithLengthLimiting.Any())
        {
            return illegalOperations;
        }

        var replaceStringOperations = operations
            .Where(op => op.OperationType == OperationType.Replace && op.value is string);
        foreach (var operation in replaceStringOperations)
        {
            var propName = operation.path.TrimStart('/');
            var propertyWithLengthLimiting =
                propertiesWithLengthLimiting.SingleOrDefault(p => p.Name == propName);
            if (propertyWithLengthLimiting is null)
            {
                continue;
            }

            var stringLengthAttribute = propertyWithLengthLimiting.GetCustomAttribute<StringLengthAttribute>(false)!;
            if (!stringLengthAttribute.IsValid(operation.value as string, out var message))
            {
                illegalOperations.Add($"Can't assign value to property {propName}. {message}");
            }
        }

        return illegalOperations;
    }
}
