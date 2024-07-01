using System;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsByCheckList;

public class GetPunchItemsByCheckListGuidQueryValidator : AbstractValidator<GetPunchItemsByCheckListGuidQuery>
{
    public GetPunchItemsByCheckListGuidQueryValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query)
            .Must((query, cancellationToken) => IsValidGuid(query.CheckListGuid))
            .WithMessage(query => $"CheckListGuid is not valid! Guid={query.CheckListGuid}")
            .WithState(_ => new ArgumentException());
    }

    private static bool IsValidGuid(Guid checkListGuid) => (Guid.Empty != checkListGuid);
}
