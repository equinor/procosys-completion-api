using System;
using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchAttachmentDownloadUrl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public class AccessValidatorForGetPunchAttachmentDownloadUrlQueryTests : AccessValidatorForIIsPunchQueryTests<GetPunchAttachmentDownloadUrlQuery>
{
    protected override GetPunchAttachmentDownloadUrlQuery GetPunchQueryWithAccessToProject()
        => new(PunchGuidWithAccessToProject, Guid.Empty);

    protected override GetPunchAttachmentDownloadUrlQuery GetPunchQueryWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject, Guid.Empty);
}
