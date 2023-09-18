using Equinor.ProCoSys.Completion.WebApi.InputValidators;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

public class PatchDtoValidator<T1, T2> : AbstractValidator<T1> where T1 : PatchDto<T2> where T2: class
{
    public PatchDtoValidator(IPatchOperationValidator patchOperationValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto)
            .NotNull();

        RuleFor(dto => dto.PatchDocument)
            .NotNull()
            .Must(HaveReplaceOperationsOnly)
            .WithMessage("Only 'Replace' operations are supported when patching")
            .Must(HaveUniqueReplaceOperations)
            .WithMessage("All operation paths must be unique")
            .Must(RequiredFieldsMustHaveValue)
            .WithMessage(dto => GetMessageForRequiredFields(dto.PatchDocument))
            //.Must(HaveValidRowVersionOperation)
            //.WithMessage("'RowVersion' is required and must be a valid row version")
            .Must(HaveValidOperationsOnly)
            .WithMessage(dto => GetMessageForInvalidOperations(dto.PatchDocument))
            .Must(HaveValidLengthOfStrings)
            .WithMessage(dto => GetMessageForInvalidLengthOfStrings(dto.PatchDocument));

        bool HaveReplaceOperationsOnly(JsonPatchDocument<T2> doc)
            => patchOperationValidator.HaveReplaceOperationsOnly(doc.Operations);

        bool HaveUniqueReplaceOperations(JsonPatchDocument<T2> doc)
            => patchOperationValidator.HaveUniqueReplaceOperations(doc.Operations);

        //bool HaveValidRowVersionOperation(JsonPatchDocument<T2> doc)
        //    => patchOperationValidator.HaveValidRowVersionOperation(doc.Operations);

        string? GetMessageForInvalidOperations(JsonPatchDocument<T2> doc)
            => patchOperationValidator.GetMessageForInvalidReplaceOperations(doc.Operations);

        bool HaveValidOperationsOnly(JsonPatchDocument<T2> doc)
            => patchOperationValidator.HaveValidReplaceOperationsOnly(doc.Operations);

        bool RequiredFieldsMustHaveValue(JsonPatchDocument<T2> doc)
            => patchOperationValidator.AllRequiredFieldsHaveValue(doc.Operations);

        string? GetMessageForRequiredFields(JsonPatchDocument<T2> doc)
            => patchOperationValidator.GetMessageForRequiredFields(doc.Operations);

        bool HaveValidLengthOfStrings(JsonPatchDocument<T2> doc)
            => patchOperationValidator.HaveValidLengthOfStrings(doc.Operations);

        string? GetMessageForInvalidLengthOfStrings(JsonPatchDocument<T2> doc)
            => patchOperationValidator.GetMessageForInvalidLengthOfStrings(doc.Operations);
    }
}
