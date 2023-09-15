using System.Linq;
using Equinor.ProCoSys.Completion.WebApi.InputValidators;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

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
            .Must(HaveValidRowVersionOperation)
            .WithMessage("'RowVersion' is required and must be a valid row version")
            .Must(HaveValidOperationsOnly)
            .WithMessage(dto => GetMessageForIllegalOperations(dto.PatchDocument));

        bool HaveReplaceOperationsOnly(JsonPatchDocument<T2> doc)
            => patchOperationValidator.HaveReplaceOperationsOnly(doc.Operations);

        bool HaveUniqueReplaceOperations(JsonPatchDocument<T2> doc)
            => patchOperationValidator.HaveUniqueReplaceOperations(doc.Operations);

        bool HaveValidRowVersionOperation(JsonPatchDocument<T2> doc)
            => patchOperationValidator.HaveValidRowVersionOperation(doc.Operations);

        string? GetMessageForIllegalOperations(JsonPatchDocument<T2> doc)
            => patchOperationValidator.GetMessageForIllegalReplaceOperations(doc.Operations);

        bool HaveValidOperationsOnly(JsonPatchDocument<T2> doc)
            => patchOperationValidator.HaveValidReplaceOperationsOnly(doc.Operations);
    }

    protected bool HaveStringReplaceOperationWithMaxLength(JsonPatchDocument<T2> doc, string path, int lengthMax)
    {
        var operation = doc.Operations
            .SingleOrDefault(op => op.path == $"/{path}" &&
                                   op.OperationType == OperationType.Replace);
        if (operation is null)
        {
            return false;
        }

        if (operation.value is not string str)
        {
            return false;
        }

        var l = str.Length;
        return l > 0 && l < lengthMax;
    }

    protected bool ReplaceOperationExistsFor(JsonPatchDocument<T2> doc, string path)
        => doc.Operations
            .SingleOrDefault(op => op.path == $"/{path}" &&
                                   op.OperationType == OperationType.Replace) is not null;
}
