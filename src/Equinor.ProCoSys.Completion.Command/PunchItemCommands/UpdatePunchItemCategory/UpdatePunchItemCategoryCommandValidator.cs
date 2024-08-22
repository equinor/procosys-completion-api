using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;

public class UpdatePunchItemCategoryCommandValidator : AbstractValidator<UpdatePunchItemCategoryCommand>
{
    public UpdatePunchItemCategoryCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .Must(command => !command.PunchItem.Project.IsClosed)
            .WithMessage("Project is closed!")
            .Must(command => !command.CheckListDetailsDto.IsOwningTagVoided)
            .WithMessage("Tag owning punch item is voided!")
            .Must(command => command.PunchItem.Category != command.Category)
            .WithMessage(command => $"Punch item already have category {command.Category}! Guid={command.PunchItemGuid}")
            .Must(command => !command.PunchItem.IsCleared)
            .WithMessage(command => $"Punch item is cleared! Guid={command.PunchItemGuid}");
    }
}
