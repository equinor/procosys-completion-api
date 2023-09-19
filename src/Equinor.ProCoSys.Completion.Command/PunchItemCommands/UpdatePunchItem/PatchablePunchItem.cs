using System;
using System.ComponentModel.DataAnnotations;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;

public class PatchablePunchItem
{
    // need RequiredAttribute to distinguish between type of string and nullable string?
    // when using refection to check property type of string and string? both return "System.String"
    // the ideal has been to just check if type is nullable or not to determine if it's required
    [Required]
    [StringLength(PunchItem.DescriptionLengthMax, MinimumLength = PunchItem.DescriptionLengthMin)]
    public string Description { get; set; } = null!;
    [Required]
    public Guid RaisedByOrgGuid { get; set; }
    [Required]
    public Guid ClearingByOrgGuid { get; set; }

    public Guid? PriorityGuid { get; set; }
    public Guid? SortingGuid { get; set; }
    public Guid? TypeGuid { get; set; }
    public DateTime? DueDate { get; set; }
}
