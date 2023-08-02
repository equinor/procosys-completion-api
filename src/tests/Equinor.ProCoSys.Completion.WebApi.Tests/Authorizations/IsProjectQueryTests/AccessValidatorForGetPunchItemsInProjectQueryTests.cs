using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsInProject;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemsInProjectQueryTests : AccessValidatorForIIsProjectQueryTests<GetPunchItemsInProjectQuery>
{
    protected override GetPunchItemsInProjectQuery GetProjectRequestWithAccessToProjectToTest()
        => new(ProjectGuidWithAccess);

    protected override GetPunchItemsInProjectQuery GetProjectRequestWithoutAccessToProjectToTest()
        => new(ProjectGuidWithoutAccess);
}
