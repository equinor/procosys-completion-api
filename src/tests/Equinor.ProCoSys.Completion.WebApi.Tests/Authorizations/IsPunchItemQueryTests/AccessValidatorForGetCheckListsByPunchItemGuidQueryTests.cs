using System;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetCheckListsByPunchItemGuid;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;

[TestClass]
public class AccessValidatorForGetCheckListsByPunchItemGuidQueryTests
    : AccessValidatorForIIsPunchItemRelatedQueryTests<GetCheckListsByPunchItemGuidQuery>
{
    protected override GetCheckListsByPunchItemGuidQuery GetPunchItemQueryWithAccessToProject()
        => new(Guid.Empty)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithAccess)
        };

    protected override GetCheckListsByPunchItemGuidQuery GetPunchItemQueryWithoutAccessToProject()
        => new(Guid.Empty)
        {
            ProjectDetailsDto = new ProjectDetailsDto("P", ProjectGuidWithoutAccess)
        };
}
