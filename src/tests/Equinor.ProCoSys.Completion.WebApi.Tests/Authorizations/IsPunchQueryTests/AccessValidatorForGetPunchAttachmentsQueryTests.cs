using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemAttachmentsQueryTests : AccessValidatorForIIsPunchQueryTests<GetPunchItemAttachmentsQuery>
{
    protected override GetPunchItemAttachmentsQuery GetPunchItemQueryWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject);

    protected override GetPunchItemAttachmentsQuery GetPunchItemQueryWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject);
}
