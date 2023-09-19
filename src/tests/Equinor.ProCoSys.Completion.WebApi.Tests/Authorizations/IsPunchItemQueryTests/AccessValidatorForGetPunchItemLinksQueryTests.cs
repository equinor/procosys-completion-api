using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemLinks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemLinksQueryTests : AccessValidatorForIIsPunchItemQueryTests<GetPunchItemLinksQuery>
{
    protected override GetPunchItemLinksQuery GetPunchItemQueryWithAccessToProject()
        => new(PunchItemGuidWithAccessToProjectAndContent);

    protected override GetPunchItemLinksQuery GetPunchItemQueryWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject);
}
