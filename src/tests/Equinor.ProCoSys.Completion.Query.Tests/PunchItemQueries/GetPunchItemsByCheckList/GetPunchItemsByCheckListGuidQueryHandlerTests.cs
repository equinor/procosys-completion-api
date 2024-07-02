using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsByCheckList;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.GetPunchItemsByCheckList;

[TestClass]
public class GetPunchItemsByCheckListGuidQueryHandlerTests
{
    private PunchItemDetailsDto _punchItemDetails;

    private LibraryItemDto _raisedByOrg;
    private LibraryItemDto _clearingByOrg;
    private const string Code = "A30";

    private IPunchItemService _punchItemServiceMock;
    private GetPunchItemsByCheckListGuidQuery _query;
    private GetPunchItemsByCheckListGuidQueryHandler _dut;

    [TestInitialize]
    public void Setup_OkState()
    {
        _raisedByOrg = new LibraryItemDto(Guid.NewGuid(), Code, "raisedBy", LibraryType.COMPLETION_ORGANIZATION.ToString()); 
        _clearingByOrg = new LibraryItemDto(Guid.NewGuid(), Code, "clearingBy", LibraryType.COMPLETION_ORGANIZATION.ToString());

        _punchItemDetails = PunchItemDetailsDtoMock(_raisedByOrg, _clearingByOrg);

        _punchItemServiceMock = Substitute.For<IPunchItemService>();
        _punchItemServiceMock.GetByCheckListGuid(_punchItemDetails.CheckListGuid, default)
            .Returns(new List<PunchItemDetailsDto>(){ _punchItemDetails });

        _query = new GetPunchItemsByCheckListGuidQuery(_punchItemDetails.CheckListGuid);
        _dut = new GetPunchItemsByCheckListGuidQueryHandler(_punchItemServiceMock);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnCorrectCreatedPunchItems()
    {
        // Act
        var result = await _dut.Handle(_query, default);
        var data = result.Data;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ResultType.Ok, result.ResultType);
        Assert.IsTrue(data.Count == 1);
        CollectionAssert.Contains(data.ToList(), _punchItemDetails);
        await _punchItemServiceMock.Received(1).GetByCheckListGuid(_punchItemDetails.CheckListGuid, Arg.Any<CancellationToken>());
        Assert.AreEqual(data.First().Guid, _punchItemDetails.Guid);
    }

    private PunchItemDetailsDto PunchItemDetailsDtoMock(LibraryItemDto raisedByOrg, LibraryItemDto clearedByOrg) =>
        new(
            Guid: Guid.NewGuid(),
            CheckListGuid: Guid.NewGuid(),
            ProjectName: "Mock Project",
            ItemNo: 12345,
            Category: "Mock Category",
            Description: "This is a mock description.",
            CreatedBy: new PersonDto(Guid.NewGuid(), "John Doe", "","", ""),
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
            raisedByOrg,
            clearedByOrg,
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
            RowVersion: "1.0"
        );
}
