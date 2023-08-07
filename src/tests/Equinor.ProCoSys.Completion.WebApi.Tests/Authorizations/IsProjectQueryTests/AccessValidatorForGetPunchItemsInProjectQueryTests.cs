using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsInProject;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemsInProjectQueryTests : AccessValidatorForIIsProjectQueryTests<GetPunchItemsInProjectQuery>
{
    protected override GetPunchItemsInProjectQuery GetProjectQueryWithAccessToProjectToTest()
        => new(ProjectGuidWithAccess);

    protected override GetPunchItemsInProjectQuery GetProjectQueryWithoutAccessToProjectToTest()
        => new(ProjectGuidWithoutAccess);
}
