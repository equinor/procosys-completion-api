using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;

public class Person : EntityBase, IAggregateRoot, IHaveGuid
{
    public const int FirstNameLengthMax = 128;
    public const int LastNameLengthMax = 128;
    public const int UserNameLengthMax = 128;
    public const int EmailLengthMax = 128;

#pragma warning disable CS8618
    protected Person()
#pragma warning restore CS8618
    {
    }

    public Person(Guid guid, string firstName, string lastName, string userName, string email, bool superuser)
    {
        Guid = guid;
        FirstName = firstName;
        LastName = lastName;
        UserName = userName;
        Email = email;
        Superuser = superuser;
        ProCoSys4LastUpdated = DateTime.Now;
    }

    // private setters needed for Entity Framework
    public Guid Guid { get; private set; } //Azure AD Oid
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public bool Superuser { get; set; }
    public DateTime ProCoSys4LastUpdated { get; set; }
    public DateTime SyncTimestamp { get; set; }

    public string GetFullName() => $"{FirstName} {LastName}";
}
