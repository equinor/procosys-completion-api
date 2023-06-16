using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchLinks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public class AccessValidatorForGetPunchLinksQueryTests : AccessValidatorForIPunchQueryTests<GetPunchLinksQuery>
{
    protected override GetPunchLinksQuery GetPunchCommandWithAccessToProject()
        => new(PunchGuidWithAccessToProject);

    protected override GetPunchLinksQuery GetPunchCommandWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject);
}
