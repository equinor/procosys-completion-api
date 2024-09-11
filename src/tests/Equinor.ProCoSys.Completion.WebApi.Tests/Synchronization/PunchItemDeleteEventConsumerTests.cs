using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class PunchItemDeleteEventConsumerTests
{
    private readonly IPunchItemRepository _punchItemRepoMock = Substitute.For<IPunchItemRepository>();
    
    private readonly IPlantSetter _plantSetter = Substitute.For<IPlantSetter>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly PunchItemDeleteEventConsumer _dut;
    private readonly ConsumeContext<PunchItemDeleteEvent> _contextMock = Substitute.For<ConsumeContext<PunchItemDeleteEvent>>();

    private const string Plant = "PCS$OSEBERG_C";

    public PunchItemDeleteEventConsumerTests() =>
        _dut = new PunchItemDeleteEventConsumer(
            Substitute.For<ILogger<PunchItemDeleteEventConsumer>>(),
            _punchItemRepoMock,
            _unitOfWorkMock
            );


    [TestMethod]
    public async Task Consume_ShouldDeletePunchItem_On_Delete_Behavior()
    {
        // Arrange
        var guid = Guid.NewGuid();

        var bEvent = GetTestEvent(guid, Plant, "delete");
        _contextMock.Message.Returns(bEvent);
        _punchItemRepoMock.RemoveByGuidAsync(guid, Arg.Any<CancellationToken>()).Returns(true);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        await _punchItemRepoMock.Received(1).RemoveByGuidAsync(guid, Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).SaveChangesFromSyncAsync();
    }


    private static PunchItemDeleteEvent GetTestEvent(Guid guid, string plant, string? behavior) =>
        new(guid, plant, behavior);
}
