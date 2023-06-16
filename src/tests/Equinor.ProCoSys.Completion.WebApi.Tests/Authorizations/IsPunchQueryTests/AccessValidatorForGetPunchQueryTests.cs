using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public class AccessValidatorForGetPunchQueryTests : AccessValidatorForIPunchQueryTests<GetPunchQuery>
{
    protected override GetPunchQuery GetPunchCommandWithAccessToProject()
        => new(PunchGuidWithAccessToProject);

    protected override GetPunchQuery GetPunchCommandWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject);
}
