using System.Collections.Generic;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Equinor.ProCoSys.Completion.WebApi.InputValidators;

public interface IPatchOperationValidator
{
    bool HaveValidReplaceOperationsOnly<T>(List<Operation<T>> docOperations) where T : class;
    string? GetMessageForIllegalReplaceOperations<T>(List<Operation<T>> operations) where T : class;
    bool HaveReplaceOperationsOnly<T>(List<Operation<T>> operations) where T : class;
    bool HaveUniqueReplaceOperations<T>(List<Operation<T>> operations) where T : class;
    bool HaveValidRowVersionOperation<T>(List<Operation<T>> operations) where T : class;
    bool AllRequiredFieldsHaveValue<T>(List<Operation<T>> operations) where T : class;
    string? GetMessageForRequiredFields<T>(List<Operation<T>> operations) where T : class;
}
