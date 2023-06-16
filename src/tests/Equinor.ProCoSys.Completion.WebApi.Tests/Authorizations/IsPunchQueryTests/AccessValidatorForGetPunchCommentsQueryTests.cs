using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchComments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public class AccessValidatorForGetPunchCommentsQueryTests : AccessValidatorForIPunchQueryTests<GetPunchCommentsQuery>
{
    protected override GetPunchCommentsQuery GetPunchCommandWithAccessToProject()
        => new(PunchGuidWithAccessToProject);

    protected override GetPunchCommentsQuery GetPunchCommandWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject);
}
