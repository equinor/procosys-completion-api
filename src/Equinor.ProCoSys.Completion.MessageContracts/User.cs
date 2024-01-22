using System;

namespace Equinor.ProCoSys.Completion.MessageContracts;

public record User(Guid Oid, string FullName) : IUser;
