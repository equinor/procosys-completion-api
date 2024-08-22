using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnclearPunchItem;

public class UnclearPunchItemCommandValidator : AbstractValidator<UnclearPunchItemCommand>
{
    public UnclearPunchItemCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .Must(command => !command.PunchItem.Project.IsClosed)
            .WithMessage("Project is closed!")
            .Must(command => !command.CheckListDetailsDto.IsOwningTagVoided)
            .WithMessage("Tag owning punch item is voided!")
            .Must(command => command.PunchItem.IsCleared)
            .WithMessage(command => $"Punch item can not be uncleared. The punch item is not cleared! Guid={command.PunchItemGuid}")
            .Must(command => !command.PunchItem.IsVerified)
            .WithMessage(command => $"Punch item can not be uncleared. The punch item is verified! Guid={command.PunchItemGuid}");
    }
}
