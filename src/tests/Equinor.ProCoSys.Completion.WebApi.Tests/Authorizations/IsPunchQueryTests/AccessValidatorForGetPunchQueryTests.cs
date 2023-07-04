using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemQueryTests : AccessValidatorForIIsPunchQueryTests<GetPunchItemQuery>
{
    protected override GetPunchItemQuery GetPunchItemQueryWithAccessToProject()
        => new(PunchItemGuidWithAccessToProject);

    protected override GetPunchItemQuery GetPunchItemQueryWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject);
}
