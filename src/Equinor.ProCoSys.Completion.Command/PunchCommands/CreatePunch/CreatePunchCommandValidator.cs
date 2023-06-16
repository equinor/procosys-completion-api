using System.Threading.Tasks;
using System.Threading;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunch;

public class CreatePunchCommandValidator : AbstractValidator<CreatePunchCommand>
{
    public CreatePunchCommandValidator(IProjectValidator projectValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .MustAsync(BeAnExistingProjectAsync)
            .WithMessage(command => $"Project with this name does not exist! Guid={command.ProjectName}")
            .MustAsync(NotBeAClosedProjectAsync)
            .WithMessage("Project is closed!");

        async Task<bool> BeAnExistingProjectAsync(CreatePunchCommand command, CancellationToken cancellationToken)
            => await projectValidator.ExistsAsync(command.ProjectName, cancellationToken);

        async Task<bool> NotBeAClosedProjectAsync(CreatePunchCommand command, CancellationToken cancellationToken)
            => !await projectValidator.IsClosed(command.ProjectName, cancellationToken);
    }
}
