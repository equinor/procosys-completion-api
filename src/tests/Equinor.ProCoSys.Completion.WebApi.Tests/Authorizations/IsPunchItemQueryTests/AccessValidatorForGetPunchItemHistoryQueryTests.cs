using System;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemHistory;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemHistoryQueryTests
    : AccessValidatorForQueryNeedProjectAccessTests<GetPunchItemHistoryQuery>
{
    protected override GetPunchItemHistoryQuery GetQueryWithAccessToProjectToTest()
        => new(Guid.Empty)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithAccess)
        };

    protected override GetPunchItemHistoryQuery GetQueryWithoutAccessToProjectToTest()
        => new(Guid.Empty)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithoutAccess)
        };
}
