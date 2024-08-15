using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsInProject;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemsInProjectQueryTests : AccessValidatorForQueryNeedAccessTests<GetPunchItemsInProjectQuery>
{
    protected override GetPunchItemsInProjectQuery GetQueryWithAccessToProjectToTest()
        => new(ProjectGuidWithAccess);

    protected override GetPunchItemsInProjectQuery GetQueryWithoutAccessToProjectToTest()
        => new(ProjectGuidWithoutAccess);
}
