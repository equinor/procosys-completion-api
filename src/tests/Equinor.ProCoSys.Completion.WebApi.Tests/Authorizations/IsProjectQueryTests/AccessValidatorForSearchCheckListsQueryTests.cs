using Equinor.ProCoSys.Completion.Query.ProjectQueries.SearchCheckLists;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectQueryTests;

[TestClass]
public class AccessValidatorForSearchCheckListsQueryTests : AccessValidatorForQueryNeedAccessTests<SearchCheckListsQuery>
{
    protected override SearchCheckListsQuery GetQueryWithAccessToProjectToTest()
        => new(ProjectGuidWithAccess, null, null, null, null, null, null);

    protected override SearchCheckListsQuery GetQueryWithoutAccessToProjectToTest()
        => new(ProjectGuidWithoutAccess, null, null, null, null, null, null);
}
