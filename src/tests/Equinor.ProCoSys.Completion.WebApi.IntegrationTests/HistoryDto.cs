using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public record HistoryDto(
    Guid? EventForParentGuid,
    Guid EventForGuid,
    Guid EventByOid,
    DateTime EventAtUtc,
    string EventDisplayName,
    string EventByFullName,
    List<PropertyDto> Properties,
    string RowVersion);

