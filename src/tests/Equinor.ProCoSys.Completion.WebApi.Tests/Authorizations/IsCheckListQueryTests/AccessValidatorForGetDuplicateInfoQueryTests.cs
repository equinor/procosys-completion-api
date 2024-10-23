using System;
using Equinor.ProCoSys.Completion.Query;
using Equinor.ProCoSys.Completion.Query.CheckListQueries.GetDuplicateInfo;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsCheckListQueryTests;

[TestClass]
public class AccessValidatorForGetDuplicateInfoQueryTests
    : AccessValidatorForQueryNeedProjectAccessTests<GetDuplicateInfoQuery>
{
    protected override GetDuplicateInfoQuery GetQueryWithAccessToProjectToTest()
        => new(Guid.Empty)
        {
            CheckListDetailsDto = new CheckListDetailsDto(Guid.Empty, "FT", "FG", "RC", "TRC", "TRD", "TFC", "TFD", ProjectGuidWithAccess)
        };

    protected override GetDuplicateInfoQuery GetQueryWithoutAccessToProjectToTest() 
        => new (Guid.Empty)
        {
            CheckListDetailsDto = new CheckListDetailsDto(Guid.Empty, "FT", "FG", "RC", "TRC", "TRD", "TFC", "TFD", ProjectGuidWithoutAccess)
        };
}
