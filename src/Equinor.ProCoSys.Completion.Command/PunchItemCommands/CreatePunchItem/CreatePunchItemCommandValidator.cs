using System.Threading.Tasks;
using System.Threading;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;

public class CreatePunchItemCommandValidator : AbstractValidator<CreatePunchItemCommand>
{
    public CreatePunchItemCommandValidator(IProjectValidator projectValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync(BeAnExistingProjectAsync)
            .WithMessage(command => $"Project with this Guid does not exist! Guid={command.ProjectGuid}")
            .MustAsync(NotBeAClosedProjectAsync)
            .WithMessage("Project is closed!");

        async Task<bool> BeAnExistingProjectAsync(CreatePunchItemCommand command, CancellationToken cancellationToken)
            => await projectValidator.ExistsAsync(command.ProjectGuid, cancellationToken);

        async Task<bool> NotBeAClosedProjectAsync(CreatePunchItemCommand command, CancellationToken cancellationToken)
            => !await projectValidator.IsClosedAsync(command.ProjectGuid, cancellationToken);
    }
}
