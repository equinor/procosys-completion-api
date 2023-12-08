using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.LabelCommands.CreateLabel;

public class CreateLabelCommandValidator : AbstractValidator<CreateLabelCommand>
{
    public CreateLabelCommandValidator(ILabelValidator labelValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => NotBeAnExistingLabelAsync(command.Label, cancellationToken))
            .WithMessage(command => $"Label already exist! Label={command.Label}");

        async Task<bool> NotBeAnExistingLabelAsync(string labelGuid, CancellationToken cancellationToken)
            => !await labelValidator.ExistsAsync(labelGuid, cancellationToken);
    }
}
