using System;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItem;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Authorizations.IsPunchItemQueryTests;

[TestClass]
public class AccessValidatorForGetPunchItemQueryTests : AccessValidatorForQueryNeedAccessTests<GetPunchItemQuery>
{
    protected override GetPunchItemQuery GetQueryWithAccessToProjectToTest()
        => new(Guid.Empty)
        {
            PunchItemDetailsDto = PunchItemDetailsDtoMock(ProjectGuidWithAccess)
        };

    protected override GetPunchItemQuery GetQueryWithoutAccessToProjectToTest()
        => new(Guid.Empty)
        {
            PunchItemDetailsDto = PunchItemDetailsDtoMock(ProjectGuidWithoutAccess)
        };

    private PunchItemDetailsDto PunchItemDetailsDtoMock(Guid projectGuid) =>
        new(
            Guid: Guid.NewGuid(),
            CheckListGuid: Guid.NewGuid(),
            ProjectName: null!,
            ProjectGuid: projectGuid,
            ItemNo: 0,
            Category: null!,
            Description: null!,
            CreatedBy: null!,
            CreatedAtUtc: DateTime.UtcNow,
            ModifiedBy: null,
            ModifiedAtUtc: null,
            IsReadyToBeCleared: true,
            IsReadyToBeUncleared: false,
            ClearedBy: null,
            ClearedAtUtc: null,
            IsReadyToBeRejected: false,
            RejectedBy: null,
            RejectedAtUtc: null,
            IsReadyToBeVerified: true,
            IsReadyToBeUnverified: false,
            VerifiedBy: null,
            VerifiedAtUtc: null,
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
            AttachmentCount: 0,
            RowVersion: null!
        );
}
