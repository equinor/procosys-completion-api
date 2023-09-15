using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.WebApi.InputValidators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public class PatchPunchItemDtoValidator : PatchDtoValidator<PatchPunchItemDto, PatchablePunchItem>
{
    public PatchPunchItemDtoValidator(IPatchOperationValidator patchOperationValidator)
        : base(patchOperationValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto)
            .NotNull();

        RuleFor(dto => dto.PatchDocument)
            .NotNull()
            .Must(doc => HaveStringReplaceOperationWithMaxLength(
                doc, 
                nameof(PunchItem.Description), 
                PunchItem.DescriptionLengthMax))
            .WithMessage(
                $"'{nameof(PunchItem.Description)}' is required as string and max length is {PunchItem.DescriptionLengthMax}")
            .When(dto => ReplaceOperationExistsFor(
                dto.PatchDocument, 
                nameof(PunchItem.Description)), 
                ApplyConditionTo.CurrentValidator);
    }
}
