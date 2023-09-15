using System;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

public class PatchablePunchItem
{
    public string Description { get; set; } = null!;
    public Guid RaisedByOrgGuid { get; set; }
    public string RowVersion { get; set; } = null!;

    public DateTime? DueDate { get; set; }
    public Guid? PriorityGuid { get; set; }
}
