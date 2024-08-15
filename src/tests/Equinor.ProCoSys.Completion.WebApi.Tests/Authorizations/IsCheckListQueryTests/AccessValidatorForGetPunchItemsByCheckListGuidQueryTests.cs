using System;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsByCheckList;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsCheckListQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemsByCheckListGuidQueryTests : AccessValidatorForIIsCheckListQueryTests<GetPunchItemsByCheckListGuidQuery>
{
    protected override GetPunchItemsByCheckListGuidQuery GetCheckListQueryWithAccessToProject()
        => new(Guid.Empty)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithAccess)
        };

    protected override GetPunchItemsByCheckListGuidQuery GetCheckListQueryWithoutAccessToProject() 
        => new (Guid.Empty)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithoutAccess)
        };
}
