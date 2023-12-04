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
    User? ActionBy { get; }
    User? ClearedBy { get; }
    DateTime? ClearedAtUtc { get; }
    User? RejectedBy { get; }
    DateTime? RejectedAtUtc { get; }
    User? VerifiedBy { get; }
    DateTime? VerifiedAtUtc { get; }
    User CreatedBy { get; }
    DateTime CreatedAtUtc { get; }
}
