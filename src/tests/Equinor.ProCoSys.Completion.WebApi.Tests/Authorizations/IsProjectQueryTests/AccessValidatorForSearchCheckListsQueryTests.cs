using Equinor.ProCoSys.Completion.Query.ProjectQueries.SearchCheckLists;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsProjectQueryTests;

[TestClass]
public class AccessValidatorForSearchCheckListsQueryTests : AccessValidatorForQueryNeedProjectAccessTests<SearchCheckListsQuery>
{
    protected override SearchCheckListsQuery GetQueryWithAccessToProjectToTest()
        => new(ProjectGuidWithAccess, false, null, null, null, null, null, null);

    protected override SearchCheckListsQuery GetQueryWithoutAccessToProjectToTest()
        => new(ProjectGuidWithoutAccess, false, null, null, null, null, null, null);
}
