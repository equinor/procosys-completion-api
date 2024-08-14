using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;

public class DeletePunchItemCommandValidator : AbstractValidator<DeletePunchItemCommand>
{
    public DeletePunchItemCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .Must(command => !command.PunchItem.Project.IsClosed)
            .WithMessage("Project is closed!")
            .Must(command => !command.PunchItem.IsCleared)
            .WithMessage(command => $"Punch item is cleared! Guid={command.PunchItemGuid}");
    }
}
