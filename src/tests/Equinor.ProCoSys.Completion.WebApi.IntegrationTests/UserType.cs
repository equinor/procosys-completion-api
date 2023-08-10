namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public enum UserType
{
    Anonymous,
    Writer,
    // todo setup RestrictedWriter which have restriction role for a Responsible
    Reader,
    NoPermissionUser
}
