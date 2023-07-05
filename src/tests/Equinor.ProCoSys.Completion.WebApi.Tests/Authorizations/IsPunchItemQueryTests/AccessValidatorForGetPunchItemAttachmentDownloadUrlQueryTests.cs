using System;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachmentDownloadUrl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemAttachmentDownloadUrlQueryTests : AccessValidatorForIIsPunchItemQueryTests<GetPunchItemAttachmentDownloadUrlQuery>
{
    protected override GetPunchItemAttachmentDownloadUrlQuery GetPunchItemQueryWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject, Guid.Empty);

    protected override GetPunchItemAttachmentDownloadUrlQuery GetPunchItemQueryWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, Guid.Empty);
}
