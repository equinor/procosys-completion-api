using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.Validators.LibraryItemValidators;
using Equinor.ProCoSys.Completion.Command.Validators.ProjectValidators;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;

public class CreatePunchItemCommandValidator : AbstractValidator<CreatePunchItemCommand>
{
    public CreatePunchItemCommandValidator(
        IProjectValidator projectValidator,
        ILibraryItemValidator libraryItemValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            // validate given Project
            .MustAsync(BeAnExistingProjectAsync)
            .WithMessage(command => $"Project does not exist! Guid={command.ProjectGuid}")
            .MustAsync(NotBeAClosedProjectAsync)
            .WithMessage(command => $"Project is closed! Guid={command.ProjectGuid}")

            // validate given RaisedByOrg
            .MustAsync((command, cancellationToken)
                => BeAnExistingLibraryItemAsync(command.RaisedByOrgGuid, cancellationToken))
            .WithMessage(command
                => $"Library item does not exist! Guid={command.RaisedByOrgGuid}")
            .MustAsync((command, cancellationToken)
                => NotBeAVoidedLibraryItemAsync(command.RaisedByOrgGuid, cancellationToken))
            .WithMessage(command
                => $"Library item is voided! Guid={command.RaisedByOrgGuid}")
            .MustAsync((command, cancellationToken)
                => BeALibraryItemOfTypeAsync(
                    command.RaisedByOrgGuid, 
                    LibraryType.COMPLETION_ORGANIZATION,
                    cancellationToken))
            .WithMessage(command =>
                $"Library item is not a {LibraryType.COMPLETION_ORGANIZATION}! " +
                $"Guid={command.RaisedByOrgGuid}")

            // validate given ClearingByOrg
            .MustAsync((command, cancellationToken)
                => BeAnExistingLibraryItemAsync(command.ClearingByOrgGuid, cancellationToken))
            .WithMessage(command
                => $"Library item does not exist! Guid={command.ClearingByOrgGuid}")
            .MustAsync((command, cancellationToken)
                => NotBeAVoidedLibraryItemAsync(command.ClearingByOrgGuid, cancellationToken))
            .WithMessage(command
                => $"Library item is voided! Guid={command.ClearingByOrgGuid}")
            .MustAsync((command, cancellationToken)
                => BeALibraryItemOfTypeAsync(
                    command.ClearingByOrgGuid,
                    LibraryType.COMPLETION_ORGANIZATION,
                    cancellationToken))
            .WithMessage(command =>
                $"Library item is not a {LibraryType.COMPLETION_ORGANIZATION}! " +
                $"Guid={command.ClearingByOrgGuid}");

        async Task<bool> BeAnExistingProjectAsync(CreatePunchItemCommand command, CancellationToken cancellationToken)
            => await projectValidator.ExistsAsync(command.ProjectGuid, cancellationToken);

        async Task<bool> NotBeAClosedProjectAsync(CreatePunchItemCommand command, CancellationToken cancellationToken)
            => !await projectValidator.IsClosedAsync(command.ProjectGuid, cancellationToken);

        async Task<bool> BeAnExistingLibraryItemAsync(Guid guid, CancellationToken cancellationToken)
            => await libraryItemValidator.ExistsAsync(guid, cancellationToken);

        async Task<bool> BeALibraryItemOfTypeAsync(Guid guid, LibraryType type, CancellationToken cancellationToken)
            => await libraryItemValidator.HasTypeAsync(guid, type, cancellationToken);

        async Task<bool> NotBeAVoidedLibraryItemAsync(Guid guid, CancellationToken cancellationToken)
            => !await libraryItemValidator.IsVoidedAsync(guid, cancellationToken);
    }
}
