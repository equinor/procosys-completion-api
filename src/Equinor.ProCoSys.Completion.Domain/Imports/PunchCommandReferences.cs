using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Imports;

public sealed record CreatePunchReferences : ICommandReferences
{
    public Guid ProjectGuid { get; set; }
    public Guid CheckListGuid { get; set; }
    public Guid RaisedByOrgGuid { get; set; }
    public Guid ClearedByOrgGuid { get; set; }
    public Guid? TypeGuid { get; set; }
    public Optional<ActionByPerson?> ClearedBy { get; set; }
    public Optional<ActionByPerson?> VerifiedBy { get; set; }
    public Optional<ActionByPerson?> RejectedBy { get; set; }
    public ImportError[] Errors { get; set; } = [];
}

public sealed record UpdatePunchReferences : ICommandReferences
{
    public Guid ProjectGuid { get; set; }
    public Guid CheckListGuid { get; set; }
    public Guid RaisedByOrgGuid { get; set; }
    public Guid ClearedByOrgGuid { get; set; }
    public Guid? TypeGuid { get; set; }
    public Optional<ActionByPerson?> ClearedBy { get; set; }
    public Optional<ActionByPerson?> VerifiedBy { get; set; }
    public Optional<ActionByPerson?> RejectedBy { get; set; }
    public ImportError[] Errors { get; set; } = [];
    public PunchItem? PunchItem { get; set; }
}

public interface ICommandReferences
{
    public Guid ProjectGuid { get; set; }
    public Guid CheckListGuid { get; set; }
    public Guid RaisedByOrgGuid { get; set; }
    public Guid ClearedByOrgGuid { get; set; }
    public Guid? TypeGuid { get; set; }
    public Optional<ActionByPerson?> ClearedBy { get; set; }
    public Optional<ActionByPerson?> VerifiedBy { get; set; }
    public Optional<ActionByPerson?> RejectedBy { get; set; }
    public ImportError[] Errors { get; set; }
}
