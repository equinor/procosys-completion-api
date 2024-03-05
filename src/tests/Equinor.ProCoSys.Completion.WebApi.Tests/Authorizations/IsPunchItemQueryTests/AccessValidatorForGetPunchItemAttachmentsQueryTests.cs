using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemAttachmentsQueryTests : AccessValidatorForIIsPunchItemQueryTests<GetPunchItemAttachmentsQuery>
{
    protected override GetPunchItemAttachmentsQuery GetPunchItemQueryWithAccessToProject()
        => new(PunchItemGuidWithAccessToProjectAndContent, null, null);

    protected override GetPunchItemAttachmentsQuery GetPunchItemQueryWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject, null, null);
}
