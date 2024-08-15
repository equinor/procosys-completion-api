using System;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemLinks;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemLinksQueryTests
    : AccessValidatorForQueryNeedAccessTests<GetPunchItemLinksQuery>
{
    protected override GetPunchItemLinksQuery GetQueryWithAccessToProjectToTest()
        => new(Guid.Empty)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithAccess)
        };

    protected override GetPunchItemLinksQuery GetQueryWithoutAccessToProjectToTest()
        => new(Guid.Empty)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithoutAccess)
        };
}
