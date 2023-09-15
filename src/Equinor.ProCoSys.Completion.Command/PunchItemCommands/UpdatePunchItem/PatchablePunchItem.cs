using System;
using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

public class PatchablePunchItem
{
    [Required]
    public string Description { get; set; } = null!;
    [Required]
    public Guid RaisedByOrgGuid { get; set; }
    [Required]
    public string RowVersion { get; set; } = null!;

    public DateTime? DueDate { get; set; }
    public Guid? PriorityGuid { get; set; }
}
