using System;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemComments;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemCommentsQueryTests
    : AccessValidatorForIIsPunchItemRelatedQueryTests<GetPunchItemCommentsQuery>
{
    protected override GetPunchItemCommentsQuery GetPunchItemQueryWithAccessToProject()
        => new(Guid.Empty)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithAccess)
        };

    protected override GetPunchItemCommentsQuery GetPunchItemQueryWithoutAccessToProject()
        => new(Guid.Empty)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithoutAccess)
        };
}
