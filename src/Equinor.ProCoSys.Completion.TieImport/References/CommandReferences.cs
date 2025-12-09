using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ImportPunch;

namespace Equinor.ProCoSys.Completion.TieImport.References;

public sealed record CommandReferences 
{
    public Guid ProjectGuid { get; set; }
    public Guid CheckListGuid { get; set; }
    public Guid RaisedByOrgGuid { get; set; }
    public Guid ClearedByOrgGuid { get; set; }
    public Guid? TypeGuid { get; set; }
    public Guid? PriorityGuid { get; set; }
    public Guid? SortingGuid { get; set; }
    
    // References for related entities
    public Guid? WorkOrderGuid { get; set; }
    public Guid? OriginalWorkOrderGuid { get; set; }
    public Guid? DocumentGuid { get; set; }
    public Guid? SWCRGuid { get; set; }
    
    // Person references
    public Guid? ActionByPersonOid { get; set; }
    
    // Action by person with date (for status transitions)
    public ActionByPerson? ClearedBy { get; set; }
    public ActionByPerson? VerifiedBy { get; set; }
    public ActionByPerson? RejectedBy { get; set; }

    public ImportError[] Errors { get; set; } = [];
}
