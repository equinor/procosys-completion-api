using System;

namespace Equinor.ProCoSys.Completion.MessageContracts;

// todo 109355/109612 Flytt til Domain.Events.IntegrationEvents.HistoryEvents
public record User(Guid Oid, string FullName) : IUser;
