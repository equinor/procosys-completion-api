using Equinor.ProCoSys.Completion.Query.ProjectQueries.GetPunchItems;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemsQueryTests : AccessValidatorForQueryNeedAccessTests<GetPunchItemsQuery>
{
    protected override GetPunchItemsQuery GetQueryWithAccessToProjectToTest()
        => new(ProjectGuidWithAccess);

    protected override GetPunchItemsQuery GetQueryWithoutAccessToProjectToTest()
        => new(ProjectGuidWithoutAccess);
}
