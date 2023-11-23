using System;

namespace Equinor.ProCoSys.Completion.MessageContracts;

public interface IUser
{
    Guid Oid { get; init; }
    string FullName { get; init; }
}
