namespace Equinor.ProCoSys.Completion.WebApi.Authorizations;

public interface IProjectAccessChecker
{
    bool HasCurrentUserAccessToProject(string projectName);
}