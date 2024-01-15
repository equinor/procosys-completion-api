using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.HistoryEvents;

// todo 109354 to be renamed to Property after current Property is renamed to ChangedProperty
public record NewProperty(string Name, object? Value) : INewProperty;
