using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsByCheckList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsCheckListQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemsByCheckListGuidQueryTests : AccessValidatorForIIsCheckListQueryTests<GetPunchItemsByCheckListGuidQuery>
{
    protected override GetPunchItemsByCheckListGuidQuery GetCheckListQueryWithAccessToProject() 
        => new (CheckListGuidWithAccessToProjectAndContent);

    protected override GetPunchItemsByCheckListGuidQuery GetCheckListQueryWithoutAccessToProject() 
        => new (CheckListGuidWithoutAccessToProject);
}
