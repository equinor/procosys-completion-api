using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchAttachments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public class AccessValidatorForGetPunchAttachmentsQueryTests : AccessValidatorForIPunchQueryTests<GetPunchAttachmentsQuery>
{
    protected override GetPunchAttachmentsQuery GetPunchCommandWithAccessToProject()
        => new(PunchGuidWithAccessToProject);

    protected override GetPunchAttachmentsQuery GetPunchCommandWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject);
}
