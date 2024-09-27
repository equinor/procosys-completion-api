using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Query.CheckListQueries.GetPunchItems;

public class GetPunchItemsQueryValidator : AbstractValidator<GetPunchItemsQuery>
{
    public GetPunchItemsQueryValidator(IProjectValidator projectValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query)
            .MustAsync(BeAnExistingProjectAsync)
            .WithMessage(query =>
                $"Project with this guid does not exist! Guid={query.CheckListDetailsDto.ProjectGuid}")
            .WithState(_ => new EntityNotFoundException())
            .MustAsync(NotBeAClosedProjectAsync)
            .WithMessage(query =>
                $"Project is closed. Punch items are not available in closed projects! Project Guid={query.CheckListDetailsDto.ProjectGuid}");

        async Task<bool> BeAnExistingProjectAsync(GetPunchItemsQuery query, CancellationToken cancellationToken)
            => await projectValidator.ExistsAsync(query.CheckListDetailsDto.ProjectGuid, cancellationToken);

        async Task<bool> NotBeAClosedProjectAsync(GetPunchItemsQuery query, CancellationToken cancellationToken)
            => !await projectValidator.IsClosedAsync(query.CheckListDetailsDto.ProjectGuid, cancellationToken);
    }
}
