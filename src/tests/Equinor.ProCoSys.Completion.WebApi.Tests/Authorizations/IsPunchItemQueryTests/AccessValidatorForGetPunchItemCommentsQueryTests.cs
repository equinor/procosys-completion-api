using System;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemComments;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemCommentsQueryTests
    : AccessValidatorForQueryNeedProjectAccessTests<GetPunchItemCommentsQuery>
{
    protected override GetPunchItemCommentsQuery GetQueryWithAccessToProjectToTest()
        => new(Guid.Empty)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithAccess)
        };

    protected override GetPunchItemCommentsQuery GetQueryWithoutAccessToProjectToTest()
        => new(Guid.Empty)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithoutAccess)
        };
}
