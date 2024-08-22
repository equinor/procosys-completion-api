using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;

public class CreatePunchItemCommentCommandValidator : AbstractValidator<CreatePunchItemCommentCommand>
{
    public CreatePunchItemCommentCommandValidator(ILabelValidator labelValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .Must(command => !command.PunchItem.Project.IsClosed)
            .WithMessage("Project is closed!")
            .Must(command => !command.CheckListDetailsDto.IsOwningTagVoided)
            .WithMessage("Tag owning punch item is voided!")
            .Must(command => !command.PunchItem.IsVerified)
            .WithMessage(command => $"Punch item comments can't be added. Punch item is verified! Guid={command.PunchItemGuid}");

        RuleForEach(command => command.Labels)
            .MustAsync((_, label, _, token) => BeAnExistingLabelAsync(label, token))
            .WithMessage((_, label) => $"Label doesn't exist! Label={label}")
            .MustAsync((_, label, _, token) => NotBeAVoidedLabelAsync(label, token))
            .WithMessage((_, label) => $"Label is voided! Label={label}");

        async Task<bool> BeAnExistingLabelAsync(string label, CancellationToken cancellationToken)
            => await labelValidator.ExistsAsync(label, cancellationToken);

        async Task<bool> NotBeAVoidedLabelAsync(string label, CancellationToken cancellationToken)
            => !await labelValidator.IsVoidedAsync(label, cancellationToken);
    }
}
