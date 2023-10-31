using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.PunchItem;

public interface IPunchItem
{
    Guid ProjectGuid { get; }
    string ProjectName { get; }
    string ProjectDescription { get; }
    Guid CheckListGuid { get; }
    string Category { get; }
    int ItemNo { get; }
    string Description { get; }
    string RaisedByOrgCode { get; }
    string ClearingByOrgCode { get; }
    string? SortingCode { get; }
    string? TypeCode { get; }
    string? PriorityCode { get; }
    DateTime? DueTimeUtc { get; }
    int? Estimate { get; }
    string? ExternalItemNo { get; }
    bool MaterialRequired { get; }
    DateTime? MaterialETAUtc { get; }
    string? MaterialExternalNo { get; }
    string? WorkOrderNo { get; }
    string? OriginalWorkOrderNo { get; }
    string? DocumentNo { get; }
    int? SWCRNo { get; }
    Guid? ActionByOid { get; }
    Guid? ClearedByOid { get; }
    DateTime? ClearedAtUtc { get; }
    Guid? RejectedByOid { get; }
    DateTime? RejectedAtUtc { get; }
    Guid? VerifiedByOid { get; }
    DateTime? VerifiedAtUtc { get; }
    Guid CreatedByOid { get; }
    DateTime CreatedAtUtc { get; }
}
