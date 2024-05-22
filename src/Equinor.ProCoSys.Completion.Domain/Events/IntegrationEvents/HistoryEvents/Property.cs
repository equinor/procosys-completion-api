using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;

public record Property(
    string Name, 
    object? CurrentValue, 
    ValueDisplayType ValueDisplayType = ValueDisplayType.StringAsText)
    : IProperty;
