using System;
using Equinor.ProCoSys.Completion.Query;
using Equinor.ProCoSys.Completion.Query.CheckListQueries.GetPunchItems;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsCheckListQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemsQueryTests
    : AccessValidatorForQueryNeedAccessTests<GetPunchItemsQuery>
{
    protected override GetPunchItemsQuery GetQueryWithAccessToProjectToTest()
        => new(Guid.Empty)
        {
            CheckListDetailsDto = new CheckListDetailsDto(Guid.Empty, "FT", "FG", "RC", "TRC", "TRD", "TFC", "TFD", ProjectGuidWithAccess)
        };

    protected override GetPunchItemsQuery GetQueryWithoutAccessToProjectToTest() 
        => new (Guid.Empty)
        {
            CheckListDetailsDto = new CheckListDetailsDto(Guid.Empty, "FT", "FG", "RC", "TRC", "TRD", "TFC", "TFD", ProjectGuidWithoutAccess)
        };
}
