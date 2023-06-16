using System;
using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchAttachmentDownloadUrl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public class AccessValidatorForGetPunchAttachmentDownloadUrlQueryTests : AccessValidatorForIPunchQueryTests<GetPunchAttachmentDownloadUrlQuery>
{
    protected override GetPunchAttachmentDownloadUrlQuery GetPunchCommandWithAccessToProject()
        => new(PunchGuidWithAccessToProject, Guid.Empty);

    protected override GetPunchAttachmentDownloadUrlQuery GetPunchCommandWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject, Guid.Empty);
}
