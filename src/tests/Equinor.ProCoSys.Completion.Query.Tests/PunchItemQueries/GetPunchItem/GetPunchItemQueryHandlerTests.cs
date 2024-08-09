using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItem;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.Tests.PunchItemQueries.GetPunchItem;

[TestClass]
public class GetPunchItemQueryHandlerTests
{
    private PunchItemDetailsDto _punchItemDetails;

    private LibraryItemDto _raisedByOrg;
    private LibraryItemDto _clearingByOrg;
    private const string Code = "A30";

    private IPunchItemService _punchItemServiceMock;
    private GetPunchItemQuery _query;
    private GetPunchItemQueryHandler _dut;

    [TestInitialize]
    public void Setup_OkState()
    {
        _raisedByOrg = new LibraryItemDto(Guid.NewGuid(), Code, "raisedBy", LibraryType.COMPLETION_ORGANIZATION.ToString());
        _clearingByOrg = new LibraryItemDto(Guid.NewGuid(), Code, "clearingBy", LibraryType.COMPLETION_ORGANIZATION.ToString());

        _punchItemDetails = PunchItemDetailsDtoMock(_raisedByOrg, _clearingByOrg);

        _punchItemServiceMock = Substitute.For<IPunchItemService>();
        _punchItemServiceMock.GetByGuid(_punchItemDetails.Guid, default).Returns(_punchItemDetails);

        _query = new GetPunchItemQuery(_punchItemDetails.Guid);
        _dut = new GetPunchItemQueryHandler(_punchItemServiceMock);
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
        Assert.AreEqual(punchItemDetails, _punchItemDetails);
    }
    
    [TestMethod]
    public async Task Handle_ShouldReturnNotFoundOnNonExistentPunch()
    {
        // Act
        _punchItemServiceMock.GetByGuid(_punchItemDetails.Guid, default).Returns((PunchItemDetailsDto) null);
        var result = await _dut.Handle(_query, default);
        var punchItemDetails = result.Data;

        // Assert
        Assert.AreEqual(ResultType.NotFound, result.ResultType);
        Assert.IsNull(punchItemDetails);
    }

    private PunchItemDetailsDto PunchItemDetailsDtoMock(LibraryItemDto raisedByOrg, LibraryItemDto clearedByOrg) =>
        new(
            Guid: Guid.NewGuid(),
            CheckListGuid: Guid.NewGuid(),
            ProjectName: "Mock Project",
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
            RowVersion: "AAAAAAAAABA="
        );
}
