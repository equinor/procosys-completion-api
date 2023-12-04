namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests;

public enum UserType
{
    Anonymous,
    Writer,
    // todo 104207 setup RestrictedWriter which have restriction role for a Responsible
    Reader,
    NoPermissionUser
}
