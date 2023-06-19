using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchLinks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public class AccessValidatorForGetPunchLinksQueryTests : AccessValidatorForIIsPunchQueryTests<GetPunchLinksQuery>
{
    protected override GetPunchLinksQuery GetPunchQueryWithAccessToProject()
        => new(PunchGuidWithAccessToProject);

    protected override GetPunchLinksQuery GetPunchQueryWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject);
}
