using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.WebApi.InputValidators;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public class PatchPunchItemDtoValidator : PatchDtoValidator<PatchPunchItemDto, PatchablePunchItem>
{
    public PatchPunchItemDtoValidator(IPatchOperationValidator patchOperationValidator)
        : base(patchOperationValidator)
    {
    }
}
