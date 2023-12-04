using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public class PatchPunchItemDtoValidator : PatchDtoValidator<PatchPunchItemDto, PatchablePunchItem>
{
    public PatchPunchItemDtoValidator(
        IRowVersionInputValidator rowVersionValidator,
        IPatchOperationInputValidator patchOperationValidator)
        : base(patchOperationValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto).NotNull();

        RuleFor(dto => dto.RowVersion)
            .NotNull()
            .Must(HaveValidRowVersion)
            .WithMessage(dto => $"Dto does not have valid rowVersion! RowVersion={dto.RowVersion}");

        bool HaveValidRowVersion(string rowVersion)
            => rowVersionValidator.IsValid(rowVersion);
    }
}
