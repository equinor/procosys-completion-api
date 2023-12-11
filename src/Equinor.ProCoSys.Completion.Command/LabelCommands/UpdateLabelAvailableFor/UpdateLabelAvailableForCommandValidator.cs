using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.LabelCommands.UpdateLabelAvailableFor;

public class UpdateLabelAvailableForCommandValidator : AbstractValidator<UpdateLabelAvailableForCommand>
{
    public UpdateLabelAvailableForCommandValidator(
        ILabelValidator labelValidator,
        ILabelEntityValidator labelEntityValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync((command, cancellationToken) => BeAnExistingLabelAsync(command.Text, cancellationToken))
            .WithMessage(command => $"Label does not exist! Guid={command.Text}");

        RuleForEach(command => command.AvailableFor)
            .MustAsync((_, entityType, _, cancellationToken)
                => BeAnExistingLabelEntityAsync(entityType, cancellationToken))
            .WithMessage((_, entityType) => $"Label entity does not exist! Type={entityType}");

        async Task<bool> BeAnExistingLabelAsync(string labelGuid, CancellationToken cancellationToken)
            => await labelValidator.ExistsAsync(labelGuid, cancellationToken);

        async Task<bool> BeAnExistingLabelEntityAsync(
            EntityTypeWithLabels entityType,
            CancellationToken cancellationToken)
            => await labelEntityValidator.ExistsAsync(entityType, cancellationToken);
    }
}
