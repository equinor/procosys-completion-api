namespace Equinor.ProCoSys.Completion.TieImport.Models;

public sealed record CommandReferences 
{
    public Guid ProjectGuid { get; set; }
    public Guid CheckListGuid { get; set; }
    public Guid RaisedByOrgGuid { get; set; }
    public Guid ClearedByOrgGuid { get; set; }
    public Guid? TypeGuid { get; set; }
    public ImportError[] Errors { get; set; } = [];
}
