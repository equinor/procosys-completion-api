using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemComments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemCommentsQueryTests : AccessValidatorForIIsPunchQueryTests<GetPunchItemCommentsQuery>
{
    protected override GetPunchItemCommentsQuery GetPunchItemQueryWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject);

    protected override GetPunchItemCommentsQuery GetPunchItemQueryWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject);
}
