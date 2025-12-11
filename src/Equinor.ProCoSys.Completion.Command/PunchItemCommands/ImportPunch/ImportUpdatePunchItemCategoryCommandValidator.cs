using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportPunch;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportPunchItem;

public class ImportUpdatePunchItemCategoryCommandValidator : AbstractValidator<ImportUpdatePunchItemCommand>
{
    public ImportUpdatePunchItemCategoryCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        // Only validate category rules when Category is being changed
        When(command => command.Category.HasValue, () =>
        {
            RuleFor(command => command)
                .Must(command => !command.PunchItem.Project.IsClosed)
                .WithMessage("Project is closed!")
                .Must(command => !command.CheckListDetailsDto.IsOwningTagVoided)
                .WithMessage("Tag owning punch item is voided!")
                .Must(command => command.PunchItem.Category != command.Category)
                .WithMessage(command => $"Punch item already have category {command.Category}! Guid={command.PunchItemGuid}")
                .Must(command => !command.PunchItem.IsCleared)
                .WithMessage(command => $"Punch item is cleared! Guid={command.PunchItemGuid}");
        });
    }
}
