using System;

namespace Equinor.ProCoSys.Completion.Query;

public record PersonDto(
    Guid Guid,
    string FirstName,
    string LastName,
    string UserName,
    string Email);
