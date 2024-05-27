using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemHistory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemHistoryQueryTests : AccessValidatorForIIsPunchItemQueryTests<GetPunchItemHistoryQuery>
{
    protected override GetPunchItemHistoryQuery GetPunchItemQueryWithAccessToProject()
        => new(PunchItemGuidWithAccessToProjectAndContent);

    protected override GetPunchItemHistoryQuery GetPunchItemQueryWithoutAccessToProject()
        => new(PunchItemGuidWithoutAccessToProject);
}
