using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnverifyPunchItem;

public class UnverifyPunchItemCommandValidator : AbstractValidator<UnverifyPunchItemCommand>
{
    public UnverifyPunchItemCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .Must(command => !command.PunchItem.Project.IsClosed)
            .WithMessage("Project is closed!")
            .Must(command => !command.CheckListDetailsDto.IsOwningTagVoided)
            .WithMessage("Tag owning punch item is voided!")
            .Must(command => command.PunchItem.IsVerified)
            .WithMessage(command => $"Punch item can not be unverified. The punch item is not verified! Guid={command.PunchItemGuid}");
    }
}
