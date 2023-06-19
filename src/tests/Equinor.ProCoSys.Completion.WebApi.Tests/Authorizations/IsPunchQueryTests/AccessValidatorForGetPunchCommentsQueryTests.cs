using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchComments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public class AccessValidatorForGetPunchCommentsQueryTests : AccessValidatorForIIsPunchQueryTests<GetPunchCommentsQuery>
{
    protected override GetPunchCommentsQuery GetPunchQueryWithAccessToProject()
        => new(PunchGuidWithAccessToProject);

    protected override GetPunchCommentsQuery GetPunchQueryWithoutAccessToProject()
        => new(PunchGuidWithoutAccessToProject);
}
