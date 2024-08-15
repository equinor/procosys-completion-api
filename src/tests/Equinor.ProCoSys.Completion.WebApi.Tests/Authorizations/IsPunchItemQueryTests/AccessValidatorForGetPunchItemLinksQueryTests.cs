using System;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemLinks;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemLinksQueryTests
    : AccessValidatorForIIsPunchItemRelatedQueryTests<GetPunchItemLinksQuery>
{
    protected override GetPunchItemLinksQuery GetPunchItemQueryWithAccessToProject()
        => new(Guid.Empty)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithAccess)
        };

    protected override GetPunchItemLinksQuery GetPunchItemQueryWithoutAccessToProject()
        => new(Guid.Empty)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithoutAccess)
        };
}
