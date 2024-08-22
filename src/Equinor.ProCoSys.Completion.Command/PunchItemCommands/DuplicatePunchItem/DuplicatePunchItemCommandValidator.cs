using System.Linq;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DuplicatePunchItem;

public class DuplicatePunchItemCommandValidator : AbstractValidator<DuplicatePunchItemCommand>
{
    public DuplicatePunchItemCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(command => command)
            .Must(command => !command.PunchItem.Project.IsClosed)
            .WithMessage("Project is closed!")
            .Must(ExistInSameProject)
            .WithMessage("Punch item to duplicate and all check lists to copy to must be in same project!")
            .Must(NotBeInAVoidedTagForAnyCheckList)
            .WithMessage("Check lists to copy to can not belong to a voided tag!")
            ;

        bool NotBeInAVoidedTagForAnyCheckList(DuplicatePunchItemCommand command)
        {
            var voidedTagCount = command.CheckListDetailsDtos.Where(c => c.IsOwningTagVoided).Count();
            return voidedTagCount == 0;
        }

        bool ExistInSameProject(DuplicatePunchItemCommand command)
        {
            var projectGuids = command.CheckListDetailsDtos.Select(c => c.ProjectGuid).ToList();
            projectGuids.Add(command.PunchItem.Project.Guid);
            return projectGuids.Distinct().Count() == 1;
        }
    }
}
