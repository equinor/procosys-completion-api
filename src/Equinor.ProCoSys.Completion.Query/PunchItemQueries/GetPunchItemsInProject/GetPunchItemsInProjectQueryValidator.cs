using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsInProject;

public class GetPunchItemsInProjectQueryValidator : AbstractValidator<GetPunchItemsInProjectQuery>
{
    public GetPunchItemsInProjectQueryValidator(IProjectValidator projectValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query)
            .MustAsync(BeAnExistingProjectAsync)
            .WithMessage(query => $"Project with this guid does not exist! Guid={query.ProjectGuid}")
            .WithState(_ => new EntityNotFoundException());

        async Task<bool> BeAnExistingProjectAsync(GetPunchItemsInProjectQuery query, CancellationToken cancellationToken)
            => await projectValidator.ExistsAsync(query.ProjectGuid, cancellationToken);
    }
}
