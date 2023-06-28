using System;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public record PersonDto(
    Guid Guid,
    string FirstName,
    string LastName,
    string UserName,
    string Email);
