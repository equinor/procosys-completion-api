using System.ComponentModel.DataAnnotations;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public class PatchPunchItemDto : PatchDto<PatchablePunchItem>
{
    [Required] 
    public string RowVersion { get; set; } = null!;
}
