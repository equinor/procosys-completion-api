using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsInProject;

public class GetPunchItemsInProjectQueryValidator : AbstractValidator<GetPunchItemsInProjectQuery>
{
    public GetPunchItemsInProjectQueryValidator(IProjectValidator projectValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        // todo add unit tests
        RuleFor(command => command)
            // validate given Project
            .MustAsync(BeAnExistingProjectAsync)
            .WithMessage(command => $"Project does not exist! Guid={command.ProjectGuid}");

        async Task<bool> BeAnExistingProjectAsync(GetPunchItemsInProjectQuery query, CancellationToken cancellationToken)
            => await projectValidator.ExistsAsync(query.ProjectGuid, cancellationToken);
    }
}
