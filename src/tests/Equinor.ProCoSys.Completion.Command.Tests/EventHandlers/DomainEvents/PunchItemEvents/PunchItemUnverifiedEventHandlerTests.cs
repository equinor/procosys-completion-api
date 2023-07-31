using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemDomainEvents;
using MassTransit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents.IntegrationEvents;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers.DomainEvents.PunchItemEvents;

[TestClass]
public class PunchItemUnverifiedEventHandlerTests : EventHandlerTestBase
{
    private PunchItemUnverifiedEventHandler _dut;
    private PunchItemUnverifiedDomainEvent _punchItemUnverifiedEvent;
    private Mock<IPublishEndpoint> _publishEndpointMock;
    private PunchItemUnverifiedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        var punchItem = new PunchItem("X", new Project("X", Guid.NewGuid(), "Pro", "Desc"), "F");
        punchItem.Clear(_person);
        punchItem.Verify(_person);
        punchItem.SetModified(_person);

        _punchItemUnverifiedEvent = new PunchItemUnverifiedDomainEvent(punchItem);
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _dut = new PunchItemUnverifiedEventHandler(_publishEndpointMock.Object, new Mock<ILogger<PunchItemUnverifiedEventHandler>>().Object);
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<PunchItemUnverifiedIntegrationEvent>(),
                It.IsAny<IPipe<PublishContext<PunchItemUnverifiedIntegrationEvent>>>(), default))
            .Callback<PunchItemUnverifiedIntegrationEvent, IPipe<PublishContext<PunchItemUnverifiedIntegrationEvent>>,
                CancellationToken>((evt, _, _) =>
            {
                _publishedIntegrationEvent = evt;
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_PunchItemUnverifiedIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemUnverifiedEvent, default);

        // Assert
        _publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<PunchItemUnverifiedIntegrationEvent>(),
                It.IsAny<IPipe<PublishContext<PunchItemUnverifiedIntegrationEvent>>>(), default), Times.Once);
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemUnverifiedEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual("Punch item unverified", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_punchItemUnverifiedEvent.PunchItem.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_punchItemUnverifiedEvent.PunchItem.ModifiedAtUtc, _publishedIntegrationEvent.ModifiedAtUtc);
        Assert.AreEqual(_punchItemUnverifiedEvent.PunchItem.ModifiedByOid, _publishedIntegrationEvent.ModifiedByOid);
    }
}
