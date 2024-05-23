using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.HistoryAggregate;

namespace Equinor.ProCoSys.Completion.Query.History;

public record HistoryDto(
    Guid? EventForParentGuid,
    Guid EventForGuid,
    Guid EventByOid,
    DateTime EventAtUtc,
    string EventDisplayName,
    string EventByFullName,
    List<PropertyDto> Properties,
    string RowVersion);

