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
            .WithMessage("Check lists to copy to can not belong to a voided tag!");

        bool NotBeInAVoidedTagForAnyCheckList(DuplicatePunchItemCommand command)
            => command.CheckListDetailsDtoList.Count(c => c.IsOwningTagVoided) == 0;

        bool ExistInSameProject(DuplicatePunchItemCommand command)
        {
            var punchItemProjectGuid = command.PunchItem.Project.Guid;
            return command.CheckListDetailsDtoList.All(c => c.ProjectGuid == punchItemProjectGuid);
        }
    }
}
