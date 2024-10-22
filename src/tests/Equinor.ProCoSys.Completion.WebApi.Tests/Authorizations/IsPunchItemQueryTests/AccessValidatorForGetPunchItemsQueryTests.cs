using System;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItems;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemsQueryTests : AccessValidatorForQueryNeedManyProjectAccessTests<GetPunchItemsQuery>
{
    protected override GetPunchItemsQuery GetQueryWithAccessToAllProjectsToTest()
        => new([])
        {
            PunchItemsDetailsDto = [PunchItemTinyDetailsDtoMock(ProjectGuidWithAccess)]
        };

    protected override GetPunchItemsQuery GetQueryWithoutAccessToAllProjectsToTest()
        => new([])
        {
            PunchItemsDetailsDto = 
            [
                PunchItemTinyDetailsDtoMock(ProjectGuidWithAccess), 
                PunchItemTinyDetailsDtoMock(ProjectGuidWithoutAccess)
            ]
        };

    private PunchItemTinyDetailsDto PunchItemTinyDetailsDtoMock(Guid projectGuid) =>
        new(
            Guid: Guid.NewGuid(),
            CheckListGuid: Guid.NewGuid(),
            ProjectName: null!,
            ProjectGuid: projectGuid,
            ItemNo: 0,
            Category: null!,
            Description: null!,
            IsReadyToBeCleared: true,
            IsReadyToBeUncleared: false,
            IsReadyToBeRejected: false,
            IsReadyToBeVerified: true,
            IsReadyToBeUnverified: false,
            RaisedByOrg: null!,
            ClearingByOrg: null!,
            Priority: null,
            Sorting: null,
            Type: null,
            ActionBy: null,
            DueTimeUtc: null,
            Estimate: null,
            ExternalItemNo: null,
            MaterialRequired: true,
            MaterialETAUtc: null,
            MaterialExternalNo: null,
            WorkOrder: null,
            OriginalWorkOrder: null,
            Document: null,
            SWCR: null,
            RowVersion: null!
        );
}
