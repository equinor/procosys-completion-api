using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;

public record HistoryCreatedIntegrationEvent(
    string Plant,
    string DisplayName,
    Guid Guid,
    Guid? ParentGuid,
    User EventBy,
    DateTime EventAtUtc,
    List<INewProperty> Properties) : IHistoryItemCreatedV1;
