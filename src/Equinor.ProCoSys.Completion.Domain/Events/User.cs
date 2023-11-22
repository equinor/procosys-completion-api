using System;
using Equinor.ProCoSys.Completion.MessageContracts;

namespace Equinor.ProCoSys.Completion.Domain.Events;

public record User(Guid Oid, string FullName) : IUser;
