using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsByCheckList;

public class GetPunchItemsByCheckListGuidQueryValidator : AbstractValidator<GetPunchItemsByCheckListGuidQuery>
{
    public GetPunchItemsByCheckListGuidQueryValidator(ICheckListValidator checkListValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query)
            
            // validate given CheckList
            .MustAsync(BeAnExistingCheckListAsync)
            .WithMessage(query => $"CheckList does not exist with Guid '{query.CheckListGuid}'")
            .WithState(_ => new EntityNotFoundException());
        async Task<bool> BeAnExistingCheckListAsync(GetPunchItemsByCheckListGuidQuery query, CancellationToken cancellationToken)
            => await checkListValidator.ExistsAsync(query.CheckListGuid, cancellationToken);
    }
}
