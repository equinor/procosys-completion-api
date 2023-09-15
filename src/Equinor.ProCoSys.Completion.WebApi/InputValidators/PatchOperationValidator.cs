using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Equinor.ProCoSys.Completion.WebApi.InputValidators;

public class PatchOperationValidator : IPatchOperationValidator
{
    private readonly IRowVersionValidator _rowVersionValidator;
    
    public PatchOperationValidator(IRowVersionValidator rowVersionValidator) => _rowVersionValidator = rowVersionValidator;

    public bool HaveValidReplaceOperationsOnly<T>(List<Operation<T>> operations) where T : class
    {
        var illegalOperations = GetIllegalOperations(operations);
        return illegalOperations.Count == 0;
    }

    public string? GetMessageForIllegalReplaceOperations<T>(List<Operation<T>> operations) where T : class
    {
        var illegalOperations = GetIllegalOperations(operations);
        if (illegalOperations.Count == 0)
        {
            return null;
        }

        var messages = illegalOperations.Select(o => o.Value);
        return string.Join('|', messages);
    }

    public bool HaveReplaceOperationsOnly<T>(List<Operation<T>> operations) where T : class
        => operations.All(o => o.OperationType == OperationType.Replace);

    public bool HaveUniqueReplaceOperations<T>(List<Operation<T>> operations) where T : class
    {
        var allPaths = operations.Select(o => o.path).ToList();
        return allPaths.Count == allPaths.Distinct().Count();
    }

    public bool HaveValidRowVersionOperation<T2>(List<Operation<T2>> operations) where T2 : class
    {
        var rowVersion = operations
            .Where(op => op.path == "/RowVersion" &&
                                   op.value is string &&
                                   op.OperationType == OperationType.Replace)
            .Select(op => op.value as string)
            .SingleOrDefault();
        if (rowVersion == null)
        {
            return false;
        }

        return _rowVersionValidator.IsValid(rowVersion);
    }

    private Dictionary<string, string> GetIllegalOperations<T>(List<Operation<T>> operations) where T : class
    {
        var replaceOperations = CollectReplaceOperations(operations);
        var writableProperties = CollectWritableProperties(typeof(T));

        var illegalOperations = GetIllegalOperations(typeof(T).Name, replaceOperations, writableProperties);
        return illegalOperations;
    }

    private Dictionary<string, string> GetIllegalOperations(
        string typeName,
        Dictionary<string, object?> replaceOperations, 
        Dictionary<string, Type> writableProperties)
    {
        var illegalOperations = new Dictionary<string, string>();
        foreach (var operation in replaceOperations)
        {
            if (!writableProperties.ContainsKey(operation.Key))
            {
                illegalOperations.Add(operation.Key, $"{typeName} don't have writable property {operation.Key}");
                continue;
            }

            var propType = writableProperties[operation.Key];

            if (!CanAssign(operation.Value, propType))
            {
                if (operation.Value is null)
                {
                    illegalOperations.Add(operation.Key, 
                        $"Can't assign null-value to property {operation.Key} of type {propType} in {typeName}");
                }
                else
                {
                    illegalOperations.Add(operation.Key, 
                        $"Can't assign value '{operation.Value}' of type {operation.Value.GetType()} to property {operation.Key} of type {propType} in {typeName}");
                }
            }
        }

        return illegalOperations;
    }

    private static bool CanAssign(object? value, Type propType)
    {
        if (value == null)
        {
            // a null value can be assigned to propType if it is nullable
            return IsNullableType(propType);
        }

        var valueType = value.GetType();
        if (propType.IsAssignableFrom(valueType))
        {
            return true;
        }

        if ((propType == typeof(double) || propType == typeof(double?)) && value is int)
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
            return double.TryParse(str, out _);
        }
        if (propType == typeof(Guid) || propType == typeof(Guid?))
        {
            return Guid.TryParse(str, out _);
        }
        if (propType == typeof(DateTime) || propType == typeof(DateTime?))
        {
            return DateTime.TryParse(str, out _);
        }
        return false;
    }

    private static bool IsNullableType(Type propType)
    {
        // special case for string: when using reflection to get type of a nullable string (string?),
        // propType.IsGenericType is false. The type for a string? is still "System.String"
        if (propType == typeof(string))
        {
            return true;
        }
        return propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    private Dictionary<string, Type> CollectWritableProperties(Type type)
        => type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where( p => p.CanWrite)
            .ToDictionary(p => p.Name, p => p.PropertyType);

    private Dictionary<string, object?> CollectReplaceOperations<T>(List<Operation<T>> operations) where T : class
        => operations
            .Where(op => op.OperationType == OperationType.Replace)
            .ToDictionary(op => op.path.TrimStart('/'), op => op.value == null ? null : op.value);
}
