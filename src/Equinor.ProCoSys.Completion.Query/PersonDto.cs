using System;

namespace Equinor.ProCoSys.Completion.Query;

public class PersonDto
{
    public PersonDto(
        Guid guid,
        string firstName,
        string lastName,
        string userName,
        string email)
    {
        Guid = guid;
        FirstName = firstName;
        LastName = lastName;
        UserName = userName;
        Email = email;
    }

    public Guid Guid { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string UserName { get; }
    public string Email { get; }
}
