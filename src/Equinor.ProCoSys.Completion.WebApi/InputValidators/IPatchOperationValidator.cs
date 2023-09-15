using System.Collections.Generic;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Equinor.ProCoSys.Completion.WebApi.InputValidators;

public interface IPatchOperationValidator
{
    bool HaveValidReplaceOperationsOnly<T>(List<Operation<T>> docOperations) where T : class;
    string? GetMessageForIllegalReplaceOperations<T>(List<Operation<T>> operations) where T : class;
    bool HaveReplaceOperationsOnly<T>(List<Operation<T>> operations) where T : class;
    bool HaveUniqueReplaceOperations<T>(List<Operation<T>> operations) where T : class;
    bool HaveValidRowVersionOperation<T2>(List<Operation<T2>> operations) where T2 : class;
}
