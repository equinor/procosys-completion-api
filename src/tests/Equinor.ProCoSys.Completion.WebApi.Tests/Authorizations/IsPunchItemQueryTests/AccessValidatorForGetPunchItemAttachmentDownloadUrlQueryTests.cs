using System;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachmentDownloadUrl;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemAttachmentDownloadUrlQueryTests
    : AccessValidatorForQueryNeedAccessTests<GetPunchItemAttachmentDownloadUrlQuery>
{
    protected override GetPunchItemAttachmentDownloadUrlQuery GetQueryWithAccessToProjectToTest()
        => new(Guid.Empty, Guid.Empty)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithAccess)
        };

    protected override GetPunchItemAttachmentDownloadUrlQuery GetQueryWithoutAccessToProjectToTest()
        => new(Guid.Empty, Guid.Empty)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithoutAccess)
        };
}
