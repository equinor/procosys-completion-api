using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Equinor.ProCoSys.Completion.Command;

public static class OperationListExtension
{
    public static Operation<T>? GetReplaceOperation<T>(this List<Operation<T>> operations, string propName) where T : class
    {
        var operation = operations.SingleOrDefault(op =>
            op.OperationType == OperationType.Replace && op.path == $"/{propName}");
        return operation;
    }
}
