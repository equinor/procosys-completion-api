using System;
using Equinor.ProCoSys.Completion.Query;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsByCheckList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsCheckListQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemsByCheckListGuidQueryTests
    : AccessValidatorForQueryNeedAccessTests<GetPunchItemsByCheckListGuidQuery>
{
    protected override GetPunchItemsByCheckListGuidQuery GetQueryWithAccessToProjectToTest()
        => new(Guid.Empty)
        {
            CheckListDetailsDto = new CheckListDetailsDto(Guid.Empty, ProjectGuidWithAccess)
        };

    protected override GetPunchItemsByCheckListGuidQuery GetQueryWithoutAccessToProjectToTest() 
        => new (Guid.Empty)
        {
            CheckListDetailsDto = new CheckListDetailsDto(Guid.Empty, ProjectGuidWithoutAccess)
        };
}
