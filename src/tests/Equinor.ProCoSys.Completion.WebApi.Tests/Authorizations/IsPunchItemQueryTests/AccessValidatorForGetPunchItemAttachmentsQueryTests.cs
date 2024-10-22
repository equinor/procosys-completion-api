using System;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachments;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemAttachmentsQueryTests
    : AccessValidatorForQueryNeedProjectAccessTests<GetPunchItemAttachmentsQuery>
{
    protected override GetPunchItemAttachmentsQuery GetQueryWithAccessToProjectToTest()
        => new(Guid.Empty, null!, null!)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithAccess)
        };

    protected override GetPunchItemAttachmentsQuery GetQueryWithoutAccessToProjectToTest()
        => new(Guid.Empty, null!, null!)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithoutAccess)
        };
}
