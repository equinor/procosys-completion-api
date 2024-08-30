using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Query.ProjectQueries.GetPunchItems;

public class GetPunchItemsQueryValidator : AbstractValidator<GetPunchItemsQuery>
{
    public GetPunchItemsQueryValidator(IProjectValidator projectValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query)
            .MustAsync(BeAnExistingProjectAsync)
            .WithMessage(query => $"Project with this guid does not exist! Guid={query.ProjectGuid}")
            .WithState(_ => new EntityNotFoundException());

        async Task<bool> BeAnExistingProjectAsync(GetPunchItemsQuery query, CancellationToken cancellationToken)
            => await projectValidator.ExistsAsync(query.ProjectGuid, cancellationToken);
    }
}
