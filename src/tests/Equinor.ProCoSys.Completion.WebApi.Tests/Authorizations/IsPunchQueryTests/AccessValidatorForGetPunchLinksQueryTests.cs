using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemLinks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemLinksQueryTests : AccessValidatorForIIsPunchQueryTests<GetPunchItemLinksQuery>
{
    protected override GetPunchItemLinksQuery GetPunchItemQueryWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject);

    protected override GetPunchItemLinksQuery GetPunchItemQueryWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject);
}
