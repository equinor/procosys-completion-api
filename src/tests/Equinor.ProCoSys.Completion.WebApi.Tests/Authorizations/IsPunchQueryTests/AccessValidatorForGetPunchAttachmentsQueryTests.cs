using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchAttachments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public class AccessValidatorForGetPunchAttachmentsQueryTests : AccessValidatorForIIsPunchQueryTests<GetPunchAttachmentsQuery>
{
    protected override GetPunchAttachmentsQuery GetPunchQueryWithAccessToProject()
        => new(PunchGuidWithAccessToProject);

    protected override GetPunchAttachmentsQuery GetPunchQueryWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject);
}
