using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemLink;

public class CreatePunchItemLinkCommandValidator : AbstractValidator<CreatePunchItemLinkCommand>
{
    public CreatePunchItemLinkCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .Must(command => !command.PunchItem.Project.IsClosed)
            .WithMessage("Project is closed!")
            .Must(command => !command.CheckListDetailsDto.IsOwningTagVoided)
            .WithMessage("Tag owning punch item is voided!")
            .Must(command => !command.PunchItem.IsCleared)
            .WithMessage(command => $"Punch item links can't be added. Punch item is cleared! Guid={command.PunchItemGuid}");
    }
}
