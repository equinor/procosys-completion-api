using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using FluentValidation;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

public class PatchDtoValidator<T> : AbstractValidator<T> where T : PatchDto
{
    public PatchDtoValidator(IRowVersionValidator rowVersionValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto)
            .NotNull();

        RuleFor(dto => dto.PatchDocument)
            .NotNull()
            .Must(HaveReplaceOperationsOnly)
            .WithMessage("Only 'Replace' operations are supported when patching")
            .Must(HaveUniqueOperations)
            .WithMessage("All operation paths must be unique")
            .Must(HaveValidRowVersionOperation)
            .WithMessage($"'{nameof(PunchItem.RowVersion)}' is required and must be a valid row version");

        bool HaveValidRowVersionOperation(JsonPatchDocument doc)
        {
            var rowVersionOperation = doc.Operations
                .SingleOrDefault(op => op.path == $"/{nameof(PunchItem.RowVersion)}" && 
                                      op.OperationType == OperationType.Replace);
            if (rowVersionOperation?.value == null)
            {
                return false;
            }

            return rowVersionValidator.IsValid(rowVersionOperation.value as string);
        }

        bool HaveReplaceOperationsOnly(JsonPatchDocument doc)
            => doc.Operations.All(o => o.OperationType == OperationType.Replace);

        bool HaveUniqueOperations(JsonPatchDocument doc)
        {
            var allPaths = doc.Operations.Select(o => o.path).ToList();
            return allPaths.Count == allPaths.Distinct().Count();
        }
    }

    protected bool HaveStringReplaceOperationWithMaxLength(JsonPatchDocument doc, string path, int lengthMax)
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

    protected bool ReplaceOperationExistsFor(JsonPatchDocument doc, string path)
        => doc.Operations
            .SingleOrDefault(op => op.path == $"/{path}" &&
                                   op.OperationType == OperationType.Replace) is not null;
}
