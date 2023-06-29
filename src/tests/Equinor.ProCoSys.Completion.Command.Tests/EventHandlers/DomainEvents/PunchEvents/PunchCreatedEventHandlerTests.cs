using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchEvents;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;
using MassTransit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers.DomainEvents.PunchEvents;

[TestClass]
public class PunchCreatedEventHandlerTests : EventHandlerTestBase
{
    private PunchCreatedEventHandler _dut;
    private PunchCreatedEvent _punchCreatedEvent;
    private Mock<IPublishEndpoint> _publishEndpointMock;
    private PunchCreatedMessage _publishedMessage;
    private Mock<ILogger<PunchCreatedEventHandler>> _mockLogger;

    [TestInitialize]
    public void Setup()
    {
        var projectGuid = Guid.NewGuid();
        var punch = new Punch("X", new Project("X", projectGuid, "Pro", "Desc"), "F");
        punch.SetCreated(_person);

        _punchCreatedEvent = new PunchCreatedEvent(punch, projectGuid);
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _mockLogger = new Mock<ILogger<PunchCreatedEventHandler>>();
        _dut = new PunchCreatedEventHandler(_publishEndpointMock.Object, _mockLogger.Object);
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<PunchCreatedMessage>(),It.IsAny<IPipe<PublishContext<PunchCreatedMessage>>>(),default))
            .Callback<PunchCreatedMessage,IPipe<PublishContext<PunchCreatedMessage>>, CancellationToken>((message, _,_) =>
            {
                _publishedMessage = message;
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublishPunchCreatedMessage()
    {
        // Act
        await _dut.Handle(_punchCreatedEvent, default);

        // Assert
        _publishEndpointMock.Verify(p => p.Publish(It.IsAny<PunchCreatedMessage>(),It.IsAny<IPipe<PublishContext<PunchCreatedMessage>>>(), default), Times.Once);
    }

    [TestMethod]
    public async Task Handle_ShouldPublishCorrectPunchCreatedMessage()
    {
        // Act
        await _dut.Handle(_punchCreatedEvent, default);

        // Assert
        Assert.IsNotNull(_publishedMessage);
        Assert.AreEqual(_punchCreatedEvent.ProjectGuid, _publishedMessage.ProjectGuid);
        Assert.AreEqual(_punchCreatedEvent.Punch.Guid, _publishedMessage.Guid);
        Assert.AreEqual(_punchCreatedEvent.Punch.CreatedAtUtc, _publishedMessage.CreatedAtUtc);
        Assert.AreEqual(_punchCreatedEvent.Punch.CreatedByOid, _publishedMessage.CreatedByOid);
        Assert.AreEqual(_punchCreatedEvent.Punch.ItemNo, _publishedMessage.ItemNo);
    }
}
