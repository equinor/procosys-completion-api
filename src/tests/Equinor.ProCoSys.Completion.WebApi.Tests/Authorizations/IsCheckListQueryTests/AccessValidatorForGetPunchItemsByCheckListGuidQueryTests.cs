using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsByCheckList;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsCheckListQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemsByCheckListGuidQueryTests : AccessValidatorForIIsCheckListQueryTests<GetPunchItemsByCheckListGuidQuery>
{
    protected override GetPunchItemsByCheckListGuidQuery GetCheckListQueryWithAccessToProject() 
        => new (CheckListGuidWithAccessToProjectAndContent, PunchListStatusFilter.All);

    protected override GetPunchItemsByCheckListGuidQuery GetCheckListQueryWithoutAccessToProject() 
        => new (CheckListGuidWithoutAccessToContent, PunchListStatusFilter.All);
}
