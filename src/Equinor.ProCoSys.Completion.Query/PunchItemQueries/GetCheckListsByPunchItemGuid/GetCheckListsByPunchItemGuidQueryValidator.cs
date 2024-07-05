using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetCheckListsByPunchItemGuid;

public class GetCheckListsByPunchItemGuidQueryValidator : AbstractValidator<GetCheckListsByPunchItemGuidQuery>
{
    public GetCheckListsByPunchItemGuidQueryValidator(IPunchItemValidator punchItemValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query)
            .MustAsync((query, cancellationToken) => BeAnExistingPunchItemAsync(query.PunchItemGuid, cancellationToken))
            .WithMessage(query => $"Punch item with this guid does not exist! Guid={query.PunchItemGuid}")
            .WithState(_ => new EntityNotFoundException());

        async Task<bool> BeAnExistingPunchItemAsync(Guid punchItemGuid, CancellationToken cancellationToken)
            => await punchItemValidator.ExistsAsync(punchItemGuid, cancellationToken);
    }
}
