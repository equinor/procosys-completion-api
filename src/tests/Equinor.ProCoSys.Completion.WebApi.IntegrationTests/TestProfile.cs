using System;
using System.Text;
using Newtonsoft.Json;
using AuthPerson = Equinor.ProCoSys.Auth.Person.ProCoSysPerson;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public class TestProfile
{
    public string Oid { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; }
    public string UserName { get; set; }
    public bool Superuser { get; set; }
    public bool IsAppToken { get; set; }
    public string[] AppRoles { get; set; }

    public AuthPerson AsAuthProCoSysPerson()
        => new AuthPerson
        {
            AzureOid = Oid ?? throw new ArgumentException($"Bad test setup. {nameof(Oid)} needed"),
            Email = Email ?? throw new ArgumentException($"Bad test setup. {nameof(Email)} needed"),
            FirstName = FirstName ?? throw new ArgumentException($"Bad test setup. {nameof(FirstName)} needed"),
            LastName = LastName ?? throw new ArgumentException($"Bad test setup. {nameof(LastName)} needed"),
            UserName = UserName ?? throw new ArgumentException($"Bad test setup. {nameof(UserName)} needed")
        };

    public Guid Guid => new(Oid);

    public override string ToString() => $"{FullName} {Oid}";
        
    /// <summary>
    /// Wraps profile by serializing, encoding and then converting to base 64 string.
    /// "Bearer" is also added, making it ready to be added as Authorization header
    /// </summary>
    /// <returns>Serialized, encoded string ready for authorization header</returns>
    public string CreateBearerToken()
    {
        var serialized = JsonConvert.SerializeObject(this);
        var tokenBytes = Encoding.UTF8.GetBytes(serialized);
        var tokenString = Convert.ToBase64String(tokenBytes);

        return $"Bearer {tokenString}";
    }
}
