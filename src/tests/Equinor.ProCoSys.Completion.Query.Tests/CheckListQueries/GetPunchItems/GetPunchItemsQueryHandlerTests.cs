﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Equinor.ProCoSys.Completion.Query.CheckListQueries.GetPunchItems;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Query.Tests.CheckListQueries.GetPunchItems;

[TestClass]
public class GetPunchItemsQueryHandlerTests
{
    private PunchItemDetailsDto _punchItemDetails;

    private IPunchItemService _punchItemServiceMock;
    private GetPunchItemsQuery _query;
    private GetPunchItemsQueryHandler _dut;

    [TestInitialize]
    public void Setup_OkState()
    {
        _punchItemDetails = PunchItemDetailsDtoMock();

        _punchItemServiceMock = Substitute.For<IPunchItemService>();
        _punchItemServiceMock.GetByCheckListGuid(_punchItemDetails.CheckListGuid, default)
            .Returns(new List<PunchItemDetailsDto>{ _punchItemDetails });

        _query = new GetPunchItemsQuery(_punchItemDetails.CheckListGuid);
        _dut = new GetPunchItemsQueryHandler(_punchItemServiceMock);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnCorrectCreatedPunchItems()
    {
        // Act
        var result = await _dut.Handle(_query, default);

        // Assert
        var data = result?.ToList();
        Assert.IsInstanceOfType(data, typeof(List<PunchItemDetailsDto>));
        Assert.IsTrue(0 < data.Count);
        CollectionAssert.Contains(data, _punchItemDetails);
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
            RaisedByOrg: new LibraryItemDto(Guid.NewGuid(), "A30", "raisedBy", LibraryType.COMPLETION_ORGANIZATION.ToString()),
            ClearingByOrg: new LibraryItemDto(Guid.NewGuid(), "A30", "clearingBy", LibraryType.COMPLETION_ORGANIZATION.ToString()),
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
