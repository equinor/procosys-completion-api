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
    IUser? ActionBy { get; }
    IUser? ClearedBy { get; }
    DateTime? ClearedAtUtc { get; }
    IUser? RejectedBy { get; }
    DateTime? RejectedAtUtc { get; }
    IUser? VerifiedBy { get; }
    DateTime? VerifiedAtUtc { get; }
    IUser CreatedBy { get; }
    DateTime CreatedAtUtc { get; }
}
