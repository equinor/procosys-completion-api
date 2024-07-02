using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetCheckListsByPunchItemGuid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;

[TestClass]
public class AccessValidatorForGetCheckListsByPunchItemGuidQueryTests : AccessValidatorForIIsPunchItemQueryTests<GetCheckListsByPunchItemGuidQuery>
{
    protected override GetCheckListsByPunchItemGuidQuery GetPunchItemQueryWithAccessToProject() 
        => new (PunchItemGuidWithAccessToProjectAndContent);

    protected override GetCheckListsByPunchItemGuidQuery GetPunchItemQueryWithoutAccessToProject() 
        => new (PunchItemGuidWithoutAccessToProject);
}
