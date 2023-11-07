using System.Collections.Generic;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

public interface IPatchOperationInputValidator
{
    bool HaveValidReplaceOperationsOnly<T>(List<Operation<T>> operations) where T : class;
    string? GetMessageForInvalidReplaceOperations<T>(List<Operation<T>> operations) where T : class;
    bool HaveReplaceOperationsOnly<T>(List<Operation<T>> operations) where T : class;
    bool HaveUniqueReplaceOperations<T>(List<Operation<T>> operations) where T : class;
    bool AllRequiredFieldsHaveValue<T>(List<Operation<T>> operations) where T : class;
    string? GetMessageForRequiredFields<T>(List<Operation<T>> operations) where T : class;
    bool HaveValidLengthOfStrings<T>(List<Operation<T>> operations) where T : class;
    string? GetMessageForInvalidLengthOfStrings<T>(List<Operation<T>> operations) where T : class;
}
