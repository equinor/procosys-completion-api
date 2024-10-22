#nullable enable
using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public sealed record PunchItemTinyDetailsDto(
    Guid Guid,
    Guid CheckListGuid,
    string ProjectName,
    Guid ProjectGuid,
    long ItemNo,
    string Category,
    string Description,
    bool IsReadyToBeCleared,
    bool IsReadyToBeUncleared,
    bool IsReadyToBeRejected,
    bool IsReadyToBeVerified,
    bool IsReadyToBeUnverified,
    LibraryItemDto RaisedByOrg,
    LibraryItemDto ClearingByOrg,
    LibraryItemDto? Priority,
    LibraryItemDto? Sorting,
    LibraryItemDto? Type,
    PersonDto? ActionBy,
    DateTime? DueTimeUtc,
    int? Estimate,
    string? ExternalItemNo,
    bool MaterialRequired,
    DateTime? MaterialETAUtc,
    string? MaterialExternalNo,
    WorkOrderDto? WorkOrder,
    WorkOrderDto? OriginalWorkOrder,
    DocumentDto? Document,
    SWCRDto? SWCR,
    string RowVersion);
