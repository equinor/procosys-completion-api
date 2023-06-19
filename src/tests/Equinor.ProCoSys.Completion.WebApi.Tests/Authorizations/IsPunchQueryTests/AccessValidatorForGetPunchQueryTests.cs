using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public class AccessValidatorForGetPunchQueryTests : AccessValidatorForIIsPunchQueryTests<GetPunchQuery>
{
    protected override GetPunchQuery GetPunchQueryWithAccessToProject()
        => new(PunchGuidWithAccessToProject);

    protected override GetPunchQuery GetPunchQueryWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject);
}
