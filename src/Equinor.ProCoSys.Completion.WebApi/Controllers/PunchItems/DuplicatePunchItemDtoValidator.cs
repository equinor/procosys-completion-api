using System.Collections.Generic;
using System;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public class DuplicatePunchItemDtoValidator : AbstractValidator<DuplicatePunchItemDto>
{
    public DuplicatePunchItemDtoValidator(IOptionsMonitor<ApplicationOptions> applicationOptions)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto).NotNull();

        RuleFor(dto => dto.CheckListGuids)
            .NotNull()
            .Must(c => c.Count > 0 && c.Count <= applicationOptions.CurrentValue.MaxDuplicatePunch)
            .WithMessage($"Number of check lists to duplicate to must be between 1 and {applicationOptions.CurrentValue.MaxDuplicatePunch}!")
            .Must(BeUniqueCheckLists)
            .WithMessage("Check lists must be unique!");

        bool BeUniqueCheckLists(IList<Guid> checkListGuids) => checkListGuids.Distinct().Count() == checkListGuids.Count;
    }
}
