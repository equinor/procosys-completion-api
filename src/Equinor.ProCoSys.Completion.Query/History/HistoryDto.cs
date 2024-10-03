using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.Query.History;

public sealed record HistoryDto(
    Guid? EventForParentGuid,
    Guid EventForGuid,
    Guid EventByOid,
    DateTime EventAtUtc,
    string EventDisplayName,
    string EventByFullName,
    List<PropertyDto> Properties,
    string RowVersion)
{
    public override string ToString() => $"HistoryDto(EventByOid = \"{EventByOid}\", EventForGuid = \"{EventForGuid}\", EventForParentGuid = \"{EventForParentGuid}\")";
}

