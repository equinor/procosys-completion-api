using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItem;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.GetPunchItem;

[TestClass]
public class GetPunchItemQueryHandlerTests
{
    private GetPunchItemQuery _query;
    private GetPunchItemQueryHandler _dut;

    [TestInitialize]
    public void Setup_OkState()
    {
        var punchItemDetails = PunchItemDetailsDtoMock();

        _query = new GetPunchItemQuery(punchItemDetails.Guid) { PunchItemDetailsDto = punchItemDetails };
        _dut = new GetPunchItemQueryHandler();
    }

    [TestMethod]
    public async Task Handle_ShouldReturnCorrectPunchItem()
    {
        // Act
        var result = await _dut.Handle(_query, default);
        var punchItemDetails = result.Data;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.AreEqual(_query.PunchItemDetailsDto, punchItemDetails);
    }

    private PunchItemDetailsDto PunchItemDetailsDtoMock() =>
        new(
            Guid: Guid.NewGuid(),
            CheckListGuid: Guid.NewGuid(),
            ProjectName: "Mock Project",
            ProjectGuid: Guid.NewGuid(), 
            ItemNo: 12345,
            Category: "Mock Category",
            Description: "This is a mock description.",
            CreatedBy: new PersonDto(Guid.NewGuid(), "John Doe", "", "", ""),
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
            new LibraryItemDto(Guid.NewGuid(), "r", "raisedBy", LibraryType.COMPLETION_ORGANIZATION.ToString()),
            new LibraryItemDto(Guid.NewGuid(), "c", "clearingBy", LibraryType.COMPLETION_ORGANIZATION.ToString()),
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
            AttachmentCount: 3,
            RowVersion: "AAAAAAAAABA="
        );
}
