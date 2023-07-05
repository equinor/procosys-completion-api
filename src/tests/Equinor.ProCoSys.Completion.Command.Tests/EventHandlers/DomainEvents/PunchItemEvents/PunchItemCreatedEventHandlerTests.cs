using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchItemEvents;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchItemEvents;
using MassTransit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers.DomainEvents.PunchItemEvents;

[TestClass]
public class PunchItemCreatedEventHandlerTests : EventHandlerTestBase
{
    private PunchItemCreatedEventHandler _dut;
    private PunchItemCreatedEvent _punchItemCreatedEvent;
    private Mock<IPublishEndpoint> _publishEndpointMock;
    private PunchItemCreatedIntegrationEvent _publishedIntegrationEvent;

    [TestInitialize]
    public void Setup()
    {
        var projectGuid = Guid.NewGuid();
        var punchItem = new PunchItem("X", new Project("X", projectGuid, "Pro", "Desc"), "F");
        punchItem.SetCreated(_person);

        _punchItemCreatedEvent = new PunchItemCreatedEvent(punchItem, projectGuid);
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _dut = new PunchItemCreatedEventHandler(_publishEndpointMock.Object, new Mock<ILogger<PunchItemCreatedEventHandler>>().Object);
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<PunchItemCreatedIntegrationEvent>(),
                It.IsAny<IPipe<PublishContext<PunchItemCreatedIntegrationEvent>>>(), default))
            .Callback<PunchItemCreatedIntegrationEvent, IPipe<PublishContext<PunchItemCreatedIntegrationEvent>>,
                CancellationToken>((evt, _, _) =>
            {
                _publishedIntegrationEvent = evt;
            });
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_PunchItemCreatedIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemCreatedEvent, default);

        // Assert
        _publishEndpointMock.Verify(
            p => p.Publish(It.IsAny<PunchItemCreatedIntegrationEvent>(),
                It.IsAny<IPipe<PublishContext<PunchItemCreatedIntegrationEvent>>>(), default), Times.Once);
    }

    [TestMethod]
    public async Task Handle_ShouldPublish_CorrectIntegrationEvent()
    {
        // Act
        await _dut.Handle(_punchItemCreatedEvent, default);

        // Assert
        Assert.IsNotNull(_publishedIntegrationEvent);
        Assert.AreEqual("Punch item created", _publishedIntegrationEvent.DisplayName);
        Assert.AreEqual(_punchItemCreatedEvent.ProjectGuid, _publishedIntegrationEvent.ProjectGuid);
        Assert.AreEqual(_punchItemCreatedEvent.PunchItem.Guid, _publishedIntegrationEvent.Guid);
        Assert.AreEqual(_punchItemCreatedEvent.PunchItem.CreatedAtUtc, _publishedIntegrationEvent.CreatedAtUtc);
        Assert.AreEqual(_punchItemCreatedEvent.PunchItem.CreatedByOid, _publishedIntegrationEvent.CreatedByOid);
        Assert.AreEqual(_punchItemCreatedEvent.PunchItem.ItemNo, _publishedIntegrationEvent.ItemNo);
    }
}
