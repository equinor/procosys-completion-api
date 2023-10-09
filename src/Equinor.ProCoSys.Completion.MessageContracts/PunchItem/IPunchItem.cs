using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.PunchItem;

public interface IPunchItem
{
    Guid ProjectGuid { get; }
    string ProjectName { get; }
    string ProjectDescription { get; }
    int ItemNo { get; }
    Guid? ClearedByOid { get; }
    DateTime? ClearedAtUtc { get; }
    Guid? RejectedByOid { get; }
    DateTime? RejectedAtUtc { get; }
    Guid? VerifiedByOid { get; }
    DateTime? VerifiedAtUtc { get; }
}
