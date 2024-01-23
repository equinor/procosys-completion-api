using System;

namespace Equinor.ProCoSys.Completion.MessageContracts.PunchItem;

public interface IPunchItem
{
    string Plant { get; }
    Guid ProjectGuid { get; }
    string ProjectName { get; }
    string ProjectDescription { get; }
    Guid CheckListGuid { get; }
    string Category { get; }
    int ItemNo { get; }
    string Description { get; }
    string RaisedByOrgCode { get; }
    Guid RaisedByOrgGuid { get; }
    string ClearingByOrgCode { get; }
    Guid ClearingByOrgGuid { get; }
    string? SortingCode { get; }
    Guid? SortingGuid { get; }
    string? TypeCode { get; }
    Guid? TypeGuid { get; }
    string? PriorityCode { get; }
    Guid? PriorityGuid { get; }
    DateTime? DueTimeUtc { get; }
    int? Estimate { get; }
    string? ExternalItemNo { get; }
    bool MaterialRequired { get; }
    DateTime? MaterialETAUtc { get; }
    string? MaterialExternalNo { get; }
    string? WorkOrderNo { get; }
    Guid? WorkOrderGuid { get; }
    string? OriginalWorkOrderNo { get; }
    Guid? OriginalWorkOrderGuid { get; }
    string? DocumentNo { get; }
    Guid? DocumentGuid { get; }
    int? SWCRNo { get; }
    Guid? SWCRGuid { get; }
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
